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

    ;WITH ActiveGoals AS
    (
        SELECT
            g.Id AS GoalId,
            g.MetricType,
            g.TargetValue,
            g.[Filter],
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
            ag.[Filter],
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
           AND (
                JSON_VALUE(g.[Filter], '$.MinDuration') IS NULL
                OR w.DurationMin >= TRY_CONVERT(INT, JSON_VALUE(g.[Filter], '$.MinDuration'))
           )
           AND (
                JSON_QUERY(g.[Filter], '$.AllowedTypes') IS NULL
                OR NOT EXISTS (SELECT 1 FROM OPENJSON(g.[Filter], '$.AllowedTypes'))
                OR w.WorkoutTypeId IN
                   (
                       SELECT TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(j.[value], '$.Id'))
                       FROM OPENJSON(g.[Filter], '$.AllowedTypes') j
                       WHERE TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(j.[value], '$.Id')) IS NOT NULL
                   )
           )
           AND (
                JSON_QUERY(g.[Filter], '$.AllowedDays') IS NULL
                OR NOT EXISTS (SELECT 1 FROM OPENJSON(g.[Filter], '$.AllowedDays'))
                OR (((DATEDIFF(DAY, '19000107', CAST(w.[Date] AS DATE)) % 7) + 7) % 7) IN
                   (
                       SELECT TRY_CONVERT(INT, d.[value])
                       FROM OPENJSON(g.[Filter], '$.AllowedDays') d
                       WHERE TRY_CONVERT(INT, d.[value]) IS NOT NULL
                   )
           )
           AND (
                JSON_VALUE(g.[Filter], '$.TimeOfDay.Start') IS NULL
                OR JSON_VALUE(g.[Filter], '$.TimeOfDay.End') IS NULL
           )
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
           AND (
                JSON_VALUE(g.[Filter], '$.MinDuration') IS NULL
                OR w.DurationMin >= TRY_CONVERT(INT, JSON_VALUE(g.[Filter], '$.MinDuration'))
           )
           AND (
                JSON_QUERY(g.[Filter], '$.AllowedTypes') IS NULL
                OR NOT EXISTS (SELECT 1 FROM OPENJSON(g.[Filter], '$.AllowedTypes'))
                OR w.WorkoutTypeId IN
                   (
                       SELECT TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(j.[value], '$.Id'))
                       FROM OPENJSON(g.[Filter], '$.AllowedTypes') j
                       WHERE TRY_CONVERT(UNIQUEIDENTIFIER, JSON_VALUE(j.[value], '$.Id')) IS NOT NULL
                   )
           )
           AND (
                JSON_QUERY(g.[Filter], '$.AllowedDays') IS NULL
                OR NOT EXISTS (SELECT 1 FROM OPENJSON(g.[Filter], '$.AllowedDays'))
                OR (((DATEDIFF(DAY, '19000107', CAST(w.[Date] AS DATE)) % 7) + 7) % 7) IN
                   (
                       SELECT TRY_CONVERT(INT, d.[value])
                       FROM OPENJSON(g.[Filter], '$.AllowedDays') d
                       WHERE TRY_CONVERT(INT, d.[value]) IS NOT NULL
                   )
           )
           AND (
                JSON_VALUE(g.[Filter], '$.TimeOfDay.Start') IS NULL
                OR JSON_VALUE(g.[Filter], '$.TimeOfDay.End') IS NULL
           )
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
    UPDATE g
    SET g.[Status] = 1
    FROM dbo.Goals g
    INNER JOIN GoalMetrics gm ON gm.GoalId = g.Id
    WHERE g.[Status] = 0
      AND gm.CurrentValue >= g.TargetValue;
END;
GO
