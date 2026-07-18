import { HttpErrorResponse } from "@angular/common/http";
import { convertToParamMap, ActivatedRoute, Router } from "@angular/router";
import { BehaviorSubject, of, throwError } from "rxjs";
import { TestBed } from "@angular/core/testing";
import {
  defaultFrontendConfig,
  FRONTEND_CONFIG,
} from "../../../../core/config/frontend-config";
import { ExpenseEntriesApi } from "../../../expense-entries/data-access/expense-entries-api";
import {
  CreateExpenseEntryRequest,
  ExpenseEntry,
  PagedResult,
} from "../../../expense-entries/models/expense-entry.model";
import { ExpenseReportsApi } from "../../data-access/expense-reports-api";
import { ExpenseReport } from "../../models/expense-report.model";
import { ExpenseReportDetailPage } from "./expense-report-detail-page";

const report: ExpenseReport = {
  id: "report-1",
  userId: "user-1",
  assignedUserFullName: "Marc Assin",
  year: 2025,
  month: 11,
  title: "Marc Assin - Novembre 2025",
  createdAtUtc: "2026-07-18T05:00:00Z",
};

const entry: ExpenseEntry = {
  id: "entry-1",
  expenseReportId: "report-1",
  reportYear: 2025,
  reportMonth: 11,
  expenseDate: "2025-11-03",
  description: "Train",
  amount: 86,
  currency: "EUR",
  merchantName: "SNCF Connect",
  street: "1 rue du Rail",
  postalCode: "69002",
  city: "Lyon",
  createdAtUtc: "2026-07-18T05:00:00Z",
  updatedAtUtc: null,
};

const createRequest: CreateExpenseEntryRequest = {
  expenseDate: "2025-11-15",
  description: "Taxi",
  amount: 34,
  billingAddress: {
    merchantName: "Taxi Lyon",
    street: "24 Avenue des Frais",
    postalCode: "69002",
    city: "Lyon",
  },
};

function pagedEntries(pageNumber: number): PagedResult<ExpenseEntry> {
  return {
    items: [entry],
    pageNumber,
    pageSize: 5,
    totalCount: 6,
    totalPages: 2,
  };
}

class ExpenseReportsApiStub {
  readonly getReport = vi.fn(() => of(report));
  readonly listReports = vi.fn(() => of([report]));
  readonly createReport = vi.fn(() => of(report));
  readonly deleteReport = vi.fn(() => of(undefined));
}

class ExpenseEntriesApiStub {
  readonly listEntries = vi.fn((_reportId: string, pageNumber: number) =>
    of(pagedEntries(pageNumber)),
  );
  readonly createEntry = vi.fn(() => of(entry));
}

class RouterStub {
  readonly navigate = vi.fn(() => Promise.resolve(true));
}

describe("ExpenseReportDetailPage", () => {
  let expenseReportsApi: ExpenseReportsApiStub;
  let expenseEntriesApi: ExpenseEntriesApiStub;
  let router: RouterStub;
  let paramMap: BehaviorSubject<ReturnType<typeof convertToParamMap>>;

  beforeEach(async () => {
    expenseReportsApi = new ExpenseReportsApiStub();
    expenseEntriesApi = new ExpenseEntriesApiStub();
    router = new RouterStub();
    paramMap = new BehaviorSubject(convertToParamMap({ id: "report-1" }));

    await TestBed.configureTestingModule({
      imports: [ExpenseReportDetailPage],
      providers: [
        { provide: ExpenseReportsApi, useValue: expenseReportsApi },
        { provide: ExpenseEntriesApi, useValue: expenseEntriesApi },
        { provide: ActivatedRoute, useValue: { paramMap } },
        { provide: Router, useValue: router },
        { provide: FRONTEND_CONFIG, useValue: defaultFrontendConfig },
      ],
    }).compileComponents();
  });

  it("loads the report and first expense page from the route id", () => {
    const fixture = TestBed.createComponent(ExpenseReportDetailPage);

    fixture.detectChanges();

    expect(expenseReportsApi.getReport).toHaveBeenCalledWith("report-1");
    expect(expenseEntriesApi.listEntries).toHaveBeenCalledWith("report-1", 1);
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "Marc Assin - Novembre 2025",
    );
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "Train",
    );
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "Page 1 / 2",
    );
  });

  it("loads the next page when pagination advances", () => {
    const fixture = TestBed.createComponent(ExpenseReportDetailPage);

    fixture.detectChanges();
    fixture.componentInstance.goToPage(2);
    fixture.detectChanges();

    expect(expenseEntriesApi.listEntries).toHaveBeenLastCalledWith(
      "report-1",
      2,
    );
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "Page 2 / 2",
    );
  });

  it("refreshes the first page after a successful expense creation", () => {
    const fixture = TestBed.createComponent(ExpenseReportDetailPage);

    fixture.detectChanges();
    fixture.componentInstance.goToPage(2);
    fixture.detectChanges();
    fixture.componentInstance.createEntry(createRequest);
    fixture.detectChanges();

    expect(expenseEntriesApi.createEntry).toHaveBeenCalledWith(
      "report-1",
      createRequest,
    );
    expect(expenseEntriesApi.listEntries).toHaveBeenLastCalledWith(
      "report-1",
      1,
    );
  });

  it("does not delete the report when confirmation is cancelled", () => {
    vi.spyOn(window, "confirm").mockReturnValueOnce(false);
    const fixture = TestBed.createComponent(ExpenseReportDetailPage);

    fixture.detectChanges();
    fixture.componentInstance.deleteReport();

    expect(window.confirm).toHaveBeenCalledWith(
      expect.stringContaining("supprimée définitivement avec ses dépenses"),
    );
    expect(expenseReportsApi.deleteReport).not.toHaveBeenCalled();
  });

  it("deletes the report and returns to the reports list when confirmed", () => {
    vi.spyOn(window, "confirm").mockReturnValueOnce(true);
    const fixture = TestBed.createComponent(ExpenseReportDetailPage);

    fixture.detectChanges();
    fixture.componentInstance.deleteReport();

    expect(expenseReportsApi.deleteReport).toHaveBeenCalledWith("report-1");
    expect(router.navigate).toHaveBeenCalledWith(["/reports"]);
  });

  it("displays API errors raised while deleting the report", () => {
    vi.spyOn(window, "confirm").mockReturnValueOnce(true);
    expenseReportsApi.deleteReport.mockReturnValueOnce(
      throwError(() => new HttpErrorResponse({ status: 404 })),
    );
    const fixture = TestBed.createComponent(ExpenseReportDetailPage);

    fixture.detectChanges();
    fixture.componentInstance.deleteReport();
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "Une erreur est survenue",
    );
  });

  it("displays localized business errors raised while creating an expense", () => {
    expenseEntriesApi.createEntry.mockReturnValueOnce(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: {
              code: "expense_entry.monthly_quota_reached",
              message: "Monthly quota reached.",
            },
          }),
      ),
    );
    const fixture = TestBed.createComponent(ExpenseReportDetailPage);

    fixture.detectChanges();
    fixture.componentInstance.createEntry(createRequest);
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "Le quota mensuel de dépenses est atteint pour cet utilisateur.",
    );
  });

  it("displays API errors raised while loading the detail", () => {
    expenseReportsApi.getReport.mockReturnValueOnce(
      throwError(() => new HttpErrorResponse({ status: 404 })),
    );
    const fixture = TestBed.createComponent(ExpenseReportDetailPage);

    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "Une erreur est survenue",
    );
  });
});
