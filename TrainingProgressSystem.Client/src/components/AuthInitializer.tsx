import { useEffect, type ReactNode } from 'react';
import { useLocation } from 'react-router-dom';
import { useAppDispatch } from '@/store/hooks';
import { setUser, clearUser } from '@/store/authSlice';
import { authApi } from '@/services/api';

const PUBLIC_PATHS = ['/login', '/register'];

interface AuthInitializerProps {
  children: ReactNode;
}

/**
 * Bootstraps authentication state on every full page load:
 * - Skips the /me check on public pages (login, register) to avoid
 *   noise 401s in the console when there is no session.
 * - On protected pages, calls GET /me; the axios interceptor will
 *   transparently attempt a token refresh if needed.
 * - Listens for the global `auth:failure` event to clear state when
 *   a mid-session refresh fails.
 */
const AuthInitializer = ({ children }: AuthInitializerProps) => {
  const dispatch = useAppDispatch();
  const location = useLocation();

  useEffect(() => {
    // No session exists yet on public pages — skip the bootstrap request.
    if (PUBLIC_PATHS.includes(location.pathname)) {
      dispatch(clearUser());
      return;
    }

    authApi
      .getCurrentUser()
      .then((user) => dispatch(setUser(user)))
      .catch(() => dispatch(clearUser()));

    const handleAuthFailure = () => dispatch(clearUser());
    window.addEventListener('auth:failure', handleAuthFailure);
    return () => window.removeEventListener('auth:failure', handleAuthFailure);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dispatch]);

  return <>{children}</>;
};

export default AuthInitializer;
