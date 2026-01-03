# Reporting Service

A RESTful API built with **.NET 10.0** providing task management reporting capabilities such as tasks per user, completed tasks per week, and team performance metrics, with support for **Excel export**, **pagination**, and **stored procedures**.

---

## 📦 Overview

- **Version:** 1.1.0  
- **Author(s):** Productive Build Cycle Team  
- **Repository:** [Productive-Build-Cycle/Reporting](https://github.com/Productive-Build-Cycle/Reporting.git)  
- **Date:** January 2026  

---

## 🚀 Features

- Tasks per user report with filters (status, date range)
- Weekly completed tasks report
- Team performance summary
- Excel export for all reports (using EPPlus)
- Pagination for large datasets
- Stored procedures for optimized queries
- Clean Architecture implementation

---

## 🧩 Architecture

```plaintext
Reporting.Api (Controllers)
↓
Reporting.Application (Services, DTOs, Interfaces)
↓
Reporting.Infrastructure (Repositories, DbContext, Migrations)
↓
Reporting.Domain (Entities)
```

- **Clean Architecture** pattern (domain-centric)
- **Entity Framework Core 10.0.1** ORM
- **Dependency Injection** and **Repository Pattern**

---

## 🛠️ Technology Stack

| Layer | Technology |
|-------|-------------|
| Backend | .NET 10.0, ASP.NET Core |
| Database | SQL Server, EF Core Migrations |
| Export | EPPlus 8.4.0 |
| Documentation | Swagger / OpenAPI |
| Tools | .NET CLI, BenchmarkDotNet |

---

## 🔗 API Endpoints

| Endpoint | Description |
|-----------|--------------|
| **GET /api/v1/reports/tasks-per-user** | Tasks per user summary |
| **GET /api/v1/reports/completed-tasks-per-week** | Weekly completed tasks |
| **GET /api/v1/reports/team-performance-summary** | Team performance metrics |
| **GET /api/v1/reports/.../export** | Excel export version of each report |

All endpoints support pagination and filtering by date/status/team.

---

## 📊 Performance

| Query Type | Fastest Method | Avg. Time (ms) | Notes |
|-------------|----------------|----------------|-------|
| Tasks per user | EF Core LINQ | 33.8 | Best speed & consistency |
| Completed tasks/week | Stored Procedure | 11.3 | Best memory usage |
| Team performance | EF Core LINQ | 39.1 | Fastest overall |

Optimized with:
- Indexed columns (`UserId`, `TeamId`, `Status`, `CreatedAt`, `CompletedAt`)
- `AsNoTracking()` for read-only queries
- Stored procedures for heavy reports

---

## 🧪 Testing & Deployment

- **Planned:** Unit & integration tests (xUnit/NUnit), CI/CD pipeline  
- **Current:** Manual deployment with auto migrations on startup  
- **Target coverage:** 70%+  

---

## 🔐 Security & Reliability

- HTTPS enforced in production  
- Input validation & SQL injection prevention via EF Core  
- CORS enabled (development)  
- Recommended: JWT authentication, rate limiting, global exception handler  

---

## 🧱 Future Enhancements

- JWT authentication & role-based access  
- Automated CI/CD pipeline  
- Response caching (Redis/IMemoryCache)  
- Global exception middleware (Serilog/NLog)  
- Health checks & monitoring  
- Scheduled report generation  

---

## 👥 Contributors

| Name | GitHub | Role |
|------|--------|------|
| Mina Golzari Dalir | [@MinaGolzari](https://github.com/MinaGolzari) | Project Lead |
| Hosna Hajimohammadi | [@Lodgoer](https://github.com/Lodgoer) | Developer |
| Soheil Sadeghii | [@SoheilSadeghii](https://github.com/SoheilSadeghii) | Developer |

---

## 📚 References

- [.NET Docs](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core Docs](https://docs.microsoft.com/ef/core/)
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core/)
- [Productive Build Cycle Org](https://github.com/Productive-Build-Cycle)

---

### 📘 For More Details

For complete architecture explanation, database schema, and API design, please refer to the full project documentation:  
[**PROJECT_DOCUMENTATION.md**](https://github.com/Productive-Build-Cycle/Reporting/blob/feature/project-setup-environment/docs/PROJECT_DOCUMENTATION.md)

---


**© 2026 Productive Build Cycle – Reporting Service**
