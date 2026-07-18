import { TestBed } from "@angular/core/testing";
import { of, throwError } from "rxjs";
import { UsersApi } from "../../data-access/users-api";
import { CreateUserRequest, User } from "../../models/user.model";
import { UsersPage } from "./users-page";

const user: User = {
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

class UsersApiStub {
  readonly listUsers = vi.fn(() => of([user]));
  readonly listAssignableUsers = vi.fn(() => of([user]));
  readonly createUser = vi.fn(() => of(user));
}

describe("UsersPage", () => {
  let usersApi: UsersApiStub;

  beforeEach(async () => {
    usersApi = new UsersApiStub();
    await TestBed.configureTestingModule({
      imports: [UsersPage],
      providers: [{ provide: UsersApi, useValue: usersApi }],
    }).compileComponents();
  });

  it("renders loaded users from the API", () => {
    const fixture = TestBed.createComponent(UsersPage);

    fixture.detectChanges();

    expect(usersApi.listUsers).toHaveBeenCalledTimes(1);
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "Ada Lovelace"
    );
  });

  it("refreshes the list after a user is created", () => {
    const fixture = TestBed.createComponent(UsersPage);
    const request: CreateUserRequest = {
      firstName: "Grace",
      lastName: "Hopper",
      street: "2 rue du Test",
      postalCode: "69001",
      city: "Lyon",
      monthlyExpenseQuota: 4,
    };

    fixture.detectChanges();
    fixture.componentInstance.createUser(request);
    fixture.detectChanges();

    expect(usersApi.createUser).toHaveBeenCalledWith(request);
    expect(usersApi.listUsers).toHaveBeenCalledTimes(2);
  });

  it("displays API errors raised during user creation", () => {
    usersApi.createUser.mockReturnValueOnce(
      throwError(() => ({
        status: 400,
        error: { code: "user.invalid", message: "Invalid user." },
      }))
    );
    const fixture = TestBed.createComponent(UsersPage);

    fixture.detectChanges();
    fixture.componentInstance.createUser({
      firstName: "Ada",
      lastName: "Lovelace",
      street: "1 rue du Test",
      postalCode: "75001",
      city: "Paris",
      monthlyExpenseQuota: 5,
    });
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      "Une erreur est survenue"
    );
  });
});
