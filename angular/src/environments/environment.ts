import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44333/',
  redirectUri: baseUrl,
  clientId: 'NexusCore_App',
  responseType: 'code',
  scope: 'offline_access NexusCore',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'NexusCore',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44333',
      rootNamespace: 'NexusCore',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
