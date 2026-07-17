// Provides HTTP access to user endpoints used by the frontend.
import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { FRONTEND_CONFIG } from '../../../core/config/frontend-config';
import { CreateUserRequest, User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UsersApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(FRONTEND_CONFIG);

  listUsers() {
    return this.http.get<readonly User[]>(`${this.config.apiBaseUrl}/users`);
  }

  listAssignableUsers() {
    return this.http.get<readonly User[]>(`${this.config.apiBaseUrl}/users/assignable`);
  }

  createUser(request: CreateUserRequest) {
    return this.http.post<User>(`${this.config.apiBaseUrl}/users`, request);
  }
}
