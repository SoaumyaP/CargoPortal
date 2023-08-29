export interface PushNotification {
    id: number;
    messageKey: string;
    type: PushNotificationType;
}

export enum PushNotificationType {
    New,
    Read,
    ReadAll
}