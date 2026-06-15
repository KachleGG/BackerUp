import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs';
import { Client, ClientSummary, CreateClientRequest, UpdateClientRequest } from '../models/client.model';
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

  getSummary(): Observable<ClientSummary> {
    return this.http.get<any>(`${this.url}/summary`).pipe(
      map(summary => ({
        total: summary.total ?? summary.Total ?? 0,
        approved: summary.approved ?? summary.Approved ?? 0,
        pendingApproval: summary.pendingApproval ?? summary.PendingApproval ?? 0,
        online: summary.online ?? summary.Online ?? 0,
        offline: summary.offline ?? summary.Offline ?? 0,
      })),
    );
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
