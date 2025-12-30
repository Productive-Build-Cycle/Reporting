-- Stored Procedure for Completed Tasks Per Week Report
-- This stored procedure retrieves weekly breakdown of completed tasks within a date range.
-- Uses SQL Server DATEPART function for efficient week calculation.

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetCompletedTasksPerWeek]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_GetCompletedTasksPerWeek]
GO

CREATE PROCEDURE [dbo].[sp_GetCompletedTasksPerWeek]
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DATEPART(YEAR, CompletedAt) AS [Year],
        DATEPART(WEEK, CompletedAt) AS [WeekNumber],
        COUNT(*) AS [CompletedTasksCount]
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
END
GO

