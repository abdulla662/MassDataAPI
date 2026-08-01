using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using MassDataAPI.Data;
using MassDataAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MassDataAPI.Services;

public class DataSeeder(AppDbContext db, ILogger<DataSeeder> logger)
{
    private static readonly string[] Countries = ["Egypt", "USA", "UK", "Germany", "France", "UAE", "Saudi Arabia", "Canada", "Australia", "India", "Brazil", "Japan"];
    private static readonly string[] Departments = ["Engineering", "Marketing", "Finance", "HR", "Sales", "Operations", "Legal", "Design", "Product", "Data Science"];
    private static readonly string[] FirstNames = ["Ahmed", "Sara", "Mohamed", "Nour", "Ali", "Hana", "Omar", "Aya", "Youssef", "Mariam", "Khaled", "Dina", "James", "Emma", "Liam", "Olivia", "Noah", "Sophia", "Lucas", "Isabella"];
    private static readonly string[] LastNames = ["Hassan", "Ibrahim", "Ali", "Mahmoud", "Smith", "Johnson", "Williams", "Brown", "Taylor", "Anderson", "Wilson", "Moore", "Thomas", "Jackson", "White"];

    public async Task SeedAsync(int targetCount = 1_000_000)
    {
        var existing = await db.Employees.CountAsync();
        if (existing >= targetCount)
        {
            logger.LogInformation("Database already has {Count:N0} records — skipping seed.", existing);
            return;
        }

        logger.LogInformation("Seeding {Count:N0} employee records…", targetCount);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        await db.Database.ExecuteSqlRawAsync("DELETE FROM Employees");

        var rng = new Random(42);
        const int batchSize = 5_000;
        int seeded = 0;

        while (seeded < targetCount)
        {
            var batch = new List<Employee>(batchSize);
            int take = Math.Min(batchSize, targetCount - seeded);

            for (int i = 0; i < take; i++)
            {
                var firstName = FirstNames[rng.Next(FirstNames.Length)];
                var lastName  = LastNames[rng.Next(LastNames.Length)];
                var country   = Countries[rng.Next(Countries.Length)];
                var dept      = Departments[rng.Next(Departments.Length)];
                var age       = rng.Next(22, 60);
                var salary    = Math.Round(rng.Next(25_000, 200_000) + rng.NextDouble() * 1000, 2);
                var joinYear  = rng.Next(2010, 2025);
                var joinDate  = new DateOnly(joinYear, rng.Next(1, 13), rng.Next(1, 28));
                var gender    = rng.Next(2) == 0 ? "Male" : "Female";

                batch.Add(new Employee
                {
                    Name       = $"{firstName} {lastName}",
                    Email      = $"{firstName.ToLower()}.{lastName.ToLower()}{seeded + i + 1}@example.com",
                    Country    = country,
                    Department = dept,
                    Age        = age,
                    Salary     = (decimal)salary,
                    JoinDate   = joinDate,
                    Gender     = gender,
                });
            }

            await db.Employees.AddRangeAsync(batch);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            seeded += take;
            logger.LogInformation("  Seeded {Seeded:N0} / {Total:N0}", seeded, targetCount);
        }

        sw.Stop();
        logger.LogInformation("Seeding complete — {Count:N0} records in {Elapsed:F1}s", seeded, sw.Elapsed.TotalSeconds);
    }
}
