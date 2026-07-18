import { ComponentRef } from "@angular/core";
import { TestBed } from "@angular/core/testing";
import { User } from "../../../users/models/user.model";
import { CreateExpenseReportRequest } from "../../models/expense-report.model";
import { ExpenseReportForm } from "./expense-report-form";

const assignableUser: User = {
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

describe("ExpenseReportForm", () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExpenseReportForm],
    }).compileComponents();
  });

  it("requires an assignable user before emitting a report creation request", () => {
    const fixture = TestBed.createComponent(ExpenseReportForm);
    const component = fixture.componentInstance;
    const componentRef: ComponentRef<ExpenseReportForm> = fixture.componentRef;
    const emitted: CreateExpenseReportRequest[] = [];
    componentRef.setInput("users", [assignableUser]);
    component.createReport.subscribe((request) => emitted.push(request));

    component.submit();

    expect(emitted).toEqual([]);
    expect(component.form.controls.userId.touched).toBe(true);
  });

  it("emits a monthly report request with numeric year and month", () => {
    const fixture = TestBed.createComponent(ExpenseReportForm);
    const component = fixture.componentInstance;
    const componentRef: ComponentRef<ExpenseReportForm> = fixture.componentRef;
    const emitted: CreateExpenseReportRequest[] = [];
    componentRef.setInput("users", [assignableUser]);
    component.createReport.subscribe((request) => emitted.push(request));

    component.form.setValue({ userId: "user-1", year: 2025, month: 10 });
    component.submit();

    expect(emitted).toEqual([{ userId: "user-1", year: 2025, month: 10 }]);
    expect(typeof emitted[0].year).toBe("number");
    expect(typeof emitted[0].month).toBe("number");
  });
});
