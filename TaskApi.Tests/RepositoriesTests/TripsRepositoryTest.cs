using Microsoft.EntityFrameworkCore;
using TaskApi.DTOs;
using TaskApi.Models;
using TaskApi.Repositories;

namespace TaskApi.Tests;

[TestFixture]
public class TripRepositoryTest
{
    private ITripsRepository _tripsRepository;
    private TripContext _tripContext;

    [SetUp]
    public void SetUp()
    {
        ArrangeTripContext();
        _tripsRepository = new TripsRepository(_tripContext);
    }

    [TearDown]
    public void TearDown()
    {
        _tripContext.Database.EnsureDeleted();
    }

    [Test]
    public void GetAllTrips_ReturnsAll()
    {
        var allTrips = _tripsRepository.GetAllTrips();

        Assert.That(allTrips.Count(), Is.EqualTo(_tripContext.TripItems.Count()));
    }

    [TestCase("Poland", 2)]
    [TestCase("poland", 2)]
    [TestCase("", 0)]
    public void GetSpecificSimplifiedTrips_ReturnsSpecificTrips(string country, int result)
    {
        var specificTrips = _tripsRepository.GetSpecificSimplifiedTrips(country);
        Assert.That(specificTrips.Count(), Is.EqualTo(result));
    }

    [Test]
    public void GetConcreteFullTripByName_ReturnsConcreteTrip()
    {
        var concreteTrip = _tripsRepository.GetConcreteFullTripByName("Test1");
        Assert.Multiple(() =>
        {
            Assert.That(concreteTrip, Is.Not.Null);
            Assert.That(concreteTrip.Name, Is.EqualTo(_tripContext.TripItems.First().Name));
            Assert.That(concreteTrip.Country, Is.EqualTo(_tripContext.TripItems.First().Country));
            Assert.That(concreteTrip.Description, Is.EqualTo(_tripContext.TripItems.First().Description));
            Assert.That(concreteTrip.NumberOfSeats, Is.EqualTo(_tripContext.TripItems.First().NumberOfSeats));
            Assert.That(concreteTrip.StartDate, Is.EqualTo(_tripContext.TripItems.First().StartDate));
        });
    }

    [Test]
    public void PostNewTrip_AddNewTripToDatabase()
    {
        var dto = new FullTripDTO()
        {
            Name = "Newly added Trip",
            Country = "Austria",
            Description = "This is new Trip only for testers!",
            NumberOfSeats = 100,
            StartDate = DateTime.MinValue
        };

        _tripsRepository.AddNewTrip(dto);

        var newlyAddedTrip = _tripContext.TripItems.Last();

        Assert.Multiple(() =>
        {
            Assert.That(newlyAddedTrip.Name, Is.EqualTo(dto.Name));
            Assert.That(newlyAddedTrip.Country, Is.EqualTo(dto.Country));
            Assert.That(newlyAddedTrip.Description, Is.EqualTo(dto.Description));
            Assert.That(newlyAddedTrip.NumberOfSeats, Is.EqualTo(dto.NumberOfSeats));
            Assert.That(newlyAddedTrip.StartDate, Is.EqualTo(dto.StartDate));
        });

    }

    [Test]
    public void EditTrip_ChangesSpecificTrip()
    {
        var name = "Test3";

        var dto = new FullTripDTO()
        {
            Name = "Changed Name",
            Country = "Changed Country",
            Description = "Changed Desc",
            NumberOfSeats = 50,
            StartDate = DateTime.MaxValue
        };

        _tripsRepository.EditTrip(name, dto);

        var changedTrip = _tripContext.TripItems.First(x => x.Name == dto.Name);

        Assert.Multiple(() =>
        {
            Assert.That(changedTrip.Name, Is.EqualTo(dto.Name));
            Assert.That(changedTrip.Country, Is.EqualTo(dto.Country));
            Assert.That(changedTrip.Description, Is.EqualTo(dto.Description));
            Assert.That(changedTrip.NumberOfSeats, Is.EqualTo(dto.NumberOfSeats));
            Assert.That(changedTrip.StartDate, Is.EqualTo(dto.StartDate));
        });

    }

    [Test]
    public void AddEmailToTheTrip_AddsNewEmail()
    {
        var email = "test@test.com";

        var originalTrip = _tripContext.TripItems.First();
        _tripsRepository.AddEmailToTheTrip(email, originalTrip.Name);
        var changedTrip = _tripContext.TripItems.First();
        Assert.Multiple(() =>
        {
            Assert.That(changedTrip.Name, Is.EqualTo(originalTrip.Name));
            Assert.That(changedTrip.Country, Is.EqualTo(originalTrip.Country));
            Assert.That(changedTrip.Description, Is.EqualTo(originalTrip.Description));
            Assert.That(changedTrip.NumberOfSeats, Is.EqualTo(originalTrip.NumberOfSeats));
            Assert.That(changedTrip.StartDate, Is.EqualTo(originalTrip.StartDate));
            Assert.That(changedTrip.RegisteredEmails.Count(), Is.EqualTo(1));
            Assert.That(changedTrip.RegisteredEmails.First(), Is.EqualTo(email));
        });
    }

    [Test]
    public void RemoveTrip_RemovesSpecificTrip()
    {
        var tripNameToRemove = "Test3";
        _tripsRepository.RemoveTrip(tripNameToRemove);

        Assert.Multiple(() =>
        {
            Assert.That(_tripContext.TripItems.Count(), Is.EqualTo(4));
            Assert.That(_tripContext.TripItems.FirstOrDefault(item => item.Name.Equals(tripNameToRemove)), Is.Null);
        });
    }

    private void ArrangeTripContext()
    {
        var options = new DbContextOptionsBuilder<TripContext>().UseInMemoryDatabase("TestDatabase").Options;

        _tripContext = new TripContext(options);

        _tripContext.TripItems.Add(new TripModel() { Id = 1, Country = "Poland", Description = "", Name = "Test1", NumberOfSeats = 20, StartDate = DateTime.MinValue });
        _tripContext.TripItems.Add(new TripModel() { Id = 2, Country = "Poland", Description = "", Name = "Test2", NumberOfSeats = 50, StartDate = DateTime.MinValue });
        _tripContext.TripItems.Add(new TripModel() { Id = 3, Country = "Jamaica", Description = "", Name = "Test3", NumberOfSeats = 5, StartDate = DateTime.MinValue });
        _tripContext.TripItems.Add(new TripModel() { Id = 4, Country = "Iceland", Description = "", Name = "Test4", NumberOfSeats = 12, StartDate = DateTime.MinValue });
        _tripContext.TripItems.Add(new TripModel() { Id = 5, Country = "Norway", Description = "", Name = "Test5", NumberOfSeats = 6, StartDate = DateTime.MinValue });

        _tripContext.SaveChanges();

    }

}
