// Users page orchestrates API loading, creation and UI state for the focused scope.
import { AsyncPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import {
  BehaviorSubject,
  catchError,
  finalize,
  map,
  of,
  switchMap,
  take,
  tap,
} from "rxjs";
import { ApiError } from "../../../../core/errors/api-error";
import { mapApiError } from "../../../../core/errors/api-error.mapper";
import { EmptyState } from "../../../../shared/ui/empty-state/empty-state";
import { PageHeader } from "../../../../shared/ui/page-header/page-header";
import { UsersApi } from "../../data-access/users-api";
import { CreateUserRequest, User } from "../../models/user.model";
import { UserForm } from "../../ui/user-form/user-form";
import { UsersTable } from "../../ui/users-table/users-table";

interface UsersState {
  readonly users: readonly User[];
  readonly error: ApiError | null;
}

@Component({
  imports: [AsyncPipe, EmptyState, PageHeader, UserForm, UsersTable],
  templateUrl: "./users-page.html",
  styleUrl: "./users-page.css",
})
export class UsersPage {
  private readonly usersApi = inject(UsersApi);
  private readonly refreshUsers = new BehaviorSubject<void>(undefined);

  readonly loading = signal(false);
  readonly createError = signal<ApiError | null>(null);
  readonly usersState$ = this.refreshUsers.pipe(
    tap(() => this.loading.set(true)),
    switchMap(() =>
      this.usersApi.listUsers().pipe(
        map((users): UsersState => ({ users, error: null })),
        catchError((error) => of({ users: [], error: mapApiError(error) })),
        finalize(() => this.loading.set(false))
      )
    )
  );

  createUser(request: CreateUserRequest) {
    this.createError.set(null);
    this.usersApi
      .createUser(request)
      .pipe(take(1))
      .subscribe({
        next: () => this.refreshUsers.next(),
        error: (error) => this.createError.set(mapApiError(error)),
      });
  }
}
