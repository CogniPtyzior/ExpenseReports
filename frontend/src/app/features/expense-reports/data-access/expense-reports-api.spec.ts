import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { defaultFrontendConfig, FRONTEND_CONFIG } from '../../../core/config/frontend-config';
import { ExpenseReportsApi } from './expense-reports-api';

describe('ExpenseReportsApi', () => {
  let api: ExpenseReportsApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ExpenseReportsApi,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: FRONTEND_CONFIG, useValue: defaultFrontendConfig },
      ],
    });
    api = TestBed.inject(ExpenseReportsApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('lists expense reports through the configured API base path', () => {
    api.listReports().subscribe((reports) => expect(reports).toEqual([]));

    const request = http.expectOne('/api/expense-reports');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('creates expense reports through the configured API base path', () => {
    const body = { userId: 'user-1', year: 2025, month: 10 };

    api.createReport(body).subscribe();

    const request = http.expectOne('/api/expense-reports');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush({ id: 'report-1', assignedUserFullName: 'Ada Lovelace', title: 'Ada Lovelace - Octobre 2025', ...body });
  });
});
