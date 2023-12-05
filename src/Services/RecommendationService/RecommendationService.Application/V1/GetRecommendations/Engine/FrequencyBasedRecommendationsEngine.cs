using EventManagementService.Domain.Models;
using EventManagementService.Domain.Models.Events;
using RecommendationService.Application.V1.GetRecommendations.Util;
using RecommendationService.Domain;
using RecommendationService.Domain.Events;

namespace RecommendationService.Application.V1.GetRecommendations.Engine;

public class FrequencyBasedRecommendationsEngine : IRecommendationsEngine
{
    private const float BaselineWeight = 1f;
    private const float BaselineRelevanceScore = 100f;

    public Recommendations Process(User user, IReadOnlyCollection<Event> completedEvents,
        IReadOnlyCollection<Review> reviews, InterestSurvey survey,
        IReadOnlyCollection<Event> futureEvents)
    {
        var relevantEvents = completedEvents
            .Where(e => e.Attendees != null)
            .Where(e => e.Attendees!.Any(u => u.UserId == user.UserId)).ToList();
        var relevantReviews = reviews
            .Where(review => review.ReviewerId == user.UserId)
            .ToList();

        var weights = CalculateWeights(relevantEvents, GetFrequencyMaps(relevantEvents), relevantReviews);

        // tuple where (score, event)
        var scoredEvents = futureEvents.Select(e => (ScoreEvent(e, weights), e));

        var rankedRecommendations = scoredEvents
            .OrderByDescending(pair => pair.Item1) // Highest score first
            .Select(pair => new Recommendation {Event = pair.e, RelevanceScore = pair.Item1})
            .ToList();

        return new Recommendations
        {
            User = user,
            EventsProcessed = relevantEvents,
            ReviewsProcessed = reviews,
            Result = rankedRecommendations
        };
    }

    private float ScoreEvent(Event e, WeightMaps weights)
    {
        float categoryFactor = 1.0f;

        categoryFactor = weights.CategoryWeights.TryGetValue(e.Category, out float categoryWeight)
            ? categoryWeight
            : BaselineWeight;

        var keywordFactors = e.Keywords
            .Where(weights.KeywordWeights.ContainsKey)
            .Select(kw => weights.KeywordWeights[kw])
            .ToList();

        float finalScore = BaselineRelevanceScore * categoryFactor;
        keywordFactors.ForEach(factor => finalScore *= factor);

        return finalScore;
    }

    private WeightMaps CalculateWeights(IReadOnlyCollection<Event> events, FrequencyMaps frequencyMaps,
        IReadOnlyCollection<Review> reviews)
    {
        var categories = Enum.GetValues<Category>();
        var keywords = Enum.GetValues<Keyword>();
        var categoryWeights = new Dictionary<Category, float>();
        var keywordWeights = new Dictionary<Keyword, float>();
        var indexedEvents = EventUtil.IndexEvents(events);

        foreach (var category in categories)
        {
            if (categoryWeights.ContainsKey(category))
            {
                continue;
            }

            categoryWeights.Add(category, 1f);
        }

        foreach (var keyword in keywords)
        {
            if (keywordWeights.ContainsKey(keyword))
            {
                continue;
            }

            keywordWeights.Add(keyword, 1f);
        }

        var orderedCategoryFrequencyAscending = frequencyMaps.CategoryFrequencyMap
            .OrderBy(freq => freq.Value) // Ascending
            .Select(kv => (kv.Key, kv.Value))
            .ToList();

        var orderedKeywordFrequencyAscending = frequencyMaps.KeywordFrequencyMap
            .OrderBy(freq => freq.Value)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();

        foreach (var review in reviews)
        {
            var rating = review.Rate;
            var reviewEvent = indexedEvents[review.EventId];
            switch (rating)
            {
                case 0:
                    var veryBadReviewPenaltyFactor = 0.5f;
                    categoryWeights[reviewEvent.Category] *= veryBadReviewPenaltyFactor;
                    reviewEvent.Keywords.ToList().ForEach(kw => keywordWeights[kw] *= veryBadReviewPenaltyFactor);
                    break;
                case > 0 and < 3:
                    var badReviewPenaltyFactor = 0.9f;
                    categoryWeights[reviewEvent.Category] *= badReviewPenaltyFactor;
                    reviewEvent.Keywords.ToList().ForEach(kw => keywordWeights[kw] *= badReviewPenaltyFactor);
                    break;
                case 3:
                    categoryWeights[reviewEvent.Category] *= 1f;
                    reviewEvent.Keywords.ToList().ForEach(kw => keywordWeights[kw] *= 1f);
                    break;
                case 4:
                    var goodReviewRewardFactor = 1.1f;
                    categoryWeights[reviewEvent.Category] *= 1.1f;
                    reviewEvent.Keywords.ToList().ForEach(kw => keywordWeights[kw] *= goodReviewRewardFactor);
                    break;
                default:
                    var veryGoodReviewRewardFactor = 1.5f;
                    categoryWeights[reviewEvent.Category] *= veryGoodReviewRewardFactor;
                    reviewEvent.Keywords.ToList().ForEach(kw => keywordWeights[kw] *= veryGoodReviewRewardFactor);
                    break;
            }
        }

        var keywordFrequencyWeights =
            Scaling.ScaleToRange(1f, 2f, orderedKeywordFrequencyAscending.Select(kv => (float) kv.Value)).ToList();
        var categoryFrequencyWeights =
            Scaling.ScaleToRange(1f, 2f, orderedCategoryFrequencyAscending.Select(kv => (float) kv.Value)).ToList();

        for (var i = 0; i < keywordFrequencyWeights.Count; i++)
        {
            var keyword = orderedKeywordFrequencyAscending[i].Key;
            keywordWeights[keyword] *= keywordFrequencyWeights[i];
        }

        for (var i = 0; i < orderedCategoryFrequencyAscending.Count; i++)
        {
            var category = orderedCategoryFrequencyAscending[i].Key;
            categoryWeights[category] *= categoryFrequencyWeights[i];
        }

        return new WeightMaps(categoryWeights, keywordWeights);
    }

    private FrequencyMaps GetFrequencyMaps(IReadOnlyCollection<Event> events)
    {
        IDictionary<Category, int> categoryFrequencyMap = new Dictionary<Category, int>();
        IDictionary<Keyword, int> keywordFrequencyMap = new Dictionary<Keyword, int>();
        foreach (var @event in events)
        {
            Category category = @event.Category;
            if (!categoryFrequencyMap.ContainsKey(category))
            {
                categoryFrequencyMap.Add(category, 1);
            }
            else
            {
                categoryFrequencyMap[category] += 1;
            }

            @event.Keywords.ToList().ForEach(kw =>
            {
                if (!keywordFrequencyMap.ContainsKey(kw))
                {
                    keywordFrequencyMap.Add(kw, 1);
                }
                else
                {
                    keywordFrequencyMap[kw] += 1;
                }
            });
        }

        return new FrequencyMaps(categoryFrequencyMap, keywordFrequencyMap);
    }
}