import { GoalPeriodType, GoalStatus, getGoalUnit, type GoalsListItemDto } from '@/types/workout';
import { cn } from '@/lib/utils';
import { Target, Check } from 'lucide-react';

interface GoalCardProps {
  goal: GoalsListItemDto;
  className?: string;
}

export function GoalCard({ goal, className }: GoalCardProps) {
  const progressInfo = goal.progressInfo ?? { progressPercentage: 0, currentValue: 0, isCompleted: false };
  const progress = Math.min(progressInfo.progressPercentage, 100);
  const isComplete = progressInfo.isCompleted || goal.status === GoalStatus.Completed || progress >= 100;
  const unit = getGoalUnit(goal.metricType);
  const periodLabel = (() => {
    switch (goal.periodType) {
      case GoalPeriodType.Weekly: return 'weekly';
      case GoalPeriodType.Monthly: return 'monthly';
      case GoalPeriodType.CustomRange: return 'custom range';
      case GoalPeriodType.RollingWindow: return 'rolling window';
      default: return 'goal';
    }
  })();

  return (
    <div className={cn('workout-card', className)}>
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-2">
          <div
            className={cn(
              'w-8 h-8 rounded-lg flex items-center justify-center',
              isComplete ? 'bg-green-100' : 'bg-accent'
            )}
          >
            {isComplete ? (
              <Check className="w-4 h-4 text-green-600" />
            ) : (
              <Target className="w-4 h-4 text-accent-foreground" />
            )}
          </div>
          <div>
            <h4 className="font-medium text-foreground">{goal.name}</h4>
            <p className="text-xs text-muted-foreground capitalize">
              {periodLabel} goal
            </p>
          </div>
        </div>
        <span
          className={cn(
            'text-sm font-semibold',
            isComplete ? 'text-green-600' : 'text-foreground'
          )}
        >
          {progressInfo.currentValue}/{goal.targetValue} {unit}
        </span>
      </div>

      <div className="relative h-2 bg-muted rounded-full overflow-hidden">
        <div
          className={cn(
            'absolute inset-y-0 left-0 rounded-full transition-all duration-500',
            isComplete ? 'bg-green-500' : 'bg-primary'
          )}
          style={{ width: `${progress}%` }}
        />
      </div>

      <p className="mt-2 text-xs text-muted-foreground text-right">
        {isComplete ? '🎉 Goal achieved!' : `${(100 - progress).toFixed(0)}% to go`}
      </p>
    </div>
  );
}
