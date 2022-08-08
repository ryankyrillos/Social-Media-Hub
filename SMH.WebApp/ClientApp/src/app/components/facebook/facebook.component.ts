import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { SMH_API } from 'src/app/api/SMH_API';

@Component({
    templateUrl: './facebook.component.html'
})
export class FacebookComponent {
    items: MenuItem[];

    activeItem: MenuItem;

    isLoggedIn: boolean = false;

    authCode: string;

    fbCodeRedirect: SMH_API.FacebookCodeRedirect = new SMH_API.FacebookCodeRedirect();

    redirect: SMH_API.Redirect = new SMH_API.Redirect({
        redirect_uri: "http://localhost:4200/facebook",
    });

    constructor(private fbClient: SMH_API.Client, private route: ActivatedRoute) {
        this.route.queryParams.subscribe(params => {
            this.authCode = params['code'];

            if (this.authCode !== null && this.authCode !== "" && this.authCode !== undefined) {
                this.fbCodeRedirect.code = this.authCode;
                this.fbCodeRedirect.redirect_uri = this.redirect.redirect_uri;

                this.fbClient.facebookGetAccessToken(this.fbCodeRedirect).subscribe(response => {
                    this.isLoggedIn = true;
                    localStorage.setItem('userAccessToken', response['token']);
                    window.location.href = "http://localhost:4200/facebook/text";
                });
            }
        });
    }

    ngOnInit() {
        if (localStorage.getItem("userAccessToken") !== "" && localStorage.getItem("userAccessToken") !== null) {
            this.isLoggedIn = true;
            if (window.location.href === "http://localhost:4200/facebook") {
				window.location.href = "http://localhost:4200/facebook/text";
			}
        } else {
            localStorage.setItem('userAccessToken', "");
        }
        this.items = [
            { label: 'Text', icon: 'pi pi-fw pi-book', routerLink: ['/facebook/text'] },
            { label: 'Image', icon: 'pi pi-fw pi-image', routerLink: ['/facebook/image'] },
            { label: 'Video', icon: 'pi pi-fw pi-video', routerLink: ['/facebook/video'] }
        ];
        this.activeItem = this.items[0];
        localStorage.setItem('pageAccessToken', "");
    }

    onLoginClick() {
        this.fbClient.facebookLogin(this.redirect).subscribe(response => {
            window.location.href = response['url'];
        });
    }

    onLogoutClick() {
        this.isLoggedIn = false;
        localStorage.clear();
    }

}
