import { useState, useEffect } from 'react';
import { Layout } from '@/components/Layout';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  User,
  Share2,
  Copy,
  Check,
  Dumbbell,
  Clock,
  Trophy,
  Flame,
} from 'lucide-react';
import { toast } from '@/hooks/use-toast';
import { cn } from '@/lib/utils';
import { analyticsApi } from '@/services/api/analyticsApi';
import type { ProfileAnalyticsDto } from '@/services/api/analyticsApi';
import apiClient from '@/services/api/client';

const ProfilePage = () => {
  const [username, setUsername] = useState('Fitness Enthusiast');
  const [copied, setCopied] = useState(false);
  const [isSharing, setIsSharing] = useState(false);
  const [analytics, setAnalytics] = useState<ProfileAnalyticsDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    analyticsApi
      .getProfileAnalytics()
      .then(setAnalytics)
      .catch(() =>
        toast({ title: 'Failed to load profile analytics', variant: 'destructive' })
      )
      .finally(() => setIsLoading(false));
  }, []);

  const totalWorkouts = analytics?.totalWorkoutsCompleted ?? 0;
  const totalHours = analytics?.totalHoursTrained ?? 0;
  const completedGoals = analytics?.goalsAchieved ?? 0;
  const workoutsThisWeek = analytics?.workoutsThisWeek ?? 0;

  const shareUrl = typeof window !== 'undefined' ? window.location.origin : '';

  const handleCopyLink = async () => {
    setIsSharing(true);
    try {
      const response = await apiClient.post<{ publicUrlKey: string }>(
        '/training-service/api/v1/achievements/share'
      );
      const shareLink = `${shareUrl}/shared/${response.data.publicUrlKey}`;
      const shareText = `🏋️ Check out my fitness journey on Lovable Fitness!\n\n📊 Stats:\n• ${totalWorkouts} total workouts\n• ${totalHours} hours of training\n• ${completedGoals} goals achieved\n\nView my progress: ${shareLink}`;
      await navigator.clipboard.writeText(shareText);
      setCopied(true);
      toast({
        title: 'Copied to clipboard!',
        description: 'Share your achievements with friends.',
      });
      setTimeout(() => setCopied(false), 2000);
    } catch {
      toast({ title: 'Failed to generate share link', variant: 'destructive' });
    } finally {
      setIsSharing(false);
    }
  };

  const achievements = [
    {
      icon: Dumbbell,
      title: 'Workouts Completed',
      value: totalWorkouts,
      color: 'text-primary',
      bg: 'bg-primary/10',
    },
    {
      icon: Clock,
      title: 'Hours Trained',
      value: totalHours,
      color: 'text-success',
      bg: 'bg-success/10',
    },
    {
      icon: Trophy,
      title: 'Goals Achieved',
      value: completedGoals,
      color: 'text-warning',
      bg: 'bg-warning/10',
    },
    {
      icon: Flame,
      title: 'This Week',
      value: workoutsThisWeek,
      color: 'text-destructive',
      bg: 'bg-destructive/10',
    },
  ];

  return (
    <Layout>
      <div className="py-6 max-w-2xl mx-auto space-y-8">
        {isLoading && (
          <div className="text-center text-muted-foreground py-12">Loading profile analytics...</div>
        )}
        {!isLoading && (
          <>
        {/* Profile Header */}
        <div className="text-center space-y-4">
          <div className="w-24 h-24 mx-auto rounded-full gradient-primary flex items-center justify-center">
            <User className="w-12 h-12 text-primary-foreground" />
          </div>

          <div className="space-y-2">
            <Input
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="text-center text-xl font-bold border-none bg-transparent focus-visible:ring-0 focus-visible:ring-offset-0"
              placeholder="Your name"
            />
            <p className="text-muted-foreground">Fitness Journey Tracker</p>
          </div>
        </div>

        {/* Achievements Grid */}
        <div className="grid grid-cols-2 gap-4">
          {achievements.map((achievement) => (
            <div
              key={achievement.title}
              className="workout-card flex flex-col items-center text-center py-6"
            >
              <div
                className={cn(
                  'w-12 h-12 rounded-xl flex items-center justify-center mb-3',
                  achievement.bg
                )}
              >
                <achievement.icon className={cn('w-6 h-6', achievement.color)} />
              </div>
              <p className="text-3xl font-bold text-foreground">
                {achievement.value}
              </p>
              <p className="text-sm text-muted-foreground">{achievement.title}</p>
            </div>
          ))}
        </div>

        {/* Share Section */}
        <div className="workout-card p-6 space-y-4">
          <div className="flex items-center gap-2">
            <Share2 className="w-5 h-5 text-primary" />
            <h3 className="font-semibold text-foreground">Share Your Progress</h3>
          </div>

          <p className="text-sm text-muted-foreground">
            Share your fitness achievements with friends and inspire others on their
            journey!
          </p>

          <div className="flex flex-col sm:flex-row gap-3">
            <Button onClick={handleCopyLink} className="flex-1" disabled={isSharing}>
              {isSharing ? (
                'Generating link...'
              ) : copied ? (
                <>
                  <Check className="w-4 h-4 mr-2" />
                  Copied!
                </>
              ) : (
                <>
                  <Copy className="w-4 h-4 mr-2" />
                  Copy Share Link
                </>
              )}
            </Button>
          </div>
        </div>

        {/* Summary Card */}
        <div className="relative overflow-hidden rounded-2xl gradient-hero p-6 text-primary-foreground">
          <div className="relative z-10">
            <h3 className="text-xl font-bold mb-4">Your Fitness Summary</h3>
            <div className="space-y-2 text-primary-foreground/90">
              <p>
                🎯 You've completed <strong>{totalWorkouts}</strong> workouts
              </p>
              <p>
                ⏱️ Total training time: <strong>{totalHours}</strong> hours
              </p>
              <p>
                🏆 Goals achieved: <strong>{completedGoals}</strong>
              </p>
              <p>
                🔥 This week: <strong>{workoutsThisWeek}</strong> workouts
              </p>
            </div>
          </div>

          <div className="absolute -right-10 -top-10 w-40 h-40 bg-primary-foreground/10 rounded-full blur-2xl" />
          <div className="absolute -right-5 -bottom-10 w-32 h-32 bg-primary-foreground/5 rounded-full blur-xl" />
        </div>
          </>
        )}
      </div>
    </Layout>
  );
};

export default ProfilePage;
