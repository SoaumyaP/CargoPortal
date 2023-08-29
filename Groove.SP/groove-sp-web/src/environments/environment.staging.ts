// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
    production: false,
    spaUrl: 'https://newuatcsfeportalapp.azurewebsites.net',
    hubUrl: 'https://newuatcsfeportalapi.azurewebsites.net/hub',
    apiUrl: 'https://newuatcsfeportalapi.azurewebsites.net/api',
    commonApiUrl: 'https://newuatcsfemasterapi.azurewebsites.net/api',
    identityUrl: 'https://newuatcsfeidentity.azurewebsites.net',
    marketingUrl: 'http://www.cargofe.com',
    userTrackTraceInterval: 0,
    version: require('../../package.json').version,
    environmentName: null,
    gaTrackingId: 'G-V8JJZ4ELX8',
    gaDebugMode: false,
    supplementalApiUrl: 'https://uatsupplementalapi.azurewebsites.net/api'
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
