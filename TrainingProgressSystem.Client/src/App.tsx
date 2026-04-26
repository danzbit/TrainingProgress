import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import Index from "./pages/Index";
import Workout from "./pages/Workout";
import History from "./pages/History";
import Progress from "./pages/Progress";
import Goals from "./pages/Goals";
import Profile from "./pages/Profile";
import Login from "./pages/Login";
import Register from "./pages/Register";
import NotFound from "./pages/NotFound";
import SharedProgress from "./pages/SharedProgress";
import AiChat from "./pages/AiChat";
import AuthInitializer from "./components/AuthInitializer";
import ProtectedRoute from "./components/ProtectedRoute";
import { useSyncHub } from "./hooks/use-sync-hub";

const queryClient = new QueryClient();

const AppRoutes = () => {
  useSyncHub();
  return (
    <AuthInitializer>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/shared/:publicUrlKey" element={<SharedProgress />} />

        {/* Protected routes – redirect to /login when not authenticated */}
        <Route element={<ProtectedRoute />}>
          <Route path="/" element={<Index />} />
          <Route path="/workout" element={<Workout />} />
          <Route path="/history" element={<History />} />
          <Route path="/progress" element={<Progress />} />
          <Route path="/goals" element={<Goals />} />
          <Route path="/profile" element={<Profile />} />
          <Route path="/ai-coach" element={<AiChat />} />
        </Route>

        {/* ADD ALL CUSTOM ROUTES ABOVE THE CATCH-ALL "*" ROUTE */}
        <Route path="*" element={<NotFound />} />
      </Routes>
    </AuthInitializer>
  );
};

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <Toaster />
      <Sonner />
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
