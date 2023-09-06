import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { LoaderService } from './loader.service';
import { LoaderState } from './loader-state';

@Component({
    selector: 'app-loader',
    templateUrl: './loader.component.html'
})
export class LoaderComponent implements OnInit, OnDestroy {
    show = false;
    completeInit = false;
    private subscription: Subscription;

    constructor(private loaderService: LoaderService) { }

    ngOnDestroy(): void {
        this.subscription.unsubscribe();
    }

    ngOnInit(): void {
        this.subscription = this.loaderService.loaderState.subscribe((state: LoaderState) => {
            this.show = state.show;
        });
    }

    setStatusInit(status: boolean) {
        this.completeInit = status;
    }
}