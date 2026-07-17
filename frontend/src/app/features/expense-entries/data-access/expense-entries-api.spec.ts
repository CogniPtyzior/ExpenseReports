import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { defaultFrontendConfig, FRONTEND_CONFIG } from '../../../core/config/frontend-config';
import { ExpenseEntriesApi } from './expense-entries-api';

describe('ExpenseEntriesApi', () => {
  let api: ExpenseEntriesApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ExpenseEntriesApi,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: FRONTEND_CONFIG, useValue: defaultFrontendConfig },
      ],
    });
    api = TestBed.inject(ExpenseEntriesApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('lists paged expense entries with the requested page number', () => {
    api.listEntries('report-1', 2).subscribe((page) => expect(page.totalCount).toBe(0));

    const request = http.expectOne('/api/expense-reports/report-1/entries?pageNumber=2');
    expect(request.request.method).toBe('GET');
    request.flush({ items: [], pageNumber: 2, pageSize: 5, totalCount: 0, totalPages: 0 });
  });

  it('creates expense entries through the report resource', () => {
    const body = {
      expenseDate: '2025-10-15',
      description: 'Lunch',
      amount: 25,
      billingAddress: {
        merchantName: 'Cafe Paris',
        street: '1 Rue des Notes',
        postalCode: '75001',
        city: 'Paris',
      },
    };

    api.createEntry('report-1', body).subscribe();

    const request = http.expectOne('/api/expense-reports/report-1/entries');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush({ id: 'entry-1', expenseReportId: 'report-1', reportYear: 2025, reportMonth: 10, ...body });
  });
});
