export interface ExerciseType {
  id: string;
  name: string;
  category: string;
}

export interface Exercise {
  id: string;
  /** Client-side display name (resolved from exerciseTypeId). Not sent to the server. */
  name: string;
  /** Required by the server — maps to ExerciseType.Id */
  exerciseTypeId: string;
  sets?: number;
  reps?: number;
  weight?: number;
  duration?: number;
  notes?: string;
}
