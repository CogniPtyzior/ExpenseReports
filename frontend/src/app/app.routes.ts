import { Route } from '@angular/router';

export const appRoutes: Route[] = [
  {
    path: 'reports',
    loadComponent: () =>
      import('./features/expense-reports/pages/expense-reports-page/expense-reports-page')
        .then((module) => module.ExpenseReportsPage),
  },
  {
    path: 'reports/:id',
    loadComponent: () =>
      import('./features/expense-reports/pages/expense-report-detail-page/expense-report-detail-page')
        .then((module) => module.ExpenseReportDetailPage),
  },
  {
    path: 'users',
    loadComponent: () =>
      import('./features/users/pages/users-page/users-page').then((module) => module.UsersPage),
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'reports',
  },
  {
    path: '**',
    redirectTo: 'reports',
  },
];
