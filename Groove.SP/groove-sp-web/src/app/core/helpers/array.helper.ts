export class ArrayHelper {
    public static sortBy(array: any[], propertyName: string, descending?: 'desc') {
        array.sort((a, b) => {
            if (a[propertyName] < b[propertyName]) {
                return -1;
            }
            if (a[propertyName] > b[propertyName]) {
                return 1;
            }
            return 0;
        });

        if (descending?.toLocaleLowerCase() === 'desc') {
            array.reverse();
        }
    }

    public static uniqueBy(array: any, propertyName: string) {
        const arr = [
            ...new Map(array.map(
                item => {
                    return [item[propertyName], item];
                }
            )).values()
        ];
        return arr;
    }

    public static sumBy(array: any, propertyName: string) {
        let total = 0;
        for (let item of array) {
            if (typeof (item[propertyName]) === 'number') {
                total = total + item[propertyName];
            }
        }
        return total;
    }
}
