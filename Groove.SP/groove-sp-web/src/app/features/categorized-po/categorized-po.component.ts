import { Component, OnDestroy } from '@angular/core';
import { faFlag, faGlobe, faUsers, faCheckCircle, faSearch, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { CategorizedPOType, HttpService, LocalStorageService, POStageType, PurchaseOrderStatus, StatisticKey, StringHelper, UserContextService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { debounceTime, tap } from 'rxjs/operators';
import { ListPagingViewModel } from 'src/app/core/models/list-paging.model';
import { Subject, Subscription } from 'rxjs';
import { Separator } from 'src/app/core/models/constants/app-constants';
import { Router } from '@angular/router';

@Component({
  selector: 'app-categorized-po',
  templateUrl: './categorized-po.component.html',
  styleUrls: ['./categorized-po.component.scss']
})
export class CategorizedPOComponent implements OnDestroy {

  faUsers = faUsers;
  faGlobe = faGlobe;
  faFlag = faFlag;
  faCheckCircle = faCheckCircle;
  faSearch = faSearch;
  faPowerOff = faPowerOff;

  readonly categorizedPOType = CategorizedPOType;
  readonly purchaseOrderStatus = PurchaseOrderStatus;

  /**Define menu items to be displayed */
  readonly menu = [
    {
      name: 'label.supplier',
      categoryType: CategorizedPOType.Supplier,
      icon: faUsers
    },
    {
      name: 'label.consignee',
      categoryType: CategorizedPOType.Consignee,
      icon: faUsers
    },
    {
      name: 'label.destination',
      categoryType: CategorizedPOType.Destination,
      icon: faGlobe
    },
    {
      name: 'label.stages',
      categoryType: CategorizedPOType.Stage,
      icon: faFlag
    },
    {
      name: 'label.status',
      categoryType: CategorizedPOType.Status,
      icon: faCheckCircle
    }
  ]

  readonly poStages = [
    {
      stage: POStageType.Released,
      title: 'label.released',
      class: 'n-released',
      value: POStageType.Released,
      active: false,
      current: false
    },
    {
      stage: POStageType.ForwarderBookingRequest,
      title: 'label.forwarderBookingRequest',
      class: 'n-forwarderBookingRequest',
      value: POStageType.ForwarderBookingRequest,
      active: false,
      current: false
    },
    {
      stage: POStageType.ForwarderBookingConfirmed,
      title: 'label.forwarderBookingConfirmed',
      class: 'n-forwarderBookingConfirmed',
      value: POStageType.ForwarderBookingConfirmed,
      active: false,
      current: false
    },
    {
      stage: POStageType.ShipmentDispatch,
      title: 'label.shipmentDispatch',
      class: 'n-shipmentDispatch',
      value: POStageType.ShipmentDispatch,
      active: false,
      current: false
    },
    {
      stage: POStageType.Closed,
      title: 'label.closed',
      class: 'n-closed',
      value: POStageType.Closed,
      active: false,
      current: false
    }
  ]; 

  isPopupOpening: boolean;

  /**
   * default page size
   */
  readonly PAGE_SIZE: number = 8;

  /**
   * current selected categorized-po type.
   * */
  selectedCategory: CategorizedPOType;

  statisticKeyMapping = {
    [CategorizedPOType.Supplier]: StatisticKey.CategorizedSupplier,
    [CategorizedPOType.Consignee]: StatisticKey.CategorizedConsignee,
    [CategorizedPOType.Destination]: StatisticKey.CategorizedDestination,
    [CategorizedPOType.Stage]: StatisticKey.CategorizedStage,
    [CategorizedPOType.Status]: StatisticKey.CategorizedStatus
  };

  readonly labelMapping = {
    [CategorizedPOType.Supplier]: {
      recentTitle: "label.recentOrganizations"
    },
    [CategorizedPOType.Consignee]: {
      recentTitle: "label.recentOrganizations"
    },
    [CategorizedPOType.Destination]: {
      recentTitle: "label.recentDestinations"
    }
  }

  /** Category list data source is fetching */
  onFetchingDataSource: boolean = true;

  /** search term on categorized data list */
  searchTerm: string = '';

  /** current page number. */
  currentPage: number;

  currentUser: any;

  firstRecordIndex: number;
  lastRecordIndex: number

  localStorageKey: string = 'CategorizedPO_recentValues';

  searchCategoryKeyUp$ = new Subject<any>();

  /**
   * Store all subscriptions, then should un-subscribe at the end
   * */
  private _subscriptions: Array<Subscription> = [];

  categorizedDataSource: ListPagingViewModel<string>;
  recentCategorizedDataSource: string[] = [];

  constructor(private httpService: HttpService, private userContext: UserContextService, private router: Router) {

    userContext.getCurrentUser().subscribe(
      (user) => {
        if (user) {
          this.currentUser = user;
        }
      }
    )

    const sub$ = this.searchCategoryKeyUp$.pipe(
      debounceTime(1000),
      tap(() => {
        this.currentPage = 1;
        this._fetchCategorizedDataSource();
      }
      )).subscribe();

    this._subscriptions.push(sub$);
  }


  /**
     * To open popup
     */
  openPopupContext(categorizedPOType: CategorizedPOType): void {
    this.resetPopup();

    this.isPopupOpening = true;
    this.selectedCategory = categorizedPOType;
    this._fetchCategorizedDataSource();
    this._fetchRecentCategorizedDataSource();
  }

  /**
   * Fetch category item list.
   */
  _fetchCategorizedDataSource() {
    /** building url... */
    let url = `${environment.apiUrl}/purchaseOrders/search-categorized-po?searchTerm=${this.searchTerm}&pageOffset=${this.currentPage}&pageSize=${this.PAGE_SIZE}&type=${this.selectedCategory}`;
    if (!this.currentUser) {
      return;
    }
    let { isInternal, affiliates, customerRelationships, organizationId } = this.currentUser;
    if (!isInternal) {
      url += `&affiliates=${affiliates}&organizationId=${organizationId}`;

      if (customerRelationships) {
        url += `&customerRelationships=${customerRelationships}`;
      }
    }

    /** starting... */
    this.onFetchingDataSource = true;

    this.httpService.get(url).pipe(
      tap((rs: ListPagingViewModel<string>) => {
        this.categorizedDataSource = rs;

        // calculate data range
        this.firstRecordIndex = ((rs.page - 1) * this.PAGE_SIZE) + 1;
        this.lastRecordIndex = this.firstRecordIndex + rs.items.length - 1;
      })
    ).subscribe(
      () => this.onFetchingDataSource = false
    );
  }

  private _fetchRecentCategorizedDataSource(): void {
    // Fetch data for Recent values
    const storedValue = LocalStorageService.read<string>(this.localStorageKey);
    if (!StringHelper.isNullOrEmpty(storedValue)) {
      let recentValues = storedValue.split(Separator.Semicolon);

      // Only get the last 5 values
      let i = 0;
      while (this.recentCategorizedDataSource.length < 5 && i < recentValues.length) {
        const valueParts = recentValues[i].split(Separator.TILDE);
        const userName = valueParts[2];
        const categorizedType = valueParts[1];

        if (this.currentUser.username === userName && categorizedType === `${this.selectedCategory}`) {
          this.recentCategorizedDataSource.push(valueParts[0]);
        }
        i++;
      }
    }
  }

  onNavigate(value: string): void {
    if (this.selectedCategory !== CategorizedPOType.Stage
      && this.selectedCategory !== CategorizedPOType.Status) {
      this._storeRecentSelectedItem(this.selectedCategory, value, this.currentUser.username);
    }
    this.router.navigate(['/purchase-orders'], { state: { statisticKey: `${this.statisticKeyMapping[this.selectedCategory]}`, statisticValue: `${encodeURIComponent(value)}` } });
  }

  /**
 * To close popup
 */
  closePopupContext(): void {
    this.isPopupOpening = false;
    this.resetPopup();
  }

  //#region pager events 
  pageChange(pageInfo: any): void {
    this.currentPage = (pageInfo.skip / this.PAGE_SIZE) + 1;
    this._fetchCategorizedDataSource();
  }

  public onGoTo(page: number): void {
    this.currentPage = page;
    this._fetchCategorizedDataSource();
  }

  public onNext(page: number): void {
    this.currentPage = page + 1
    this._fetchCategorizedDataSource();
  }

  public onPrevious(page: number): void {
    this.currentPage = page - 1;
    this._fetchCategorizedDataSource();
  }
  //#endregion

  /**
     * To stored selected value to local storage
     * @param categorizedType
     * @param organizationName
     * @param userName
     */
  private _storeRecentSelectedItem(categorizedType: CategorizedPOType, organizationName: string, userName: string) {
    const toAddValue = `${organizationName}~${categorizedType}~${userName}`;
    const storedValue = LocalStorageService.read<string>(this.localStorageKey);
    let toStoreValue = '';
    let toStoreValues: Array<string> = [];
    if (StringHelper.isNullOrEmpty(storedValue)) {
      toStoreValue = '';
    } else {
      const splitted = storedValue.split(Separator.Semicolon);
      if (splitted != null && splitted.length > 0) {
        toStoreValues = splitted.filter(x => x !== toAddValue);
      }
    }
    toStoreValues.unshift(toAddValue);

    toStoreValue = toStoreValues.join(Separator.Semicolon);
    LocalStorageService.write(this.localStorageKey, toStoreValue);
  }

  /**
   * To reset state of popup
   */
  resetPopup(): void {
    this.searchTerm = '';
    this.currentPage = 1;
    this.categorizedDataSource = null;
    this.recentCategorizedDataSource = [];
    this.selectedCategory = null;
  }

  ngOnDestroy(): void {
    this._subscriptions.map(x => x.unsubscribe());
  }
}