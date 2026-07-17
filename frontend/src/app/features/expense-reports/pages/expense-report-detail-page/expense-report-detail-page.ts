// Expense report detail page prepared for paged entries and expense creation.
import { Component } from '@angular/core';
import { EmptyState } from '../../../../shared/ui/empty-state/empty-state';
import { PageHeader } from '../../../../shared/ui/page-header/page-header';

@Component({
  imports: [EmptyState, PageHeader],
  templateUrl: './expense-report-detail-page.html',
  styleUrl: './expense-report-detail-page.css',
})
export class ExpenseReportDetailPage {}
