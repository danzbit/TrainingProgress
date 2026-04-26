import { useState, useEffect } from 'react';
import { Layout } from '@/components/Layout';
import { WorkoutCard } from '@/components/WorkoutCard';
import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import { format, isSameDay } from 'date-fns';
import { CalendarDays, List, Dumbbell, ChevronLeft, ChevronRight } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { workoutApi } from '@/services/api';
import type { WorkoutsListItemDto } from '@/types/workout';

type ViewMode = 'list' | 'calendar';

const PAGE_SIZE = 5;
const MAX_VISIBLE_PAGES = 5;

const HistoryPage = () => {
  const navigate = useNavigate();

  // List view state — server-side pagination
  const [workouts, setWorkouts] = useState<WorkoutsListItemDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [page, setPage] = useState(0);

  // Calendar view state
  const [allWorkoutDates, setAllWorkoutDates] = useState<Date[]>([]);
  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const [viewModeLoaded, setViewModeLoaded] = useState(false);
  const [selectedDate, setSelectedDate] = useState<Date | undefined>(new Date());
  const [allSelectedDateWorkouts, setAllSelectedDateWorkouts] = useState<WorkoutsListItemDto[]>([]);
  const [datePage, setDatePage] = useState(0);
  const [isLoadingDateWorkouts, setIsLoadingDateWorkouts] = useState(false);

  const dateTotalPages = Math.ceil(allSelectedDateWorkouts.length / PAGE_SIZE);
  const selectedDateWorkouts = allSelectedDateWorkouts.slice(datePage * PAGE_SIZE, (datePage + 1) * PAGE_SIZE);

  const totalPages = totalCount > 0 ? Math.ceil(totalCount / PAGE_SIZE) : 0;

  // 1. Fetch total count once on mount to render correct pagination immediately
  useEffect(() => {
    const controller = new AbortController();
    workoutApi.getWorkoutCount(controller.signal)
      .then((count) => setTotalCount(count))
      .catch(() => {});
    return () => controller.abort();
  }, []);

  // Load saved view preference on mount
  useEffect(() => {
    workoutApi.getHistoryViewPreference()
      .then((mode) => setViewMode(mode))
      .catch(() => {})
      .finally(() => setViewModeLoaded(true));
  }, []);

  // When view preference is loaded and calendar is active, fetch workouts for the pre-selected date
  useEffect(() => {
    if (!viewModeLoaded || viewMode !== 'calendar' || !selectedDate) return;
    handleDateSelect(selectedDate);
  }, [viewModeLoaded, viewMode]);

  // 2. Fetch page whenever `page` changes
  useEffect(() => {
    const controller = new AbortController();
    setIsLoading(true);
    workoutApi
      .getWorkoutsPage(page, PAGE_SIZE, controller.signal)
      .then(({ items }) => setWorkouts(items))
      .catch(() => {})
      .finally(() => setIsLoading(false));
    return () => controller.abort();
  }, [page]);

  // Fetch all dates (lightweight — just for calendar dot markers)
  useEffect(() => {
    const controller = new AbortController();
    workoutApi
      .getAllWorkouts('$orderby=date desc&$top=100', controller.signal)
      .then(({ items }) => setAllWorkoutDates(items.map((w) => new Date(w.date))))
      .catch(() => {});
    return () => controller.abort();
  }, []);

  const hasWorkout = (date: Date) => allWorkoutDates.some((d) => isSameDay(d, date));

  const handleDateSelect = (date: Date | undefined) => {
    setSelectedDate(date);
    setDatePage(0);
    if (!date) return;
    setIsLoadingDateWorkouts(true);
    workoutApi
      .getWorkoutsByDate(date)
      .then((items) => setAllSelectedDateWorkouts(items))
      .catch(() => setAllSelectedDateWorkouts([]))
      .finally(() => setIsLoadingDateWorkouts(false));
  };

  const goToDatePage = (p: number) => setDatePage(p);

  const renderDatePageButtons = () => {
    if (dateTotalPages <= 1) return null;
    const hasPrev = datePage > 0;
    const hasNext = datePage < dateTotalPages - 1;
    const half = Math.floor(MAX_VISIBLE_PAGES / 2);
    let start = Math.max(0, datePage - half);
    const end = Math.min(dateTotalPages, start + MAX_VISIBLE_PAGES);
    start = Math.max(0, end - MAX_VISIBLE_PAGES);
    const buttons: React.ReactNode[] = [];
    buttons.push(
      <Button key="prev" variant="outline" size="sm" onClick={() => goToDatePage(datePage - 1)} disabled={!hasPrev || isLoadingDateWorkouts}>
        <ChevronLeft className="w-4 h-4" />
      </Button>
    );
    if (start > 0) {
      buttons.push(<Button key={0} variant="outline" size="sm" onClick={() => goToDatePage(0)} disabled={isLoadingDateWorkouts}>1</Button>);
      if (start > 1) buttons.push(<span key="s-ellipsis" className="px-1 text-muted-foreground self-center">…</span>);
    }
    for (let i = start; i < end; i++) {
      buttons.push(
        <Button key={i} variant={i === datePage ? 'default' : 'outline'} size="sm" onClick={() => goToDatePage(i)} disabled={isLoadingDateWorkouts}>
          {i + 1}
        </Button>
      );
    }
    if (end < dateTotalPages) {
      if (end < dateTotalPages - 1) buttons.push(<span key="e-ellipsis" className="px-1 text-muted-foreground self-center">…</span>);
      buttons.push(<Button key={dateTotalPages - 1} variant="outline" size="sm" onClick={() => goToDatePage(dateTotalPages - 1)} disabled={isLoadingDateWorkouts}>{dateTotalPages}</Button>);
    }
    buttons.push(
      <Button key="next" variant="outline" size="sm" onClick={() => goToDatePage(datePage + 1)} disabled={!hasNext || isLoadingDateWorkouts}>
        <ChevronRight className="w-4 h-4" />
      </Button>
    );
    return <div className="flex flex-row items-center justify-center gap-1 pt-2 overflow-x-auto">{buttons}</div>;
  };

  const goToPage = (p: number) => setPage(p);

  const renderPageButtons = () => {
    if (totalPages <= 1) return null;
    const hasPrev = page > 0;
    const hasNext = page < totalPages - 1;

    const half = Math.floor(MAX_VISIBLE_PAGES / 2);
    let start = Math.max(0, page - half);
    const end = Math.min(totalPages, start + MAX_VISIBLE_PAGES);
    start = Math.max(0, end - MAX_VISIBLE_PAGES);

    const buttons: React.ReactNode[] = [];
    buttons.push(
      <Button key="prev" variant="outline" size="sm" onClick={() => goToPage(page - 1)} disabled={!hasPrev || isLoading}>
        <ChevronLeft className="w-4 h-4" />
      </Button>
    );
    if (start > 0) {
      buttons.push(<Button key={0} variant="outline" size="sm" onClick={() => goToPage(0)} disabled={isLoading}>1</Button>);
      if (start > 1) buttons.push(<span key="s-ellipsis" className="px-1 text-muted-foreground self-center">…</span>);
    }
    for (let i = start; i < end; i++) {
      buttons.push(
        <Button key={i} variant={i === page ? 'default' : 'outline'} size="sm" onClick={() => goToPage(i)} disabled={isLoading}>
          {i + 1}
        </Button>
      );
    }
    if (end < totalPages) {
      if (end < totalPages - 1) buttons.push(<span key="e-ellipsis" className="px-1 text-muted-foreground self-center">…</span>);
      buttons.push(<Button key={totalPages - 1} variant="outline" size="sm" onClick={() => goToPage(totalPages - 1)} disabled={isLoading}>{totalPages}</Button>);
    }
    buttons.push(
      <Button key="next" variant="outline" size="sm" onClick={() => goToPage(page + 1)} disabled={!hasNext || isLoading}>
        <ChevronRight className="w-4 h-4" />
      </Button>
    );
    return <div className="flex flex-row items-center justify-center gap-1 pt-2 overflow-x-auto">{buttons}</div>;
  };

  return (
    <Layout>
      <div className="py-6 space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-foreground">Workout History</h1>
            <p className="text-muted-foreground">
              {totalCount} workout{totalCount !== 1 ? 's' : ''} logged
            </p>
          </div>

          <div className="flex items-center gap-2">
            <Button variant={viewMode === 'list' ? 'default' : 'outline'} size="sm" onClick={() => {
              setViewMode('list');
              workoutApi.saveHistoryViewPreference('list').catch(() => {});
            }}>
              <List className="w-4 h-4 mr-1" />
              List
            </Button>
            <Button variant={viewMode === 'calendar' ? 'default' : 'outline'} size="sm" onClick={() => {
              setViewMode('calendar');
              workoutApi.saveHistoryViewPreference('calendar').catch(() => {});
              if (selectedDate) handleDateSelect(selectedDate);
            }}>
              <CalendarDays className="w-4 h-4 mr-1" />
              Calendar
            </Button>
          </div>
        </div>

        {/* Content */}
        {!viewModeLoaded || isLoading ? (
          <p className="text-sm text-muted-foreground text-center py-16">Loading workouts...</p>
        ) : viewMode === 'list' ? (
          <div className="space-y-4">
            {workouts.length > 0 ? (
              <>
                {workouts.map((workout, i) => (
                  <WorkoutCard key={`${workout.date}-${i}`} workout={workout} />
                ))}
                {renderPageButtons()}
              </>
            ) : (
              <div className="text-center py-16">
                <Dumbbell className="w-16 h-16 mx-auto text-muted-foreground/30 mb-4" />
                <h3 className="text-lg font-medium text-foreground mb-2">No workouts yet</h3>
                <p className="text-muted-foreground mb-4">Start tracking your fitness journey today!</p>
                <Button onClick={() => navigate('/workout')}>Log Your First Workout</Button>
              </div>
            )}
          </div>
        ) : (
          <div className="grid lg:grid-cols-2 gap-6">
            {/* Calendar */}
            <div className="workout-card p-4">
              <Calendar
                mode="single"
                selected={selectedDate}
                onSelect={handleDateSelect}
                className="w-full pointer-events-auto"
                modifiers={{ workout: (date) => hasWorkout(date) }}
                modifiersStyles={{
                  workout: { fontWeight: 'bold', backgroundColor: 'hsl(var(--primary) / 0.1)', borderRadius: '50%' },
                }}
              />
            </div>

            {/* Selected Date Workouts */}
            <div className="space-y-4">
              <h3 className="font-semibold text-foreground">
                {selectedDate ? format(selectedDate, 'EEEE, MMMM d, yyyy') : 'Select a date'}
              </h3>
              {isLoadingDateWorkouts ? (
                <p className="text-sm text-muted-foreground text-center py-8">Loading...</p>
              ) : selectedDateWorkouts.length > 0 ? (
                <div className="space-y-3">
                  {selectedDateWorkouts.map((workout, i) => (
                    <WorkoutCard key={`${workout.date}-${i}`} workout={workout} />
                  ))}
                  {renderDatePageButtons()}
                </div>
              ) : (
                <div className="workout-card text-center py-8">
                  <p className="text-muted-foreground">No workouts on this day</p>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};

export default HistoryPage;
