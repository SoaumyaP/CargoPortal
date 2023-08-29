export class UserAuditLogModel {
    constructor(
        public operatingSystem: string,
        public browser: string,
        public screenSize: string,
        public userAgent: string,
        public feature: string,
        public accessDateTime: Date
    ) {}
}
