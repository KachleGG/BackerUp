import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Client, CreateClientRequest, UpdateClientRequest } from '../models/client.model';
import { API_BASE } from '../app.constants';

@Injectable({ providedIn: 'root' })
export class ClientsService {
  private url = `${API_BASE}/api/Clients`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Client[]> {
    return this.http.get<Client[]>(this.url);
  }

  getById(id: string): Observable<Client> {
    return this.http.get<Client>(`${this.url}/${id}`);
  }

  create(request: CreateClientRequest): Observable<Client> {
    // Backend exposes a dedicated register endpoint
    return this.http.post<Client>(`${this.url}/register`, request);
  }

  getPending(): Observable<Client[]> {
    return this.http.get<Client[]>(`${this.url}/pending`);
  }

  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/approve`, {});
  }

  update(id: string, request: UpdateClientRequest): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
