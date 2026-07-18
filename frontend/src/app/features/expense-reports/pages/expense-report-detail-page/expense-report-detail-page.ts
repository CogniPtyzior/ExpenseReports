// Expense report detail page coordinates report loading and paged expense entry reads.
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
  tap,
} from "rxjs";
import { ApiError } from "../../../../core/errors/api-error";
import { mapApiError } from "../../../../core/errors/api-error.mapper";
import { ExpenseEntriesApi } from "../../../expense-entries/data-access/expense-entries-api";
import { ExpenseEntriesTable } from "../../../expense-entries/ui/expense-entries-table/expense-entries-table";
import {
  PagedResult,
  ExpenseEntry,
} from "../../../expense-entries/models/expense-entry.model";
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
  imports: [AsyncPipe, EmptyState, ExpenseEntriesTable, PageHeader],
  templateUrl: "./expense-report-detail-page.html",
  styleUrl: "./expense-report-detail-page.css",
})
export class ExpenseReportDetailPage {
  private readonly route = inject(ActivatedRoute);
  private readonly expenseReportsApi = inject(ExpenseReportsApi);
  private readonly expenseEntriesApi = inject(ExpenseEntriesApi);
  private readonly requestedPage = new BehaviorSubject(1);
  private readonly reportId$ = this.route.paramMap.pipe(
    map((params) => params.get("id") ?? ""),
    distinctUntilChanged(),
    tap(() => this.requestedPage.next(1))
  );

  readonly loading = signal(false);
  readonly detailState$ = combineLatest([
    this.reportId$,
    this.requestedPage,
  ]).pipe(
    tap(() => this.loading.set(true)),
    switchMap(([reportId, pageNumber]) =>
      forkJoin({
        report: this.expenseReportsApi.getReport(reportId),
        entries: this.expenseEntriesApi.listEntries(reportId, pageNumber),
      }).pipe(
        map((state): ExpenseReportDetailState => ({ ...state, error: null })),
        catchError((error) =>
          of({ report: null, entries: null, error: mapApiError(error) })
        ),
        finalize(() => this.loading.set(false))
      )
    )
  );

  goToPage(pageNumber: number) {
    if (pageNumber < 1) {
      return;
    }

    this.requestedPage.next(pageNumber);
  }
}
