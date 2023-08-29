/**
 * - Decoration for class which must contain constructor method.
 * - It works as creating new object. Ex.: const obj = new CruiseOrderModel();
 * - To enable auto map run-time object to provided model type, please 1. set value for every property = undefined or 2. set default value.
 * @example
 * 1.lineEstimatedDeliveryDate: string | null = undefined;
 * 2.id: number = 0;
 */
export function Model<T extends ConstructorBase>(constructor: T) {
    return class extends constructor {
        constructor(...args: any[]) {
            // call default to create new instance
            super();
            // call method to initialize data
            this.initObject(args[0]);
        }
    };
}

/**
 * - Decoration for property of class which must contain constructor method.
 * - It will define how to initialize property with provided data type.
 * - Data type is defined by declaration. Ex.: @DataType(Number) id: number = undefined;
 */
export function DataType(constructor?: ConstructorBase) {
    return (instance: any, propertyKey: string) => {
        if (constructor) {
            Reflect.defineMetadata(
                CONSTRUCTOR_META,
                constructor,
                instance,
                propertyKey
            );
        }
    };
}

/**
 * Base of model, it contains constructor method and audit properties.
 */
export class ModelBase {
    rowVersion: string;
    createdBy: string;
    createdDate: string;
    updatedBy: string;
    updatedDate: string | null;

    /**
     * Constructor method.
     */
    constructor(payload?: any) {

    }

    /**
     * Method to initialize object, allow to override if needed.
     */
    public initObject(payload: any) {
        // tslint:disable-next-line:forin
        for (const key in payload) {
            if (this.hasOwnProperty(key)) {
                const constructorMeta: ConstructorBase = Reflect.getMetadata(
                    CONSTRUCTOR_META,
                    this,
                    key
                );

                const designType = Reflect.getMetadata(
                    'design:type',
                    this,
                    key
                );
                const factory = constructorMeta || designType;

                const isArray = designType === Array;

                const value = isArray
                    ? payload[key].map((v: any) => this._parseValue(v, factory))
                    : this._parseValue(payload[key], factory);

                (this as IndexableBase)[key] = value;
            }
        }
    }

    /**
     * To parse value from source to destination.
     */
    private _parseValue(sourceValue: any, destinationDataType: ConstructorBase) {
        if (destinationDataType) {
            // Make it sample currently
            return new destinationDataType(sourceValue);

            // if (factory === Date) {
            //     return new factory(value);
            // } else if (factory === LocalDate) {
            //     return new factory(value);
            // } else if (factory.prototype instanceof ModelBase.constructor) {
            //     return new factory(value);
            // } else {
            //     return new factory(value);
            // }
        }
        return sourceValue;
    }
}

const CONSTRUCTOR_META = Symbol('CONSTRUCTOR_META');

/**
 * Type that contains constructor method.
 */
// tslint:disable-next-line:interface-over-type-literal
export type ConstructorBase<T = any> = { new (...args: any[]): T };

/**
 * Type that is able is access property be index.
 * - Ex.: complexValue['propertyA'].
 */
// tslint:disable-next-line:interface-over-type-literal
export type IndexableBase = { [key: string]: any };

