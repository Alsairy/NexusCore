import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
    {
      path: '/',
      name: '::Menu:Home',
      iconClass: 'fas fa-home',
      order: 1,
      layout: eLayoutType.application,
    },
    {
      path: '/saudi',
      name: '::Menu:Saudi',
      iconClass: 'fas fa-mosque',
      order: 100,
      layout: eLayoutType.application,
    },
    {
      path: '/saudi/hijri-calendar',
      name: '::Menu:Saudi:HijriCalendar',
      parentName: '::Menu:Saudi',
      iconClass: 'fas fa-calendar-alt',
      order: 1,
      layout: eLayoutType.application,
    },
    {
      path: '/saudi/zatca',
      name: '::Menu:Saudi:Zatca',
      parentName: '::Menu:Saudi',
      iconClass: 'fas fa-file-invoice-dollar',
      order: 2,
      layout: eLayoutType.application,
      requiredPolicy: 'NexusCore.Zatca.Invoices',
    },
    {
      path: '/saudi/zatca/invoices',
      name: '::Menu:Saudi:Zatca:Invoices',
      parentName: '::Menu:Saudi:Zatca',
      order: 1,
      layout: eLayoutType.application,
      requiredPolicy: 'NexusCore.Zatca.Invoices',
    },
    {
      path: '/saudi/zatca/sellers',
      name: '::Menu:Saudi:Zatca:Sellers',
      parentName: '::Menu:Saudi:Zatca',
      order: 2,
      layout: eLayoutType.application,
      requiredPolicy: 'NexusCore.Zatca.Sellers.Manage',
    },
    {
      path: '/saudi/nafath',
      name: '::Menu:Saudi:Nafath',
      parentName: '::Menu:Saudi',
      iconClass: 'fas fa-fingerprint',
      order: 3,
      layout: eLayoutType.application,
      requiredPolicy: 'NexusCore.Nafath.Login',
    },
    {
      path: '/saudi/workflows',
      name: '::Menu:Saudi:Workflows',
      parentName: '::Menu:Saudi',
      iconClass: 'fas fa-project-diagram',
      order: 4,
      layout: eLayoutType.application,
      requiredPolicy: 'NexusCore.Workflows.Approve',
    },
    {
      path: '/saudi/workflows/inbox',
      name: '::Menu:Saudi:Workflows:Inbox',
      parentName: '::Menu:Saudi:Workflows',
      order: 1,
      layout: eLayoutType.application,
      requiredPolicy: 'NexusCore.Workflows.Approve',
    },
    {
      path: '/saudi/workflows/designer',
      name: '::Menu:Saudi:Workflows:Designer',
      parentName: '::Menu:Saudi:Workflows',
      order: 2,
      layout: eLayoutType.application,
      requiredPolicy: 'NexusCore.Workflows.Definitions.Create',
    },
    {
      path: '/saudi/workflows/delegations',
      name: '::Menu:Saudi:Workflows:Delegations',
      parentName: '::Menu:Saudi:Workflows',
      order: 3,
      layout: eLayoutType.application,
      requiredPolicy: 'NexusCore.Workflows.Delegate',
    },
  ]);
}
