# API Client Documentation

This directory contains a reusable axios-based HTTP client for communicating with the BFF (Backend for Frontend) server.

## Quick Start

### Basic Usage

```typescript
import { workoutApi, goalApi } from '@/services/api';

// Get all workouts
const workouts = await workoutApi.getWorkouts();

// Create a new workout
const newWorkout = await workoutApi.createWorkout({
  userId: 'user-123',
  date: new Date(),
  workoutTypeId: 'type-123',
  durationMin: 45,
  notes: 'Great session',
  exercises: [],
});

// Get all goals
const goals = await goalApi.getGoals();

// Update goal progress
await goalApi.updateGoalProgress('goal-123', 10);
```

## Environment Configuration

### .env.local (Development)
```env
VITE_API_BASE_URL=http://localhost:5187
```

### .env.example
Copy this file to create `.env.local` and update values as needed.

## Architecture

### File Structure

```
src/services/api/
├── types.ts          # TypeScript interfaces and types
├── client.ts         # Axios instance configuration
├── requests.ts       # Generic request functions (GET, POST, PUT, etc.)
├── workoutApi.ts     # Workout-specific endpoints
├── goalApi.ts        # Goal-specific endpoints
└── index.ts          # Barrel exports
```

### Core Components

#### 1. **types.ts** - Type Definitions
- `ApiResponse<T>` - Standard API response wrapper
- `ApiErrorWithStatus` - Enhanced error type with status and code
- `PaginatedResponse<T>` - For paginated endpoints
- `ApiRequestConfig` - Request configuration options

#### 2. **client.ts** - Axios Instance
- Configures axios with base URL from environment
- Request interceptor: Adds auth token from localStorage
- Response interceptor: Handles errors globally
- Auto-redirect to login on 401 Unauthorized

#### 3. **requests.ts** - Generic Request Functions
- `apiGet<T>(endpoint, config?)` - GET request
- `apiPost<T, D>(endpoint, data, config?)` - POST request
- `apiPut<T, D>(endpoint, data, config?)` - PUT request
- `apiPatch<T, D>(endpoint, data, config?)` - PATCH request
- `apiDelete<T>(endpoint, config?)` - DELETE request
- `apiGetPaginated<T>(endpoint, pageNumber, pageSize, config?)` - Paginated GET
- `buildEndpoint(...segments)` - Helper to construct endpoint URLs

#### 4. **workoutApi.ts** - Workout Endpoints
Pre-configured API methods for workout operations:
- `getWorkouts(userId?)` - Fetch workouts
- `getWorkout(id)` - Fetch single workout
- `createWorkout(data)` - Create new workout
- `updateWorkout(id, updates)` - Replace workout
- `patchWorkout(id, updates)` - Partial update
- `deleteWorkout(id)` - Delete workout
- `getWorkoutsForDateRange(userId, start, end)` - Date-range query

#### 5. **goalApi.ts** - Goal Endpoints
Pre-configured API methods for goal operations:
- `getGoals(userId?)` - Fetch goals
- `getGoal(id)` - Fetch single goal
- `createGoal(data)` - Create new goal
- `updateGoal(id, updates)` - Replace goal
- `patchGoal(id, updates)` - Partial update
- `deleteGoal(id)` - Delete goal
- `updateGoalProgress(id, value)` - Update progress value
- `getUserGoals(userId)` - User-specific goals

## Usage Examples

### In React Components

```typescript
import { useEffect, useState } from 'react';
import { workoutApi, ApiErrorWithStatus } from '@/services/api';

function WorkoutList() {
  const [workouts, setWorkouts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<ApiErrorWithStatus | null>(null);

  useEffect(() => {
    const fetchWorkouts = async () => {
      try {
        setLoading(true);
        const data = await workoutApi.getWorkouts('user-123');
        setWorkouts(data);
      } catch (err) {
        setError(err as ApiErrorWithStatus);
      } finally {
        setLoading(false);
      }
    };

    fetchWorkouts();
  }, []);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;

  return (
    <ul>
      {workouts.map(w => (
        <li key={w.id}>{w.name || 'Untitled'}</li>
      ))}
    </ul>
  );
}
```

### In Redux Thunks

```typescript
import { createAsyncThunk } from '@reduxjs/toolkit';
import { workoutApi } from '@/services/api';

export const fetchWorkouts = createAsyncThunk(
  'workouts/fetchWorkouts',
  async (userId: string) => {
    return await workoutApi.getWorkouts(userId);
  }
);
```

### With React Query

