import { useState, useEffect, useCallback } from 'react';
import { Layout } from '@/components/Layout';
import { GoalCard } from '@/components/GoalCard';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import {
  GOAL_METRIC_OPTIONS,
  GOAL_PERIOD_OPTIONS,
  GoalMetricType,
  GoalPeriodType,
  type GoalsListItemDto,
} from '@/types/workout';
import { getApiErrorDescription, goalOrchestrationApi, notificationApi, workoutApi } from '@/services/api';
import type { ReminderResponse } from '@/services/api';
import { ChevronLeft, ChevronRight, Plus, Target, Bell, Trash2 } from 'lucide-react';
import { toast } from '@/hooks/use-toast';

const PAGE_SIZE = 6;
const MAX_VISIBLE_PAGES = 5;

const GoalsPage = () => {
  const [allGoals, setAllGoals] = useState<GoalsListItemDto[]>([]);
  const [reminders, setReminders] = useState<ReminderResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(0);
  const [refreshKey, setRefreshKey] = useState(0);

  // Refresh when the server notifies that a workout was created (goals progress updated)
  // or a goal was saved.
  useEffect(() => {
    const refresh = () => setRefreshKey((k) => k + 1);
    window.addEventListener('sync:workout_created', refresh);
    window.addEventListener('sync:goal_saved', refresh);
    return () => {
      window.removeEventListener('sync:workout_created', refresh);
      window.removeEventListener('sync:goal_saved', refresh);
    };
  }, []);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [newGoal, setNewGoal] = useState<{
    name: string;
    description: string;
    targetValue: string;
    metricType: GoalMetricType;
    periodType: GoalPeriodType;
    startDate: string;
    endDate: string;
  }>({
    name: '',
    description: '',
    targetValue: '',
    metricType: GoalMetricType.WorkoutCount,
    periodType: GoalPeriodType.Weekly,
    startDate: '',
    endDate: '',
  });

  const fetchGoals = useCallback((signal?: AbortSignal) => {
    setIsLoading(true);
    Promise.all([
      workoutApi.getAllGoals(signal),
      notificationApi.getReminders(signal),
    ])
      .then(([{ items }, reminderItems]) => {
        setAllGoals(items);
        setReminders(reminderItems);
      })
      .catch(() => {})
      .finally(() => setIsLoading(false));
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    fetchGoals(controller.signal);
    return () => controller.abort();
  }, [fetchGoals, refreshKey]);

  const totalPages = Math.ceil(allGoals.length / PAGE_SIZE);
  const goals = allGoals.slice(page * PAGE_SIZE, (page + 1) * PAGE_SIZE);

  const handleAddGoal = async () => {
    if (!newGoal.name || !newGoal.description || !newGoal.targetValue) {
      toast({
        title: 'Missing information',
        description: 'Please fill in all fields.',
        variant: 'destructive',
      });
      return;
    }

    if (newGoal.periodType === GoalPeriodType.CustomRange && (!newGoal.startDate || !newGoal.endDate)) {
      toast({
        title: 'Missing dates',
        description: 'Custom Range period requires both a start and end date.',
        variant: 'destructive',
      });
      return;
    }

    const startDate = newGoal.periodType === GoalPeriodType.CustomRange && newGoal.startDate
      ? new Date(newGoal.startDate)
      : new Date();
    const correlationId = crypto.randomUUID();

    setIsSubmitting(true);
    try {
      await goalOrchestrationApi.saveGoal(
        {
          name: newGoal.name,
          description: newGoal.description,
          metricType: newGoal.metricType,
          periodType: newGoal.periodType,
          targetValue: parseInt(newGoal.targetValue),
          startDate: startDate.toISOString(),
          endDate: newGoal.endDate ? new Date(newGoal.endDate).toISOString() : undefined,
          correlationId,
        },
        correlationId,
      );

      setNewGoal({
        name: '',
        description: '',
        targetValue: '',
        metricType: GoalMetricType.WorkoutCount,
        periodType: GoalPeriodType.Weekly,
        startDate: '',
        endDate: '',
      });
      setDialogOpen(false);
      setPage(0);
      fetchGoals();

      toast({
        title: 'Goal created! 🎯',
        description: 'Your new fitness goal has been set.',
      });
    } catch (err: unknown) {
      const description = getApiErrorDescription(err, 'Something went wrong. Please try again.');
      toast({
        title: 'Failed to create goal',
        description,
        variant: 'destructive',
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteGoal = async (id: string) => {
    try {
      await goalOrchestrationApi.deleteGoal(id);
    } catch {
      toast({
        title: 'Failed to delete goal',
        description: 'Something went wrong. Please try again.',
        variant: 'destructive',
      });
      return;
    }
    setAllGoals((prev) => prev.filter((g) => g.id !== id));
    toast({
      title: 'Goal removed',
      description: 'The goal has been deleted.',
    });
  };

  const renderPageButtons = () => {
    if (totalPages <= 1) return null;
    const half = Math.floor(MAX_VISIBLE_PAGES / 2);
    let start = Math.max(0, page - half);
    const end = Math.min(totalPages, start + MAX_VISIBLE_PAGES);
    start = Math.max(0, end - MAX_VISIBLE_PAGES);

    const buttons: React.ReactNode[] = [];

    buttons.push(
      <Button key="prev" variant="outline" size="sm" onClick={() => setPage((p) => p - 1)} disabled={page === 0 || isLoading}>
        <ChevronLeft className="w-4 h-4" />
      </Button>
    );
    if (start > 0) {
      buttons.push(<Button key={0} variant="outline" size="sm" onClick={() => setPage(0)} disabled={isLoading}>1</Button>);
      if (start > 1) buttons.push(<span key="s-ellipsis" className="px-1 text-muted-foreground self-center">…</span>);
    }
    for (let i = start; i < end; i++) {
      buttons.push(
        <Button key={i} variant={i === page ? 'default' : 'outline'} size="sm" onClick={() => setPage(i)} disabled={isLoading}>
          {i + 1}
        </Button>
      );
    }
    if (end < totalPages) {
      if (end < totalPages - 1) buttons.push(<span key="e-ellipsis" className="px-1 text-muted-foreground self-center">…</span>);
      buttons.push(<Button key={totalPages - 1} variant="outline" size="sm" onClick={() => setPage(totalPages - 1)} disabled={isLoading}>{totalPages}</Button>);
    }
    buttons.push(
      <Button key="next" variant="outline" size="sm" onClick={() => setPage((p) => p + 1)} disabled={page >= totalPages - 1 || isLoading}>
        <ChevronRight className="w-4 h-4" />
      </Button>
    );

    return <div className="flex items-center justify-center gap-1 pt-2 flex-wrap">{buttons}</div>;
  };

  return (
    <Layout>
      <div className="py-6 space-y-8">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-foreground">Goals & Reminders</h1>
            <p className="text-muted-foreground">
              {allGoals.length} goal{allGoals.length !== 1 ? 's' : ''} set
            </p>
          </div>

          <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
            <DialogTrigger asChild>
              <Button disabled={isLoading}>
                <Plus className="w-4 h-4 mr-2" />
                New Goal
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Create New Goal</DialogTitle>
              </DialogHeader>

              <div className="space-y-4 pt-4">
                <div className="space-y-2">
                  <Label>Goal Title</Label>
                  <Input
                    placeholder="e.g., Weekly Workouts"
                    value={newGoal.name}
                    onChange={(e) =>
                      setNewGoal({ ...newGoal, name: e.target.value })
                    }
                  />
                </div>

                <div className="space-y-2">
                  <Label>Description</Label>
                  <Input
                    placeholder="e.g., Keep a consistent training habit"
                    value={newGoal.description}
                    onChange={(e) =>
                      setNewGoal({ ...newGoal, description: e.target.value })
                    }
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Target</Label>
                    <Input
                      type="number"
                      placeholder="4"
                      value={newGoal.targetValue}
                      onChange={(e) =>
                        setNewGoal({ ...newGoal, targetValue: e.target.value })
                      }
                    />
                  </div>

                  <div className="space-y-2">
                    <Label>Metric</Label>
                    <Select
                      value={String(newGoal.metricType)}
                      onValueChange={(v) =>
                        setNewGoal({ ...newGoal, metricType: Number(v) as GoalMetricType })
                      }
                    >
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        {GOAL_METRIC_OPTIONS.map((option) => (
                          <SelectItem key={option.value} value={String(option.value)}>
                            {option.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="space-y-2">
                  <Label>Period</Label>
                  <Select
                    value={String(newGoal.periodType)}
                    onValueChange={(v) =>
                      setNewGoal({ ...newGoal, periodType: Number(v) as GoalPeriodType, startDate: '', endDate: '' })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {GOAL_PERIOD_OPTIONS.map((option) => (
                        <SelectItem key={option.value} value={String(option.value)}>
                          {option.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {newGoal.periodType === GoalPeriodType.CustomRange && (
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label>Start Date</Label>
                      <Input
                        type="date"
                        value={newGoal.startDate}
                        max={newGoal.endDate || undefined}
                        onChange={(e) =>
                          setNewGoal({ ...newGoal, startDate: e.target.value })
                        }
                      />
                    </div>
                    <div className="space-y-2">
                      <Label>End Date</Label>
                      <Input
                        type="date"
                        value={newGoal.endDate}
                        min={newGoal.startDate || new Date().toISOString().split('T')[0]}
                        onChange={(e) =>
                          setNewGoal({ ...newGoal, endDate: e.target.value })
                        }
                      />
                    </div>
                  </div>
                )}

                <Button onClick={handleAddGoal} className="w-full" disabled={isSubmitting}>
                  {isSubmitting ? 'Creating...' : 'Create Goal'}
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>

        {/* Reminders Section */}
        {reminders.length > 0 && (
          <div className="bg-warning/10 border border-warning/30 rounded-xl p-4">
            <div className="flex items-center gap-2 mb-3">
              <Bell className="w-5 h-5 text-warning" />
              <h3 className="font-semibold text-foreground">Reminders</h3>
            </div>
            <div className="space-y-2">
              {reminders.map((reminder, index) => (
                <p key={index} className="text-sm text-muted-foreground">
                  {reminder.message}
                </p>
              ))}
            </div>
          </div>
        )}

        {/* Goals List */}
        <div className="space-y-4">
          <h2 className="text-lg font-semibold text-foreground">Your Goals</h2>

          {isLoading ? (
            <p className="text-sm text-muted-foreground text-center py-16">Loading goals...</p>
          ) : goals.length > 0 ? (
            <>
              <div className="grid md:grid-cols-2 gap-4">
                {goals.map((goal) => (
                  <div key={goal.id} className="relative group">
                    <GoalCard goal={goal} />
                    <Button
                      variant="ghost"
                      size="icon"
                      className="absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity h-8 w-8 text-muted-foreground hover:text-destructive"
                      onClick={() => handleDeleteGoal(goal.id)}
                    >
                      <Trash2 className="w-4 h-4" />
                    </Button>
                  </div>
                ))}
              </div>
              {renderPageButtons()}
            </>
          ) : (
            <div className="text-center py-16">
              <Target className="w-16 h-16 mx-auto text-muted-foreground/30 mb-4" />
              <h3 className="text-lg font-medium text-foreground mb-2">
                No goals yet
              </h3>
              <p className="text-muted-foreground mb-4">
                Set your first fitness goal to stay motivated!
              </p>
              <Button onClick={() => setDialogOpen(true)}>
                <Plus className="w-4 h-4 mr-2" />
                Create Your First Goal
              </Button>
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
};

export default GoalsPage;
