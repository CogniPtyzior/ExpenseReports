// Displays users without owning data loading or mutations.
import { Component, input } from "@angular/core";
import { User } from "../../models/user.model";

@Component({
  selector: "app-users-table",
  templateUrl: "./users-table.html",
  styleUrl: "./users-table.css",
})
export class UsersTable {
  readonly users = input.required<readonly User[]>();
}
