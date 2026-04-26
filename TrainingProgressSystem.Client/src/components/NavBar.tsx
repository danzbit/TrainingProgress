import { NavLink as RouterNavLink, useLocation, useNavigate } from 'react-router-dom';
import { cn } from '@/lib/utils';
import { 
  LayoutDashboard, 
  Dumbbell, 
  History, 
  TrendingUp, 
  Target, 
  User,
  Menu,
  X,
  LogOut,
} from 'lucide-react';
import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { useAppDispatch } from '@/store/hooks';
import { clearUser } from '@/store/authSlice';
import { authApi } from '@/services/api';

const navItems = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/workout', label: 'Log Workout', icon: Dumbbell },
  { to: '/history', label: 'History', icon: History },
  { to: '/progress', label: 'Progress', icon: TrendingUp },
  { to: '/goals', label: 'Goals', icon: Target },
  { to: '/profile', label: 'Profile', icon: User },
];

export function Navbar() {
  const location = useLocation();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const handleLogout = async () => {
    await authApi.logout();
    dispatch(clearUser());
    navigate('/login', { replace: true });
  };

  return (
    <>
      {/* Desktop Navigation */}
      <nav className="hidden md:flex fixed top-0 left-0 right-0 z-50 h-16 items-center justify-between px-6 bg-card/95 backdrop-blur-md border-b border-border">
        <div className="flex items-center gap-2">
          <div className="w-9 h-9 rounded-xl gradient-primary flex items-center justify-center">
            <Dumbbell className="w-5 h-5 text-primary-foreground" />
          </div>
          <span className="font-semibold text-lg text-foreground">Lovable Fitness</span>
        </div>

        <div className="flex items-center gap-1">
          {navItems.map((item) => {
            const isActive = location.pathname === item.to;
            return (
              <RouterNavLink
                key={item.to}
                to={item.to}
                className={cn(
                  'flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200',
                  isActive
                    ? 'bg-primary text-primary-foreground'
                    : 'text-muted-foreground hover:text-foreground hover:bg-accent'
                )}
              >
                <item.icon className="w-4 h-4" />
                <span>{item.label}</span>
              </RouterNavLink>
            );
          })}
          <button
            onClick={handleLogout}
            className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200 text-muted-foreground hover:text-foreground hover:bg-accent"
          >
            <LogOut className="w-4 h-4" />
            <span>Log out</span>
          </button>
        </div>
      </nav>

      {/* Mobile Navigation */}
      <nav className="md:hidden fixed top-0 left-0 right-0 z-50 h-14 flex items-center justify-between px-4 bg-card/95 backdrop-blur-md border-b border-border">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg gradient-primary flex items-center justify-center">
            <Dumbbell className="w-4 h-4 text-primary-foreground" />
          </div>
          <span className="font-semibold text-foreground">Lovable Fitness</span>
        </div>

        <Button
          variant="ghost"
          size="icon"
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
        >
          {mobileMenuOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
        </Button>
      </nav>

      {/* Mobile Menu Overlay */}
      {mobileMenuOpen && (
        <div className="md:hidden fixed inset-0 z-40 pt-14 bg-background/95 backdrop-blur-sm animate-fade-in">
          <div className="flex flex-col p-4 gap-2">
            {navItems.map((item) => {
              const isActive = location.pathname === item.to;
              return (
                <RouterNavLink
                  key={item.to}
                  to={item.to}
                  onClick={() => setMobileMenuOpen(false)}
                  className={cn(
                    'flex items-center gap-3 px-4 py-3 rounded-xl text-base font-medium transition-all duration-200',
                    isActive
                      ? 'bg-primary text-primary-foreground'
                      : 'text-muted-foreground hover:text-foreground hover:bg-accent'
                  )}
                >
                  <item.icon className="w-5 h-5" />
                  <span>{item.label}</span>
                </RouterNavLink>
              );
            })}
            <button
              onClick={() => { setMobileMenuOpen(false); handleLogout(); }}
              className="flex items-center gap-3 px-4 py-3 rounded-xl text-base font-medium transition-all duration-200 text-muted-foreground hover:text-foreground hover:bg-accent"
            >
              <LogOut className="w-5 h-5" />
              <span>Log out</span>
            </button>
          </div>
        </div>
      )}
    </>
  );
}
