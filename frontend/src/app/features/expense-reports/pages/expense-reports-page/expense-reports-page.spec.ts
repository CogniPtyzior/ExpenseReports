import { HttpErrorResponse } from "@angular/common/http";
import { provideLocationMocks } from "@angular/common/testing";
import { TestBed } from "@angular/core/testing";
import { provideRouter } from "@angular/router";
import { of, throwError } from "rxjs";
import { UsersApi } from "../../../users/data-access/users-api";
import { User } from "../../../users/models/user.model";
import { ExpenseReportsApi } from "../../data-access/expense-reports-api";
import {
  CreateExpenseReportRequest,
  ExpenseReport,
} from "../../models/expense-report.model";
import { ExpenseReportsPage } from "./expense-reports-page";

const user: User = {
  id: "user-1",
  firstName: "Ada",
  lastName: "Lovelace",
  fullName: "Ada Lovelace",
  street: "1 rue du Test",
  postalCode: "75001",
  city: "Paris",
  monthlyExpenseQuota: 5,
  isActive: true,
  canBeAssignedToExpenseReport: true,
};

const report: ExpenseReport = {
  id: "report-1",
  userId: "user-1",
  assignedUserFullName: "Ada Lovelace",
  year: 2025,
  month: 10,
  title: "Ada Lovelace - Octobre 2025",
  createdAtUtc: "2025-10-15T12:00:00Z",
};

class ExpenseReportsApiStub {
  readonly listReports = vi.fn(() => of([report]));
  readonly getReport = vi.fn(() => of(report));
  readonly createReport = vi.fn(() => of(report));
  readonly deleteReport = vi.fn(() => of(undefined));
}

class UsersApiStub {
  readonly listUsers = vi.fn(() => of([user]));
  readonly listAssignableUsers = vi.fn(() => of([user]));
  readonly createUser = vi.fn(() => of(user));
}

describe("ExpenseReportsPage", () => {
  let expenseReportsApi: ExpenseReportsApiStub;
  let usersApi: UsersApiStub;

  beforeEach(async () => {
    expenseReportsApi = new ExpenseReportsApiStub();
    usersApi = new UsersApiStub();
    await TestBed.configureTestingModule({
      imports: [ExpenseReportsPage],
      providers: [
        provideRouter([]),
        provideLocationMocks(),
        { provide: ExpenseReportsApi, useValue: expenseReportsApi },
        { provide: UsersApi, useValue: usersApi },
      ],
    }).compileComponents();
  });

  it("renders reports and assignable users from the API", () => {
    const fixture = TestBed.createComponent(ExpenseReportsPage);

    fixture.detectChanges();

    expect(expenseReportsApi.listReports).toHaveBeenCalledTimes(1);
    expect(usersApi.listAssignableUsers).toHaveBeenCalledTimes(1);
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "Ada Lovelace - Octobre 2025"
    );
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "Ada Lovelace"
    );
  });

  it("refreshes reports after a report is created", () => {
    const fixture = TestBed.createComponent(ExpenseReportsPage);
    const request: CreateExpenseReportRequest = {
      userId: "user-1",
      year: 2025,
      month: 10,
    };

    fixture.detectChanges();
    fixture.componentInstance.createReport(request);
    fixture.detectChanges();

    expect(expenseReportsApi.createReport).toHaveBeenCalledWith(request);
    expect(expenseReportsApi.listReports).toHaveBeenCalledTimes(2);
  });

  it("displays the backend duplicate report conflict", () => {
    expenseReportsApi.createReport.mockReturnValueOnce(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: {
              code: "expense_report.already_exists",
              message: "Une note existe déjà pour cet utilisateur et ce mois.",
            },
          })
      )
    );
    const fixture = TestBed.createComponent(ExpenseReportsPage);

    fixture.detectChanges();
    fixture.componentInstance.createReport({
      userId: "user-1",
      year: 2025,
      month: 10,
    });
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "Une note existe déjà pour cet utilisateur et ce mois."
    );
  });
});
