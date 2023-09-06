import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import { LoaderState } from './loader-state';

@Injectable()
export class LoaderService {
    private loaderSubject = new Subject<LoaderState>();
    loaderState = this.loaderSubject.asObservable();

    show() {
        this.loaderSubject.next({show: true} as LoaderState);
    }

    hide() {
        this.loaderSubject.next({show: false} as LoaderState);
    }
}