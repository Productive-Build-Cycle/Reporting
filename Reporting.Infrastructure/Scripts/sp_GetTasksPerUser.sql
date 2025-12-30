-- Stored Procedure for Tasks Per User Report
-- This stored procedure retrieves task counts per user with optional filters.
-- Optimized for performance with query plan caching.

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetTasksPerUser]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_GetTasksPerUser]
GO

CREATE PROCEDURE [dbo].[sp_GetTasksPerUser]
    @Status NVARCHAR(50) = NULL,
    @From DATETIME2 = NULL,
    @To DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.Id AS UserId,
        u.Name AS UserName,
        COUNT(t.Id) AS TasksCount
    FROM Tasks t
    INNER JOIN Users u ON t.UserId = u.Id
    WHERE 
        (@Status IS NULL OR t.Status = @Status)
        AND (@From IS NULL OR t.CreatedAt >= @From)
        AND (@To IS NULL OR t.CreatedAt <= @To)
    GROUP BY u.Id, u.Name
    ORDER BY TasksCount DESC;
END
GO

