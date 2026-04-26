import { useEffect, useState } from 'react';
import { Layout } from '@/components/Layout';
import { StatCard } from '@/components/StatCard';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line,
  PieChart,
  Pie,
  Cell,
} from 'recharts';
import { Dumbbell, Clock, Flame, TrendingUp, Trophy } from 'lucide-react';
import { format, parseISO } from 'date-fns';
import { analyticsApi } from '@/services/api/analyticsApi';
import type {
  WorkoutDailyTrendPointDto,
  WorkoutCountByTypeDto,
  WorkoutStatisticsOverviewDto,
  WorkoutSummaryDto,
} from '@/services/api/analyticsApi';
import { toast } from '@/hooks/use-toast';

const CHART_COLORS = [
  'hsl(217, 91%, 60%)',
  'hsl(230, 91%, 65%)',
  'hsl(245, 80%, 65%)',
  'hsl(190, 80%, 50%)',
  'hsl(160, 70%, 45%)',
  'hsl(280, 70%, 60%)',
  'hsl(30, 80%, 55%)',
];

const ProgressPage = () => {
  const [isLoading, setIsLoading] = useState(true);
  const [summary, setSummary] = useState<WorkoutSummaryDto | null>(null);
  const [overview, setOverview] = useState<WorkoutStatisticsOverviewDto | null>(null);
  const [dailyTrend, setDailyTrend] = useState<WorkoutDailyTrendPointDto[]>([]);
  const [countByType, setCountByType] = useState<WorkoutCountByTypeDto[]>([]);
  const [refreshKey, setRefreshKey] = useState(0);

  // Refresh analytics when a new workout is created via saga
  useEffect(() => {
    const refresh = () => setRefreshKey((k) => k + 1);
    window.addEventListener('sync:workout_created', refresh);
    return () => window.removeEventListener('sync:workout_created', refresh);
  }, []);

  useEffect(() => {
    Promise.allSettled([
      analyticsApi.getSummary(),
      analyticsApi.getStatisticsOverview(),
      analyticsApi.getDailyTrend(),
      analyticsApi.getCountByType(),
    ]).then(([summaryRes, overviewRes, trendRes, typeRes]) => {
      if (summaryRes.status === 'fulfilled') setSummary(summaryRes.value);
      else toast({ title: 'Failed to load summary stats', variant: 'destructive' });

      if (overviewRes.status === 'fulfilled') setOverview(overviewRes.value);
      else toast({ title: 'Failed to load statistics overview', variant: 'destructive' });

      if (trendRes.status === 'fulfilled') setDailyTrend(trendRes.value);
      else toast({ title: 'Failed to load daily trend', variant: 'destructive' });

      if (typeRes.status === 'fulfilled') setCountByType(typeRes.value);
      else toast({ title: 'Failed to load workout type data', variant: 'destructive' });

      setIsLoading(false);
    });
  }, [refreshKey]);

  const dailyData = dailyTrend.map((point) => ({
    day: format(parseISO(point.date), 'EEE'),
    workouts: point.workoutsCount,
    duration: point.durationMin,
  }));

  const typeDistribution = countByType.filter((t) => t.workoutsCount > 0);
  const totalWorkouts = overview?.totalWorkoutsCompleted ?? 0;

  return (
    <Layout>
      <div className="py-6 space-y-8">
        {/* Header */}
        <div>
          <h1 className="text-2xl font-bold text-foreground">Progress Dashboard</h1>
          <p className="text-muted-foreground">
            Track your fitness journey and see your improvements
          </p>
        </div>

        {isLoading ? (
          <div className="text-center text-muted-foreground py-12">Loading analytics...</div>
        ) : (
          <>
            {/* Stats Grid */}
            <div className="grid grid-cols-2 lg:grid-cols-5 gap-4">
              <StatCard
                title="This Week"
                value={overview?.workoutsThisWeek ?? 0}
                subtitle="workouts"
                icon={Dumbbell}
              />
              <StatCard
                title="Week Duration"
                value={summary?.weekDurationMin ?? 0}
                subtitle="minutes"
                icon={Clock}
              />
              <StatCard
                title="Total Workouts"
                value={overview?.totalWorkoutsCompleted ?? 0}
                subtitle="all time"
                icon={Flame}
              />
              <StatCard
                title="Total Training"
                value={overview?.totalTrainingHours ?? 0}
                subtitle="hours"
                icon={TrendingUp}
              />
              <StatCard
                title="Goals Achieved"
                value={overview?.totalAchievedGoals ?? 0}
                subtitle="all time"
                icon={Trophy}
              />
            </div>

            {/* Charts */}
            <div className="grid lg:grid-cols-2 gap-6">
              {/* Weekly Activity Chart */}
              <div className="workout-card p-6">
                <h3 className="font-semibold text-foreground mb-4">Last 7 Days Activity</h3>
                <div className="h-64">
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart data={dailyData}>
                      <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                      <XAxis
                        dataKey="day"
                        stroke="hsl(var(--muted-foreground))"
                        fontSize={12}
                      />
                      <YAxis stroke="hsl(var(--muted-foreground))" fontSize={12} />
                      <Tooltip
                        contentStyle={{
                          backgroundColor: 'hsl(var(--card))',
                          border: '1px solid hsl(var(--border))',
                          borderRadius: '8px',
                        }}
                        labelStyle={{ color: 'hsl(var(--foreground))' }}
                      />
                      <Bar
                        dataKey="duration"
                        fill="hsl(217, 91%, 60%)"
                        radius={[4, 4, 0, 0]}
                        name="Duration (min)"
                      />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              </div>

              {/* Workout Trend */}
              <div className="workout-card p-6">
                <h3 className="font-semibold text-foreground mb-4">Workout Trend</h3>
                <div className="h-64">
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart data={dailyData}>
                      <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                      <XAxis
                        dataKey="day"
                        stroke="hsl(var(--muted-foreground))"
                        fontSize={12}
                      />
                      <YAxis stroke="hsl(var(--muted-foreground))" fontSize={12} />
                      <Tooltip
                        contentStyle={{
                          backgroundColor: 'hsl(var(--card))',
                          border: '1px solid hsl(var(--border))',
                          borderRadius: '8px',
                        }}
                        labelStyle={{ color: 'hsl(var(--foreground))' }}
                      />
                      <Line
                        type="monotone"
                        dataKey="workouts"
                        stroke="hsl(217, 91%, 60%)"
                        strokeWidth={2}
                        dot={{ fill: 'hsl(217, 91%, 60%)', strokeWidth: 2, r: 4 }}
                        name="Workouts"
                      />
                    </LineChart>
                  </ResponsiveContainer>
                </div>
              </div>

              {/* Workout Type Distribution */}
              {typeDistribution.length > 0 && (
                <div className="workout-card p-6 lg:col-span-2">
                  <h3 className="font-semibold text-foreground mb-4">
                    Workout Type Distribution
                  </h3>
                  <div className="flex flex-col lg:flex-row items-center gap-8">
                    <div className="h-64 w-full max-w-xs">
                      <ResponsiveContainer width="100%" height="100%">
                        <PieChart>
                          <Pie
                            data={typeDistribution}
                            cx="50%"
                            cy="50%"
                            innerRadius={60}
                            outerRadius={100}
                            paddingAngle={4}
                            dataKey="workoutsCount"
                          >
                            {typeDistribution.map((_, index) => (
                              <Cell
                                key={`cell-${index}`}
                                fill={CHART_COLORS[index % CHART_COLORS.length]}
                              />
                            ))}
                          </Pie>
                          <Tooltip
                            contentStyle={{
                              backgroundColor: 'hsl(var(--card))',
                              border: '1px solid hsl(var(--border))',
                              borderRadius: '8px',
                            }}
                          />
                        </PieChart>
                      </ResponsiveContainer>
                    </div>

                    <div className="flex flex-wrap gap-4 justify-center lg:justify-start">
                      {typeDistribution.map((type, index) => (
                        <div key={type.workoutTypeId} className="flex items-center gap-2">
                          <div
                            className="w-3 h-3 rounded-full"
                            style={{
                              backgroundColor: CHART_COLORS[index % CHART_COLORS.length],
                            }}
                          />
                          <span className="text-sm text-muted-foreground">
                            {type.workoutTypeName} ({type.workoutsCount})
                          </span>
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              )}
            </div>

            {/* Empty State */}
            {totalWorkouts === 0 && (
              <div className="text-center py-12">
                <TrendingUp className="w-16 h-16 mx-auto text-muted-foreground/30 mb-4" />
                <h3 className="text-lg font-medium text-foreground mb-2">No data yet</h3>
                <p className="text-muted-foreground">
                  Start logging workouts to see your progress charts
                </p>
              </div>
            )}
          </>
        )}
      </div>
    </Layout>
  );
};

export default ProgressPage;
