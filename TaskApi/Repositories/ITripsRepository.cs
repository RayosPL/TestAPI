using Microsoft.AspNetCore.Mvc;
using TaskApi.DTOs;
using TaskApi.Models;

namespace TaskApi.Repositories;

public interface ITripsRepository
{
    public IEnumerable<SimplifyTripDTO> GetAllTrips();
    public IEnumerable<SimplifyTripDTO> GetSpecificSimplifiedTrips(string country);
    public FullTripDTO? GetConcreteFullTripByName(string name);
    public void AddNewTrip(FullTripDTO dto);
    public bool AddEmailToTheTrip(string email, string trip);
    public void EditTrip(string name, FullTripDTO dto);
    public void RemoveTrip(string name);


}