import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAppSelector } from '@/store/hooks';
import { selectAuthLoading, selectIsAuthenticated } from '@/store/selectors';
import { Dumbbell } from 'lucide-react';

/**
 * Wraps protected routes.
 * – Shows a loading screen while the initial auth check is in-flight.
 * – Redirects unauthenticated users to /login, preserving the intended
 *   destination so the login page can send them back afterwards.
 */
const ProtectedRoute = () => {
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const isLoading = useAppSelector(selectAuthLoading);
  const location = useLocation();

  if (isLoading) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center gap-4 bg-background">
        <div className="w-14 h-14 rounded-2xl gradient-primary flex items-center justify-center animate-pulse">
          <Dumbbell className="w-7 h-7 text-primary-foreground" />
        </div>
        <p className="text-muted-foreground text-sm">Loading…</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <Outlet />;
};

export default ProtectedRoute;
