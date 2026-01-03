# Reporting Service - Project Documentation

---

## 1. Cover Page

**Project Title:** Reporting Service

**Version:** 1.1.0

**Author(s):** Development Team (Productive Build Cycle)

**Organization / Team:** Productive Build Cycle

**Repository:** [https://github.com/Productive-Build-Cycle/Reporting.git](https://github.com/Productive-Build-Cycle/Reporting.git)

**Date:** January 2025

---

## 2. Document Control

### Version History

| Version | Date | Author | Description |
|---------|------|--------|-------------|
| 1.1.0 | January 2025 | Development Team | Updated: Added pagination support, stored procedures, error handling improvements |
| 1.0.0 | January 2025 | Development Team | Initial documentation release |

---

## 3. Table of Contents

1. Cover Page
2. Document Control
3. Table of Contents
4. Contributors
5. Introduction
6. Business Context & Objectives
7. Requirements
8. System Architecture
9. Technology Stack
10. Application Design
11. Database Design
12. API Design
13. Security Considerations
14. Performance & Optimization
15. Error Handling & Logging
16. Testing Strategy
17. Deployment
18. Maintenance & Support
19. Risks & Mitigation
20. Glossary
21. References

---

## 4. Contributors

### Development Team

This project is developed and maintained by the **Productive Build Cycle** team.

| Name | GitHub Profile | Role |
|------|---------------|------|
| Mina Golzari Dalir | [@MinaGolzari](https://github.com/MinaGolzari) | Team Leader / Project Lead |
| Soheil Sadeghii | [@SoheilSadeghii](https://github.com/SoheilSadeghii) | Developer |
| Hosna Hajimohammadi | [@Lodgoer](https://github.com/Lodgoer) | Developer |

### Repository

- **GitHub Repository**: [Productive-Build-Cycle/Reporting](https://github.com/Productive-Build-Cycle/Reporting.git)
- **Organization**: [Productive Build Cycle](https://github.com/Productive-Build-Cycle)
- **Branch**: `feature/project-setup-environment` (current active branch)

---

## 5. Introduction

### Purpose of the Document

This document provides a comprehensive overview of the Reporting Service project, including its architecture, design, implementation details, and operational procedures. It serves as a reference for developers, architects, project managers, and stakeholders.

### Project Overview

The Reporting Service is a RESTful Web API built on .NET 10.0 that provides reporting capabilities for task management data. The service enables organizations to generate and export various reports related to task assignments, completion rates, and team performance metrics.

### Intended Audience

Developers, architects, project managers, and technical stakeholders.

### Scope (In-Scope / Out-of-Scope)

**In-Scope:**
- Task per user reporting with filtering capabilities
- Weekly completed tasks reporting
- Team performance summary reporting
- Excel export functionality for all reports
- RESTful API endpoints
- Database persistence using SQL Server
- Pagination support for large datasets

**Out-of-Scope:**
- User authentication and authorization (currently not implemented)
- Real-time data updates
- Data visualization/UI components
- Email notifications
- Scheduled report generation
- Multi-tenancy support
- Task management (CRUD operations for tasks)
- User/Team/Project management

---

## 6. Business Context & Objectives

### Business Problem

Organizations need a reliable and efficient way to generate reports on task management data to:
- Track task assignments per user
- Analyze weekly completion trends
- Evaluate team performance metrics
- Export data for further analysis in spreadsheet applications

### Project Goals

1. Provide RESTful API endpoints for generating task-related reports
2. Support flexible filtering options (date ranges, status, team)
3. Enable Excel export functionality for all reports
4. Ensure high performance with optimized database queries
5. Maintain clean, maintainable codebase using Clean Architecture principles

### Success Criteria (KPIs / Metrics)

- API response time < 500ms for standard queries
- Support for datasets with 10,000+ tasks
- 99.9% API availability
- Successful Excel export generation for all report types
- Code coverage > 70% (target for future implementation)

### Assumptions & Constraints

**Assumptions:**
- SQL Server database is available and accessible
- Users have basic knowledge of REST APIs
- Excel files are the primary export format
- Data is read-only (no write operations via API)

**Constraints:**
- .NET 10.0 runtime requirement
- SQL Server database dependency
- Single database instance (no replication)
- No authentication/authorization currently implemented
- No caching layer implemented

---

## 7. Requirements

### 6.1 Functional Requirements

**FR-01: Tasks Per User Report**
- System shall provide an API endpoint to retrieve the count of tasks assigned to each user
- System shall support filtering by task status
- System shall support filtering by date range (CreatedAt)
- System shall support pagination
- System shall return user ID, user name, and task count for each user
- System shall order results by task count (descending)

**FR-02: Completed Tasks Per Week Report**
- System shall provide an API endpoint to retrieve completed tasks grouped by week
- System shall filter by date range (CompletedAt)
- System shall support pagination
- System shall return year, week number, and completed task count
- System shall order results by year and week number

**FR-03: Team Performance Summary Report**
- System shall provide an API endpoint to retrieve team performance metrics
- System shall support filtering by date range (CreatedAt)
- System shall support filtering by team ID
- System shall support pagination
- System shall return total tasks, completed tasks, and completion rate for each team
- System shall order results by completion rate (descending), then by total tasks (descending)

**FR-04: Excel Export Functionality**
- System shall provide API endpoints to export all report types to Excel format
- System shall generate properly formatted Excel files with headers, bold styling, and auto-fitted columns
- System shall include all data (no pagination for exports - uses int.MaxValue for page size)
- System shall return files with appropriate MIME type and filename
- Excel files shall use EPPlus library for generation

**FR-05: Data Seeding**
- System shall automatically seed initial data on application startup (if database is empty)

### Non-Functional Requirements

**Performance**
- API endpoints shall respond within 500ms for standard queries
- Excel export shall complete within 2 seconds for datasets up to 10,000 records
- Database queries shall use appropriate indexing

**Scalability**
- System shall handle concurrent requests from multiple clients
- Database queries shall be optimized to prevent N+1 query problems
- System shall support pagination to handle large datasets efficiently

**Security**
- API endpoints shall use HTTPS in production (currently development only)
- System shall sanitize input parameters to prevent SQL injection
- System shall implement CORS policies (currently allowing all origins)

**Availability**
- System target availability: 99.9%
- Database connection failures shall be handled gracefully

**Maintainability**
- Code shall follow Clean Architecture principles
- Code shall be well-documented with XML comments
- Code shall follow SOLID principles
- Dependencies shall be injected using dependency injection

---

## 8. System Architecture

### High-Level Architecture Overview

The Reporting Service follows Clean Architecture principles, organizing code into distinct layers with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│         Reporting.Api (Presentation)    │
│         - Controllers                   │
│         - API Configuration             │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│     Reporting.Application (Business)    │
│         - Services                      │
│         - DTOs                          │
│         - Interfaces                    │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│   Reporting.Infrastructure (Data)       │
│         - Repositories                  │
│         - DbContext                     │
│         - Migrations                    │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      Reporting.Domain (Entities)        │
│         - Entities                      │
└─────────────────────────────────────────┘
```

### Architectural Style

**Clean Architecture (Onion Architecture)**
- Separation of concerns across layers
- Dependency inversion principle (dependencies point inward)
- Domain layer is independent of infrastructure
- Infrastructure depends on domain and application layers
- Application layer depends only on domain layer

### Key Design Decisions

1. **Clean Architecture**: Chosen for maintainability, testability, and separation of concerns
2. **Entity Framework Core**: Selected for ORM capabilities and migration support
3. **Repository Pattern**: Used to abstract data access logic
4. **DTO Pattern**: Used to transfer data between layers and API
5. **Dependency Injection**: Used for loose coupling and testability
6. **Stored Procedures**: Used for weekly completed tasks report for better performance and maintainability
7. **EPPlus Library**: Selected for Excel export functionality with formatted headers and auto-fitted columns
8. **Pagination**: Implemented to handle large datasets efficiently across all report endpoints
9. **ExcelExporter Service**: Centralized Excel generation service (located in Reporting.Application/Export) registered as scoped dependency for export endpoints

---

## 9. Technology Stack

### Backend

- **.NET 10.0**: Runtime and framework
- **ASP.NET Core Web API**: Web framework
- **C#**: Programming language
- **Entity Framework Core 10.0.1**: ORM for database access


### Database

- **SQL Server**: Relational database management system
- **Entity Framework Core Migrations**: Database versioning and migration management

### Messaging / Integration

- **REST API**: HTTP/HTTPS communication protocol
- **JSON**: Data exchange format
- **Swagger/OpenAPI**: API documentation

### DevOps / CI-CD

- **Entity Framework Migrations**: Database deployment
- **.NET CLI**: Build and deployment tooling
- Currently: Manual deployment (CI/CD pipeline not implemented)

### Third-Party Services

- **EPPlus 8.4.0**: Excel file generation library
- **Swashbuckle.AspNetCore 10.1.0**: Swagger/OpenAPI documentation generation

---

## 10. Application Design

### Module Breakdown

| Module Name | Responsibility | Dependencies |
|-------------|---------------|--------------|
| **Reporting.Domain** | Domain entities (Team, User, Project, TaskEntity) | None |
| **Reporting.Application** | Business logic, services, DTOs, interfaces, Excel export | Reporting.Domain |
| **Reporting.Infrastructure** | Data access, repositories, database context, migrations, dependency injection | Reporting.Domain, Reporting.Application |
| **Reporting.Api** | API controllers, HTTP handling, configuration | Reporting.Application, Reporting.Infrastructure |
| **Reporting.Benchmarks** | Performance benchmarking | Reporting.Application, Reporting.Infrastructure |

### Data Flow

#### Request/Response Flow

```
Client Request
    ↓
API Controller (ReportsController)
    ↓
Application Service (ReportService)
    ↓
Repository (ReportRepository)
    ↓
Database (SQL Server via EF Core / Stored Procedures)
    ↓
Repository (results)
    ↓
Application Service (processing / pagination)
    ↓
API Controller (response formatting)
    ↓
ExcelExporter (for export endpoints)
    ↓
Client Response (JSON or Excel file)
```


---

## 11. Database Design

### ER Diagram

```
┌──────────┐         ┌──────────┐         ┌──────────┐
│  Teams   │1───────<│  Users   │1       │ Projects │
│          │         │          │         │          │
│ - Id (PK)│         │ - Id (PK)│         │ - Id (PK)│
│ - Name   │         │ - Name   │         │ - Name   │
└──────────┘         │ - TeamId │         └────┬─────┘
                     └────┬─────┘              │
                          │                    │
                          │                    │
                     ┌────▼────────────────────▼────┐
                     │         Tasks                │
                     │                              │
                     │ - Id (PK)                    │
                     │ - Title                      │
                     │ - UserId (FK)                │
                     │ - TeamId (FK)                │
                     │ - ProjectId (FK)             │
                     │ - Status                     │
                     │ - CreatedAt                  │
                     │ - CompletedAt (nullable)     │
                     └──────────────────────────────┘
```

### Tables / Collections

#### Teams
- **Id** (int, PK, Identity)
- **Name** (string, required)

**Relationships:**
- One-to-Many with Users (Cascade Delete)

#### Users
- **Id** (int, PK, Identity)
- **Name** (string, required)
- **TeamId** (int, FK, required)

**Relationships:**
- Many-to-One with Teams (Cascade Delete)
- One-to-Many with Tasks (Restrict Delete)

#### Projects
- **Id** (int, PK, Identity)
- **Name** (string, required)

**Relationships:**
- One-to-Many with Tasks (Cascade Delete)

#### Tasks
- **Id** (int, PK, Identity)
- **Title** (string, required)
- **UserId** (int, FK, required)
- **TeamId** (int, FK, required)
- **ProjectId** (int, FK, required)
- **Status** (string, required, indexed)
- **CreatedAt** (DateTime, required, indexed)
- **CompletedAt** (DateTime?, nullable, indexed)

**Relationships:**
- Many-to-One with Users (Restrict Delete)
- Many-to-One with Teams (Restrict Delete)
- Many-to-One with Projects (Cascade Delete)

### Indexing Strategy

Indexes are created on the Tasks table for query performance:

1. **UserId** - For filtering tasks by user
2. **TeamId** - For filtering tasks by team
3. **Status** - For filtering tasks by status
4. **CreatedAt** - For date range filtering
5. **CompletedAt** - For completed tasks queries

### Migration Strategy

- Entity Framework Core Migrations are used for database versioning
- Migrations are applied automatically on application startup (Development)
- Migration files are located in `Reporting.Infrastructure/Migrations/`
- Stored procedures are located in `Reporting.Infrastructure/Queries/`
- Current migrations:
  - `20251228141238_InitialCreate.cs` - Initial database schema
  - `20260102091942_UpdateDatabase.cs` - Database updates
  - `20260102142420_UpdatePendingChanges.cs` - Additional changes
- Current stored procedures:
  - `GetCompletedTasksPerWeek` - Retrieves weekly completed tasks grouped by year and week number

---

## 12. API Design

### API Versioning Strategy

- Current version: **v1**
- Version specified in route: `/api/v1/reports`
- Future versions will use route-based versioning (e.g., `/api/v2/reports`)

### Authentication & Authorization

- **Current Status**: Not implemented
- **Future**: JWT Bearer token authentication recommended
- **Current**: CORS allows all origins (development configuration)

### Endpoint List

#### GET /api/v1/reports/tasks-per-user

Returns the number of tasks assigned per user.

**Query Parameters:**
- `Status` (string, optional): Filter by task status
- `From` (DateTime?, optional): Filter tasks created from this date
- `To` (DateTime?, optional): Filter tasks created up to this date
- `PageNumber` (int, default: 1): Page number for pagination
- `PageSize` (int, default: 10): Number of items per page

**Response:** `List<TasksPerUserReportDto>`
```json
[
  {
    "userId": 1,
    "userName": "Alice",
    "tasksCount": 5
  }
]
```

**Status Codes:**
- `200 OK`: Success with data
- `204 No Content`: No tasks found
- `500 Internal Server Error`: Server error

---

#### GET /api/v1/reports/completed-tasks-per-week

Returns a weekly report of completed tasks.

**Query Parameters:**
- `StartDate` (DateTime, required): Start date for filtering
- `EndDate` (DateTime, required): End date for filtering
- `PageNumber` (int, default: 1): Page number for pagination
- `PageSize` (int, default: 10): Number of items per page

**Response:** `PagedResultDto<CompletedTasksPerWeekReportDto>`
```json
{
  "items": [
    {
      "year": 2025,
      "weekNumber": 1,
      "completedTasksCount": 10
    }
  ],
  "totalCount": 52,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 6
}
```

**Status Codes:**
- `200 OK`: Success
- `500 Internal Server Error`: Server error

---

#### GET /api/v1/reports/team-performance-summary

Returns team performance summary with metrics.

**Query Parameters:**
- `StartDate` (DateTime?, optional): Filter tasks created from this date
- `EndDate` (DateTime?, optional): Filter tasks created up to this date
- `TeamId` (int?, optional): Filter by specific team ID
- `PageNumber` (int, default: 1): Page number for pagination
- `PageSize` (int, default: 10): Number of items per page

**Response:** `PagedResultDto<TeamPerformanceSummaryResponseDto>`
```json
{
  "items": [
    {
      "teamId": 1,
      "teamName": "Team Alpha",
      "totalTasks": 20,
      "completedTasks": 15,
      "completionRate": 75.0
    }
  ],
  "totalCount": 2,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

**Status Codes:**
- `200 OK`: Success
- `500 Internal Server Error`: Server error

---

#### GET /api/v1/reports/tasks-per-user/export

Exports tasks per user report to Excel format.

**Query Parameters:** Same as `/tasks-per-user` endpoint (except pagination is ignored)

**Response:** Excel file (application/vnd.openxmlformats-officedocument.spreadsheetml.sheet)

**Status Codes:**
- `200 OK`: Excel file returned
- `204 No Content`: No data to export
- `500 Internal Server Error`: Server error

---

#### GET /api/v1/reports/completed-tasks-per-week/export

Exports completed tasks per week report to Excel format.

**Query Parameters:** Same as `/completed-tasks-per-week` endpoint (pagination is ignored - all data is exported)

**Response:** Excel file (application/vnd.openxmlformats-officedocument.spreadsheetml.sheet)

**Status Codes:**
- `200 OK`: Excel file returned
- `204 No Content`: No data to export
- `500 Internal Server Error`: Server error

**Note:** This endpoint currently lacks try-catch error handling (unlike other export endpoints).

---

#### GET /api/v1/reports/team-performance-summary/export

Exports team performance summary report to Excel format.

**Query Parameters:** Same as `/team-performance-summary` endpoint (except pagination is ignored)

**Response:** Excel file (application/vnd.openxmlformats-officedocument.spreadsheetml.sheet)

**Status Codes:**
- `200 OK`: Excel file returned
- `204 No Content`: No data to export
- `500 Internal Server Error`: Server error

---

## 13. Security Considerations

### Authentication & Authorization

- **Current**: Not implemented
- **Recommendation**: JWT Bearer token authentication with role-based access control (RBAC)

### Data Protection

- **Input Validation**: Query parameters are validated by ASP.NET Core model binding
- **SQL Injection Prevention**: Entity Framework Core parameterized queries prevent SQL injection
- **Stored Procedures**: Uses parameterized stored procedures with SqlParameter for secure database access
- **HTTPS**: Recommended for production (currently development configuration)
- **Connection Strings**: Stored in appsettings.json (should use secure configuration in production)

### Common Threats & Mitigations

| Threat | Current Status | Mitigation |
|--------|---------------|------------|
| SQL Injection | Protected | EF Core parameterized queries |
| XSS (Cross-Site Scripting) | N/A | API-only service (no HTML rendering) |
| CSRF (Cross-Site Request Forgery) | Not implemented | Implement anti-forgery tokens if needed |
| Unauthorized Access | Not implemented | Implement authentication/authorization |
| Data Exposure | Protected | Use HTTPS in production |
| DoS/DDoS | Not implemented | Implement rate limiting |
| Information Disclosure | Partial | Remove detailed error messages in production |

---

## 14. Performance & Optimization

### Caching Strategy

- **Current**: No caching implemented
- **Recommendation**: Implement response caching (IMemoryCache or Redis) for frequently accessed reports

### Query Optimization

- **Indexes**: Created on Tasks table (UserId, TeamId, Status, CreatedAt, CompletedAt)
- **AsNoTracking()**: Used to improve query performance (read-only queries)
- **Pagination**: Implemented to limit data transfer across all report endpoints
- **Stored Procedures**: Used for complex queries (completed tasks per week) for better performance and maintainability
- **Eager Loading**: Used where appropriate (e.g., User.Name in tasks query)

### Performance Benchmark Results (https://github.com/Productive-Build-Cycle/Reporting/tree/perf/benchmark-query)

Performance benchmarks were conducted using BenchmarkDotNet to compare different data access strategies. Tests were run on Windows 11 with Intel Core i7-7500U CPU 2.70GHz, .NET 10.0.1.

#### Tasks Per User Query Benchmark

| Method | Mean | Error | StdDev | Ratio | Allocated | Alloc Ratio |
|--------|------|-------|--------|-------|-----------|-------------|
| **EfCore_Linq** | 33.87 ms | 3.635 ms | 0.563 ms | 1.00 | 165.95 KB | 1.00 |
| **Dapper_RawSql** | 55.38 ms | 29.050 ms | 7.544 ms | 1.64 | 59.29 KB | 0.36 |
| **StoredProcedure** | 43.44 ms | 12.526 ms | 1.938 ms | 1.28 | 55.94 KB | 0.34 |

**Findings:**
- **EfCore_Linq** provides the best performance (fastest execution time) and most consistent results (lowest error/stddev)
- **StoredProcedure** offers a balance: 28% slower but uses 66% less memory
- **Dapper_RawSql** shows highest variability and is 64% slower than EfCore_Linq

#### Team Performance Query Benchmark

| Method | Mean | Error | StdDev | Ratio | Allocated | Alloc Ratio |
|--------|------|-------|--------|-------|-----------|-------------|
| **EfCore_Linq** | 39.13 ms | 6.711 ms | 1.039 ms | 1.00 | 48.47 KB | 1.00 |
| **Dapper_RawSql** | 52.38 ms | 24.665 ms | 3.817 ms | 1.34 | 21.60 KB | 0.45 |
| **StoredProcedure** | 63.50 ms | 24.198 ms | 3.745 ms | 1.62 | 10.79 KB | 0.22 |

**Findings:**
- **EfCore_Linq** is the fastest with best consistency
- **Dapper_RawSql** is 34% slower but uses 55% less memory
- **StoredProcedure** is 62% slower but most memory-efficient (78% less memory)

#### Completed Tasks Per Week Query Benchmark

| Method | Mean | Error | StdDev | Ratio | Allocated | Alloc Ratio |
|--------|------|-------|--------|-------|-----------|-------------|
| **EfCore_Linq** | 13.21 ms | 9.001 ms | 2.338 ms | 1.02 | 44.98 KB | 1.00 |
| **Dapper_RawSql** | 12.53 ms | 4.250 ms | 1.104 ms | 0.97 | 21.12 KB | 0.47 |
| **StoredProcedure** | 11.31 ms | 6.042 ms | 0.935 ms | 0.88 | 20.37 KB | 0.45 |

**Findings:**
- **StoredProcedure** shows the best performance (fastest execution time) for this query
- **Dapper_RawSql** is slightly faster than EfCore_Linq and uses 53% less memory
- All methods show similar performance characteristics for this query type

**Summary & Recommendations:**
- **EfCore_Linq** is currently used for tasks per user and team performance queries, providing the best overall performance and consistency
- **Stored Procedures** are used for completed tasks per week report, providing optimal performance for this specific query type
- For memory-constrained scenarios, stored procedures offer significant memory savings (55-78% reduction)
- The performance differences are generally acceptable for the current workload (< 100ms for all queries)
- Current implementation using EfCore_Linq with AsNoTracking() and stored procedures provides optimal balance of performance and maintainability

### Expected Load

- **Concurrent Users**: 10-50 (estimated)
- **Requests per Second**: 10-100 (estimated)
- **Data Volume**: Up to 100,000 tasks
- **Response Time Target**: < 500ms for standard queries

---

## 15. Error Handling & Logging

### Exception Handling & Logging

- **Current**: Try-catch blocks implemented in most controller endpoints with proper error responses, default ASP.NET Core logging
- **Error Responses**: Endpoints return appropriate HTTP status codes (200, 204, 500) with error details in JSON format
- **Cancellation Tokens**: Implemented in async methods to support request cancellation
- **Exception**: The `/completed-tasks-per-week/export` endpoint currently lacks try-catch error handling
- **Recommendation**: Implement global exception handler middleware and structured logging (Serilog/NLog) for centralized error handling


---

## 16. Testing Strategy

- **Current Status**: Not implemented
- **Recommended**: Unit tests (xUnit/NUnit), integration tests (in-memory DB/TestContainers), performance tests (BenchmarkDotNet)
- **Target Coverage**: > 70%

---

## 17. Deployment

### Deployment

- **Current**: Manual deployment, migrations run automatically on startup (dev)
- **Process**: Build → Run migrations → Deploy → Configure → Start
- **Recommended**: Automated CI/CD pipeline (GitHub Actions/Azure DevOps), environment variables for secrets
- **Configuration**: `appsettings.json` (base), environment variables for production

---

## 18. Maintenance & Support

### Code Ownership

- **Repository**: [GitHub - Productive-Build-Cycle/Reporting](https://github.com/Productive-Build-Cycle/Reporting.git)
- **Team Structure**: Productive Build Cycle development team
- **Code Review**: Recommended process for pull requests
- **Documentation**: Maintained in code (XML comments) and this document

### Future Enhancements

- Authentication/authorization (JWT)
- Comprehensive testing (unit, integration, performance)
- Global exception handling and structured logging
- Health check endpoints and monitoring
- Response caching
- Additional report types
- Scheduled report generation

---

## 19. Risks & Mitigation

### Technical Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Database performance degradation | High | Medium | Implement query optimization, caching, indexing strategy |
| SQL Server unavailability | High | Low | Implement connection retry logic, database failover |
| Memory issues with large datasets | Medium | Medium | Implement pagination, streaming for exports |
| Excel export performance issues | Medium | Low | Optimize Excel generation, consider alternative formats |
| Security vulnerabilities | High | Medium | Implement authentication, input validation, security audits |
| API breaking changes | Medium | Medium | Implement API versioning strategy |
| Missing test coverage | Medium | High | Implement comprehensive testing strategy |

### Business Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Increased data volume | Medium | High | Optimize queries, implement caching, consider archiving |
| New reporting requirements | Medium | High | Design extensible architecture, maintain clean codebase |
| Integration requirements | Medium | Medium | Design API with extensibility in mind |
| Performance SLA violations | High | Low | Implement monitoring, performance testing, optimization |


---

## 20. Glossary

| Term | Definition |
|------|------------|
| **Clean Architecture** | Architectural pattern with layer separation and dependency inversion |
| **DTO** | Data Transfer Object - object for data transfer between layers |
| **EF Core** | Entity Framework Core - Microsoft's ORM for .NET |
| **Repository Pattern** | Design pattern that abstracts data access logic |
| **ExcelExporter** | Service class for generating Excel files from report data using EPPlus library |
| **PagedResultDto** | Generic DTO for paginated responses containing Items, TotalCount, PageNumber, PageSize, and TotalPages |

---

## 21. References

### Project Repository

- **GitHub Repository**: [Productive-Build-Cycle/Reporting](https://github.com/Productive-Build-Cycle/Reporting.git)
- **Organization**: [Productive Build Cycle](https://github.com/Productive-Build-Cycle)

### Contributor Profiles

- [Soheil Sadeghii](https://github.com/SoheilSadeghii) - Team Leader
- [Hosna Hajimohammadi (Lodgoer)](https://github.com/Lodgoer) - Developer
- [Mina Golzari Dalir](https://github.com/MinaGolzari) - Developer

### External Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)

---

**Document End**

