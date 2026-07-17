// Defines the normalized API error shape displayed by the frontend.
export interface ApiError {
  readonly code: string;
  readonly message: string;
}

export const genericApiError: ApiError = {
  code: 'api.unexpected_error',
  message: 'Une erreur est survenue. Veuillez réessayer.',
};
