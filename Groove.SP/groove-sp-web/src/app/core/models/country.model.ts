import { ModelBase } from "./model-base.model";

export class CountryModel extends ModelBase {
    id: number;
    code: string;
    name: string;
}
