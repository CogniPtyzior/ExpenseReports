// Models the user DTOs consumed by the frontend.
export interface User {
  readonly id: string;
  readonly firstName: string;
  readonly lastName: string;
  readonly fullName: string;
  readonly street: string;
  readonly postalCode: string;
  readonly city: string;
  readonly monthlyExpenseQuota: number;
  readonly isActive: boolean;
  readonly canBeAssignedToExpenseReport: boolean;
}

export interface CreateUserRequest {
  readonly firstName: string;
  readonly lastName: string;
  readonly street: string;
  readonly postalCode: string;
  readonly city: string;
  readonly monthlyExpenseQuota: number;
}
