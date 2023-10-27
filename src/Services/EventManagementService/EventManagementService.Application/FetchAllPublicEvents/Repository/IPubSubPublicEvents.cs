using System.Text.Json;
using EventManagementService.Application.FetchAllPublicEvents.Exceptions;
using EventManagementService.Domain.Models.Events;
using EventManagementService.Infrastructure.AppSettings;
using Google.Api.Gax;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventManagementService.Application.FetchAllPublicEvents.Repository;

public interface IPubSubPublicEvents
{
    Task PublishEvents(TopicName topicName, IReadOnlyCollection<Event> events);

    Task<IReadOnlyCollection<Event>> FetchEvents(TopicName topicName, SubscriptionName subscriptionName,
        CancellationToken cancellationToken);
}

public class PubSubPublicEvents : IPubSubPublicEvents
{
    private readonly ILogger<PubSubPublicEvents> _logger;
    private readonly string? _serviceAccountKeyJson;

    public PubSubPublicEvents
    (
        ILogger<PubSubPublicEvents> logger,
        IOptions<PubSub> options
    )
    {
        _logger = logger;
        _serviceAccountKeyJson = Environment.GetEnvironmentVariable("SERVICE_ACCOUNT_KEY_JSON") ?? null;
    }

    public async Task PublishEvents
    (
        TopicName topicName,
        IReadOnlyCollection<Event> events
    )
    {
        PublisherServiceApiClient publisherService;
        if (_serviceAccountKeyJson != null)
        {
            publisherService = await new PublisherServiceApiClientBuilder
            {
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
                Credential = GoogleCredential.FromJson(_serviceAccountKeyJson)
            }.BuildAsync();
        }
        else
        {
            publisherService = await new PublisherServiceApiClientBuilder
            {
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction
            }.BuildAsync();
        }

// Use the client as you'd normally do, to create a topic in this example.
        try
        {
            await publisherService.CreateTopicAsync(topicName);
        }
        catch (Exception e)
        {
            throw new PubSubPublisherException($"Cannot publish message: {e.Message}", e);
        }
    }

    public async Task<IReadOnlyCollection<Event>> FetchEvents
    (
        TopicName topicName,
        SubscriptionName subscriptionName,
        CancellationToken cancellationToken
    )
    {
        var events = new List<Event>();

        SubscriberServiceApiClient subscriber;

        if (_serviceAccountKeyJson != null)
        {
            subscriber = await new SubscriberServiceApiClientBuilder
            {
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
                Credential = GoogleCredential.FromJson(_serviceAccountKeyJson)
            }.BuildAsync(cancellationToken);
        }
        else
        {
            subscriber = await new SubscriberServiceApiClientBuilder
            {
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction
            }.BuildAsync(cancellationToken);
        }


        try
        {
            await subscriber.CreateSubscriptionAsync(subscriptionName, topicName, pushConfig: null,
                ackDeadlineSeconds: 60);
        }
        catch (Exception e)
        {
            _logger.LogDebug("Sub already created");
        }

        //subscriptionName, maxMessages: 10, returnImmediately: true
        var response = await subscriber.PullAsync(new PullRequest
        {
            SubscriptionAsSubscriptionName = subscriptionName,
            MaxMessages = 10
        });
        foreach (var received in response.ReceivedMessages)
        {
            var msg = received.Message;
            Console.WriteLine(msg.Data.ToStringUtf8());
            _logger.LogDebug(msg.Data.ToStringUtf8());
            events.AddRange(JsonSerializer.Deserialize<List<Event>>(msg.Data.ToStringUtf8(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!);
            Console.WriteLine(events.ToString());
        }

        if (response.ReceivedMessages.Count > 0)
        {
            await subscriber.AcknowledgeAsync(subscriptionName, response.ReceivedMessages.Select(m => m.AckId));
        }

        // NOTE: Use same subscription every time, since there is no guarantees that 
        //       messages published BEFORE subscription will be put into the new subscription
        // await subscriber.DeleteSubscriptionAsync(subscriptionName);
        return events;
    }
}