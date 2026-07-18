// Reactive form used to create monthly expense reports for assignable users.
import { Component, input, output } from "@angular/core";
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { User } from "../../../users/models/user.model";
import { CreateExpenseReportRequest } from "../../models/expense-report.model";

interface ExpenseReportFormValue {
  readonly userId: FormControl<string>;
  readonly year: FormControl<number>;
  readonly month: FormControl<number>;
}

interface MonthOption {
  readonly value: number;
  readonly label: string;
}

@Component({
  selector: "app-expense-report-form",
  imports: [ReactiveFormsModule],
  templateUrl: "./expense-report-form.html",
  styleUrl: "./expense-report-form.css",
})
export class ExpenseReportForm {
  readonly users = input.required<readonly User[]>();
  readonly createReport = output<CreateExpenseReportRequest>();

  readonly months: readonly MonthOption[] = [
    { value: 1, label: "Janvier" },
    { value: 2, label: "Février" },
    { value: 3, label: "Mars" },
    { value: 4, label: "Avril" },
    { value: 5, label: "Mai" },
    { value: 6, label: "Juin" },
    { value: 7, label: "Juillet" },
    { value: 8, label: "Août" },
    { value: 9, label: "Septembre" },
    { value: 10, label: "Octobre" },
    { value: 11, label: "Novembre" },
    { value: 12, label: "Décembre" },
  ];

  readonly years: readonly number[];
  readonly form: FormGroup<ExpenseReportFormValue>;

  constructor() {
    const currentDate = new Date();
    const currentYear = currentDate.getFullYear();
    const currentMonth = currentDate.getMonth() + 1;

    this.years = [currentYear - 1, currentYear, currentYear + 1];
    this.form = new FormGroup<ExpenseReportFormValue>({
      userId: new FormControl("", {
        nonNullable: true,
        validators: [Validators.required],
      }),
      year: new FormControl(currentYear, {
        nonNullable: true,
        validators: [Validators.required, Validators.min(2000)],
      }),
      month: new FormControl(currentMonth, {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.min(1),
          Validators.max(12),
        ],
      }),
    });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.createReport.emit(this.form.getRawValue());
  }

  hasError(controlName: keyof ExpenseReportFormValue) {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || control.dirty);
  }
}
