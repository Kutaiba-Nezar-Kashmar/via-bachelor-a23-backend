namespace EventManagementService.Application.ScraperEvents.Exceptions;

public class PubSubPublisherException : Exception
{
    public PubSubPublisherException(string message, Exception e) : base(message, e)
    {
    }
}