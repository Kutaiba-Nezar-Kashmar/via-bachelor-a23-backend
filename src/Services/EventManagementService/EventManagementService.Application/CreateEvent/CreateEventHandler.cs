using EventManagementService.Application.CreateEvent.Exceptions;
using EventManagementService.Application.CreateEvent.Repository;
using EventManagementService.Application.CreateEvent.Validators;
using EventManagementService.Domain.Models;
using EventManagementService.Domain.Models.Events;
using EventManagementService.Infrastructure.Util;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventManagementService.Application.CreateEvent;

public record CreateEventRequest(Event Event) : IRequest<Event>;

public class CreateEventHandler : IRequestHandler<CreateEventRequest, Event>
{
    private readonly ISqlCreateEvent _sqlCreateEvent;
    private readonly IFirebaseUser _firebaseUser;
    private readonly ILogger<CreateEventHandler> _logger;

    public CreateEventHandler
    (
        ISqlCreateEvent sqlCreateEvent,
        ILogger<CreateEventHandler> logger,
        IFirebaseUser firebaseUser
    )
    {
        _sqlCreateEvent = sqlCreateEvent;
        _logger = logger;
        _firebaseUser = firebaseUser;
    }

    public async Task<Event> Handle(CreateEventRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userExists = await _firebaseUser.UserExistsAsync(request.Event.Host.UserId);
            var mappedEvent = MapEvent(request, userExists);

            EventValidator.ValidateEvents(mappedEvent);

            await _sqlCreateEvent.InsertEvent(mappedEvent);

            _logger.LogInformation($"Event has been successfully created at: {DateTimeOffset.UtcNow}");
            return mappedEvent;
        }
        catch (Exception e)
        {
            _logger.LogError(e.StackTrace);
            _logger.LogCritical($"Cannot create new event at: {DateTimeOffset.UtcNow}", e);
            throw new CreateEventException(
                $"Something went wrong while creating a new event at: {DateTimeOffset.UtcNow}", e);
        }
    }

    private Event MapEvent(CreateEventRequest request, bool userExists)
    {
        EventValidator.ValidateUser(userExists);
        return new Event
        {
            Title = request.Event.Title,
            StartDate = request.Event.StartDate,
            EndDate = request.Event.EndDate,
            CreatedDate = request.Event.CreatedDate,
            LastUpdateDate = request.Event.CreatedDate,
            IsPrivate = request.Event.IsPaid,
            AdultsOnly = request.Event.AdultsOnly,
            IsPaid = request.Event.IsPaid,
            Host = new User
            {
                UserId = request.Event.Host.UserId,
                DisplayName = request.Event.Host.DisplayName,
                PhotoUrl = request.Event.Host.PhotoUrl,
                LastSeenOnline = request.Event.Host.LastSeenOnline,
                CreationDate = request.Event.Host.CreationDate
            },
            MaxNumberOfAttendees = request.Event.MaxNumberOfAttendees,
            Url = request.Event.Url,
            Description = request.Event.Description,
            Location = request.Event.Location,
            City = request.Event.City,
            GeoLocation = new GeoLocation
            {
                Lat = request.Event.GeoLocation.Lat,
                Lng = request.Event.GeoLocation.Lng
            },
            AccessCode =
                UniqueEventAccessCodeGenerator.GenerateUniqueString(request.Event.Title, request.Event.CreatedDate),
            Category = request.Event.Category,
            Keywords = request.Event.Keywords,
            Images = request.Event.Images,
            Attendees = request.Event.Attendees
        };
    }
}