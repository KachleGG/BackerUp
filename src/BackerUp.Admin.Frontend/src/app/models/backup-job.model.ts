export type BackupMethod = 'Full' | 'Incremental' | 'Differential';

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
