import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Dumbbell, Trophy, Clock, Share2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import apiClient from '@/services/api/client';
import { toast } from '@/hooks/use-toast';
import { format, parseISO } from 'date-fns';

interface SharedProgressDto {
  title: string;
  description: string | null;
  createdAt: string;
  expiration: string | null;
}

const SharedProgressPage = () => {
  const { publicUrlKey } = useParams<{ publicUrlKey: string }>();
  const [data, setData] = useState<SharedProgressDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    if (!publicUrlKey) return;

    apiClient
      .get<SharedProgressDto>(`/training-service/api/v1/achievements/shared/${publicUrlKey}`)
      .then((res) => setData(res.data))
      .catch((err) => {
        if (err?.response?.status === 404) {
          setNotFound(true);
        } else {
          toast({ title: 'Failed to load shared progress', variant: 'destructive' });
          setNotFound(true);
        }
      })
      .finally(() => setIsLoading(false));
  }, [publicUrlKey]);

  if (isLoading) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center gap-4 bg-background">
        <div className="w-14 h-14 rounded-2xl gradient-primary flex items-center justify-center animate-pulse">
          <Dumbbell className="w-7 h-7 text-primary-foreground" />
        </div>
        <p className="text-muted-foreground text-sm">Loading shared progress...</p>
      </div>
    );
  }

  if (notFound || !data) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center gap-6 bg-background px-4">
        <div className="w-20 h-20 rounded-2xl gradient-primary flex items-center justify-center">
          <Dumbbell className="w-10 h-10 text-primary-foreground" />
        </div>
        <div className="text-center space-y-2">
          <h1 className="text-2xl font-bold text-foreground">Link Expired or Not Found</h1>
          <p className="text-muted-foreground">
            This shared progress link is no longer valid.
          </p>
        </div>
        <Button asChild>
          <Link to="/login">Sign in to track your own progress</Link>
        </Button>
      </div>
    );
  }

  const sharedAt = format(parseISO(data.createdAt), 'MMMM d, yyyy');
  const expiresAt = data.expiration ? format(parseISO(data.expiration), 'MMMM d, yyyy') : null;

  return (
    <div className="min-h-screen bg-background flex flex-col items-center justify-center px-4 py-12">
      <div className="w-full max-w-md space-y-8">
        {/* Header */}
        <div className="text-center space-y-4">
          <div className="w-20 h-20 mx-auto rounded-2xl gradient-primary flex items-center justify-center">
            <Dumbbell className="w-10 h-10 text-primary-foreground" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-foreground">Fitness Journey Share</h1>
            <p className="text-muted-foreground text-sm mt-1">Someone shared their progress with you</p>
          </div>
        </div>

        {/* Share card */}
        <div className="relative overflow-hidden rounded-2xl gradient-hero p-6 text-primary-foreground space-y-4">
          <div className="relative z-10">
            <div className="flex items-center gap-2 mb-3">
              <Share2 className="w-5 h-5" />
              <h2 className="font-semibold text-lg">{data.title}</h2>
            </div>

            {data.description && (
              <p className="text-primary-foreground/80 text-sm mb-4">{data.description}</p>
            )}

            <div className="space-y-2 text-primary-foreground/90 text-sm">
              <div className="flex items-center gap-2">
                <Clock className="w-4 h-4" />
                <span>Shared on {sharedAt}</span>
              </div>
              {expiresAt && (
                <div className="flex items-center gap-2">
                  <Trophy className="w-4 h-4" />
                  <span>Valid until {expiresAt}</span>
                </div>
              )}
            </div>
          </div>

          <div className="absolute -right-10 -top-10 w-40 h-40 bg-primary-foreground/10 rounded-full blur-2xl" />
          <div className="absolute -right-5 -bottom-10 w-32 h-32 bg-primary-foreground/5 rounded-full blur-xl" />
        </div>

        {/* CTA */}
        <div className="text-center space-y-3">
          <p className="text-sm text-muted-foreground">
            Want to track your own fitness journey?
          </p>
          <div className="flex gap-3 justify-center">
            <Button asChild variant="outline">
              <Link to="/login">Sign In</Link>
            </Button>
            <Button asChild>
              <Link to="/register">Get Started</Link>
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SharedProgressPage;
