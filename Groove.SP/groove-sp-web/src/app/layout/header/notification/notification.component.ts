import { ChangeDetectionStrategy, ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core';
import { faBell, faEnvelope, faEnvelopeOpen } from '@fortawesome/free-solid-svg-icons';
import { SignalRService } from 'src/app/core/services/signal-r';
import { NotificationService } from './notification.service';
import { concatMap, tap } from 'rxjs/operators';
import { StringHelper } from 'src/app/core';
import { Router } from '@angular/router';
import { NotificationListItem } from 'src/app/core/models/notification';
import { PushNotification, PushNotificationType } from 'src/app/core/models/push-notification';
import { ListPagingViewModel } from 'src/app/core/models/list-paging.model';
import { Separator } from 'src/app/core/models/constants/app-constants';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotificationComponent implements OnInit {

  faBell = faBell;
  faEnvelope = faEnvelope;
  faEnvelopeOpen = faEnvelopeOpen;

  // to define popup is opening
  isPopupOpening: boolean = false;

  unreadTotal: number = 0;

  notifications: NotificationListItem[] = [];

  readonly DEFAULT_ITEM_PER_PAGE = 5;

  protected paging = {
    skip: 0,
    take: this.DEFAULT_ITEM_PER_PAGE,
    recordCount: 0
  };

  animation = false;

  constructor(private signalRService: SignalRService,
    private service: NotificationService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private _ngZone: NgZone,
    public translateService: TranslateService) {

    /*
      Subscribe on push notification arrived
    */
    signalRService.pushNotification$.pipe(

      concatMap((data: PushNotification) => {
        if (this.isPopupOpening) {
          this.paging.take = data.type == PushNotificationType.New ? this.notifications.length + 1 : this.notifications.length;
        }
        this.paging.skip = 0;
        this.notifications = [];

        return this._fetchNotifications$();
      }))
      .subscribe(
        (data: ListPagingViewModel<NotificationListItem>) => {

          this.notifications = [...this.notifications, ...data.items];
          this.paging.recordCount = data.totalItem;
          this.cdr.detectChanges();

          this._fetchUnreadTotal();
          this.runNotifyEffect();
        }
      );
  }

  ngOnInit() {
    this._fetchUnreadTotal();
    this._fetchNotifications();
  }

  /**
   * Fetch total number of unread notifications
   */
  _fetchUnreadTotal() {
    this.service.unreadTotal$.pipe(
      tap(
        (total: number) => {
          this.unreadTotal = total;
          this.cdr.detectChanges();
        })
    ).subscribe()
  }

  /**
   * Fetch notification list
   */
  _fetchNotifications(): void {
    this._fetchNotifications$().subscribe(
      (data: ListPagingViewModel<NotificationListItem>) => {
        this.notifications = [...this.notifications, ...data.items];
        this.paging.recordCount = data.totalItem;
        this.cdr.detectChanges();
      }
    )
  }

  _fetchNotifications$() {
    return this.service.getNotificationList$(this.paging.skip, this.paging.take).pipe(
      tap(() => {
        this.paging.skip += this.paging.take;
        this.paging.take = this.DEFAULT_ITEM_PER_PAGE;
      })
    )
  }

  /**
   * To read a message notification
   * @param i
   */
  read(i: number): void {
    let notification = this.notifications[i];
    if (!StringHelper.isNullOrWhiteSpace(notification.readUrl)) {
      this._ngZone.run(()=>{
        this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
          this.router.navigateByUrl(notification.readUrl);
        });
      });
    }

    if (notification.isRead) {
      // this message has been read
    }
    else {
      this.service.read$(notification.id).subscribe(
        (success) => {
          this.unreadTotal -= 1;
          notification.isRead = true;
          this.cdr.detectChanges();
        },
        (error) => console.log(error)
      )
    }
  }

  /**
   * To mark all messages as read
   */
  readAll() {
    this.service.readAll$().subscribe(
      (success) => {
        this.unreadTotal = 0;
        this.notifications.map(x => x.isRead = true);
        this.cdr.detectChanges();
      },
      (error) => console.log(error)
    )
  }

  get showLoadMore() {
    if (this.emptyNotification) {
      return false;
    }

    return this.notifications.length < this.paging.recordCount;
  }

  runNotifyEffect(): void {
    this.animation = true;
    // stop animation in 1s
    setTimeout(() => {
      this.animation = false;
      this.cdr.detectChanges();
    }, 1000);
  }

  /**
   * To translate the message key if any its text matches regex.
   * Example: ~notification.msg.bookingNo~ -> Booking# (SC)
   * @param val message key
   * @returns 
   */
  translateMessageKey(val: string): string {
    let reg = /~[A-Za-z\.]+~/gm;

    val = val.replace(reg, (key) => {
      key = key.replace(/~/g, '');
      return this.translateService.instant(key);
    });
    
    return val;
  }

  /**
   * No messages yet
   */
  get emptyNotification() {
    return !this.notifications || this.notifications.length === 0;
  }

  /**
     * To open popup
     */
  openPopupContext(): void {
    this.isPopupOpening = true;
  }

  /**
   * To close popup
   */
  closePopupContext(): void {
    this.isPopupOpening = false;
    this.resetPopup();
  }

  /**
   * Reset popup to default.
   */
  resetPopup(): void {
    this.paging.skip = 0;
    this.notifications = [];
    this._fetchNotifications();
  }
}
