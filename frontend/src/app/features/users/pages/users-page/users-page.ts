// Users landing page for the focused frontend scope.
import { Component } from '@angular/core';
import { EmptyState } from '../../../../shared/ui/empty-state/empty-state';
import { PageHeader } from '../../../../shared/ui/page-header/page-header';

@Component({
  imports: [EmptyState, PageHeader],
  templateUrl: './users-page.html',
  styleUrl: './users-page.css',
})
export class UsersPage {}
