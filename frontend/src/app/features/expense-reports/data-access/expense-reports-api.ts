// Provides HTTP access to expense report endpoints used by the frontend.
import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { FRONTEND_CONFIG } from '../../../core/config/frontend-config';
import { CreateExpenseReportRequest, ExpenseReport } from '../models/expense-report.model';

@Injectable({ providedIn: 'root' })
export class ExpenseReportsApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(FRONTEND_CONFIG);

  listReports() {
    return this.http.get<readonly ExpenseReport[]>(`${this.config.apiBaseUrl}/expense-reports`);
  }

  getReport(id: string) {
    return this.http.get<ExpenseReport>(`${this.config.apiBaseUrl}/expense-reports/${id}`);
  }

  createReport(request: CreateExpenseReportRequest) {
    return this.http.post<ExpenseReport>(`${this.config.apiBaseUrl}/expense-reports`, request);
  }

  deleteReport(id: string) {
    return this.http.delete<void>(`${this.config.apiBaseUrl}/expense-reports/${id}`);
  }
}
