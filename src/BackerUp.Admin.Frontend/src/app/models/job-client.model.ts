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
