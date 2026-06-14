import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Log, CreateLogRequest, LogLevel } from '../models/log.model';
import { API_BASE } from '../app.constants';

@Injectable({ providedIn: 'root' })
export class LogsService {
  private url = `${API_BASE}/api/Logs`;

  constructor(private http: HttpClient) {}

  getAll(jobsClientsId?: number, level?: LogLevel): Observable<Log[]> {
    let params: Record<string, string> = {};
    if (jobsClientsId != null) params['jobsClientsId'] = String(jobsClientsId);
    if (level != null) params['level'] = level;
    return this.http.get<Log[]>(this.url, { params });
  }

  getById(id: number): Observable<Log> {
    return this.http.get<Log>(`${this.url}/${id}`);
  }

  create(request: CreateLogRequest): Observable<Log> {
    return this.http.post<Log>(this.url, request);
  }
}
