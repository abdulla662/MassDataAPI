using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassDataAPI.Data;
using MassDataAPI.Models;

namespace MassDataAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeesController(AppDbContext db) : ControllerBase
{
    /// <summary>Query 1M+ employee records with filtering, sorting, and pagination.</summary>
    /// <param name="country">Filter by country name</param>
    /// <param name="department">Filter by department name</param>
    /// <param name="minSalary">Minimum salary</param>
    /// <param name="maxSalary">Maximum salary</param>
    /// <param name="minAge">Minimum age</param>
    /// <param name="maxAge">Maximum age</param>
    /// <param name="gender">Filter by gender (Male/Female)</param>
    /// <param name="search">Search in name or email (case-insensitive)</param>
    /// <param name="sortBy">Sort field: name, salary, age, joindate (default: id)</param>
    /// <param name="desc">Sort descending</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Records per page (max: 200, default: 20)</param>
    [HttpGet]
    public async Task<ActionResult<PagedResult<Employee>>> GetEmployees(
        [FromQuery] string? country     = null,
        [FromQuery] string? department  = null,
        [FromQuery] decimal? minSalary  = null,
        [FromQuery] decimal? maxSalary  = null,
        [FromQuery] int? minAge         = null,
        [FromQuery] int? maxAge         = null,
        [FromQuery] string? gender      = null,
        [FromQuery] string? search      = null,
        [FromQuery] string? sortBy      = null,
        [FromQuery] bool desc           = false,
        [FromQuery] int page            = 1,
        [FromQuery] int pageSize        = 20,
        CancellationToken ct            = default)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = db.Employees.AsNoTracking();

        // ── Filters ───────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(country))
            query = query.Where(e => e.Country == country);

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(e => e.Department == department);

        if (minSalary.HasValue)
            query = query.Where(e => e.Salary >= minSalary.Value);

        if (maxSalary.HasValue)
            query = query.Where(e => e.Salary <= maxSalary.Value);

        if (minAge.HasValue)
            query = query.Where(e => e.Age >= minAge.Value);

        if (maxAge.HasValue)
            query = query.Where(e => e.Age <= maxAge.Value);

        if (!string.IsNullOrWhiteSpace(gender))
            query = query.Where(e => e.Gender == gender);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e =>
                EF.Functions.Like(e.Name, $"%{search}%") ||
                EF.Functions.Like(e.Email, $"%{search}%"));

        // ── Count (on filtered set) ───────────────────────────────────────────
        var total = await query.LongCountAsync(ct);

        // ── Sort ──────────────────────────────────────────────────────────────
        query = sortBy?.ToLowerInvariant() switch
        {
            "name"     => desc ? query.OrderByDescending(e => e.Name)     : query.OrderBy(e => e.Name),
            "salary"   => desc ? query.OrderByDescending(e => e.Salary)   : query.OrderBy(e => e.Salary),
            "age"      => desc ? query.OrderByDescending(e => e.Age)      : query.OrderBy(e => e.Age),
            "joindate" => desc ? query.OrderByDescending(e => e.JoinDate) : query.OrderBy(e => e.JoinDate),
            _          => desc ? query.OrderByDescending(e => e.Id)       : query.OrderBy(e => e.Id),
        };

        // ── Paginate ──────────────────────────────────────────────────────────
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(new PagedResult<Employee>
        {
            Data       = data,
            Page       = page,
            PageSize   = pageSize,
            TotalCount = total,
        });
    }

    /// <summary>Get a single employee by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Employee>> GetById(int id, CancellationToken ct)
    {
        var emp = await db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        return emp is null ? NotFound() : Ok(emp);
    }

    /// <summary>Aggregated stats across all employees (or filtered subset).</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(
        [FromQuery] string? country    = null,
        [FromQuery] string? department = null,
        CancellationToken ct           = default)
    {
        var query = db.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(country))
            query = query.Where(e => e.Country == country);
        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(e => e.Department == department);

        var stats = await query.GroupBy(_ => 1).Select(g => new
        {
            TotalEmployees  = g.Count(),
            AvgSalary       = Math.Round((double)g.Average(e => e.Salary), 2),
            MinSalary       = g.Min(e => e.Salary),
            MaxSalary       = g.Max(e => e.Salary),
            AvgAge          = Math.Round(g.Average(e => (double)e.Age), 1),
        }).FirstOrDefaultAsync(ct);

        var byDept = await db.Employees.AsNoTracking()
            .GroupBy(e => e.Department)
            .Select(g => new { Department = g.Key, Count = g.Count(), AvgSalary = Math.Round((double)g.Average(e => e.Salary), 2) })
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);

        return Ok(new { summary = stats, byDepartment = byDept });
    }
}
