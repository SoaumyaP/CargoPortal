import { Component, OnInit } from '@angular/core';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent implements OnInit {
  version: string;

  constructor() { }

  ngOnInit() {
    if (environment.environmentName !== null) {
      this.version = `${environment.environmentName} ver ${environment.version}`;
    }
  }

  public get ReturnHomeUrl(): string {
    return environment.spaUrl + '/home';
  }

}
