export class EnumHelper {
    public static extractToKeys(arr, number): any {
        const result = [];
          const keys = Object.keys(arr);
          for (let i = 0; i < keys.length; i++) {
            const keyStr = keys[i];
            // tslint:disable-next-line:no-bitwise
            if (arr[keyStr] & number) {
                result.push(keyStr);
            }
          }
        return result;
    }

    public static extractToValues(arr, number): any {
        const result = [];
          const keys = Object.keys(arr);
          for (let i = 0; i < keys.length; i++) {
            const keyStr = keys[i];
            // tslint:disable-next-line:no-bitwise
            if (arr[keyStr] & number) {
                result.push(arr[keyStr]);
            }
          }
        return result;
    }

    public static convertToLabel(arr, value): any {
        const values = Object.values(arr);
          for (let i = 0; i < values.length; i++) {
            const type = values[i];
            // tslint:disable-next-line:no-bitwise
            if (type['value'] === value) {
                return type['label'];
            }
          }
        return value;
    }

    public static mergeToValues(idList: any): number {
        let result = 0;
          for (let i = 0; i < idList.length; i++) {
            // tslint:disable-next-line:no-bitwise
            result = result | idList[i];
          }
        return result;
    }
}