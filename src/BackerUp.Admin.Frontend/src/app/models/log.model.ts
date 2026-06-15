export type LogLevel = 'Info' | 'Warning' | 'Error';

export interface Log {
  id: number;
  jobsClientsId: number | null;
  level: LogLevel;
  description: string;
  createdAt: string;
}

export interface CreateLogRequest {
  jobsClientsId: number | null;
  level: LogLevel;
  description: string;
}
