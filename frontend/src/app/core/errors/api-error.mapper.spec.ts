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

  it("maps expense entry business errors to localized UI messages", () => {
    const quotaError = new HttpErrorResponse({
      status: 409,
      error: {
        code: "expense_entry.monthly_quota_reached",
        message: "Monthly quota reached.",
      },
    });
    const dateError = new HttpErrorResponse({
      status: 400,
      error: {
        code: "expense_entry.date_outside_report_month",
        message: "Date outside report month.",
      },
    });

    expect(mapApiError(quotaError)).toEqual({
      code: "expense_entry.monthly_quota_reached",
      message: "Le quota mensuel de dépenses est atteint pour cet utilisateur.",
    });
    expect(mapApiError(dateError)).toEqual({
      code: "expense_entry.date_outside_report_month",
      message: "La date de dépense doit appartenir au mois de la note.",
    });
  });

  it("maps backend validation errors to a localized UI message", () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        code: "validation.failed",
        message: "Validation failed.",
      },
    });

    expect(mapApiError(error)).toEqual({
      code: "validation.failed",
      message: "Les données saisies sont invalides.",
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
