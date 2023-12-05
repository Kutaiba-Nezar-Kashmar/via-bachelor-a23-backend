using MediatR;
using Microsoft.Extensions.Logging;
using RecommendationService.Application.V1.GetRecommendations.Engine;
using RecommendationService.Application.V1.GetRecommendations.Repository;
using RecommendationService.Domain;

namespace RecommendationService.Application.V1.GetRecommendations;

public record GetRecommendationsRequest(string UserId, int Limit = 10) : IRequest<Recommendations>;

public class GetRecommendationsHandler : IRequestHandler<GetRecommendationsRequest, Recommendations>
{
    private readonly ILogger<GetRecommendationsHandler> _logger;
    private readonly IRecommendationsEngine _engine;
    private readonly IEventsRepository _eventsRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ISurveyRepository _surveyRepository;

    public GetRecommendationsHandler(
        ILogger<GetRecommendationsHandler> logger,
        IEventsRepository eventsRepository,
        IReviewRepository reviewRepository,
        ISurveyRepository surveyRepository,
        IRecommendationsEngine? engine = null)
    {
        _logger = logger;
        _engine = engine ?? new FrequencyBasedRecommendationsEngine();
        _eventsRepository = eventsRepository;
        _reviewRepository = reviewRepository;
        _surveyRepository = surveyRepository;
    }

    public async Task<Recommendations> Handle(GetRecommendationsRequest request, CancellationToken cancellationToken)
    {
        var survey = await _surveyRepository.GetAsync(request.UserId);
        var reviews = await _reviewRepository.GetReviewsByUserAsync(request.UserId);
        var attendedEvents = _eventsRepository.GetEventsWhereUserHasAttendedAsync(request.UserId);
        var futureEvents = _eventsRepository.GetAllEvents(DateTimeOffset.UtcNow);
        throw new NotImplementedException();
    }
}