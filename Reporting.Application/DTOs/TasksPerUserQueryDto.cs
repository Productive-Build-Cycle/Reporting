namespace Reporting.Application.DTOs;

public record TasksPerUserQueryDto(
    string? Status,
    DateTime? From,
    DateTime? To
);