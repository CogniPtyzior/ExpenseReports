// Expense report detail page coordinates report loading and expense entry workflows.
import { AsyncPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import {
  BehaviorSubject,
  catchError,
  combineLatest,
  distinctUntilChanged,
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
import { ExpenseEntriesApi } from "../../../expense-entries/data-access/expense-entries-api";
import {
  CreateExpenseEntryRequest,
  ExpenseEntry,
  PagedResult,
} from "../../../expense-entries/models/expense-entry.model";
import { ExpenseEntriesTable } from "../../../expense-entries/ui/expense-entries-table/expense-entries-table";
import { ExpenseEntryForm } from "../../../expense-entries/ui/expense-entry-form/expense-entry-form";
import { EmptyState } from "../../../../shared/ui/empty-state/empty-state";
import { PageHeader } from "../../../../shared/ui/page-header/page-header";
import { ExpenseReportsApi } from "../../data-access/expense-reports-api";
import { ExpenseReport } from "../../models/expense-report.model";

interface ExpenseReportDetailState {
  readonly report: ExpenseReport | null;
  readonly entries: PagedResult<ExpenseEntry> | null;
  readonly error: ApiError | null;
}

@Component({
  imports: [
    AsyncPipe,
    EmptyState,
    ExpenseEntriesTable,
    ExpenseEntryForm,
    PageHeader,
  ],
  templateUrl: "./expense-report-detail-page.html",
  styleUrl: "./expense-report-detail-page.css",
})
export class ExpenseReportDetailPage {
  private readonly route = inject(ActivatedRoute);
  private readonly expenseReportsApi = inject(ExpenseReportsApi);
  private readonly expenseEntriesApi = inject(ExpenseEntriesApi);
  private readonly requestedPage = new BehaviorSubject(1);
  private readonly entriesRefresh = new BehaviorSubject<void>(undefined);
  private readonly reportId$ = this.route.paramMap.pipe(
    map((params) => params.get("id") ?? ""),
    distinctUntilChanged(),
    tap((reportId) => {
      this.currentReportId.set(reportId);
      this.requestedPage.next(1);
    }),
  );

  readonly loading = signal(false);
  readonly createError = signal<ApiError | null>(null);
  readonly currentReportId = signal("");
  readonly detailState$ = combineLatest([
    this.reportId$,
    this.requestedPage,
    this.entriesRefresh,
  ]).pipe(
    tap(() => this.loading.set(true)),
    switchMap(([reportId, pageNumber]) =>
      forkJoin({
        report: this.expenseReportsApi.getReport(reportId),
        entries: this.expenseEntriesApi.listEntries(reportId, pageNumber),
      }).pipe(
        map((state): ExpenseReportDetailState => ({ ...state, error: null })),
        catchError((error) =>
          of({ report: null, entries: null, error: mapApiError(error) }),
        ),
        finalize(() => this.loading.set(false)),
      ),
    ),
  );

  goToPage(pageNumber: number) {
    if (pageNumber < 1) {
      return;
    }

    this.requestedPage.next(pageNumber);
  }

  createEntry(request: CreateExpenseEntryRequest) {
    this.createError.set(null);
    this.expenseEntriesApi
      .createEntry(this.currentReportId(), request)
      .pipe(take(1))
      .subscribe({
        next: () => {
          if (this.requestedPage.value === 1) {
            this.entriesRefresh.next();
            return;
          }

          this.requestedPage.next(1);
        },
        error: (error) => this.createError.set(mapApiError(error)),
      });
  }
}
