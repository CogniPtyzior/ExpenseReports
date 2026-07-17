// Models expense entry DTOs and pagination contracts consumed by the frontend.
export interface ExpenseEntry {
  readonly id: string;
  readonly expenseReportId: string;
  readonly reportYear: number;
  readonly reportMonth: number;
  readonly expenseDate: string;
  readonly description: string;
  readonly amount: number;
  readonly currency: string;
  readonly merchantName: string;
  readonly street: string;
  readonly postalCode: string;
  readonly city: string;
  readonly createdAtUtc: string;
  readonly updatedAtUtc: string | null;
}

export interface ExpenseEntryAddressRequest {
  readonly merchantName: string;
  readonly street: string;
  readonly postalCode: string;
  readonly city: string;
}

export interface CreateExpenseEntryRequest {
  readonly expenseDate: string;
  readonly description: string;
  readonly amount: number;
  readonly billingAddress: ExpenseEntryAddressRequest;
}

export interface PagedResult<T> {
  readonly items: readonly T[];
  readonly pageNumber: number;
  readonly pageSize: number;
  readonly totalCount: number;
  readonly totalPages: number;
}
