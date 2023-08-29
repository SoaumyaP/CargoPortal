import { Directive, Input, ElementRef, OnDestroy } from '@angular/core';
import * as Clipboard from 'clipboard/dist/clipboard';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
@Directive({
  selector: '[clipboard]'
})
export class ClipboardDirective implements OnDestroy {

  private _clipBoard: any;
  private _selector: String;
  @Input() clipboardText: String;
  constructor(private _el: ElementRef, private _notification: NotificationPopup) {
    this._selector = this.getClassName();
    _el.nativeElement.classList.add(this._selector);
    this._clipBoard = new Clipboard('.' + this._selector, {
      text: () => {
        return this.clipboardText;
      }
    });

    this._clipBoard.on('success', () => {
      this._notification.showSuccessPopup('msg.copyToClipboardSuccess', 'label.success', true);
    });
    this._clipBoard.on('error', (e) => {
      this._notification.showErrorPopup('msg.copyToClipboardError', 'label.error', true);
    });
  }
  ngOnDestroy(): void {
    if (this._clipBoard) {
      this._clipBoard.destroy();
    }
  }
  private getClassName(): String {
    return 'cb-' + Math.floor(Math.random() * 1000) + '-' + new Date().getTime().toString();
  }
}
