import { Component, Input, OnInit, QueryList, ViewChildren } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MultiSelectComponent } from '@progress/kendo-angular-dropdowns';
import { AgentAssignmentMethodType, AgentType, DropDowns, ModeOfTransport, ModeOfTransportType, StringHelper } from 'src/app/core';
import { ComplianceFormService } from '../compliance-form.service';

@Component({
  selector: 'app-agent-assignment',
  templateUrl: './agent-assignment.component.html',
  styleUrls: ['./agent-assignment.component.scss']
})
export class AgentAssignmentComponent implements OnInit {
  @Input() agentAssignments: any;
  @Input() formErrors: any;
  @Input() isViewMode: boolean;
  @Input() countryList = [];
  @Input() mainAllLocationOptions;
  @Input() agentOrgList;
  @Input() cargoLoadabilities;

  @ViewChildren('airAgentPorts') agentPorts: QueryList<MultiSelectComponent>;

  agentTypeOption = DropDowns.AgentType;
  agentType = AgentType;
  agentAssignmentMethodType = AgentAssignmentMethodType;
  autoCreateShipmentOption = DropDowns.YesNoType;
  agentOrgFilter: any;

  constructor(
    public translateService: TranslateService,
    private service: ComplianceFormService
  ) { }

  ngOnInit() {

  }

  onAgentTypeChange(value, rowIndex) {
    if (value === AgentType.Destination) {
      this.agentAssignments[rowIndex].autoCreateShipment = null;
      this.formErrors['airAgentValidationRules'][rowIndex].autoCreateShipment.required = '';
    }
    this.checkAgentAgentTypeValidates(rowIndex);
  }

  checkAgentCountryValidates(rowIndex) {
    if (this.agentAssignments.length === 0) {
      return;
    }
    const currentRow = this.agentAssignments[rowIndex];
    if (StringHelper.isNullOrEmpty(currentRow.countryId)) {
      this.formErrors['airAgentValidationRules'][rowIndex].countryId.required =
        this.translateService.instant('validation.requiredField',
          {
            fieldName: this.translateService.instant('label.country')
          });
      return;
    }
    this.formErrors['airAgentValidationRules'][rowIndex].countryId.required = '';
  }

  checkAgentAutoCreateShipmentValidates(rowIndex) {
    if (this.agentAssignments.length === 0) {
      return;
    }
    const currentRow = this.agentAssignments[rowIndex];
    if (StringHelper.isNullOrEmpty(currentRow.autoCreateShipment)) {
      this.formErrors['airAgentValidationRules'][rowIndex].autoCreateShipment.required =
        this.translateService.instant('validation.requiredField',
          {
            fieldName: this.translateService.instant('label.autoCreateShipment')
          });
      return;
    }
    this.formErrors['airAgentValidationRules'][rowIndex].autoCreateShipment.required = '';
  }

  checkAgentAgentTypeValidates(rowIndex) {
    if (this.agentAssignments.length === 0) {
      return;
    }
    const currentRow = this.agentAssignments[rowIndex];
    if (StringHelper.isNullOrEmpty(currentRow.agentType)) {
      this.formErrors['airAgentValidationRules'][rowIndex].agentType.required =
        this.translateService.instant('validation.requiredField',
          {
            fieldName: this.translateService.instant('label.agentType')
          });
      return;
    }
    this.formErrors['airAgentValidationRules'][rowIndex].agentType.required = '';
  }

  onCountryChange(value, rowIndex) {
    const countryId = value.toString();
    this.agentAssignments[rowIndex].portLocations = this.mainAllLocationOptions
      .filter(s => s.locationiId && s.countryId === countryId);
    this.agentAssignments[rowIndex].portSelectionIds = [];
    this.checkAgentCountryValidates(rowIndex);
  }

