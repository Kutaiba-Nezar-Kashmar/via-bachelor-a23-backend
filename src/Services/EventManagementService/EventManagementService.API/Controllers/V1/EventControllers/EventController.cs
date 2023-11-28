using System.Net;
using EventManagementService.API.Controllers.V1.EventControllers.Dtos;
using EventManagementService.Application.CreateEvent;
using EventManagementService.Application.FetchAllEvents;
using EventManagementService.Application.JoinEvent;
using EventManagementService.Application.JoinEvent.Exceptions;
using EventManagementService.Application.ProcessExternalEvents;
using EventManagementService.Domain.Models;
using EventManagementService.Domain.Models.Events;
using EventManagementService.Infrastructure.Util;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.API.Controllers.V1.EventControllers;

[ApiController]
[Route("api/v1/events")]
public class EventController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("allEvents")]
    public async Task<ActionResult<List<Event>>> GetAllEvents()
    {
        // TODO: get these from appsetiing.json
        /*
        TopicName topicName = new TopicName("bachelorshenanigans", "vibeverse_events_scraped");
        SubscriptionName subscriptionName = new SubscriptionName("bachelorshenanigans", "eventmanagement");
        */

        var events = await _mediator.Send(new AllEventsRequest());

        return Ok(events);
    }

    [HttpGet("externalEvents")]
    public async Task<ActionResult<IReadOnlyCollection<EventDto>>> GetExternalEvents()
    {
        try
        {
            var events = await _mediator.Send(new ProcessExternalEventsRequest());
            var eventsToReturn = events.Select(ev => new EventDto
                {
                    Title = ev.Title,
                    StartDate = ev.StartDate,
                    LastUpdateDate = ev.LastUpdateDate,
                    EndDate = ev.EndDate,
                    CreatedDate = ev.CreatedDate,
                    Host = new UserDto
                    {
                        UserId = ev.Host.UserId,
                        DateOfBirth = ev.Host.DateOfBirth,
                        DisplayName = ev.Host.DisplayName,
                        PhotoUrl = ev.Host.PhotoUrl,
                        LastSeenOnline = ev.Host.LastSeenOnline,
                        CreationDate = ev.Host.CreationDate
                    },
                    IsPaid = ev.IsPaid,
                    Description = ev.Description,
                    Category = ev.Category.GetDescription(),
                    Keywords = ev.Keywords.Select(EnumExtensions.GetDescription),
                    AdultsOnly = ev.AdultsOnly,
                    IsPrivate = ev.IsPrivate,
                    MaxNumberOfAttendees = ev.MaxNumberOfAttendees,
                    Location = ev.Location,
                    GeoLocation = new GeoLocationDto
                    {
                        Lat = ev.GeoLocation.Lat,
                        Lng = ev.GeoLocation.Lng
                    },
                    City = ev.City
                })
                .ToList();

            return Ok(eventsToReturn);
        }
        catch (Exception e)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost("createEvent")]
    public async Task<ActionResult> CreateNewEvent([FromBody] EventDto eventDto)
    {
        try
        {
            await _mediator.Send(new CreateEventRequest(new Event
            {
                Title = eventDto.Title,
                StartDate = eventDto.StartDate,
                LastUpdateDate = eventDto.LastUpdateDate,
                EndDate = eventDto.EndDate,
                CreatedDate = eventDto.CreatedDate,
                Host = new User
                {
                    LastSeenOnline = eventDto.Host.LastSeenOnline,
                    DisplayName = eventDto.Host.DisplayName,
                    PhotoUrl = eventDto.Host.PhotoUrl,
                    UserId = eventDto.Host.UserId,
                    DateOfBirth = eventDto.Host.DateOfBirth
                },
                IsPaid = eventDto.IsPaid,
                Description = eventDto.Description,
                Category = EnumExtensions.GetEnumValueFromDescription<Category>(eventDto.Category),
                Keywords = eventDto.Keywords.Select(EnumExtensions.GetEnumValueFromDescription<Keyword>),
                AdultsOnly = eventDto.AdultsOnly,
                IsPrivate = eventDto.IsPrivate,
                MaxNumberOfAttendees = eventDto.MaxNumberOfAttendees,
                Location = eventDto.Location,
                GeoLocation = new GeoLocation
                {
                    Lng = eventDto.GeoLocation.Lng,
                    Lat = eventDto.GeoLocation.Lat
                },
                City = eventDto.City
            }));
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}