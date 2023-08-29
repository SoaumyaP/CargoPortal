import { NgModule } from '@angular/core';
import { ExcelModule } from '@progress/kendo-angular-grid';
import { UiModule } from 'src/app/ui';
import { ShortshipListComponent } from './shortship-list/shortship-list.component';
import { ShortshipListService } from './shortship-list/shortship-list.service';
import { ShortshipRoutingModule } from './shortship-routing-module';

@NgModule({
    imports: [
        ShortshipRoutingModule,
        UiModule,
        ExcelModule,
    ],
    exports: [
    ],
    declarations: [
        ShortshipListComponent
    ],
    providers: [ShortshipListService]
})

export class ShortshipModule { }
