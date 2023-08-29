export class Notification {
    id: number;
    messageKey: string;
    readUrl: string;
    createdDate: Date;
}

export class NotificationListItem extends Notification {
    isRead: boolean;
    recordCount: number;
}