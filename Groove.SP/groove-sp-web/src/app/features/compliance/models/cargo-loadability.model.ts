import { EquipmentType } from 'src/app/core';

export class CargoLoadabilityModel {
    name: string;
    equipmentType: EquipmentType;
    cyMinimumCBM: number;
    cyMaximumCBM: number;
    cfsMinimumCBM: number | null;
    cfsMaximumCBM: number | null;
}