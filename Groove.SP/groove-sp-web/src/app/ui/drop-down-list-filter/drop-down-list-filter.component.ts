import { Component, Input } from '@angular/core';
import { FilterService, BaseFilterCellComponent } from '@progress/kendo-angular-grid';

@Component({
  selector: 'app-drop-down-list-filter',
  templateUrl: './drop-down-list-filter.component.html',
  styleUrls: ['./drop-down-list-filter.component.scss']
})
export class DropDownListFilterComponent extends BaseFilterCellComponent {

    public get selectedValue(): any {
        if (this.filter !== undefined) {
            const filter = this.filter.filters.find(f => f.field.toLowerCase() === this.fieldName.toLowerCase());
            return filter ? filter.value : null;
        }
        return null;
    }

    @Input() public filter: any;
    @Input() public data: any[];
    @Input() public fieldName: string;
    @Input() public textField: string;
    @Input() public valueField: string;

    public get defaultItem(): any {
        return {
            [this.textField]: 'label.all',
            [this.valueField]: null
        };
    }

    constructor(filterService: FilterService) {
        super(filterService);
    }

    public onChange(value: any): void {
        // remove exist filter if same key.
        if (this.filter !== undefined) {
            this.filter.filters = this.filter.filters.filter(f => f.field.toLowerCase() !== this.fieldName.toLowerCase());
        }
        this.applyFilter(
            value === null ? // value of the default item
                this.removeFilter(this.fieldName) : // remove the filter
                this.updateFilter({ // add a filter for the field with the value
                    field: this.fieldName,
                    operator: 'eq',
                    value: value
                })
        ); // update the root filter
    }
}