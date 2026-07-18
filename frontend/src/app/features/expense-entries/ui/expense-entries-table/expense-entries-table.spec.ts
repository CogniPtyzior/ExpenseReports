import { ComponentRef } from "@angular/core";
import { TestBed } from "@angular/core/testing";
import {
  defaultFrontendConfig,
  FRONTEND_CONFIG,
} from "../../../../core/config/frontend-config";
import { ExpenseEntry } from "../../models/expense-entry.model";
import { ExpenseEntriesTable } from "./expense-entries-table";

const entry: ExpenseEntry = {
  id: "entry-1",
  expenseReportId: "report-1",
  reportYear: 2025,
  reportMonth: 10,
  expenseDate: "2025-10-15",
  description: "Déjeuner client",
  amount: 25,
  currency: "EUR",
  merchantName: "Café Paris",
  street: "1 rue du Test",
  postalCode: "75001",
  city: "Paris",
  createdAtUtc: "2026-07-18T05:00:00Z",
  updatedAtUtc: null,
};

describe("ExpenseEntriesTable", () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExpenseEntriesTable],
      providers: [
        { provide: FRONTEND_CONFIG, useValue: defaultFrontendConfig },
      ],
    }).compileComponents();
  });

  it("renders expense entries with French date and EUR amount", () => {
    const fixture = TestBed.createComponent(ExpenseEntriesTable);
    const componentRef: ComponentRef<ExpenseEntriesTable> =
      fixture.componentRef;

    componentRef.setInput("entries", [entry]);
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? "";
    expect(text).toContain("mercredi 15 octobre 2025");
    expect(text).toContain("Déjeuner client");
    expect(text).toContain("Café Paris");
    expect(text).toContain("25,00");
  });
});
