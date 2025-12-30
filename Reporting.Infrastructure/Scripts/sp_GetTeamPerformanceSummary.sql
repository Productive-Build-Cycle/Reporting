-- Stored Procedure for Team Performance Summary
-- This stored procedure retrieves team performance metrics including total tasks,
-- completed tasks, and completion rate with optional date and team filters.

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetTeamPerformanceSummary]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_GetTeamPerformanceSummary]
GO

CREATE PROCEDURE [dbo].[sp_GetTeamPerformanceSummary]
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @TeamId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        t.Id AS TeamId,
        t.Name AS TeamName,
        COUNT(task.Id) AS TotalTasks,
        SUM(CASE WHEN task.Status = 'Completed' OR task.CompletedAt IS NOT NULL THEN 1 ELSE 0 END) AS CompletedTasks,
        CASE 
            WHEN COUNT(task.Id) > 0 
            THEN CAST(SUM(CASE WHEN task.Status = 'Completed' OR task.CompletedAt IS NOT NULL THEN 1 ELSE 0 END) * 100.0 / COUNT(task.Id) AS DECIMAL(18,2))
            ELSE 0 
        END AS CompletionRate
    FROM Teams t
    INNER JOIN Tasks task ON t.Id = task.TeamId
    WHERE 
        (@StartDate IS NULL OR task.CreatedAt >= @StartDate)
        AND (@EndDate IS NULL OR task.CreatedAt <= @EndDate)
        AND (@TeamId IS NULL OR t.Id = @TeamId)
    GROUP BY t.Id, t.Name
    ORDER BY t.Id;
END
GO

