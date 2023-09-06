import { FormGroup, NgForm } from "@angular/forms";
import { ViewSettingModuleIdType } from "../models/enums/enums";
import { ViewSettingModel } from "../models/viewSetting.model";
import { StringHelper } from "./string.helper";

export class FormHelper {
    public static ValidateAllFields(form: NgForm | FormGroup) {
        for (const control in form.controls) {
            const formControl = form.controls[control];
            formControl.markAsTouched();
        }
    }

    /**
     * To get a list of visible columns base on ModuleId and FieldId if FieldId has value 
     * @param viewSettings viewSettings field of model
     * @param moduleId  @param moduleId value of ViewSettingModuleIdType enum
     * @param fieldId 
     * @returns 
     */
    public static getVisibleColumns(viewSettings: ViewSettingModel[], moduleId: ViewSettingModuleIdType, fieldId?: string): ViewSettingModel[] | null {
        if (!viewSettings) {
            return null;
        } else {
            let result = viewSettings.filter(c => StringHelper.caseIgnoredCompare(c.moduleId, moduleId));
            if (!StringHelper.isNullOrWhiteSpace(fieldId)) {
                result = result.filter(c => StringHelper.caseIgnoredCompare(c.field, fieldId));
            }
            return result;
        }
    }

    /**
     * Show/hide column on UI
     * @param viewSettings 
     * @param moduleId 
     * @param fieldId 
     * @returns 
     */
    public static isHiddenColumn(viewSettings: ViewSettingModel[], moduleId: ViewSettingModuleIdType, fieldId: string): boolean {
        var columns = this.getVisibleColumns(viewSettings, moduleId);
        if (columns === null) {
            return false;
        }
        return !columns?.some(c => StringHelper.caseIgnoredCompare(c.field, fieldId));
    }
}

