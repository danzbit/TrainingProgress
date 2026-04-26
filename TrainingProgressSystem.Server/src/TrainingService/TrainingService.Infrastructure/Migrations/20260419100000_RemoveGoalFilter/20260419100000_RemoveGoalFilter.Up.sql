DECLARE @ConstraintName NVARCHAR(256) = (
    SELECT dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'Goals' AND c.name = 'Filter'
);
IF @ConstraintName IS NOT NULL
    EXEC('ALTER TABLE dbo.Goals DROP CONSTRAINT [' + @ConstraintName + ']');

ALTER TABLE dbo.Goals DROP COLUMN [Filter];
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateGoalsForWorkout
    @UserId UNIQUEIDENTIFIER,
    @WorkoutId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @WorkoutDate DATE;

    SELECT @WorkoutDate = CAST(w.[Date] AS DATE)
    FROM dbo.Workouts w
    WHERE w.Id = @WorkoutId
      AND w.UserId = @UserId;

    IF @WorkoutDate IS NULL
    BEGIN
        RETURN;
    END;

    DECLARE @ComputedGoals TABLE
    (
        GoalId UNIQUEIDENTIFIER PRIMARY KEY,
        TargetValue INT NOT NULL,
        CurrentValue INT NOT NULL
    );

    ;WITH ActiveGoals AS
    (
        SELECT
            g.Id AS GoalId,
            g.MetricType,
            g.TargetValue,
            g.StartDate,
            g.EndDate,
            CASE g.PeriodType
                WHEN 0 THEN DATEADD(DAY, -((DATEDIFF(DAY, '19000101', @WorkoutDate)) % 7), @WorkoutDate)
                WHEN 1 THEN DATEFROMPARTS(YEAR(@WorkoutDate), MONTH(@WorkoutDate), 1)
                WHEN 2 THEN g.StartDate
                WHEN 3 THEN DATEADD(DAY, -6, @WorkoutDate)
                ELSE g.StartDate
            END AS RawPeriodStart,
            CASE g.PeriodType
                WHEN 0 THEN DATEADD(DAY, 6 - ((DATEDIFF(DAY, '19000101', @WorkoutDate)) % 7), @WorkoutDate)
                WHEN 1 THEN EOMONTH(@WorkoutDate)
                WHEN 2 THEN ISNULL(g.EndDate, @WorkoutDate)
                WHEN 3 THEN @WorkoutDate
                ELSE ISNULL(g.EndDate, @WorkoutDate)
            END AS RawPeriodEnd
        FROM dbo.Goals g
        WHERE g.UserId = @UserId
          AND g.[Status] = 0
          AND g.StartDate <= @WorkoutDate
          AND (g.EndDate IS NULL OR g.EndDate >= @WorkoutDate)
          AND g.MetricType IN (0, 1, 4, 5, 6, 7)
    ),
    GoalsWithPeriod AS
    (
        SELECT
            ag.GoalId,
            ag.MetricType,
            ag.TargetValue,
            CASE WHEN ag.RawPeriodStart < ag.StartDate THEN ag.StartDate ELSE ag.RawPeriodStart END AS PeriodStart,
            CASE WHEN ag.EndDate IS NOT NULL AND ag.RawPeriodEnd > ag.EndDate THEN ag.EndDate ELSE ag.RawPeriodEnd END AS PeriodEnd
        FROM ActiveGoals ag
    ),
    GeneralMetrics AS
    (
        SELECT
            g.GoalId,
            ISNULL(CASE g.MetricType
                WHEN 0 THEN CAST(COUNT_BIG(w.Id) AS INT)
                WHEN 1 THEN SUM(ISNULL(w.DurationMin, 0))
                WHEN 5 THEN COUNT(DISTINCT w.WorkoutTypeId)
                WHEN 6 THEN SUM(CASE WHEN (((DATEDIFF(DAY, '19000107', CAST(w.[Date] AS DATE)) % 7) + 7) % 7) IN (0, 6) THEN 1 ELSE 0 END)
                WHEN 7 THEN 0
                ELSE 0
            END, 0) AS CurrentValue
        FROM GoalsWithPeriod g
        LEFT JOIN dbo.Workouts w
            ON w.UserId = @UserId
           AND CAST(w.[Date] AS DATE) >= g.PeriodStart
           AND CAST(w.[Date] AS DATE) <= g.PeriodEnd
        WHERE g.MetricType IN (0, 1, 5, 6, 7)
        GROUP BY g.GoalId, g.MetricType
    ),
    StreakDates AS
    (
        SELECT
            g.GoalId,
            CAST(w.[Date] AS DATE) AS WorkoutDate
        FROM GoalsWithPeriod g
        LEFT JOIN dbo.Workouts w
            ON w.UserId = @UserId
           AND CAST(w.[Date] AS DATE) >= g.PeriodStart
           AND CAST(w.[Date] AS DATE) <= g.PeriodEnd
        WHERE g.MetricType = 4
          AND w.Id IS NOT NULL
        GROUP BY g.GoalId, CAST(w.[Date] AS DATE)
    ),
    StreakIslands AS
    (
        SELECT
            GoalId,
            WorkoutDate,
            DATEADD(DAY, -ROW_NUMBER() OVER (PARTITION BY GoalId ORDER BY WorkoutDate), WorkoutDate) AS IslandKey
        FROM StreakDates
    ),
    StreakMetrics AS
    (
        SELECT
            GoalId,
            MAX(StreakLen) AS CurrentValue
        FROM
        (
            SELECT
                GoalId,
                IslandKey,
                COUNT(*) AS StreakLen
            FROM StreakIslands
            GROUP BY GoalId, IslandKey
        ) streaks
        GROUP BY GoalId
    ),
    GoalMetrics AS
    (
        SELECT GoalId, CurrentValue FROM GeneralMetrics
        UNION ALL
        SELECT GoalId, CurrentValue FROM StreakMetrics
    )
    INSERT INTO @ComputedGoals (GoalId, TargetValue, CurrentValue)
    SELECT
        g.GoalId,
        g.TargetValue,
        ISNULL(gm.CurrentValue, 0) AS CurrentValue
    FROM GoalsWithPeriod g
    LEFT JOIN GoalMetrics gm ON gm.GoalId = g.GoalId;

    MERGE dbo.GoalProgresses AS target
    USING
    (
        SELECT
            cg.GoalId,
            cg.CurrentValue,
            CASE
                WHEN cg.TargetValue <= 0 THEN 0.0
                WHEN (CAST(cg.CurrentValue AS FLOAT) * 100.0 / cg.TargetValue) > 100.0 THEN 100.0
                ELSE CAST(cg.CurrentValue AS FLOAT) * 100.0 / cg.TargetValue
            END AS Percentage,
            CASE WHEN cg.CurrentValue >= cg.TargetValue THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsCompleted,
            SYSUTCDATETIME() AS LastCalculatedAt
        FROM @ComputedGoals cg
    ) AS source
    ON target.GoalId = source.GoalId
    WHEN MATCHED THEN
        UPDATE SET
            target.CurrentValue = source.CurrentValue,
            target.Percentage = source.Percentage,
            target.IsCompleted = source.IsCompleted,
            target.LastCalculatedAt = source.LastCalculatedAt
    WHEN NOT MATCHED THEN
        INSERT (GoalId, CurrentValue, Percentage, IsCompleted, LastCalculatedAt)
        VALUES (source.GoalId, source.CurrentValue, source.Percentage, source.IsCompleted, source.LastCalculatedAt);

    UPDATE g
    SET g.[Status] = 1
    FROM dbo.Goals g
    INNER JOIN @ComputedGoals cg ON cg.GoalId = g.Id
    WHERE g.[Status] = 0
      AND cg.CurrentValue >= g.TargetValue;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RecalculateGoalsProgress
    @UserId UNIQUEIDENTIFIER,
    @RecalculatedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ComputedGoals TABLE
    (
        GoalId UNIQUEIDENTIFIER PRIMARY KEY,
        TargetValue INT NOT NULL,
        CurrentValue INT NOT NULL
    );

    DECLARE @CurrentDate DATE = CAST(GETUTCDATE() AS DATE);

    ;WITH ActiveGoals AS
    (
        SELECT
            g.Id AS GoalId,
            g.MetricType,
            g.TargetValue,
            g.StartDate,
            g.EndDate,
            CASE g.PeriodType
                WHEN 0 THEN DATEADD(DAY, -((DATEDIFF(DAY, '19000101', @CurrentDate)) % 7), @CurrentDate)
                WHEN 1 THEN DATEFROMPARTS(YEAR(@CurrentDate), MONTH(@CurrentDate), 1)
                WHEN 2 THEN g.StartDate
                WHEN 3 THEN DATEADD(DAY, -6, @CurrentDate)
                ELSE g.StartDate
            END AS RawPeriodStart,
            CASE g.PeriodType
                WHEN 0 THEN DATEADD(DAY, 6 - ((DATEDIFF(DAY, '19000101', @CurrentDate)) % 7), @CurrentDate)
                WHEN 1 THEN EOMONTH(@CurrentDate)
                WHEN 2 THEN ISNULL(g.EndDate, @CurrentDate)
                WHEN 3 THEN @CurrentDate
                ELSE ISNULL(g.EndDate, @CurrentDate)
            END AS RawPeriodEnd
        FROM dbo.Goals g
        WHERE g.UserId = @UserId
          AND g.[Status] = 0
          AND g.StartDate <= @CurrentDate
          AND (g.EndDate IS NULL OR g.EndDate >= @CurrentDate)
          AND g.MetricType IN (0, 1, 4, 5, 6, 7)
    ),
    GoalsWithPeriod AS
    (
        SELECT
            ag.GoalId,
            ag.MetricType,
            ag.TargetValue,
            CASE WHEN ag.RawPeriodStart < ag.StartDate THEN ag.StartDate ELSE ag.RawPeriodStart END AS PeriodStart,
            CASE WHEN ag.EndDate IS NOT NULL AND ag.RawPeriodEnd > ag.EndDate THEN ag.EndDate ELSE ag.RawPeriodEnd END AS PeriodEnd
        FROM ActiveGoals ag
    ),
    GeneralMetrics AS
    (
        SELECT
            g.GoalId,
            ISNULL(CASE g.MetricType
                WHEN 0 THEN CAST(COUNT_BIG(w.Id) AS INT)
                WHEN 1 THEN SUM(ISNULL(w.DurationMin, 0))
                WHEN 5 THEN COUNT(DISTINCT w.WorkoutTypeId)
                WHEN 6 THEN SUM(CASE WHEN (((DATEDIFF(DAY, '19000107', CAST(w.[Date] AS DATE)) % 7) + 7) % 7) IN (0, 6) THEN 1 ELSE 0 END)
                WHEN 7 THEN 0
                ELSE 0
            END, 0) AS CurrentValue
        FROM GoalsWithPeriod g
        LEFT JOIN dbo.Workouts w
            ON w.UserId = @UserId
           AND CAST(w.[Date] AS DATE) >= g.PeriodStart
           AND CAST(w.[Date] AS DATE) <= g.PeriodEnd
        WHERE g.MetricType IN (0, 1, 5, 6, 7)
        GROUP BY g.GoalId, g.MetricType
    ),
    StreakDates AS
    (
        SELECT
            g.GoalId,
            CAST(w.[Date] AS DATE) AS WorkoutDate
        FROM GoalsWithPeriod g
        LEFT JOIN dbo.Workouts w
            ON w.UserId = @UserId
           AND CAST(w.[Date] AS DATE) >= g.PeriodStart
           AND CAST(w.[Date] AS DATE) <= g.PeriodEnd
        WHERE g.MetricType = 4
          AND w.Id IS NOT NULL
        GROUP BY g.GoalId, CAST(w.[Date] AS DATE)
    ),
    StreakIslands AS
    (
        SELECT
            GoalId,
            WorkoutDate,
            DATEADD(DAY, -ROW_NUMBER() OVER (PARTITION BY GoalId ORDER BY WorkoutDate), WorkoutDate) AS IslandKey
        FROM StreakDates
    ),
    StreakMetrics AS
    (
        SELECT
            GoalId,
            MAX(StreakLen) AS CurrentValue
        FROM
        (
            SELECT
                GoalId,
                IslandKey,
                COUNT(*) AS StreakLen
            FROM StreakIslands
            GROUP BY GoalId, IslandKey
        ) streaks
        GROUP BY GoalId
    ),
    GoalMetrics AS
    (
        SELECT GoalId, CurrentValue FROM GeneralMetrics
        UNION ALL
        SELECT GoalId, CurrentValue FROM StreakMetrics
    )
    INSERT INTO @ComputedGoals (GoalId, TargetValue, CurrentValue)
    SELECT
        g.GoalId,
        g.TargetValue,
        ISNULL(gm.CurrentValue, 0) AS CurrentValue
    FROM GoalsWithPeriod g
    LEFT JOIN GoalMetrics gm ON gm.GoalId = g.GoalId;

    MERGE dbo.GoalProgresses AS target
    USING
    (
        SELECT
            cg.GoalId,
            cg.CurrentValue,
            CASE
                WHEN cg.TargetValue <= 0 THEN 0.0
                WHEN (CAST(cg.CurrentValue AS FLOAT) * 100.0 / cg.TargetValue) > 100.0 THEN 100.0
                ELSE CAST(cg.CurrentValue AS FLOAT) * 100.0 / cg.TargetValue
            END AS Percentage,
            CASE WHEN cg.CurrentValue >= cg.TargetValue THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsCompleted,
            SYSUTCDATETIME() AS LastCalculatedAt
        FROM @ComputedGoals cg
    ) AS source
    ON target.GoalId = source.GoalId
    WHEN MATCHED THEN
        UPDATE SET
            target.CurrentValue = source.CurrentValue,
            target.Percentage = source.Percentage,
            target.IsCompleted = source.IsCompleted,
            target.LastCalculatedAt = source.LastCalculatedAt
    WHEN NOT MATCHED THEN
        INSERT (GoalId, CurrentValue, Percentage, IsCompleted, LastCalculatedAt)
        VALUES (source.GoalId, source.CurrentValue, source.Percentage, source.IsCompleted, source.LastCalculatedAt);

    UPDATE g
    SET g.[Status] = 1
    FROM dbo.Goals g
    INNER JOIN @ComputedGoals cg ON cg.GoalId = g.Id
    WHERE g.[Status] = 0
      AND cg.CurrentValue >= g.TargetValue;

    SELECT @RecalculatedCount = COUNT(*)
    FROM @ComputedGoals;
END;
GO
