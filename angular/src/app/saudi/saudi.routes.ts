import { Routes } from '@angular/router';

export const SAUDI_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'onboarding',
  },
  {
    path: 'onboarding',
    loadComponent: () =>
      import('./onboarding/onboarding-wizard.component').then(c => c.OnboardingWizardComponent),
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./dashboard/saudi-dashboard.component').then(c => c.SaudiDashboardComponent),
  },
  {
    path: 'hijri-calendar',
    loadComponent: () =>
      import('./hijri-calendar/hijri-calendar.component').then(c => c.HijriCalendarComponent),
  },
  {
    path: 'zatca',
    children: [
      {
        path: '',
        redirectTo: 'invoices',
        pathMatch: 'full',
      },
      {
        path: 'invoices',
        loadComponent: () =>
          import('./zatca/invoice-list/invoice-list.component').then(c => c.InvoiceListComponent),
      },
      {
        path: 'invoices/create',
        loadComponent: () =>
          import('./zatca/invoice-form/invoice-form.component').then(c => c.InvoiceFormComponent),
      },
      {
        path: 'invoices/:id',
        loadComponent: () =>
          import('./zatca/invoice-detail/invoice-detail.component').then(c => c.InvoiceDetailComponent),
      },
      {
        path: 'invoices/:id/edit',
        loadComponent: () =>
          import('./zatca/invoice-form/invoice-form.component').then(c => c.InvoiceFormComponent),
      },
      {
        path: 'sellers',
        loadComponent: () =>
          import('./zatca/seller-settings/seller-settings.component').then(c => c.SellerSettingsComponent),
      },
    ],
  },
  {
    path: 'nafath',
    children: [
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full',
      },
      {
        path: 'login',
        loadComponent: () =>
          import('./nafath/nafath-login/nafath-login.component').then(c => c.NafathLoginComponent),
      },
      {
        path: 'link-identity',
        loadComponent: () =>
          import('./nafath/nafath-link-identity/nafath-link-identity.component').then(
            c => c.NafathLinkIdentityComponent
          ),
      },
    ],
  },
  {
    path: 'settings',
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./settings/saudi-settings.component').then(c => c.SaudiSettingsComponent),
      },
      {
        path: 'certificates',
        loadComponent: () =>
          import('./settings/certificate-management.component').then(
            c => c.CertificateManagementComponent
          ),
      },
    ],
  },
  {
    path: 'workflows',
    children: [
      {
        path: '',
        redirectTo: 'inbox',
        pathMatch: 'full',
      },
      {
        path: 'inbox',
        loadComponent: () =>
          import('./workflows/approval-inbox/approval-inbox.component').then(
            c => c.ApprovalInboxComponent
          ),
      },
      {
        path: 'designer',
        loadComponent: () =>
          import('./workflows/workflow-designer/workflow-designer.component').then(
            c => c.WorkflowDesignerComponent
          ),
      },
      {
        path: 'delegations',
        loadComponent: () =>
          import('./workflows/delegations/delegations.component').then(c => c.DelegationsComponent),
      },
    ],
  },
];
