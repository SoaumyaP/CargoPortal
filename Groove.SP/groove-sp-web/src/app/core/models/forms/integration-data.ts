export class IntegrationData {
    constructor(
        // Name, it is like name of control fired event, e.x.: "[po-fulfillment-general-info] modeOfTransportValueChanged", "[po-fulfillment-general-info] movementTypeValueChanged"
        public name: string,
        // Any content to send from issuer to handler
        public content?: any,
        // Some text to describe the event, e.x: , e.x.: "To update source of equipment type on tab load as value of Mode of Transport/Movementype changed"
        public description?: string) {
    }
}
