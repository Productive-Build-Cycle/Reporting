namespace Reporting.Application.DTOs;

public record TasksPerUserQuery(
    string? Status,
    DateTime? From,
    DateTime? To,
    bool UseRawSql = false
);
