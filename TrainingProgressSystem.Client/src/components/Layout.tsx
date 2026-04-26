import type { ReactNode } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { BotMessageSquare } from 'lucide-react';
import { Navbar } from './NavBar';

interface LayoutProps {
  children: ReactNode;
}

export function Layout({ children }: LayoutProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const isOnChat = location.pathname === '/ai-coach';

  return (
    <div className="min-h-screen bg-background">
      <Navbar />
      <main className="pt-14 md:pt-16 pb-8">
        <div className="container max-w-6xl mx-auto px-4">
          {children}
        </div>
      </main>

      {/* Floating AI Coach button — hidden on the chat page itself */}
      {!isOnChat && (
        <button
          onClick={() => navigate('/ai-coach')}
          className="fixed bottom-6 right-6 z-50 flex items-center gap-2 px-4 py-3 rounded-full shadow-lg bg-primary text-primary-foreground hover:opacity-90 transition-opacity"
          aria-label="Open AI Coach"
        >
          <BotMessageSquare className="w-5 h-5" />
          <span className="text-sm font-medium">AI Coach</span>
        </button>
      )}
    </div>
  );
}
