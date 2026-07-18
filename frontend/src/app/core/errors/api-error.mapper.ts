// Converts backend and transport errors into a stable UI error model.
import { HttpErrorResponse } from "@angular/common/http";
import { ApiError, genericApiError } from "./api-error";

interface ApiErrorBody {
  readonly code?: unknown;
  readonly message?: unknown;
}

const localizedBusinessMessages: Record<string, string> = {
  "expense_report.already_exists":
    "Une note existe déjà pour cet utilisateur et ce mois.",
};

export function mapApiError(error: unknown): ApiError {
  if (error instanceof HttpErrorResponse) {
    return mapHttpError(error);
  }

  return genericApiError;
}

function mapHttpError(error: HttpErrorResponse): ApiError {
  const body = error.error as ApiErrorBody | null;
  if (
    body &&
    typeof body.code === "string" &&
    typeof body.message === "string"
  ) {
    return {
      code: body.code,
      message: localizedBusinessMessages[body.code] ?? body.message,
    };
  }

  if (error.status === 0) {
    return {
      code: "api.unreachable",
      message: "Le serveur est indisponible.",
    };
  }

  return genericApiError;
}
