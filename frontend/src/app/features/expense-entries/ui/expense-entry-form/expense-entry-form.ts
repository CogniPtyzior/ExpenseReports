// Reactive form used to add expenses to one monthly expense report.
import { Component, inject, input, output } from "@angular/core";
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { FRONTEND_CONFIG } from "../../../../core/config/frontend-config";
import { CreateExpenseEntryRequest } from "../../models/expense-entry.model";

interface ExpenseEntryFormValue {
  readonly expenseDate: FormControl<string>;
  readonly description: FormControl<string>;
  readonly amount: FormControl<number>;
  readonly merchantName: FormControl<string>;
  readonly street: FormControl<string>;
  readonly postalCode: FormControl<string>;
  readonly city: FormControl<string>;
}

export interface ExpenseEntryReportPeriod {
  readonly year: number;
  readonly month: number;
}

@Component({
  selector: "app-expense-entry-form",
  imports: [ReactiveFormsModule],
  templateUrl: "./expense-entry-form.html",
  styleUrl: "./expense-entry-form.css",
})
export class ExpenseEntryForm {
  private readonly config = inject(FRONTEND_CONFIG);

  readonly reportPeriod = input.required<ExpenseEntryReportPeriod>();
  readonly createEntry = output<CreateExpenseEntryRequest>();
  readonly descriptionMaxLength = this.config.expenseDescriptionMaxLength;
  readonly form = new FormGroup<ExpenseEntryFormValue>({
    expenseDate: new FormControl("", {
      nonNullable: true,
      validators: [Validators.required],
    }),
    description: new FormControl("", {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(this.descriptionMaxLength),
      ],
    }),
    amount: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.01)],
    }),
    merchantName: new FormControl("", {
      nonNullable: true,
      validators: [Validators.required],
    }),
    street: new FormControl("", {
      nonNullable: true,
      validators: [Validators.required],
    }),
    postalCode: new FormControl("", {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/^\d{5}$/)],
    }),
    city: new FormControl("", {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  submit() {
    this.validateDateAgainstReportMonth();
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.createEntry.emit({
      expenseDate: value.expenseDate,
      description: value.description,
      amount: value.amount,
      billingAddress: {
        merchantName: value.merchantName,
        street: value.street,
        postalCode: value.postalCode,
        city: value.city,
      },
    });
    this.form.reset({
      expenseDate: "",
      description: "",
      amount: 0,
      merchantName: "",
      street: "",
      postalCode: "",
      city: "",
    });
  }

  hasError(controlName: keyof ExpenseEntryFormValue, errorCode?: string) {
    const control = this.form.controls[controlName];
    const hasRequestedError = errorCode
      ? control.hasError(errorCode)
      : control.invalid;
    return hasRequestedError && (control.touched || control.dirty);
  }

  private validateDateAgainstReportMonth() {
    const control = this.form.controls.expenseDate;
    const value = control.value;
    if (!value) {
      return;
    }

    const [year, month] = value.split("-").map(Number);
    const period = this.reportPeriod();
    const currentErrors = { ...(control.errors ?? {}) };
    delete currentErrors["outsideReportMonth"];

    if (year !== period.year || month !== period.month) {
      control.setErrors({ ...currentErrors, outsideReportMonth: true });
      return;
    }

    control.setErrors(
      Object.keys(currentErrors).length > 0 ? currentErrors : null,
    );
  }
}
