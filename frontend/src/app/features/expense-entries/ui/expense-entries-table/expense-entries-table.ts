// Displays active expense entries without owning report loading or pagination state.
import { Component, inject, input } from "@angular/core";
import { FRONTEND_CONFIG } from "../../../../core/config/frontend-config";
import { ExpenseEntry } from "../../models/expense-entry.model";

@Component({
  selector: "app-expense-entries-table",
  templateUrl: "./expense-entries-table.html",
  styleUrl: "./expense-entries-table.css",
})
export class ExpenseEntriesTable {
  private readonly config = inject(FRONTEND_CONFIG);
  private readonly amountFormatter = new Intl.NumberFormat(
    this.config.dateLocale,
    {
      style: "currency",
      currency: "EUR",
    }
  );
  private readonly dateFormatter = new Intl.DateTimeFormat(
    this.config.dateLocale,
    {
      weekday: "long",
      day: "numeric",
      month: "long",
      year: "numeric",
    }
  );

  readonly entries = input.required<readonly ExpenseEntry[]>();

  formatExpenseDate(expenseDate: string) {
    const [year, month, day] = expenseDate.split("-").map(Number);
    return this.dateFormatter.format(new Date(year, month - 1, day));
  }

  formatAmount(amount: number) {
    return this.amountFormatter.format(amount);
  }
}
