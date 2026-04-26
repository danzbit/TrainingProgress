import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Send, Bot, User, Loader2, Trash2, ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { ScrollArea } from '@/components/ui/scroll-area';
import { cn } from '@/lib/utils';
import { aiChatApi, type ChatMessageDto } from '@/services/api';
import { toast } from '@/hooks/use-toast';

interface Message {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  isStreaming?: boolean;
}

const SUGGESTED_PROMPTS = [
  'Create a 4-week beginner strength training plan',
  'What should I eat before a morning workout?',
  'How can I improve my running endurance?',
  'Suggest a quick 20-minute HIIT workout',
];

export default function AiChat() {
  const navigate = useNavigate();
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [isStreaming, setIsStreaming] = useState(false);
  const [isLoadingHistory, setIsLoadingHistory] = useState(true);
  const abortRef = useRef<AbortController | null>(null);
  const bottomRef = useRef<HTMLDivElement>(null);

  // Load history on mount
  useEffect(() => {
    aiChatApi
      .getHistory()
      .then((history: ChatMessageDto[]) => {
        setMessages(
          history.map((m) => ({ id: m.id, role: m.role, content: m.content }))
        );
      })
      .catch(() => {
        // First visit – no history yet, silently ignore
      })
      .finally(() => setIsLoadingHistory(false));
  }, []);

  // Auto-scroll on new messages
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const sendMessage = (text: string) => {
    const trimmed = text.trim();
    if (!trimmed || isStreaming) return;

    const userMsg: Message = {
      id: crypto.randomUUID(),
      role: 'user',
      content: trimmed,
    };
    const assistantId = crypto.randomUUID();
    const assistantMsg: Message = {
      id: assistantId,
      role: 'assistant',
      content: '',
      isStreaming: true,
    };

    setMessages((prev) => [...prev, userMsg, assistantMsg]);
    setInput('');
    setIsStreaming(true);

    const controller = new AbortController();
    abortRef.current = controller;

    aiChatApi.streamMessage(
      trimmed,
      (chunk) => {
        setMessages((prev) =>
          prev.map((m) =>
            m.id === assistantId ? { ...m, content: m.content + chunk } : m
          )
        );
      },
      () => {
        setMessages((prev) =>
          prev.map((m) =>
            m.id === assistantId ? { ...m, isStreaming: false } : m
          )
        );
        setIsStreaming(false);
      },
      (err) => {
        toast({ title: 'AI error', description: err, variant: 'destructive' });
        setMessages((prev) =>
          prev.map((m) =>
            m.id === assistantId
              ? { ...m, content: 'Sorry, something went wrong. Please try again.', isStreaming: false }
              : m
          )
        );
        setIsStreaming(false);
      },
      controller.signal
    );
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      sendMessage(input);
    }
  };

  const handleClear = async () => {
    abortRef.current?.abort();
    setIsStreaming(false);
    try {
      await aiChatApi.clearHistory();
      setMessages([]);
    } catch {
      toast({ title: 'Failed to clear history', variant: 'destructive' });
    }
  };

  return (
    <div className="flex flex-col h-[calc(100vh-4rem)] max-w-3xl mx-auto px-4 py-6 gap-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="icon" onClick={() => navigate('/')} aria-label="Back to dashboard">
            <ArrowLeft className="w-5 h-5" />
          </Button>
          <div className="w-10 h-10 rounded-xl gradient-primary flex items-center justify-center">
            <Bot className="w-5 h-5 text-primary-foreground" />
          </div>
          <div>
            <h1 className="text-xl font-semibold text-foreground">AI Fitness Coach</h1>
            <p className="text-sm text-muted-foreground">Powered by Llama 3 via Groq</p>
          </div>
        </div>
        {messages.length > 0 && (
          <Button variant="ghost" size="sm" onClick={handleClear} disabled={isStreaming}>
            <Trash2 className="w-4 h-4 mr-1" />
            Clear
          </Button>
        )}
      </div>

      {/* Messages */}
      <ScrollArea className="flex-1 rounded-xl border border-border bg-card/50 p-4">
        {isLoadingHistory ? (
          <div className="flex items-center justify-center h-32 text-muted-foreground">
            <Loader2 className="w-5 h-5 animate-spin mr-2" />
            Loading history…
          </div>
        ) : messages.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-48 gap-4 text-center">
            <Bot className="w-12 h-12 text-muted-foreground opacity-50" />
            <p className="text-muted-foreground text-sm max-w-xs">
              Ask me anything about workouts, nutrition, and training plans
            </p>
            <div className="flex flex-wrap gap-2 justify-center mt-2">
              {SUGGESTED_PROMPTS.map((prompt) => (
                <button
                  key={prompt}
                  onClick={() => sendMessage(prompt)}
                  className="text-xs px-3 py-1.5 rounded-full border border-border bg-card hover:bg-accent transition-colors text-foreground"
                >
                  {prompt}
                </button>
              ))}
            </div>
          </div>
        ) : (
          <div className="flex flex-col gap-4">
            {messages.map((msg) => (
              <div
                key={msg.id}
                className={cn(
                  'flex gap-3 items-start',
                  msg.role === 'user' ? 'flex-row-reverse' : 'flex-row'
                )}
              >
                {/* Avatar */}
                <div
                  className={cn(
                    'shrink-0 w-8 h-8 rounded-full flex items-center justify-center',
                    msg.role === 'user'
                      ? 'bg-primary text-primary-foreground'
                      : 'bg-muted text-foreground'
                  )}
                >
                  {msg.role === 'user' ? (
                    <User className="w-4 h-4" />
                  ) : (
                    <Bot className="w-4 h-4" />
                  )}
                </div>

                {/* Bubble */}
                <div
                  className={cn(
                    'max-w-[80%] rounded-2xl px-4 py-3 text-sm leading-relaxed whitespace-pre-wrap',
                    msg.role === 'user'
                      ? 'bg-primary text-primary-foreground rounded-tr-sm'
                      : 'bg-muted text-foreground rounded-tl-sm'
                  )}
                >
                  {msg.content}
                  {msg.isStreaming && (
                    <span className="inline-block ml-1 animate-pulse text-muted-foreground">▋</span>
                  )}
                </div>
              </div>
            ))}
            <div ref={bottomRef} />
          </div>
        )}
      </ScrollArea>

      {/* Input */}
      <div className="flex gap-2 items-end">
        <Textarea
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Ask your AI fitness coach… (Enter to send, Shift+Enter for newline)"
          rows={2}
          disabled={isStreaming}
          className="resize-none flex-1"
        />
        <Button
          onClick={() => sendMessage(input)}
          disabled={!input.trim() || isStreaming}
          size="icon"
          className="h-[4.5rem] w-12 shrink-0"
        >
          {isStreaming ? (
            <Loader2 className="w-4 h-4 animate-spin" />
          ) : (
            <Send className="w-4 h-4" />
          )}
        </Button>
      </div>
    </div>
  );
}
