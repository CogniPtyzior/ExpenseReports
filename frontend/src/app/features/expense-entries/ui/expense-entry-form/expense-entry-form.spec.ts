import { ComponentRef } from "@angular/core";
import { TestBed } from "@angular/core/testing";
import {
  defaultFrontendConfig,
  FRONTEND_CONFIG,
} from "../../../../core/config/frontend-config";
import { ExpenseEntryForm } from "./expense-entry-form";

describe("ExpenseEntryForm", () => {
  let componentRef: ComponentRef<ExpenseEntryForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExpenseEntryForm],
      providers: [
        { provide: FRONTEND_CONFIG, useValue: defaultFrontendConfig },
      ],
    }).compileComponents();
  });

  function createForm() {
    const fixture = TestBed.createComponent(ExpenseEntryForm);
    componentRef = fixture.componentRef;
    componentRef.setInput("reportPeriod", { year: 2025, month: 11 });
    fixture.detectChanges();
    return fixture;
  }

  it("emits a create request with billing address values", () => {
    const fixture = createForm();
    const component = fixture.componentInstance;
    const emitted = vi.fn();

    component.createEntry.subscribe(emitted);
    component.form.setValue({
      expenseDate: "2025-11-15",
      description: "Train",
      amount: 86,
      merchantName: "SNCF Connect",
      street: "24 Avenue des Frais",
      postalCode: "69002",
      city: "Lyon",
    });

    component.submit();

    expect(emitted).toHaveBeenCalledWith({
      expenseDate: "2025-11-15",
      description: "Train",
      amount: 86,
      billingAddress: {
        merchantName: "SNCF Connect",
        street: "24 Avenue des Frais",
        postalCode: "69002",
        city: "Lyon",
      },
    });
  });

  it("marks the form invalid when required fields are missing", () => {
    const fixture = createForm();
    const component = fixture.componentInstance;
    const emitted = vi.fn();

    component.createEntry.subscribe(emitted);
    component.submit();
    fixture.detectChanges();

    expect(emitted).not.toHaveBeenCalled();
    expect(component.form.invalid).toBe(true);
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "La date est requise.",
    );
  });

  it("rejects a date outside the report month before calling the API", () => {
    const fixture = createForm();
    const component = fixture.componentInstance;
    const emitted = vi.fn();

    component.createEntry.subscribe(emitted);
    component.form.setValue({
      expenseDate: "2025-12-01",
      description: "Train",
      amount: 86,
      merchantName: "SNCF Connect",
      street: "24 Avenue des Frais",
      postalCode: "69002",
      city: "Lyon",
    });

    component.submit();
    fixture.detectChanges();

    expect(emitted).not.toHaveBeenCalled();
    expect(
      component.form.controls.expenseDate.hasError("outsideReportMonth"),
    ).toBe(true);
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "La date doit appartenir au mois de la note.",
    );
  });

  it("applies the configured description length limit", () => {
    const fixture = createForm();
    const component = fixture.componentInstance;

    component.form.patchValue({ description: "x".repeat(51) });
    component.form.controls.description.markAsTouched();
    fixture.detectChanges();

    expect(component.form.controls.description.hasError("maxlength")).toBe(
      true,
    );
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "La description est limitée à 50 caractères.",
    );
  });
});
