export class POAdhocChangedData {
    constructor(
        // Purchase order adhoc change priority (level 1, 2, 3)
        public priority: number,
        // Message will display to notify when Purchase order(s) has been changed.
        public message: string,
        // Id(s) of adhoc-changed Purchase order(s) of the booking.
        public purchaseOrderIds: []) {
    }
}