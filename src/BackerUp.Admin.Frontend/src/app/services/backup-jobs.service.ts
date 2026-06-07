import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BackupJob, CreateBackupJobRequest } from '../models/models';
import { API_BASE } from '../app.constants';

@Injectable({ providedIn: 'root' })
export class BackupJobsService {
  private url = `${API_BASE}/api/BackupJobs`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<BackupJob[]> {
    return this.http.get<BackupJob[]>(this.url);
  }

  getById(id: number): Observable<BackupJob> {
    return this.http.get<BackupJob>(`${this.url}/${id}`);
  }

  create(request: CreateBackupJobRequest): Observable<BackupJob> {
    return this.http.post<BackupJob>(this.url, request);
  }

  update(id: number, request: CreateBackupJobRequest): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
