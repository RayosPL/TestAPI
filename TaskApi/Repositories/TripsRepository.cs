using Microsoft.EntityFrameworkCore;
using TaskApi.DTOs;
using TaskApi.Models;

namespace TaskApi.Repositories;

public class TripsRepository : ITripsRepository
{
    private readonly TripContext _context;
    public TripsRepository(TripContext context)
    {
        _context = context;
    }
    public IEnumerable<SimplifyTripDTO> GetAllTrips()
    {
        return _context.TripItems
            .Select(item => ConvertToSimplifyTripDto(item));
    }

    public IEnumerable<SimplifyTripDTO> GetSpecificSimplifiedTrips(string country)
    {
        return _context.TripItems
        .Where(item => item.Country.Equals(country, StringComparison.OrdinalIgnoreCase))
        .Select(item => ConvertToSimplifyTripDto(item));
    }
    public FullTripDTO? GetConcreteFullTripByName(string name)
    {
        var trip = _context.TripItems.FirstOrDefault(item => item.Name.Equals(name));
        return trip is null ? null : ConvertToFullTripDto(trip);
    }

    public void AddNewTrip(FullTripDTO dto)
    {
        try
        {
            var trip = new TripModel()
            {
                Name = dto.Name,
                NumberOfSeats = dto.NumberOfSeats,
                Description = dto.Description,
                StartDate = dto.StartDate,
                Country = dto.Country
            };
            _context.TripItems.Add(trip);
            _context.SaveChanges();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public void EditTrip(string name, FullTripDTO dto)
    {
        try
        {
            var trip = _context.TripItems.First(trip => trip.Name.Equals(name));
            trip.Name = dto.Name;
            trip.Description = dto.Description;
            trip.StartDate = dto.StartDate;
            trip.Country = dto.Country;
            trip.NumberOfSeats = dto.NumberOfSeats;

            _context.Entry(trip).State = EntityState.Modified;
            _context.SaveChanges();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public bool AddEmailToTheTrip(string email, string name)
    {
        bool emailAdded = false;
        try
        {
            var trip = _context.TripItems.First(trip => trip.Name.Equals(name));
            if (!trip.RegisteredEmails.Contains(email))
            {
                trip.RegisteredEmails.Add(email);
                emailAdded = true;
                trip.NumberOfSeats--;
                _context.Entry(trip).State = EntityState.Modified;
                _context.SaveChanges();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return emailAdded;
    }
    public void RemoveTrip(string name)
    {
        try
        {
            var tripToRemove = _context.TripItems.First(trip => trip.Name.Equals(name));
            _context.TripItems.Remove(tripToRemove);
            _context.SaveChanges();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static SimplifyTripDTO ConvertToSimplifyTripDto(TripModel model) =>
    new()
    {
        Name = model.Name,
        Country = model.Country,
        StartDate = model.StartDate
    };

    private static FullTripDTO ConvertToFullTripDto(TripModel model) =>
    new()
    {
        Name = model.Name,
        Country = model.Country,
        StartDate = model.StartDate,
        Description = model.Description,
        NumberOfSeats = model.NumberOfSeats
    };
}