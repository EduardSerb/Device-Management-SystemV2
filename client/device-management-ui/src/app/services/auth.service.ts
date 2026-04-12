import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { LoginResponse } from '../models/device.models';

const TOKEN_KEY = 'dm_token';
const USER_ID_KEY = 'dm_user_id';
const EMAIL_KEY = 'dm_email';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  register(body: {
    email: string;
    password: string;
    fullName: string;
    roleName: string;
    location: string;
  }) {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/register`, body);
  }

  login(email: string, password: string) {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password });
  }

  persistSession(res: LoginResponse) {
    sessionStorage.setItem(TOKEN_KEY, res.token);
    sessionStorage.setItem(USER_ID_KEY, res.userId);
    sessionStorage.setItem(EMAIL_KEY, res.email);
  }

  logout() {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_ID_KEY);
    sessionStorage.removeItem(EMAIL_KEY);
    void this.router.navigateByUrl('/login');
  }

  isLoggedIn(): boolean {
    return !!sessionStorage.getItem(TOKEN_KEY);
  }

  userId(): string | null {
    return sessionStorage.getItem(USER_ID_KEY);
  }
}
