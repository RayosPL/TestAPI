using TaskApi.DTOs;
using TaskApi.Repositories;
using NSubstitute;
using TaskApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework.Internal;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestPlatform.CoreUtilities.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
namespace TaskApi.Tests;

[TestFixture]
public class TripsControllerTest
{
    private TripsController _tripsController;
    private ITripsRepository _tripsRepository;
    private List<SimplifyTripDTO> _trips;
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _trips = new List<SimplifyTripDTO>()
        {
            new () { Name = "Test1", Country = "Poland", StartDate = DateTime.MaxValue},
            new () { Name = "Test2", Country = "Poland", StartDate = DateTime.MaxValue},
            new () { Name = "Test3", Country = "Iceland", StartDate = DateTime.MaxValue},
            new () { Name = "Test4", Country = "Norway", StartDate = DateTime.MaxValue},

        };

        _tripsRepository = Substitute.For<ITripsRepository>();
        _tripsRepository.GetAllTrips().Returns(_trips);
        _tripsRepository.GetSpecificSimplifiedTrips("Poland").Returns(_trips.Where(x => x.Country.Equals("Poland")).ToList());
        _tripsRepository.GetSpecificSimplifiedTrips("Iceland").Returns(_trips.Where(x => x.Country.Equals("Iceland")).ToList());
        _tripsRepository.GetSpecificSimplifiedTrips("USA").Returns(_trips.Where(x => x.Country.Equals("USA")).ToList());
        _tripsRepository.GetConcreteFullTripByName("NOTEXISTING").ReturnsNull();
        _tripsRepository.When(x => x.AddNewTrip(Arg.Any<FullTripDTO>())).Do(x => _trips.Add(new SimplifyTripDTO() { Name = "New", Country = "Poland" }));
        _tripsRepository.AddEmailToTheTrip("test@test.com", Arg.Any<string>()).Returns(true);
        _tripsRepository.AddEmailToTheTrip("false@test.com", Arg.Any<string>()).Returns(false);
        _tripsRepository.When(x => x.RemoveTrip(Arg.Any<string>())).Do( x=> {});

