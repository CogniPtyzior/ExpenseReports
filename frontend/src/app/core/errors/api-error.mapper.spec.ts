import { HttpErrorResponse } from '@angular/common/http';
import { mapApiError } from './api-error.mapper';

describe('mapApiError', () => {
  it('maps backend error contracts', () => {
    const error = new HttpErrorResponse({
      status: 409,
      error: {
        code: 'expense_report.already_exists',
        message: 'Expense report already exists.',
      },
    });

    expect(mapApiError(error)).toEqual({
      code: 'expense_report.already_exists',
      message: 'Expense report already exists.',
    });
  });

  it('maps network failures to a readable message', () => {
    const error = new HttpErrorResponse({ status: 0 });

    expect(mapApiError(error)).toEqual({
      code: 'api.unreachable',
      message: 'Le serveur est indisponible.',
    });
  });
});
