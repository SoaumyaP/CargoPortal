/**
 * Most of time, it is used by fixed values
 */
export class DropdownListModel<T> {
    constructor(public label: string, public value: T) {
    }
}

/**
 * Most of time, it is used for data returned from server (DropDownListItem)
 */
export class DropDownListItemModel<T> {
    public disabled?: boolean = false;
    public selected?: boolean = false;
    constructor(public text: string, public value: T) {
    }
}
