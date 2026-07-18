// Reactive form used to create users from the focused frontend scope.
import { Component, inject, output } from "@angular/core";
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { CreateUserRequest } from "../../models/user.model";

@Component({
  selector: "app-user-form",
  imports: [ReactiveFormsModule],
  templateUrl: "./user-form.html",
  styleUrl: "./user-form.css",
})
export class UserForm {
  private readonly formBuilder = inject(NonNullableFormBuilder);

  readonly createUser = output<CreateUserRequest>();
  readonly form = this.formBuilder.group({
    firstName: ["", [Validators.required]],
    lastName: ["", [Validators.required]],
    street: ["", [Validators.required]],
    postalCode: ["", [Validators.required]],
    city: ["", [Validators.required]],
    monthlyExpenseQuota: [5, [Validators.required, Validators.min(1)]],
  });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.createUser.emit(this.form.getRawValue());
    this.form.reset({
      firstName: "",
      lastName: "",
      street: "",
      postalCode: "",
      city: "",
      monthlyExpenseQuota: 5,
    });
  }

  hasError(controlName: keyof typeof this.form.controls) {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || control.dirty);
  }
}
