import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ColumnSetting, DATE_FORMAT, ListComponent } from 'src/app/core';
import { MasterDialogListService } from './master-dialog-list.service';
import { Location } from '@angular/common';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';

@Component({
  selector: 'app-master-dialog-list',
  templateUrl: './master-dialog-list.component.html',
  styleUrls: ['./master-dialog-list.component.scss']
})
export class MasterDialogListComponent extends ListComponent implements OnInit {

  listName = 'master-dialogs';
  DATE_FORMAT = DATE_FORMAT;
  readonly AppPermissions = AppPermissions;
  faPlus = faPlus;

  columns: ColumnSetting[] = [
    {
      field: 'createdDate',
      title: 'label.messageDates',
      filter: 'date',
      format: this.DATE_FORMAT,
      width: '12%',
      sortable: true
    },
    {
      field: 'owner',
      title: 'label.from',
      filter: 'text',
      width: '14%',
      sortable: true
    },
    {
      field: 'category',
      title: 'label.category',
      filter: 'text',
      width: '10%',
      sortable: true
    },
    {
      field: 'displayOn',
      title: 'label.dialogLevel',
      filter: 'text',
      width: '10%',
      sortable: true
    },
    {
      field: 'filterCriteria',
      title: 'label.searchBy',
      filter: 'text',
      width: '10%',
      sortable: true
    },
    {
      field: 'filterValue',
      title: 'label.masterDialogApplyTo',
      filter: 'text',
      width: '14%',
      sortable: true
    },
    {
      field: 'message',
      title: 'label.message',
      filter: 'text',
      width: '25%',
      sortable: true
    },
    {
      field: 'action',
      title: 'label.action',
      filter: 'text',
      width: '5%',
      sortable: false
    }
  ];

  constructor(public service: MasterDialogListService,
    route: ActivatedRoute,
    private router: Router,
    public notification: NotificationPopup,
    location: Location) {
    super(service, route, location);
  }

  ngOnInit() {
    super.ngOnInit();
  }

  /**handle on user click on delete button */
  onDeleteBtnClicked(itemId) {
    const confirmDlg = this.notification.showConfirmationDialog('msg.deleteNoteConfirm', 'label.message');
    confirmDlg.result.subscribe(
      (result: any) => {
        if (result.value) {
          this.service.delete(itemId).subscribe(
            rsp => {
              this.notification.showSuccessPopup(
                'save.sucessNotification',
                'label.masterDialogs');
              super.ngOnInit();
            },
            err => {
              this.notification.showErrorPopup(
                'save.failureNotification',
                'label.masterDialogs');
            }
          );
        }
      });
  }

  toArrayByComma(text: string) {
    return text.split(", ");
  }

  showAllFilterValue(dataItem) {
    dataItem.isShowAllFilterValue = true;
  }

  /**handle on user click on add button */
  onAddBtnClicked() {
    this.router.navigateByUrl('/master-dialogs/add/0');
  }

}
