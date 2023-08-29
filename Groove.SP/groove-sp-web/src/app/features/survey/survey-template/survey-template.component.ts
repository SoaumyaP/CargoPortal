import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-survey-template',
  templateUrl: './survey-template.component.html',
  styleUrls: ['./survey-template.component.scss']
})
export class SurveyTemplateComponent implements OnInit, OnDestroy {
  isSubmitMode: boolean;
  isCloseMode: boolean;
  subscription: Subscription = new Subscription();
  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router
  ) { }


  ngOnInit() {
    const sub = this.activatedRoute.queryParamMap.subscribe(
      (params: ParamMap) => {
        if (params.get('mode') === 'submitted') {
          this.isSubmitMode = true;
        }
        else {
          this.isCloseMode = true;
        }
      }
    )
    this.subscription.add(sub);
  }

  backToDashboard() {
    this.router.navigate(['/home']);
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}
