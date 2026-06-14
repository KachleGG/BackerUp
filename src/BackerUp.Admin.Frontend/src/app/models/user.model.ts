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
