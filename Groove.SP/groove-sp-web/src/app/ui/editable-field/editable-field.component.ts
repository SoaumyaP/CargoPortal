import { ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { faCheck, faPencilAlt, faTimes } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-editable-field',
  templateUrl: './editable-field.component.html',
  styleUrls: ['./editable-field.component.scss']
})
export class EditableFieldComponent {
    oldValue: any;
    isEditing: boolean = false;
    faPencilAlt = faPencilAlt;
    faCheck = faCheck;
    faRemove = faTimes;

    @Input()
    isEditable: boolean = false;
    @Input()
    value: any;

    @Output()
    edit: EventEmitter<any> = new EventEmitter<any>();
    @Output()
    save: EventEmitter<any> = new EventEmitter<any>();
    @Output()
    cancel: EventEmitter<any> = new EventEmitter<any>();

    @ViewChild('editable', { static: false }) inputField;

    constructor(private changeDetectorRef: ChangeDetectorRef) {
    }

    editValue() {
        this.isEditing = true;
        this.oldValue = this.value;
        this.setFocus();
        this.edit.emit();
    }

    private setFocus() {
        this.changeDetectorRef.detectChanges();
        this.inputField.nativeElement.focus();
    }

    saveValue() {
        this.isEditing = false;
        this.save.emit(this.value);
    }

    cancelEditing() {
        this.isEditing = false;
        this.value = this.oldValue;
        this.cancel.emit();
    }
}
