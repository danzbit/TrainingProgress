import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Layout } from '@/components/Layout';
import { StatCard } from '@/components/StatCard';
import { WorkoutCard } from '@/components/WorkoutCard';
import { GoalCard } from '@/components/GoalCard';
import { Button } from '@/components/ui/button';
import { 
  Dumbbell, 
  Clock, 
  Flame, 
  Target, 
  Plus, 
  ChevronRight,
  TrendingUp
} from 'lucide-react';
import { format } from 'date-fns';
import { analyticsApi, workoutApi } from '@/services/api';
import type { WorkoutSummaryDto } from '@/services/api';
import type { WorkoutsListItemDto, GoalsListItemDto } from '@/types/workout';

const Index = () => {
  const navigate = useNavigate();

  const [summary, setSummary] = useState<WorkoutSummaryDto>({
    amountPerWeek: 0,
    weekDurationMin: 0,
    amountThisMonth: 0,
    monthlyTimeMin: 0,
  });
  const [recentWorkouts, setRecentWorkouts] = useState<WorkoutsListItemDto[]>([]);
  const [recentGoals, setRecentGoals] = useState<GoalsListItemDto[]>([]);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    const refresh = () => setRefreshKey((k) => k + 1);
    window.addEventListener('sync:workout_created', refresh);
    window.addEventListener('sync:goal_saved', refresh);
    return () => {
      window.removeEventListener('sync:workout_created', refresh);
      window.removeEventListener('sync:goal_saved', refresh);
    };
  }, []);

  useEffect(() => {
    analyticsApi.getSummary().then(setSummary).catch(() => {});
    workoutApi
      .getAllWorkouts('$orderby=date desc&$top=3')
      .then(({ items }) => setRecentWorkouts(items))
      .catch(() => {});
    workoutApi
      .getAllGoals()
      .then(({ items }) => setRecentGoals(items.slice(0, 2)))
      .catch(() => {});
  }, [refreshKey]);

  const today = new Date();
  const greeting = today.getHours() < 12 ? 'Good morning' : today.getHours() < 18 ? 'Good afternoon' : 'Good evening';

  return (
    <Layout>
      <div className="py-6 space-y-8">
        {/* Hero Section */}
        <div className="relative overflow-hidden rounded-2xl gradient-hero p-6 md:p-8 text-primary-foreground">
          <div className="relative z-10">
            <p className="text-primary-foreground/80 text-sm mb-1">
              {format(today, 'EEEE, MMMM d')}
            </p>
            <h1 className="text-2xl md:text-3xl font-bold mb-2">{greeting}! 👋</h1>
            <p className="text-primary-foreground/90 mb-6 max-w-md">
              Ready to crush your fitness goals today? Track your progress and stay motivated.
            </p>
            <Button
              onClick={() => navigate('/workout')}
              className="bg-primary-foreground text-primary hover:bg-primary-foreground/90"
            >
              <Plus className="w-4 h-4 mr-2" />
              Log Workout
            </Button>
          </div>
          
          {/* Decorative elements */}
          <div className="absolute -right-10 -top-10 w-40 h-40 bg-primary-foreground/10 rounded-full blur-2xl" />
          <div className="absolute -right-5 -bottom-10 w-32 h-32 bg-primary-foreground/5 rounded-full blur-xl" />
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard
            title="This Week"
            value={summary.amountPerWeek}
            subtitle="workouts"
            icon={Dumbbell}
            className="animate-slide-up stagger-1"
          />
          <StatCard
            title="Week Duration"
            value={summary.weekDurationMin}
            subtitle="minutes"
            icon={Clock}
            className="animate-slide-up stagger-2"
          />
          <StatCard
            title="This Month"
            value={summary.amountThisMonth}
            subtitle="workouts"
            icon={Flame}
            className="animate-slide-up stagger-3"
          />
          <StatCard
            title="Monthly Time"
            value={summary.monthlyTimeMin}
            subtitle="minutes"
            icon={TrendingUp}
            className="animate-slide-up stagger-4"
          />
        </div>

        {/* Quick Actions */}
        <div className="grid md:grid-cols-2 gap-6">
          {/* Recent Workouts */}
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold text-foreground">Recent Workouts</h2>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => navigate('/history')}
                className="text-muted-foreground"
              >
                View All
                <ChevronRight className="w-4 h-4 ml-1" />
              </Button>
            </div>

            {recentWorkouts.length > 0 ? (
              <div className="space-y-3">
                {recentWorkouts.map((workout, i) => (
                  <WorkoutCard key={`${workout.date}-${i}`} workout={workout} compact />
                ))}
              </div>
            ) : (
              <div className="workout-card flex flex-col items-center justify-center py-8 text-center">
                <Dumbbell className="w-12 h-12 text-muted-foreground/30 mb-3" />
                <p className="text-muted-foreground mb-3">No workouts yet</p>
                <Button variant="outline" size="sm" onClick={() => navigate('/workout')}>
                  Log Your First Workout
                </Button>
              </div>
            )}
          </div>

          {/* Goals Overview */}
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold text-foreground">Your Goals</h2>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => navigate('/goals')}
                className="text-muted-foreground"
              >
                Manage
                <ChevronRight className="w-4 h-4 ml-1" />
              </Button>
            </div>

            {recentGoals.length > 0 ? (
              <div className="space-y-3">
                {recentGoals.map((goal) => (
                  <GoalCard key={goal.id} goal={goal} />
                ))}
              </div>
            ) : (
              <div className="workout-card flex flex-col items-center justify-center py-8 text-center">
                <Target className="w-12 h-12 text-muted-foreground/30 mb-3" />
                <p className="text-muted-foreground mb-3">No goals set</p>
                <Button variant="outline" size="sm" onClick={() => navigate('/goals')}>
                  Set Your First Goal
                </Button>
              </div>
            )}
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default Index;
