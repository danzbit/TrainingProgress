import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Dumbbell, Eye, EyeOff, UserPlus } from 'lucide-react';
import { toast } from '@/hooks/use-toast';
import { authApi, getApiErrorDescription } from '@/services/api';
import { useAppDispatch } from '@/store/hooks';
import { setUser } from '@/store/authSlice';

const RegisterPage = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!fullName.trim() || !email.trim() || !password.trim() || !confirmPassword.trim()) {
      toast({
        title: 'Missing fields',
        description: 'Please fill in all fields.',
        variant: 'destructive',
      });
      return;
    }

    if (password !== confirmPassword) {
      toast({
        title: 'Passwords do not match',
        description: 'Please make sure both passwords are the same.',
        variant: 'destructive',
      });
      return;
    }

    if (password.length < 8) {
      toast({
        title: 'Password too short',
        description: 'Password must be at least 8 characters.',
        variant: 'destructive',
      });
      return;
    }

    setIsLoading(true);
    try {
      await authApi.register({ userName: fullName, email, password });
      // After registration the server may or may not set cookies automatically.
      // Attempt to hydrate the Redux store in case it does.
      try {
        const user = await authApi.getCurrentUser();
        dispatch(setUser(user));
      } catch {
        // Server didn't issue cookies yet — user will be prompted to log in
      }
      toast({
        title: 'Account created! 🎉',
        description: "Welcome to Training Progress. Let's get started!",
      });
      navigate('/');
    } catch (err) {
      toast({
        title: 'Registration failed',
        description: getApiErrorDescription(err, 'Something went wrong. Please try again.'),
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
            <h1 className="text-3xl font-bold">Start Your Journey</h1>
            <p className="text-primary-foreground/80 text-base leading-relaxed">
              Join thousands of athletes tracking their progress and achieving their fitness goals.
            </p>
          </div>
          <div className="space-y-3 text-left">
            {[
              'Log workouts with detailed exercise tracking',
              'Monitor your progress with visual charts',
              'Set and achieve your fitness goals',
            ].map((feature) => (
              <div key={feature} className="flex items-center gap-3">
                <div className="w-5 h-5 rounded-full bg-primary-foreground/20 flex items-center justify-center flex-shrink-0">
                  <div className="w-2 h-2 rounded-full bg-primary-foreground" />
                </div>
                <span className="text-primary-foreground/90 text-sm">{feature}</span>
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
            <h2 className="text-2xl font-bold text-foreground">Create your account</h2>
            <p className="text-muted-foreground">Start tracking your fitness journey today</p>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-5">
            <div className="space-y-2">
              <Label htmlFor="fullName">Full Name</Label>
              <Input
                id="fullName"
                type="text"
                autoComplete="name"
                placeholder="John Doe"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                autoComplete="email"
                placeholder="you@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="password">Password</Label>
              <div className="relative">
                <Input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="new-password"
                  placeholder="Min. 8 characters"
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

            <div className="space-y-2">
              <Label htmlFor="confirmPassword">Confirm Password</Label>
              <div className="relative">
                <Input
                  id="confirmPassword"
                  type={showConfirm ? 'text' : 'password'}
                  autoComplete="new-password"
                  placeholder="Repeat your password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  className="pr-10"
                />
                <button
                  type="button"
                  aria-label={showConfirm ? 'Hide confirm password' : 'Show confirm password'}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
                  onClick={() => setShowConfirm((v) => !v)}
                  tabIndex={-1}
                >
                  {showConfirm ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
            </div>

            <Button type="submit" className="w-full" size="lg" disabled={isLoading}>
              <UserPlus className="w-4 h-4 mr-2" />
              {isLoading ? 'Creating account...' : 'Create Account'}
            </Button>

            <p className="text-xs text-center text-muted-foreground">
              By creating an account you agree to our{' '}
              <span className="text-primary hover:underline cursor-pointer">Terms of Service</span>{' '}
              and{' '}
              <span className="text-primary hover:underline cursor-pointer">Privacy Policy</span>.
            </p>
          </form>

          {/* Divider */}
          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-border" />
            </div>
            <div className="relative flex justify-center text-xs">
              <span className="bg-background px-3 text-muted-foreground">
                Already have an account?
              </span>
            </div>
          </div>

          <Button asChild variant="outline" className="w-full" size="lg">
            <Link to="/login">Sign in instead</Link>
          </Button>
        </div>
      </div>
    </div>
  );
};

export default RegisterPage;
