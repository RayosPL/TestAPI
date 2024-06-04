using System.ComponentModel.DataAnnotations;
namespace TaskApi.DTOs;


public class SimplifyTripDTO
{
    public string Name { get; set; } = string.Empty;    
    public string Country {get; set; } = string.Empty;
    public DateTime StartDate {get; set; }
}

public class FullTripDTO
{    
    [Required, StringLength(50)]
    public string Name { get; set; }
    [Required, StringLength(20)]
    public string Country { get; set; }
    public string Description { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required, Range(1, 100)]
    public int NumberOfSeats { get; set; }
    
}