        _tripsController = new TripsController(_tripsRepository);
    }


    [Test]
    public void GetAllTrips_ReturnsAllTrips()
    {
        var result = _tripsController.GetAllTrips() as OkObjectResult;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            var items = result.Value as List<SimplifyTripDTO>;
            Assert.That(items, Is.Not.Null);
            Assert.That(items.Count(), Is.EqualTo(_trips.Count()));
        });
    }

    [Test]
    [TestCase("Poland", 2)]
    [TestCase("Iceland", 1)]
    [TestCase("USA", 0)]
    public void GetTrips_ReturnsFilteredTrips(string country, int count)
    {
        var result = _tripsController.GetTrips(country) as OkObjectResult;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            var items = result.Value as List<SimplifyTripDTO>;
            Assert.That(items, Is.Not.Null);
            Assert.That(items.Count(), Is.EqualTo(count));
        });
    }

    [Test]
    public void GetConcreteTripByName_ReturnsConcreteTrip()
    {
        var result = _tripsController.GetConcreteTripByName("Full Trip") as OkObjectResult;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value as FullTripDTO, Is.Not.Null);
        });
    }

    [Test]
    public void GetConcreteTripByName_ReturnsNotFound()
    {
        var result = _tripsController.GetConcreteTripByName("NOTEXISTING") as NotFoundResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void PostTrip_ReturnsOkResult()
    {
        var result = _tripsController.PostTrip(new FullTripDTO()) as OkObjectResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void PostTrip_ReturnsBadRequestIfAlreadyExists()
    {
        var fullTripDto = new FullTripDTO()
        {
            Name = "Full Trip",
            Country = "USA",
            Description = "We're all living in America",
            NumberOfSeats = 5,
            StartDate = DateTime.MinValue
        };

        _tripsRepository.GetConcreteFullTripByName(fullTripDto.Name).Returns(fullTripDto);
        var result = _tripsController.PostTrip(fullTripDto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void EditTrip_ReturnsOkResult()
    {
        var fullTripDto = new FullTripDTO()
        {
            Name = "New Trip",
            Country = "Canada",
            Description = "Peaceful walk",
            NumberOfSeats = 5,
            StartDate = DateTime.MinValue
        };
        var result = _tripsController.EditTrip("Test1", fullTripDto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void EditTrip_ReturnsBadRequestIfNameAlreadyTaken()
    {
        var fullTripDto = new FullTripDTO()
        {
            Name = "Full Trip",
            Country = "USA",
            Description = "We're all living in America",
            NumberOfSeats = 0,
            StartDate = DateTime.MinValue
        };
        _tripsRepository.GetConcreteFullTripByName(fullTripDto.Name).Returns(fullTripDto);

        var result = _tripsController.EditTrip("Test1", fullTripDto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void RegisterForATrip_ReturnsOkResult()
    {
        var fullTripDto = new FullTripDTO()
        {
            Name = "Full Trip",
            Country = "USA",
            Description = "We're all living in America",
            NumberOfSeats = 5,
            StartDate = DateTime.MinValue
        };
        _tripsRepository.GetConcreteFullTripByName(fullTripDto.Name).Returns(fullTripDto);

        var email = "test@test.com";
        var result = _tripsController.RegisterForATrip(fullTripDto.Name, email) as OkResult;

        Assert.That(result, Is.Not.Null);
    }
    [Test]
    public void RegisterForATrip_ReturnsBadRequest_WhenEmailAlreadyAdded()
    {        
        var fullTripDto = new FullTripDTO()
        {
            Name = "Full Trip",
            Country = "USA",
            Description = "We're all living in America",
            NumberOfSeats = 5,
            StartDate = DateTime.MinValue
        };
        _tripsRepository.GetConcreteFullTripByName(fullTripDto.Name).Returns(fullTripDto);

        var email = "false@test.com";
        var result = _tripsController.RegisterForATrip(fullTripDto.Name, email) as BadRequestObjectResult;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value.ToString(), Is.EqualTo("Email already registered"));
        });
    }
    [Test]
    public void RegisterForATrip_ReturnsBadRequest_WhenNoSeatsLeft()
    {
        var fullTripDto = new FullTripDTO()
        {
            Name = "Full Trip",
            Country = "USA",
            Description = "We're all living in America",
            NumberOfSeats = 0,
            StartDate = DateTime.MinValue
        };
        _tripsRepository.GetConcreteFullTripByName(fullTripDto.Name).Returns(fullTripDto);

        var email = "false@test.com";
        var result = _tripsController.RegisterForATrip(fullTripDto.Name, email) as BadRequestObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value.ToString(), Is.EqualTo("This trip has no seats available"));
        });
    }
    [Test]
    public void RegisterForATrip_ReturnsBadRequest()
    {
        var email = "false@test.com";
        var result = _tripsController.RegisterForATrip("NOTEXISTING", email) as BadRequestObjectResult;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value.ToString(), Is.EqualTo("This trip does not exists"));
        });
    }

    [Test]
    public void RemoveTrip_ReturnsOkResult()
    {       
        var fullTripDto = new FullTripDTO()
        {
            Name = "Full Trip",
            Country = "USA",
            Description = "We're all living in America",
            NumberOfSeats = 0,
            StartDate = DateTime.MinValue
        };
        _tripsRepository.GetConcreteFullTripByName(fullTripDto.Name).Returns(fullTripDto);

        var result = _tripsController.RemoveTrip(fullTripDto.Name) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
    }
    [Test]
    public void RemoveTrip_ReturnsBadRequest()
    {       

        var result = _tripsController.RemoveTrip("NOTEXISTING") as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
    }
}
