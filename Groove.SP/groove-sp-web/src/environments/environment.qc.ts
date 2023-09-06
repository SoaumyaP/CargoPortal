// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
    production: false,
    spaUrl: 'https://g-sp.azurewebsites.net',
    hubUrl: 'https://g-sp-api.azurewebsites.net/hub',
    apiUrl: 'https://g-sp-api.azurewebsites.net/api',
    commonApiUrl: 'https://g-cs-api.azurewebsites.net/api',
    identityUrl: 'https://g-cs-is.azurewebsites.net',
    marketingUrl: 'https://g-sp-marketing.azurewebsites.net',
    userTrackTraceInterval: 0,
    version: require('../../package.json').version,
    environmentName: 'qc',
    gaTrackingId: 'G-HKLLNZV41B',
    gaDebugMode: false,
    supplementalApiUrl: 'https://localhost:7204/api'
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
