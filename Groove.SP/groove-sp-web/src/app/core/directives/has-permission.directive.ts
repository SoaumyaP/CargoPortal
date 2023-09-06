import { OnInit, Directive, Input, ElementRef, TemplateRef, ViewContainerRef } from '@angular/core';
import { UserContextService } from 'src/app/core/auth';


@Directive({
    selector: '[hasPermission]'
})

export class HasPermissionDirective implements OnInit {
    private permissions = [];
    private isLoaded = false;
    constructor(
        private templateRef: TemplateRef<any>,
        private viewContainer: ViewContainerRef,
        private userContextService: UserContextService
    ) {
    }

    ngOnInit(): void {
        this.userContextService.getCurrentUser().subscribe(_ => {
            this.updateView();
        });
    }

    @Input()
    set hasPermission(val: any[]) {
        this.permissions = val;
    }

    private updateView() {
        if (!this.isLoaded) {
            if (this.checkPermission()) {
                this.viewContainer.createEmbeddedView(this.templateRef);
            } else {
                this.viewContainer.clear();
            }
            this.isLoaded = true;
        }
    }

    private checkPermission() {
        let hasPermission = false;
        for (const checkPermission of this.permissions) {
            this.userContextService.isGranted(checkPermission).subscribe(
                result => {
                    hasPermission = result;
            });

            if (!hasPermission) {
                break;
            }
        }
        return hasPermission;
      }
}

