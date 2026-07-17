// Provides HTTP access to expense entry endpoints used by the frontend.
import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { FRONTEND_CONFIG } from '../../../core/config/frontend-config';
import { CreateExpenseEntryRequest, ExpenseEntry, PagedResult } from '../models/expense-entry.model';

@Injectable({ providedIn: 'root' })
export class ExpenseEntriesApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(FRONTEND_CONFIG);

  listEntries(expenseReportId: string, pageNumber: number) {
    const url = `${this.config.apiBaseUrl}/expense-reports/${expenseReportId}/entries`;
    return this.http.get<PagedResult<ExpenseEntry>>(url, { params: { pageNumber } });
  }

  createEntry(expenseReportId: string, request: CreateExpenseEntryRequest) {
    const url = `${this.config.apiBaseUrl}/expense-reports/${expenseReportId}/entries`;
    return this.http.post<ExpenseEntry>(url, request);
  }
}
