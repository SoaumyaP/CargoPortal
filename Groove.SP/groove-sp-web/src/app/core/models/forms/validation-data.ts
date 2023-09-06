import { ValidationDataType } from '../enums/enums';

export class ValidationData {
    /**
     *
     */
    constructor (
        // validation type (input, business)
        public type: ValidationDataType = ValidationDataType.Input,
        public status: boolean,
        public message?: string
        ) {
    }
}
