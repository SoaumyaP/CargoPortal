import { ElementRef } from "@angular/core";
import { StringHelper } from "../helpers";
import { Incoterm, ModeOfTransport, Movement, PackageUOMType, RoutingOrderStageType, RoutingOrderStatus, UnitUOMType } from "./enums/enums";
import { NoteModel } from "./note.model";

export interface RoutingOrderModel {
    id: number;
    routingOrderNumber: string;
    routingOrderDate: Date;
    cargoReadyDate: Date | null;
    stage: RoutingOrderStageType;
    status: RoutingOrderStatus;
    statusName: string;
    numberOfLineItems: number;
    incoterm: Incoterm;
    modeOfTransport: ModeOfTransport;
    logisticsService: number | null;
    movementType: Movement;
    shipFromId: number;
    shipToId: number;
    shipFromName: string;
    shipToName: string;
    earliestShipDate: string | null;
    expectedShipDate: Date;
    expectedDeliveryDate: Date | null;
    latestShipDate: string | null;
    lastShipmentDate: Date;
    carrierId: number | null;
    vesselName: string;
    voyageNo: string;
    isContainDangerousGoods: boolean;
    isBatteryOrChemical: boolean;
    isCIQOrFumigation: boolean;
    isExportLicence: boolean;
    remarks: string;
    createdBy: string | null;
    createdDate: Date;
    contacts: RoutingOrderContactModel[] | null;
    lineItems: ROLineItemModel[];
    containers: RoutingOrderContainerModel[];
    invoices: RoutingOrderInvoiceModel[];
}

export class RoutingOrderContactModel {
    id: number;
    routingOrderId: number;
    organizationId: number;
    organizationCode: string;
    organizationRole: string;
    companyName: string;
    addressLine1: string;
    addressLine2: string;
    addressLine3: string;
    addressLine4: string;
    contactName: string;
    contactNumber: string;
    contactEmail: string;
    routingOrder: RoutingOrderModel;

    /**The address is combined from all address line. */
    address: string | null;

    /**To mark whether this contact has been removed. */
    removed: boolean | null;

    /**To hide contact from the GUI. */
    hidden: boolean | null;

    /**To mark this contact is inputted manually. */
    isManualInput: boolean | null;

    /**Commonly used for sorting purposes. */
    contactSequence: number | null;

    /**To store role option data source. */
    organizationRoleOptions: any[] | null;
}

export interface ROLineItemModel {
    id: number;
    routingOrderId: number;
    poNo: string;
    itemNo: string;
    descriptionOfGoods: string;
    chineseDescription: string;
    orderedUnitQty: number;
    unitUOM: UnitUOMType;
    bookedPackage: number | null;
    packageUOM: PackageUOMType | null;
    grossWeight: number | null;
    netWeight: number | null;
    volume: number | null;
    hsCode: string;
    commodity: string;
    shippingMarks: string;
    routingOrder: RoutingOrderModel;
}

export interface RoutingOrderContainerModel {
    id: number;
    routingOrderId: number;
    containerType: number;
    quantity: number | null;
    volume: number | null;
    routingOrder: RoutingOrderModel;

    /**To mark whether this container has been removed. */
    removed: boolean | null;

    /**To store equipment type option data source. */
    equipmentTypeOptions: any[] | null;
}

export interface RoutingOrderInvoiceModel {
    id: number;
    routingOrderId: number;
    invoiceType: string;
    invoiceNumber: string;
    routingOrder: RoutingOrderModel;
}

export interface RoutingOrderTabModel {
    /**
     * Text displayed on sticky bar
     */
    text: string;

    /**
     * Indicate if current tab is on focus
     */
    selected: boolean;

    /**
     * Indicate if current tab is view only
     */
    readonly: boolean;

    /**
     * Section element id which links to tab
     */
    sectionId: string;

    /**
     * Section element which links to tab
     */
    sectionElementRef?: ElementRef;
}

export class RoutingOrderNoteModel extends NoteModel {

    static defaultCategory = 'General';

    routingOrderId: number;
    lineItems?: Array<string>;
    isMasterNote: boolean =  false;


    constructor(owner?: string, createdBy?: string, createdDate?: Date, lineItems?: Array<string>, category?: string) {
        super(owner, createdBy, createdDate);

        // Set default value for lineItems options
        if (StringHelper.isNullOrEmpty(lineItems)) {
            this.lineItems = [];
        } else {
            this.lineItems = lineItems;
        }

        // Set default value for category options
        if (StringHelper.isNullOrEmpty(category)) {
            this.category = RoutingOrderNoteModel.defaultCategory;
        } else {
            this.category = category;
        }
    }

    /** Convert from based Note model to Purchase order note */
    public MapFrom(source: NoteModel) {
        this.id = source.id;
        this.category = source.category;
        this.noteText = source.noteText;
        this.lineItems = StringHelper.isNullOrEmpty(source.extendedData) ? [] : JSON.parse(source.extendedData);
        this.extendedData = source.extendedData;
        this.createdDate = source.createdDate;
        this.createdBy = source.createdBy;
        this.owner = source.owner;
    }

    public MapFromMasterNote(source) {
        const {id, category, message, createdDate, createdBy, owner} = source.masterDialog;
        this.isMasterNote = true;
        this.id = id;
        this.category = category;
        this.noteText = message;
        this.lineItems = StringHelper.isNullOrEmpty(source.extendedData) ? [] : JSON.parse(source.extendedData);
        this.extendedData = source.extendedData;
        this.createdDate = createdDate;
        this.createdBy = createdBy;
        this.owner = owner;
    }
}