import { TestBed } from "@angular/core/testing";
import { CreateUserRequest } from "../../models/user.model";
import { UserForm } from "./user-form";

describe("UserForm", () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserForm],
    }).compileComponents();
  });

  it("marks controls as touched instead of emitting an invalid user", () => {
    const fixture = TestBed.createComponent(UserForm);
    const component = fixture.componentInstance;
    const emitted: CreateUserRequest[] = [];
    component.createUser.subscribe((request) => emitted.push(request));

    component.submit();

    expect(emitted).toEqual([]);
    expect(component.form.controls.firstName.touched).toBe(true);
  });

  it("emits the user creation request when the form is valid", () => {
    const fixture = TestBed.createComponent(UserForm);
    const component = fixture.componentInstance;
    const emitted: CreateUserRequest[] = [];
    component.createUser.subscribe((request) => emitted.push(request));

    component.form.setValue({
      firstName: "Ada",
      lastName: "Lovelace",
      street: "1 rue du Test",
      postalCode: "75001",
      city: "Paris",
      monthlyExpenseQuota: 5,
    });
    component.submit();

    expect(emitted).toEqual([
      {
        firstName: "Ada",
        lastName: "Lovelace",
        street: "1 rue du Test",
        postalCode: "75001",
        city: "Paris",
        monthlyExpenseQuota: 5,
      },
    ]);
  });
});
