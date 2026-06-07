export type BackupMethod = 'Full' | 'Incremental' | 'Differential';
export type LogLevel = 'Info' | 'Warning' | 'Error';

export interface RetentionDto {
  count: number;
  size: number;
}

export interface BackupJob {
  id: number;
  method: BackupMethod;
  timing: string;
  createdAt: string;
  sources: string[];
  targets: string[];
  retention: RetentionDto | null;
}

export interface CreateBackupJobRequest {
  method: BackupMethod;
  timing: string;
  sources: string[];
  targets: string[];
  retention: RetentionDto | null;
}

export interface Client {
  id: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  jobIds?: number[];
}

export interface CreateClientRequest {
  name: string;
  isActive: boolean;
  jobIds?: number[];
}

export interface UpdateClientRequest {
  name: string;
  isActive: boolean;
  jobIds?: number[];
}

export interface User {
  id: string;
  username: string;
  createdAt: string;
}

export interface CreateUserRequest {
  username: string;
  password: string;
}

export interface UpdateUserRequest {
  username: string;
  password?: string;
}

export interface JobClient {
  id: number;
  jobId: number;
  clientId: string;
  isActive: boolean;
}

export interface CreateJobClientRequest {
  jobId: number;
  clientId: string;
  isActive: boolean;
}

export interface UpdateJobClientRequest {
  isActive: boolean;
}

export interface Log {
  id: number;
  jobsClientsId: number;
  level: LogLevel;
  description: string;
  createdAt: string;
}

export interface CreateLogRequest {
  jobsClientsId: number;
  level: LogLevel;
  description: string;
}
