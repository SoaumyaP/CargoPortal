
export class MathHelper {
    public static calculateCBM(length: number, width: number, height: number): number {
        return length * width * height / 1000000;
    }

    /**
     * Deal with floating point number precision in javascript.
     * @param number A numeric value.
     */
    public static roundToThreeDecimals(number: number): number {
        return +(Math.round(+(number + 'e+3'))  + 'e-3');
    }

    public static roundToTwoDecimals(number: number): number {
        return +(Math.round(+(number + 'e+2'))  + 'e-2');
    }

}

