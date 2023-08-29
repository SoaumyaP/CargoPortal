import { AgentType } from 'src/app/core';

export class AgentAssignmentModel {
    autoCreateShipment: number | null;
    agentType: AgentType;
    countryId: number | null;
    portSelectionIds: string[];
    agentOrganizationId: number;
    order: number;
}