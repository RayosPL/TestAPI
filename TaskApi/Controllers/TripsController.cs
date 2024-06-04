using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskApi.DTOs;
using TaskApi.Models;
using TaskApi.Repositories;

namespace TaskApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TripsController : ControllerBase
{

    private ITripsRepository _tripsRepository;
    public TripsController(ITripsRepository tripsRepository)
    {
        _tripsRepository = tripsRepository;
    }


    [HttpGet]
    public IActionResult GetAllTrips()
    {
        var tripItems = _tripsRepository.GetAllTrips();
        return Ok(tripItems);
    }

    [HttpGet("country/{country}")]
    public IActionResult GetTrips(string country)
    {
        var tripItems = _tripsRepository.GetSpecificSimplifiedTrips(country);
        return Ok(tripItems);
    } 

    [HttpGet("{name}")]
    public IActionResult GetConcreteTripByName(string name)
    {
        var conreteTrip = _tripsRepository.GetConcreteFullTripByName(name);
        return conreteTrip is null ? NotFound() : Ok(conreteTrip);
    }

    [HttpPost]
    public IActionResult PostTrip(FullTripDTO dto)
    {
        var existingTrip = _tripsRepository.GetConcreteFullTripByName(dto.Name);
        if (existingTrip is not null)
        {
            return BadRequest("A trip with given name already exists!");
        }

        _tripsRepository.AddNewTrip(dto);

        return Ok(dto);
    }

    [HttpPut("{name}")]
    public IActionResult EditTrip(string name, FullTripDTO dto)
    {
        var existingTrip = _tripsRepository.GetConcreteFullTripByName(dto.Name);
        if (existingTrip is not null && dto.Name != name)
        {
            return BadRequest("A trip with given name already exists!");
        }
        else
        {
            _tripsRepository.EditTrip(name, dto);
            return Ok(dto);
        }

    }

    [HttpPut("{name}/{email}")]
    public IActionResult RegisterForATrip(string name, string email)
    {
        var trip = _tripsRepository.GetConcreteFullTripByName(name);
        if (trip is not null)
        {
            if(trip.NumberOfSeats == 0)
            {
                return BadRequest("This trip has no seats available");
            }
            else
            {
                var emailAdded = _tripsRepository.AddEmailToTheTrip(email, name);
                return emailAdded ? Ok() : BadRequest("Email already registered");
            }
        }
        else
        {
            return BadRequest("This trip does not exists");
        }
    }
    
    [HttpDelete]
    public IActionResult RemoveTrip(string name)
    {
        var trip = _tripsRepository.GetConcreteFullTripByName(name);
        if (trip is not null)
        {
            _tripsRepository.RemoveTrip(name);
            return Ok(trip);
        }
        else
        {
            return BadRequest("This trip does not exists");
        }
    }

}
