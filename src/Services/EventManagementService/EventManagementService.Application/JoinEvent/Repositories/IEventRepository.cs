using Dapper;
using EventManagementService.Application.JoinEvent.Data;
using EventManagementService.Domain.Models;
using EventManagementService.Domain.Models.Events;
using EventManagementService.Infrastructure;
using Npgsql;
using Category = EventManagementService.Domain.Models.Events.Category;
using Keyword = EventManagementService.Domain.Models.Events.Keyword;

namespace EventManagementService.Application.JoinEvent.Repositories;

public interface IEventRepository
{
    public Task<Event?> GetByIdAsync(int id);
    public Task<bool> AddAttendeeToEventAsync(string userId, int eventId);
}

public class EventRepository : IEventRepository
{
    private readonly IConnectionStringManager _connectionStringManager;

    public EventRepository(IConnectionStringManager connectionStringManager)
    {
        _connectionStringManager = connectionStringManager;
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        await using var connection = new NpgsqlConnection(_connectionStringManager.GetConnectionString());
        await connection.OpenAsync();

        var queryParams = new { @eventId = id };
        var existingEventQueryResult =
            await connection.QueryFirstAsync<EventEntity>(SqlQueries.QueryAllFromEventTableByEventId, queryParams);

        var keywordsQueryResult = await connection.QueryAsync<int>(SqlQueries.QueryEventKeywords, queryParams);
        var keywords = keywordsQueryResult.Select(kw => (Keyword)kw);

        var images = await connection.QueryAsync<string>(SqlQueries.QueryEventImages, queryParams);

        var attendees = await connection.QueryAsync<string>(SqlQueries.QueryEventAttendees, queryParams);

        Event existingEvent = new()
        {
            Description = existingEventQueryResult.description,
            Category = (Category)existingEventQueryResult.category_id,
            AccessCode = existingEventQueryResult.access_code,
            AdultsOnly = existingEventQueryResult.adult_only,
            CreatedDate = existingEventQueryResult.created_date,
            Keywords = keywords,
            EndDate = existingEventQueryResult.end_date,
            IsPrivate = existingEventQueryResult.is_private,
            StartDate = existingEventQueryResult.start_date,
            LastUpdateDate = existingEventQueryResult.last_update_date,
            MaxNumberOfAttendees = existingEventQueryResult.max_number_of_attendees,
            Host = new User { UserId = existingEventQueryResult.host_id },
            IsPaid = existingEventQueryResult.is_paid,
            Images = images,
            Title = existingEventQueryResult.title,
            Url = existingEventQueryResult.url,
            Attendees = attendees,
            Id = existingEventQueryResult.id,
            /*Location = new Location()
            {
                Id = existingEventQueryResult.location_id,
                City = existingEventQueryResult.city,
                Country = existingEventQueryResult.country,
                SubPremise = existingEventQueryResult.sub_premise,
                GeoLocation = new GeoLocation()
                {
                    Lat = existingEventQueryResult.geolocation_lat,
                    Lng = existingEventQueryResult.geolocation_lng
                },
                PostalCode = existingEventQueryResult.postal_code,
                StreetName = existingEventQueryResult.street_name,
                StreetNumber = existingEventQueryResult.street_number
            }*/
        };
        return existingEvent;
    }

    public async Task<bool> AddAttendeeToEventAsync(string userId, int eventId)
    {
        await using var connection = new NpgsqlConnection(_connectionStringManager.GetConnectionString());
        await connection.OpenAsync();
        var queryParams = new
        {
            @userId = userId,
            @eventId = eventId,
        };
        var rowsAffected = await connection.ExecuteAsync(SqlQueries.AddAttendeeToEvent, queryParams);

        return rowsAffected > 0;
    }
}