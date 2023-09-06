import { Directive, Input } from '@angular/core';
import { Validator, AbstractControl, NG_VALIDATORS, ValidationErrors } from '@angular/forms';
import { StringHelper } from '../helpers/string.helper';

@Directive({
    selector: '[appGreaterThan]',
    providers: [
        {
            provide: NG_VALIDATORS,
            useExisting: AppGreaterThanDirective,
            multi: true,
        },
    ],
})
export class AppGreaterThanDirective implements Validator {

    @Input('appGreaterThan') comparingTo?: number;
    validate(control: AbstractControl): ValidationErrors | null {
        return ValidateGreaterThan(control, this.comparingTo);
    }


}

export function ValidateGreaterThan(control: AbstractControl, comparingTo?: number): ValidationErrors  | null  {
    if (!StringHelper.isNullOrEmpty(comparingTo) &&
        control &&
        !StringHelper.isNullOrEmpty(control.value) &&
        control.value <= comparingTo) {
        return { greaterThan: true }; // return object if the validation is not passed.
    }
    return null; // return null if validation is passed.
}
