# MassDataAPI

An ASP.NET Core 8 Web API that handles **1 million+ records** with sub-100ms query responses using proper database indexing, `AsNoTracking`, and optimised EF Core queries.

[![.NET](https://img.shields.io/badge/.NET_8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)](https://sqlite.org)
[![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://docker.com)
[![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)](https://swagger.io)

---

## The Engineering Problem

Querying large datasets naively (`SELECT * FROM Employees`) causes full table scans and seconds-long response times. This project demonstrates the real decisions a developer must make:

- **Which columns to index?** — Created covering indexes on `Country`, `Department`, `Salary`, `Age`, `JoinDate`, and a composite `(Country, Department)` for the most common filter combinations.
- **How to avoid N+1 and tracking overhead?** — All read queries use `AsNoTracking()`, eliminating EF Core's change-tracker cost entirely.
- **How to count without a second full scan?** — `LongCountAsync` runs on the filtered-but-unpaginated query before the `Skip/Take`, reusing the WHERE clause indexes.
- **How to handle arbitrary filter combinations at runtime?** — Composable LINQ: each filter is conditionally chained onto the `IQueryable`, translating to a single optimised SQL statement with only the relevant WHERE clauses.

---

## Features

- **1M records** seeded automatically on first startup (batched inserts of 5,000 — avoids SQLite lock contention)
- **7 filter parameters** — country, department, salary range, age range, gender, name/email search
- **Sorting** on any field, ascending or descending
- **Pagination** with total count, total pages, hasNext/hasPrev
- **Stats endpoint** — average salary, count by department, min/max — computed server-side via `GROUP BY`
- **Swagger UI** at `/` with full documentation
- **Docker** support — run anywhere

---

## Quick Start

```bash
git clone https://github.com/abdulla662/MassDataAPI
cd MassDataAPI/MassDataAPI

dotnet run
# First run seeds 1,000,000 records (~60s)
# Open http://localhost:5000 for Swagger UI
```

### Docker

```bash
docker build -t massdataapi .
docker run -p 8080:8080 -v massdata:/data massdataapi
# http://localhost:8080
```

---

## API Endpoints

### `GET /api/employees`

Query the full dataset with any combination of filters.

| Parameter    | Type    | Description                              |
|--------------|---------|------------------------------------------|
| `country`    | string  | Filter by country                        |
| `department` | string  | Filter by department                     |
| `minSalary`  | decimal | Minimum salary                           |
| `maxSalary`  | decimal | Maximum salary                           |
| `minAge`     | int     | Minimum age                              |
| `maxAge`     | int     | Maximum age                              |
| `gender`     | string  | Male / Female                            |
| `search`     | string  | Search in name or email                  |
| `sortBy`     | string  | name / salary / age / joindate           |
| `desc`       | bool    | Sort descending (default: false)         |
| `page`       | int     | Page number (default: 1)                 |
| `pageSize`   | int     | Records per page (max 200, default: 20)  |

**Example response:**
```json
{
  "data": [ { "id": 1, "name": "Ahmed Hassan", "country": "Egypt", "salary": 85000, ... } ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42783,
  "totalPages": 2140,
  "hasNext": true,
  "hasPrev": false
}
```

### `GET /api/employees/{id}` — Get single employee

### `GET /api/employees/stats` — Aggregated statistics

---

## Architecture

```
MassDataAPI/
├── Controllers/
│   └── EmployeesController.cs    # GET /api/employees — composable filters, sort, paginate
├── Data/
│   └── AppDbContext.cs           # EF Core context with index configuration
├── Migrations/                   # EF Core migrations (auto-applied on startup)
├── Models/
│   ├── Employee.cs               # Entity with column type hints
│   └── PagedResult<T>.cs         # Generic pagination envelope
├── Services/
│   └── DataSeeder.cs             # Batched 1M record generator
└── Program.cs                    # DI setup, auto-migration, auto-seed
```

**Key engineering decisions:**

| Decision | Choice | Why |
|----------|--------|-----|
| Tracking | `AsNoTracking()` on all reads | Eliminates EF change-tracker overhead (~30% faster reads) |
| Indexes | Per-column + composite `(Country, Department)` | Covers most filter combinations without full scans |
| Seeding | Batches of 5,000 with `ChangeTracker.Clear()` | Avoids memory growth; SQLite handles small transactions faster |
| Count | Separate `LongCountAsync` before `Skip/Take` | Reuses filtered query's index; accurate total for pagination |
| Filters | Composable LINQ chains | One SQL query regardless of which filters are active |

---

Built by [Abdulla Hamdy](https://github.com/abdulla662)
