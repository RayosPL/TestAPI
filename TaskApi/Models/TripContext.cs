using Microsoft.EntityFrameworkCore;

namespace TaskApi.Models;

public class TripContext : DbContext
{
    public TripContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<TripModel> TripItems {get; set;} 
}