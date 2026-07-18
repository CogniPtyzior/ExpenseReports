// Expense reports page orchestrates list loading and monthly report creation.
import { AsyncPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import {
  BehaviorSubject,
  catchError,
  finalize,
  forkJoin,
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
import { UsersApi } from "../../../users/data-access/users-api";
import { User } from "../../../users/models/user.model";
import { ExpenseReportsApi } from "../../data-access/expense-reports-api";
import {
  CreateExpenseReportRequest,
  ExpenseReport,
} from "../../models/expense-report.model";
import { ExpenseReportForm } from "../../ui/expense-report-form/expense-report-form";
import { ExpenseReportsTable } from "../../ui/expense-reports-table/expense-reports-table";

interface ExpenseReportsState {
  readonly reports: readonly ExpenseReport[];
  readonly users: readonly User[];
  readonly error: ApiError | null;
}

@Component({
  imports: [
    AsyncPipe,
    EmptyState,
    ExpenseReportForm,
    ExpenseReportsTable,
    PageHeader,
  ],
  templateUrl: "./expense-reports-page.html",
  styleUrl: "./expense-reports-page.css",
})
export class ExpenseReportsPage {
  private readonly expenseReportsApi = inject(ExpenseReportsApi);
  private readonly usersApi = inject(UsersApi);
  private readonly refreshReports = new BehaviorSubject<void>(undefined);

  readonly loading = signal(false);
  readonly createError = signal<ApiError | null>(null);
  readonly reportsState$ = this.refreshReports.pipe(
    tap(() => this.loading.set(true)),
    switchMap(() =>
      forkJoin({
        reports: this.expenseReportsApi.listReports(),
        users: this.usersApi.listAssignableUsers(),
      }).pipe(
        map((state): ExpenseReportsState => ({ ...state, error: null })),
        catchError((error) =>
          of({ reports: [], users: [], error: mapApiError(error) })
        ),
        finalize(() => this.loading.set(false))
      )
    )
  );

  createReport(request: CreateExpenseReportRequest) {
    this.createError.set(null);
    this.expenseReportsApi
      .createReport(request)
      .pipe(take(1))
      .subscribe({
        next: () => this.refreshReports.next(),
        error: (error) => this.createError.set(mapApiError(error)),
      });
  }
}
