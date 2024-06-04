using System.ComponentModel.DataAnnotations;
namespace TaskApi.Models;

public class TripModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public int NumberOfSeats { get; set; }

    public List<string> RegisteredEmails { get; set; } = [];
}
