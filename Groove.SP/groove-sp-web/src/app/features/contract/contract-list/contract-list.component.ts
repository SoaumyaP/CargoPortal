import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ContractMasterStatus, ListComponent } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ContractListService } from './contract-list.service';
import { Location } from '@angular/common';
import { faPlus } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-contract-list',
  templateUrl: './contract-list.component.html',
  styleUrls: ['./contract-list.component.scss']
})
export class ContractListComponent extends ListComponent implements OnInit {
  listName = 'contracts';
  readonly AppPermissions = AppPermissions;
  contractMasterStatus = ContractMasterStatus;
  faPlus = faPlus;

  constructor(
    public service: ContractListService,
    route: ActivatedRoute,
    location: Location,
    private router: Router
  ) {
    super(service, route, location);
  }

  ngOnInit() {
    super.ngOnInit();
  }

  onAddNewContract() {
    this.router.navigate(['contracts/add/0']);
  }
}
