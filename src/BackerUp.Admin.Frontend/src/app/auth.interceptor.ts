import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './services/auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const accessToken = authService.getAccessToken();
  const isAuthRequest = request.url.includes('/api/Auth/');

  const authRequest = accessToken && !isAuthRequest
    ? request.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } })
    : request;

  return next(authRequest).pipe(
    catchError(error => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isAuthRequest) {
        return throwError(() => error);
      }

      const refreshToken = authService.getRefreshToken();
      if (!refreshToken) {
        authService.clearTokens();
        router.navigate(['/login']);
        return throwError(() => error);
      }

      return authService.refresh().pipe(
        switchMap(() => {
          const refreshedToken = authService.getAccessToken();
          const retriedRequest = refreshedToken
            ? request.clone({ setHeaders: { Authorization: `Bearer ${refreshedToken}` } })
            : request;

          return next(retriedRequest);
        }),
        catchError(refreshError => {
          authService.clearTokens();
          router.navigate(['/login']);
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};