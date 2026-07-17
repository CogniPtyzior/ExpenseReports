// Models expense report DTOs exchanged with the backend.
export interface ExpenseReport {
  readonly id: string;
  readonly userId: string;
  readonly assignedUserFullName: string;
  readonly year: number;
  readonly month: number;
  readonly title: string;
  readonly createdAtUtc: string;
}

export interface CreateExpenseReportRequest {
  readonly userId: string;
  readonly year: number;
  readonly month: number;
}
