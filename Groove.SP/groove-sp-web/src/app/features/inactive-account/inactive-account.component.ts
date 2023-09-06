import { Component, OnInit } from '@angular/core';
import { faPhone, faEnvelope } from '@fortawesome/free-solid-svg-icons';
import { environment } from 'src/environments/environment';
import { UserContextService } from 'src/app/core';
import { ActivatedRoute, Params } from '@angular/router';
@Component({
    selector: 'app-inactive-account',
    templateUrl: './inactive-account.component.html',
    styleUrls: ['./inactive-account.component.scss']
})
export class InactiveAccountComponent implements OnInit {
    faPhone = faPhone;
    faEnvelope = faEnvelope;
    inactiveUser: boolean;
    title: string;
    content: string;

    constructor(private userContextService: UserContextService, private route: ActivatedRoute) { }

    ngOnInit() {
        const pageName = this.route.snapshot.data['pageName'];

        if (pageName === 'inactiveRole') {
            this.title = 'INACTIVE USER ROLE';
            this.content = 'User role is inactive';
        } else {
            this.title = 'INACTIVE ACCOUNT';
            this.content = 'This account is inactive';
        }
    }

    close() {
        this.userContextService.logout();
    }
}