  onPortSelectionFilterChange(value, rowIndex) {
    const agentPortComps = this.agentPorts.toArray();
    if (value.length >= 3) {
      this.agentAssignments[rowIndex].portLocations = this.mainAllLocationOptions
        .filter(s => s.locationiId && s.countryId === this.agentAssignments[rowIndex].countryId.toString() &&
          s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    } else if (!value) {
      this.agentAssignments[rowIndex].portLocations = this.mainAllLocationOptions
        .filter(s => s.locationiId && s.countryId === this.agentAssignments[rowIndex].countryId.toString());
      agentPortComps[rowIndex - 2].toggle(true);
    } else {
      agentPortComps[rowIndex - 2].toggle(false);
    }
  }

  isPortAgentSelected(description, rowIndex): boolean {
    return this.agentAssignments[rowIndex].portSelectionIds.some(item => item === description);
  }

  agentOrgValueChange(value, rowIndex) {
    const selectedItem = this.agentOrgList.find(
      (element) => {
        return element.name === value;
      });

    if (StringHelper.isNullOrEmpty(selectedItem)) {
      this.agentAssignments[rowIndex].agentOrganizationId = null;
      this.agentAssignments[rowIndex].agentOrganizationName = '';
      if (!StringHelper.isNullOrEmpty(value)) {
        this.formErrors['airAgentValidationRules'][rowIndex].agentOrganizationId.required =
          this.translateService.instant('validation.requiredField',
            {
              fieldName: this.translateService.instant('label.organization')
            });
      }
      return;
    }
    this.formErrors['airAgentValidationRules'][rowIndex].agentOrganizationId.required = '';
    this.formErrors['airAgentValidationRules'][rowIndex].agentOrganizationId.notExists = '';
    this.service.checkAgentOrgHasContactEmail(selectedItem.id).subscribe(data => {
      if (!data) {
        this.formErrors['airAgentValidationRules'][rowIndex].agentOrganizationId.notExists =
          this.translateService.instant('validation.assignAgentMissingContactEmail');
        return;
      }

      if (!StringHelper.isNullOrEmpty(selectedItem)) {
        this.agentAssignments[rowIndex].agentOrganizationId = selectedItem.id;
        this.agentAssignments[rowIndex].agentOrganizationName = selectedItem.name;
        this.formErrors['airAgentValidationRules'][rowIndex].agentOrganizationId.required = '';
        this.formErrors['airAgentValidationRules'][rowIndex].agentOrganizationId.notExists = '';
      }
    });
  }

  agentOrgFilterChange(value) {
    this.agentOrgFilter = [];
    if (value.length >= 3) {
      this.agentOrgFilter = this.agentOrgList.filter((s) => s.name.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    }
  }

  checkAgentOrgValidates(rowIndex) {
    if (this.agentAssignments.length === 0) {
      return;
    }
    const currentRow = this.agentAssignments[rowIndex];
    if (StringHelper.isNullOrEmpty(currentRow.agentOrganizationId)) {
      this.formErrors['airAgentValidationRules'][rowIndex].agentOrganizationId.required =
        this.translateService.instant('validation.requiredField',
          {
            fieldName: this.translateService.instant('label.organization')
          });
      return;
    }
    this.formErrors['airAgentValidationRules'][rowIndex].agentOrganizationId.required = '';
  }

  addBlankAssignmentRow() {
    if (!this.agentAssignments) {
      this.agentAssignments = [];
    }
    this.agentAssignments.push({
      portSelectionIds: [],
      countryId: null,
      order: this.agentAssignments.length + 1,
      modeOfTransport: ModeOfTransportType.Air
    });
    // Using rowIndex as a key
    const rowIndex = this.agentAssignments.length - 1;
    this.formErrors['airAgentValidationRules'][rowIndex] = this.getAgentGridValidation();
  }

  removeAssignment(index) {
    this.agentAssignments.splice(index, 1);
    this.agentAssignments = Object.assign([], this.agentAssignments);
  }

  getAgentGridValidation() {
    return {
      autoCreateShipment: {
        required: ''
      },
      countryId: {
        required: '',
      },
      agentType: {
        required: '',
      },
      agentOrganizationId: {
        required: '',
        notExists: ''
      }
    };
  }

  validate() {
      let gridValid = true;

      // check destination org input
      if (StringHelper.isNullOrEmpty(this.agentAssignments[1].agentOrganizationId) || this.agentAssignments[1].agentOrganizationId === 0) {
        this.checkAgentOrgValidates(1);
        gridValid = false;
      }
      for (let i = 2; i < this.agentAssignments.length; i++) {
        const agentAssignment = this.agentAssignments[i];
        if (StringHelper.isNullOrEmpty(agentAssignment.agentType) ||
          StringHelper.isNullOrEmpty(agentAssignment.countryId) ||
          StringHelper.isNullOrEmpty(agentAssignment.agentOrganizationId) ||
          agentAssignment.agentOrganizationId === 0) {
          gridValid = false;
        }
        if (StringHelper.isNullOrEmpty(agentAssignment.agentType) || agentAssignment.agentType === AgentType.Origin) {
          if (StringHelper.isNullOrEmpty(agentAssignment.autoCreateShipment)) {
            gridValid = false;
          }
          this.checkAgentAutoCreateShipmentValidates(i);
        }

        this.checkAgentAgentTypeValidates(i);
        this.checkAgentCountryValidates(i);
        this.checkAgentOrgValidates(i);
      }

    return gridValid;
    }
}
