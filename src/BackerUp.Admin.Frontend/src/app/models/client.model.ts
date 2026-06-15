export interface Client {
  id: string;
  name: string;
  isActive: boolean;
  isApproved?: boolean;
  isOnline?: boolean;
  lastHealthCheck?: string | null;
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

export interface ClientSummary {
  total: number;
  approved: number;
  pendingApproval: number;
  online: number;
  offline: number;
}
