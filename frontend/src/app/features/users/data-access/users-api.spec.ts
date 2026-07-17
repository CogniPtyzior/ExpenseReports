import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { defaultFrontendConfig, FRONTEND_CONFIG } from '../../../core/config/frontend-config';
import { UsersApi } from './users-api';

describe('UsersApi', () => {
  let api: UsersApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        UsersApi,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: FRONTEND_CONFIG, useValue: defaultFrontendConfig },
      ],
    });
    api = TestBed.inject(UsersApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('lists users through the configured API base path', () => {
    api.listUsers().subscribe((users) => expect(users).toEqual([]));

    const request = http.expectOne('/api/users');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('creates a user through the configured API base path', () => {
    const body = {
      firstName: 'Ada',
      lastName: 'Lovelace',
      street: '1 rue du Test',
      postalCode: '75001',
      city: 'Paris',
      monthlyExpenseQuota: 5,
    };

    api.createUser(body).subscribe();

    const request = http.expectOne('/api/users');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush({ ...body, id: '1', fullName: 'Ada Lovelace', isActive: true, canBeAssignedToExpenseReport: true });
  });
});
