namespace Reporting.Application.DTOs;

public record TasksPerUserQueryDto(
    string? Status,
    DateTime? From,
    DateTime? To,
    int PageNumber = 1,
    int PageSize = 10
);