import { HttpErrorResponse } from "@angular/common/http";
import { mapApiError } from "./api-error.mapper";

describe("mapApiError", () => {
  it("maps known backend business error codes to localized UI messages", () => {
    const error = new HttpErrorResponse({
      status: 409,
      error: {
        code: "expense_report.already_exists",
        message: "Expense report already exists.",
      },
    });

    expect(mapApiError(error)).toEqual({
      code: "expense_report.already_exists",
      message: "Une note existe déjà pour cet utilisateur et ce mois.",
    });
  });

  it("keeps unknown backend error messages when no localized UI message exists", () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        code: "user.invalid",
        message: "Invalid user.",
      },
    });

    expect(mapApiError(error)).toEqual({
      code: "user.invalid",
      message: "Invalid user.",
    });
  });

  it("maps network failures to a readable message", () => {
    const error = new HttpErrorResponse({ status: 0 });

    expect(mapApiError(error)).toEqual({
      code: "api.unreachable",
      message: "Le serveur est indisponible.",
    });
  });
});
