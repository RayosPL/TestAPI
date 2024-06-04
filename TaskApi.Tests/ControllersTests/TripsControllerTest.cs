using TaskApi.DTOs;
using TaskApi.Repositories;
using NSubstitute;
using TaskApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute.ReturnsExtensions;
namespace TaskApi.Tests;

[TestFixture]
public class TripsControllerTest
{
    private TripsController _tripsController;
    private ITripsRepository _tripsRepository;
    private List<SimplifyTripDTO> _trips;
    private FullTripDTO _fullTripDto;
    [OneTimeSetUp]
    public void SetUp()
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
        _fullTripDto = new FullTripDTO()
        {
            Name = "Full Trip",
            Country = "USA",
            Description = "We're all living in America",
            NumberOfSeats = 66,
            StartDate = DateTime.MinValue
        };
        _tripsRepository.GetConcreteFullTripByName(_fullTripDto.Name).Returns(_fullTripDto);
        _tripsRepository.GetConcreteFullTripByName("NOTEXISTING").ReturnsNull();
        _tripsRepository.When(x => x.AddNewTrip(Arg.Any<FullTripDTO>())).Do(x => _trips.Add(new SimplifyTripDTO() { Name = "New", Country = "Poland" }));

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
        var result = _tripsController.PostTrip(_fullTripDto) as BadRequestObjectResult;
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
        var result = _tripsController.EditTrip("Test1", _fullTripDto) as BadRequestObjectResult;
        Assert.That(result, Is.Not.Null);
    }
}
