CREATE OR ALTER PROCEDURE dbo.GetCompletedTasksPerWeek
    @StartDate DATETIME,
    @EndDate   DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DATEPART(YEAR, CompletedAt) AS [Year],
        DATEPART(WEEK, CompletedAt) AS [WeekNumber],
        COUNT(*) AS CompletedTasksCount
    FROM Tasks
    WHERE
        CompletedAt IS NOT NULL
        AND CompletedAt >= @StartDate
        AND CompletedAt <= @EndDate
    GROUP BY
        DATEPART(YEAR, CompletedAt),
        DATEPART(WEEK, CompletedAt)
    ORDER BY
        [Year],
        [WeekNumber];
END;
