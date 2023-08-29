import { CountryModel } from "./country.model";
import { ModelBase } from "./model-base.model";

export class LocationModel extends ModelBase {
    id: number;
    /** Location code */
    name: string;
    /** Location name or location description */
    locationDescription: string;
    countryId: number;
    ediSonPortCode: string;
    country: CountryModel;
}
