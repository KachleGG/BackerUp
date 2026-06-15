import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, tap } from 'rxjs';
import { API_BASE } from '../app.constants';
import { AuthResponse, LoginRequest } from '../models/auth.model';

const ACCESS_TOKEN_KEY = 'backerup.accessToken';
const REFRESH_TOKEN_KEY = 'backerup.refreshToken';
const USERNAME_KEY = 'backerup.username';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private url = `${API_BASE}/api/Auth`;

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<void> {
    return this.http.post<AuthResponse>(`${this.url}/login`, request).pipe(
      tap(response => {
        this.storeTokens(response);
        localStorage.setItem(USERNAME_KEY, request.username);
      }),
      map(() => void 0),
    );
  }

  refresh(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('Missing refresh token');
    }

    return this.http.post<AuthResponse>(`${this.url}/refresh`, { refreshToken }).pipe(
      tap(response => this.storeTokens(response)),
    );
  }

  logout(): Observable<void> {
    const refreshToken = this.getRefreshToken();
    this.clearTokens();

    if (!refreshToken) {
      return new Observable(subscriber => {
        subscriber.next();
        subscriber.complete();
      });
    }

    return this.http.post<void>(`${this.url}/logout`, { refreshToken });
  }

  setSession(response: AuthResponse): void {
    this.storeTokens(response);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  getUsername(): string | null {
    return localStorage.getItem(USERNAME_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  clearTokens(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USERNAME_KEY);
  }

  private storeTokens(response: AuthResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
  }
}