// Displays expense reports and links to their detail view without owning loading concerns.
import { DatePipe } from "@angular/common";
import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ExpenseReport } from "../../models/expense-report.model";

@Component({
  selector: "app-expense-reports-table",
  imports: [DatePipe, RouterLink],
  templateUrl: "./expense-reports-table.html",
  styleUrl: "./expense-reports-table.css",
})
export class ExpenseReportsTable {
  readonly reports = input.required<readonly ExpenseReport[]>();
}
