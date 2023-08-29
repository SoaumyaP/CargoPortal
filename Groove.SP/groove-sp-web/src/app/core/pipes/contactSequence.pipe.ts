import { Pipe, PipeTransform } from '@angular/core';
import { OrganizationNameRole, RoleSequence } from '../models/enums/enums';

@Pipe({ name: 'contactSequence', pure: false })
export class ContactSequencePipe implements PipeTransform {
    /**
     * To sort contacts in sequence by organization role.
     * @param contacts
     * @returns 
     */
    transform(contacts: any[]): any {
        if (!contacts) {
            return contacts;
        }
        contacts.forEach(contact => {
            switch (contact.organizationRole)
            {
                case OrganizationNameRole.Principal:
                    contact.contactSequence = RoleSequence.Principal;
                    break;
                case OrganizationNameRole.Shipper:
                    contact.contactSequence = RoleSequence.Shipper;
                    break;
                case OrganizationNameRole.Consignee:
                    contact.contactSequence = RoleSequence.Consignee;
                    break;
                case OrganizationNameRole.NotifyParty:
                    contact.contactSequence = RoleSequence.NotifyParty;
                    break;
                case OrganizationNameRole.AlsoNotify:
                    contact.contactSequence = RoleSequence.AlsoNotifyParty;
                    break;
                case OrganizationNameRole.Supplier:
                    contact.contactSequence = RoleSequence.Supplier;
                    break;
                case OrganizationNameRole.Delegation:
                    contact.contactSequence = RoleSequence.Delegation;
                    break;
                case OrganizationNameRole.Pickup:
                    contact.contactSequence = RoleSequence.PickupAddress;
                    break;
                case OrganizationNameRole.BillingParty:
                    contact.contactSequence = RoleSequence.BillingAddress;
                    break;
                case OrganizationNameRole.OriginAgent:
                    contact.contactSequence = RoleSequence.OriginAgent;
                    break;
                case OrganizationNameRole.DestinationAgent:
                    contact.contactSequence = RoleSequence.DestinationAgent;
                    break;
                default:
                    break;
            }
        });

        contacts.sort((a, b) => {
            if (a.contactSequence < b.contactSequence) {
                return -1;
            }
            if (a.contactSequence > b.contactSequence) {
                return 1;
            }
            return 0;
        });

        return contacts;
    }
}
