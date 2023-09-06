import { Observable, of } from "rxjs";
import { switchMap, tap } from "rxjs/operators";

/**
 * The custom operator will execute whatever you pass instantly and then switch to Original Observable.
 * @param callback 
 * @returns 
 */
export function startWithTap<T>(callback: () => void) {
    return (source: Observable<T>) =>
    of({}).pipe(tap(callback), switchMap((o) => source));
}