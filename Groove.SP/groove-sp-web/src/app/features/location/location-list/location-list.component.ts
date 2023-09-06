import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { faPencilAlt, faPlus } from '@fortawesome/free-solid-svg-icons';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ListComponent } from 'src/app/core/list/list.component';
import { LocationListService } from './location-list.service';
import { Location } from '@angular/common';
import { FormMode } from 'src/app/core/models/enums/enums';
import { LocationModel } from 'src/app/core/models/location.model';

@Component({
  selector: 'app-location-list',
  templateUrl: './location-list.component.html',
  styleUrls: ['./location-list.component.scss']
})
export class LocationListComponent extends ListComponent implements OnInit {
  listName = 'locations';
  readonly AppPermissions = AppPermissions;
  faPlus = faPlus;
  faPencilAlt = faPencilAlt;
  locationFormMode: string;
  isOpenLocationForm: boolean;
  locationModel: LocationModel = new LocationModel();

  constructor(
    public service: LocationListService,
    route: ActivatedRoute,
    location: Location) {
    super(service, route, location);
  }

  ngOnInit() {
    super.ngOnInit();
  }

  onAddNewLocation() {
    this.isOpenLocationForm = true;
    this.locationFormMode = FormMode.Add;
    this.locationModel = new LocationModel();
  }

  onEditLocation(rowData) {
    this.isOpenLocationForm = true;
    this.locationFormMode = FormMode.Edit;
    this.locationModel = { ...rowData };
  }

  onCloseLocationForm(isSavedSuccessfully) {
    this.isOpenLocationForm = false;
    if (isSavedSuccessfully) {
      this.ngOnInit();
    }
  }
}
