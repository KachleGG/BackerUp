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
