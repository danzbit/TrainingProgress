import { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Dumbbell, Eye, EyeOff, LogIn } from 'lucide-react';
import { toast } from '@/hooks/use-toast';
import { authApi, getApiErrorDescription } from '@/services/api';
import { useAppDispatch } from '@/store/hooks';
import { setUser } from '@/store/authSlice';

const LoginPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const dispatch = useAppDispatch();

  // Redirect back to the page the user tried to visit before being sent here
  const from = (location.state as { from?: Location })?.from?.pathname ?? '/';

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!email.trim() || !password.trim()) {
      toast({
        title: 'Missing fields',
        description: 'Please fill in all fields.',
        variant: 'destructive',
      });
      return;
    }

    setIsLoading(true);
    try {
      await authApi.login({ userNameOrEmail: email, password });
      // Populate Redux state with the authenticated user
      const user = await authApi.getCurrentUser();
      dispatch(setUser(user));
      toast({
        title: 'Welcome back! 👋',
        description: 'You have been logged in successfully.',
      });
      navigate(from, { replace: true });
    } catch (err) {
      toast({
        title: 'Sign in failed',
        description: getApiErrorDescription(err, 'Invalid credentials. Please try again.'),
        variant: 'destructive',
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-background flex">
      {/* Left decorative panel */}
      <div className="hidden lg:flex lg:w-1/2 xl:w-2/5 gradient-hero flex-col items-center justify-center p-12 text-primary-foreground relative overflow-hidden">
        <div className="relative z-10 text-center space-y-6 max-w-sm">
          <div className="w-20 h-20 mx-auto rounded-2xl bg-primary-foreground/20 backdrop-blur-sm flex items-center justify-center">
            <Dumbbell className="w-10 h-10 text-primary-foreground" />
          </div>
          <div className="space-y-3">
            <h1 className="text-3xl font-bold">Training Progress</h1>
            <p className="text-primary-foreground/80 text-base leading-relaxed">
              Track every rep, celebrate every milestone, and crush your fitness goals.
            </p>
          </div>
          <div className="grid grid-cols-3 gap-4 pt-4">
            {[
              { value: '10K+', label: 'Workouts' },
              { value: '500+', label: 'Athletes' },
              { value: '98%', label: 'Satisfaction' },
            ].map((stat) => (
              <div key={stat.label} className="text-center">
                <p className="text-2xl font-bold">{stat.value}</p>
                <p className="text-primary-foreground/70 text-xs mt-1">{stat.label}</p>
              </div>
            ))}
          </div>
        </div>

        {/* Decorative blobs */}
        <div className="absolute -top-16 -left-16 w-64 h-64 bg-primary-foreground/10 rounded-full blur-3xl" />
        <div className="absolute -bottom-16 -right-16 w-72 h-72 bg-primary-foreground/10 rounded-full blur-3xl" />
      </div>

      {/* Right form panel */}
      <div className="flex-1 flex items-center justify-center p-6 md:p-12">
        <div className="w-full max-w-md space-y-8">
          {/* Mobile logo */}
          <div className="flex items-center gap-3 lg:hidden">
            <div className="w-10 h-10 rounded-xl gradient-primary flex items-center justify-center">
              <Dumbbell className="w-5 h-5 text-primary-foreground" />
            </div>
            <span className="text-lg font-bold text-foreground">Training Progress</span>
          </div>

          {/* Heading */}
          <div className="space-y-2">
            <h2 className="text-2xl font-bold text-foreground">Welcome back</h2>
            <p className="text-muted-foreground">Sign in to your account to continue</p>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-5">
            <div className="space-y-2">
              <Label htmlFor="userNameOrEmail">Email or User Name</Label>
              <Input
                id="userNameOrEmail"
                type="text"
                autoComplete="username"
                placeholder="you@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <Label htmlFor="password">Password</Label>
                <button
                  type="button"
                  className="text-xs text-primary hover:underline"
                  tabIndex={-1}
                >
                  Forgot password?
                </button>
              </div>
              <div className="relative">
                <Input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password"
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="pr-10"
                />
                <button
                  type="button"
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
                  onClick={() => setShowPassword((v) => !v)}
                  tabIndex={-1}
                >
                  {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
            </div>

            <Button type="submit" className="w-full" size="lg" disabled={isLoading}>
              <LogIn className="w-4 h-4 mr-2" />
              {isLoading ? 'Signing in...' : 'Sign In'}
            </Button>
          </form>

          {/* Divider */}
          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-border" />
            </div>
            <div className="relative flex justify-center text-xs">
              <span className="bg-background px-3 text-muted-foreground">
                Don't have an account?
              </span>
            </div>
          </div>

          <Button asChild variant="outline" className="w-full" size="lg">
            <Link to="/register">Create an account</Link>
          </Button>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
