IF OBJECT_ID(N'dbo.sp_UpdateGoalsForWorkout', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_UpdateGoalsForWorkout;
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Goals_UserId_Status_Start_End_Perf'
      AND object_id = OBJECT_ID(N'dbo.Goals')
)
BEGIN
    DROP INDEX IX_Goals_UserId_Status_Start_End_Perf ON dbo.Goals;
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Workouts_UserId_Date_Perf'
      AND object_id = OBJECT_ID(N'dbo.Workouts')
)
BEGIN
    DROP INDEX IX_Workouts_UserId_Date_Perf ON dbo.Workouts;
END;
GO
