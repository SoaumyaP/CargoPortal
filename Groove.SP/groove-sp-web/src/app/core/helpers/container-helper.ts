
export class ContainerHelper {
    private static alphaBets = [10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 34, 35, 36, 37, 38];
    private static isLetterUpperCase(charactor) {
        return charactor >= 'A' && charactor <= 'Z';
    }

    private static isNumber(charactor) {
        return charactor >= '0' && charactor <= '9';
    }

    public static checkDigitContainer(containerNumber: string) {
        if (!this.isLetterUpperCase(containerNumber[0]) ||
            !this.isLetterUpperCase(containerNumber[1]) ||
            !this.isLetterUpperCase(containerNumber[2])) {
            return false;
        }
        if (containerNumber[3] !== 'J' &&
            containerNumber[3] !== 'U' &&
            containerNumber[3] !== 'Z') {
            return false;
        }
        if (!this.isNumber(containerNumber[4]) ||
            !this.isNumber(containerNumber[5]) ||
            !this.isNumber(containerNumber[6]) ||
            !this.isNumber(containerNumber[7]) ||
            !this.isNumber(containerNumber[8]) ||
            !this.isNumber(containerNumber[9])) {
            return false;
        }
        if (!this.isNumber(containerNumber[10])) {
            return false;
        }

        let sum = 0;
        sum += this.alphaBets[containerNumber[0].charCodeAt(0) - 65];
        sum += this.alphaBets[containerNumber[1].charCodeAt(0) - 65] * 2;
        sum += this.alphaBets[containerNumber[2].charCodeAt(0) - 65] * 4;
        sum += this.alphaBets[containerNumber[3].charCodeAt(0) - 65] * 8;
        sum += +containerNumber[4] * 16;
        sum += +containerNumber[5] * 32;
        sum += +containerNumber[6] * 64;
        sum += +containerNumber[7] * 128;
        sum += +containerNumber[8] * 256;
        sum += +containerNumber[9] * 512;

        const totalA = sum;

        sum = Math.floor(sum / 11);
        sum *= 11;

        let res = totalA - sum;
        res = res === 10 ? 0 : res;
        return res === +containerNumber[10];
    }
}

