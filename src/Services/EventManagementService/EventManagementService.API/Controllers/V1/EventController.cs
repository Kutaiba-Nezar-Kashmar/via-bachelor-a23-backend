using System.Net;
using EventManagementService.API.Controllers.V1.Dtos;
using EventManagementService.Application.FetchAllEvents;
using EventManagementService.Application.JoinEvent;
using EventManagementService.Application.JoinEvent.Exceptions;
using EventManagementService.Domain.Models.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.API.Controllers.V1;

[ApiController]
[Route("api/v1/events")]
public class EventController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EventController> _logger;

    public EventController
    (
        IMediator mediator,
        ILogger<EventController> logger
    )
    {
        _mediator = mediator;
        _logger = logger;
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
}