using Microsoft.EntityFrameworkCore;
using MassDataAPI.Models;

namespace MassDataAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(e =>
        {
            // Covering indexes for the most common filter combinations
            e.HasIndex(x => x.Country);
            e.HasIndex(x => x.Department);
            e.HasIndex(x => x.Salary);
            e.HasIndex(x => x.Age);
            e.HasIndex(x => x.JoinDate);
            e.HasIndex(x => new { x.Country, x.Department });
        });
    }
}
