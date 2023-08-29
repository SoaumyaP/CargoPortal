// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
    production: false,
    spaUrl: 'http://localhost:4209',
    hubUrl: 'http://localhost:56153/hub',
    apiUrl: 'http://localhost:56153/api',
    commonApiUrl: 'http://localhost:50896/api',
    identityUrl: 'http://localhost:44392',
    marketingUrl: 'https://g-sp-marketing.azurewebsites.net',
    userTrackTraceInterval: 0,
    version: require('../../package.json').version,
    environmentName: 'Local',
    gaTrackingId: 'G-XKXS3HDXBK',
    gaDebugMode: true, // To enable debug mode, set to true
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
