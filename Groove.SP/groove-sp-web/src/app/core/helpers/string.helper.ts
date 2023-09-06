import { FileRestrictions } from '@progress/kendo-angular-upload';
import { MultipleEmailValidationPattern } from '../models/constants/app-constants';

export class StringHelper {
    public static profilePictureRestrictions: FileRestrictions = {
        allowedExtensions: ['.jpg', '.jpeg', '.png'],
        maxFileSize: 153600, // 150Kb
    };

    /**
     * To compare two values which are string/text in case insensitive/ignored mode
     * @param value1 String value 1
     * @param value2 String value 2
     * @returns
     */
    public static caseIgnoredCompare(value1: string, value2: string): boolean {
        const v1 = value1 || '';
        const v2 = value2 || '';
        return v1.toLowerCase() === v2.toLowerCase();
    }

    /**
     * To find a string/text included in the other source string
     * @param source Source string
     * @param value2 String to compare
     * @param isIgnoreCaseSensitivity Default value is true
     * @returns 
     */
    public static includes(source: string, value2: string, isIgnoreCaseSensitivity: boolean = true) {
        const v1 = source || '';
        const v2 = value2 || '';
        if (isIgnoreCaseSensitivity) {
            return v1.toLowerCase().includes(v2.toLowerCase());
        }
        return v1.includes(v2);
    }

    public static isNullOrEmpty(input: any): boolean {
        // Null or empty
        if (input === null || input === undefined || typeof input === 'undefined' || input === '') {
            return true;
        }

        // Array empty
        if (typeof input.length === 'number' && typeof input !== 'function') {
            return !input.length;
        }

        // Blank string like '   '
        if (typeof input === 'string' && input.match(/\S/) === null) {
            return true;
        }

        return false;
    }

    public static isNullOrWhiteSpace(input: any): boolean {
        if (input === undefined || input === null || input === '' || (typeof input === 'string' && input.match(/^\s+$/) !== null)) {
            return true;
        }

        return false;
    }

    public static validateEmail(input: string): boolean {
        const emailRegex = /^[a-z0-9!#$%&'*+\/=?^_`{|}~.-]+@[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)*\.[a-z]{2,3}$/i;
        return emailRegex.test(input);
    }

    public static validateEmailSeparateByComma(input: string): boolean {
        const emailRegex = MultipleEmailValidationPattern;
        return emailRegex.test(input);
    }

    public static translateFormat(path) {
        const nodes = path.split('.');
        let tranlation = 'permission';
        nodes.forEach(element => {
            tranlation += '.' + element.charAt(0).toLowerCase() + element.slice(1);
        });
        return tranlation;
    }

    public static toUpperCaseFirstLetter(input: string, keepOtherLetter: boolean = false): string {
        if (!this.isNullOrEmpty(input)) {
            if (!keepOtherLetter) {
                input = input.toLowerCase();
            }
            return input.charAt(0).toUpperCase() + input.slice(1);
        }
    }

    /**
     * Check a string is Digit
     * @param value 
     * @returns 123413 -> true, 12wew3-> false, 123423.45 -> false
     */
    public static isDigit(value: string) {
        return value && /^\d+$/.test(value);
    }
}
