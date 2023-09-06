import { Component, OnInit } from '@angular/core';
import { RolesListService } from './roles-list.service';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent, RoleStatus } from 'src/app/core';

@Component({
  selector: 'app-roles-list',
  templateUrl: './roles-list.component.html',
  styleUrls: ['./roles-list.component.scss']
})
export class RolesListComponent extends ListComponent implements OnInit {
  listName = 'roles';
  roleStatus = RoleStatus;

  constructor(public service: RolesListService, route: ActivatedRoute, location: Location ) {
    super(service, route, location);
  }

  ngOnInit() {
    super.ngOnInit();
  }

}
