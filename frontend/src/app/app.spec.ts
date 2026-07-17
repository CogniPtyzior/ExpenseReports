import { provideLocationMocks } from '@angular/common/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { appRoutes } from './app.routes';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(appRoutes), provideLocationMocks()],
    }).compileComponents();
  });

  it('renders the application shell without the Nx starter screen', () => {
    const fixture = TestBed.createComponent(App);

    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.brand-mark')?.textContent).toContain('Expense Management');
    expect(compiled.textContent).toContain('Gestion des notes de frais');
    expect(compiled.textContent).toContain('Notes de frais');
    expect(compiled.textContent).toContain('Utilisateurs');
    expect(compiled.textContent).not.toContain('Welcome to Expense Management');
  });
});
