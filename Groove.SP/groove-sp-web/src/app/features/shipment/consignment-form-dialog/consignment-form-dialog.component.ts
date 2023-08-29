import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { DropDowns, UserContextService, FormComponent, StringHelper } from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { ConsignmentFormDialogService } from './consignment-form-dialog.service';

@Component({
    selector: 'app-consignment-form-dialog',
    templateUrl: './consignment-form-dialog.component.html'
})
export class ConsignmentFormDialogComponent extends FormComponent implements OnInit {
    @Input() public consignmentFormDialogOpened: boolean = false;
    @Input() public consignments: any;

    @Output() close: EventEmitter<any> = new EventEmitter<any>();
    @Output() add: EventEmitter<any> = new EventEmitter<any>();
    @Output() edit: EventEmitter<any> = new EventEmitter<any>();

    model: any;
    modeOfTransportOptions = DropDowns.ModeOfTransportStringType;
    allLocationOptions: any[];
    filteredLocationOptions: any[];
    filteredExecutionAgentOptions: any[];
    executionAgentOptions: any[];
    executionAgentName: any;
    currentUser: any;
    loginOrganizationName: string;

    validationRules = {
        sequence: {
            required: 'label.sequence'
        },
        modeOfTransport: {
            required: 'label.modeOfTransport'
        },
        executionAgent: {
            required: 'label.executionAgent',
            alreadyUsed: 'validation.executionAgentAlreadyUsed'
        },
        consignmentDate: {
            required: 'label.consignmentDates'
        },
        shipFromETDDate: {
            required: 'label.shipFromETDDates'
        },
        shipToETADate: {
            required: 'label.shipToETADates'
        }
    };

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: ConsignmentFormDialogService,
        public translateService: TranslateService,
        private _userContext: UserContextService) {
        super(route, service, notification, translateService, router);
    }

    async ngOnInit() {
        this._userContext.getCurrentUser().subscribe(async user => {
            if (user) {
                this.currentUser = user;
                if (!this.currentUser.isInternal) {
                    this.service.affiliateCodes = this.currentUser.affiliates;
                    const loginOrganization = await this.service.getOrganization(this.currentUser.organizationId).toPromise();
                    this.loginOrganizationName = loginOrganization.name;
                }

                this.model = this.initModel();
            }
        });

        this.service.getOrganizations().subscribe(data => {
            this.executionAgentOptions = data;
            this.executionAgentOptions.forEach(item => {
                item.label = item.code + ' - ' + item.name;
            });
        });

        this.service.getAllLocations().subscribe(data => {
            this.allLocationOptions = data;
        });
    }

    private initModel() {
        if (this.currentUser.isInternal) {
            return {
                consignmentDate: new Date()
            };
        }

        return {
            consignmentDate: new Date(),
            executionAgentId: this.currentUser.organizationId,
            executionAgentName: this.loginOrganizationName
        };
    }

    executionAgentFilterChange(value: string) {
        this.filteredExecutionAgentOptions = [];
        if (value.length >= 3) {
            this.filteredExecutionAgentOptions = this.executionAgentOptions.filter((s) => s.label.toLowerCase()
                .indexOf(value.toLowerCase()) !== -1);
        }
    }

    executionAgentValueChange(agentName: string) {
        this.model.executionAgentId = null;
        this.setValidControl("executionAgent");
        if (StringHelper.isNullOrEmpty(agentName)) {
            this.setInvalidControl("executionAgent", "required");
            return;
        }
        const executionAgentOrganization = this.executionAgentOptions.find(x => x.label.toLowerCase() === agentName.toLowerCase());
        if (executionAgentOrganization) {
            this.model.executionAgentId = executionAgentOrganization.id;
            this.model.executionAgentName = agentName;
            this.executionAgentName = executionAgentOrganization.name;

            // Cannot add multiple consignments for the same Execution Agent
            if (!this.isValidExecutionAgent(agentName)) {
                this.setInvalidControl("executionAgent", "alreadyUsed");
            }
        }
    }

    /** Return false if the Execution Agent has already been used */
    private isValidExecutionAgent(agentName: string) {
        if (StringHelper.isNullOrEmpty(agentName)) {
            return true;
        }
        const selectedExecutionAgents = this.consignments.map(x => x.executionAgentId);
        return !selectedExecutionAgents?.includes(this.model.executionAgentId);
    }

    locationFilterChange(value) {
        this.filteredLocationOptions = [];
        if (value.length >= 3) {
            this.filteredLocationOptions = this.allLocationOptions.filter((s) => s.locationDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        }
    }

    onFormClosed() {
        this.resetCurrentForm();
        this.consignmentFormDialogOpened = false;
        this.model = this.initModel();
        this.close.emit();
    }

    onAddClick() {
        this.validateAllFields(false);

        if (!this.mainForm.valid) {
            return;
        }

        this.model.executionAgentName = this.executionAgentName;

        this.model.status = 'Active';
        this.add.emit(this.model);
        this.model = this.initModel();
    }

    public get isAllowAddConsignment() {
        if (!this.currentUser?.isInternal) {
            return this.isValidExecutionAgent(this.loginOrganizationName);
        }

        return true;
    }
}