```typescript
import { useQuery, useMutation } from '@tanstack/react-query';
import { workoutApi } from '@/services/api';

function useWorkouts(userId: string) {
  return useQuery({
    queryKey: ['workouts', userId],
    queryFn: () => workoutApi.getWorkouts(userId),
  });
}

function useCreateWorkout() {
  return useMutation({
    mutationFn: workoutApi.createWorkout,
  });
}
```

## Error Handling

### Global Error Handling

The axios response interceptor handles:
- ❌ 401 Unauthorized → Clears token & redirects to login
- ❌ All errors → Wrapped as `ApiErrorWithStatus` with `.code`, `.message`, `.status`, `.details`

### Per-Request Error Handling

```typescript
try {
  await workoutApi.createWorkout(data);
} catch (error) {
  const err = error as ApiErrorWithStatus;
  console.log('Status:', err.status);
  console.log('Code:', err.code);
  console.log('Message:', err.message);
  console.log('Details:', err.details);
}
```

## Authentication

### HTTP-Only Cookies

The client is configured to use HTTP-only cookies for authentication. This approach provides better security than token-based auth because:

- ✅ Cookies are automatically sent with every request via `withCredentials: true`
- ✅ HTTP-only flag prevents JavaScript access (XSS protection)
- ✅ Secure flag ensures transmission only over HTTPS in production
- ✅ SameSite attribute prevents CSRF attacks

### Setting Auth Cookie

The server sets the HTTP-only cookie on successful login. No client-side action needed:

```typescript
// After login API call, server responds with:
// Set-Cookie: authToken=<token>; HttpOnly; Secure; SameSite=Strict
// Cookies are automatically included in all subsequent requests
```

### Logout

The server should invalidate the cookie:

```typescript
// Server clears the cookie on logout
// Redirect to login on 401 Unauthorized response
window.location.href = '/login';
```

### Backend Configuration

Ensure your BFF sets cookies correctly in the login response:

```csharp
// Example: ASP.NET Core
Response.Cookies.Append(
  "authToken",
  token,
  new CookieOptions
  {
    HttpOnly = true,        // Prevent JavaScript access
    Secure = true,          // HTTPS only in production
    SameSite = SameSiteMode.Strict,
    MaxAge = TimeSpan.FromHours(1) // Or set Expires
  }
);
```

## Adding New API Modules

### Pattern: Create `src/services/api/featureApi.ts`

```typescript
import { apiGet, apiPost, buildEndpoint } from './requests';

const FEATURE_ENDPOINT = '/api/features';

export const featureApi = {
  getFeatures: () => apiGet<Feature[]>(FEATURE_ENDPOINT),
  
  getFeature: (id: string) => 
    apiGet<Feature>(buildEndpoint(FEATURE_ENDPOINT, id)),
  
  createFeature: (data: CreateFeatureInput) => 
    apiPost<Feature>(FEATURE_ENDPOINT, data),
};
```

### Update `src/services/api/index.ts`

```typescript
export { featureApi } from './featureApi';
```

## Configuration Options

### Request Config

```typescript
import { ApiRequestConfig, apiGet } from '@/services/api';

const config: ApiRequestConfig = {
  headers: { 'X-Custom-Header': 'value' },
  params: { filter: 'active' },
  timeout: 5000,
};

const result = await apiGet('/api/data', config);
```

### Custom Axios Config

Modify `src/services/api/client.ts`:

```typescript
const client = axios.create({
  baseURL,
  timeout: 10000, // ← Change timeout
  headers: {
    'Content-Type': 'application/json',
    'X-App-Version': '1.0', // ← Add custom headers
  },
});
```

## Best Practices

1. **Use Domain-Specific APIs**: Create `xyzApi` modules instead of raw `apiGet/Post` calls
2. **Handle Errors**: Always wrap API calls in try-catch or use promise .catch()
3. **Type Safety**: Leverage TypeScript generics (e.g., `apiGet<UserData>(...)`)
4. **Loading States**: Use loading/error states in components
5. **Reuse Selectors**: For Redux, fetch data once and reuse via selectors
6. **Environment Variables**: Always use env vars for base URLs, never hardcode

## Troubleshooting

### "Cannot GET /api/workouts"
- Verify `VITE_API_BASE_URL` matches your backend server
- Check that backend is running and listening on that port

### 401 Unauthorized
- Token may be expired → refresh token logic needed
- Token may not be stored in localStorage
- Backend may require different Authorization header format

### CORS Errors
- Backend should have CORS enabled
- Check BFF configuration in launchSettings.json

### Type Errors
- Ensure response types match your domain types
- Use `as const` for const objects if needed

## Dependencies

- `axios` - HTTP client library
- `react` - UI framework
- `typescript` - Type safety

## See Also

- Backend BFF Documentation
- Redux Store Setup (src/store/)
- React Router Setup (src/pages/)
