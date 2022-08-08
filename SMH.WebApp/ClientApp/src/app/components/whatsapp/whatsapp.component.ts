import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { SMH_API } from 'src/app/api/SMH_API';

@Component({
    templateUrl: './whatsapp.component.html',
})
export class WhatsappComponent implements OnInit {
    items: MenuItem[] = [
        { label: 'Template', icon: 'pi pi-fw pi-file', routerLink: ['/whatsapp/template'] },
        { label: 'Text', icon: 'pi pi-fw pi-book', routerLink: ['/whatsapp/text'] },
        { label: 'Image', icon: 'pi pi-fw pi-image', routerLink: ['/whatsapp/image'] },
        { label: 'Video', icon: 'pi pi-fw pi-video', routerLink: ['/whatsapp/video'] },
        { label: 'Sticker', icon: 'pi pi-fw pi-tag', routerLink: ['/whatsapp/sticker'] }
    ];

    activeItem: MenuItem = this.items[0];

    isLoggedIn: boolean = false;

    authCode: string;

    fbCodeRedirect: SMH_API.FacebookCodeRedirect = new SMH_API.FacebookCodeRedirect();

    redirect: SMH_API.Redirect = new SMH_API.Redirect({
        redirect_uri: "https://localhost:7130/WhatsApp/Login",
    });

    constructor(private waClient: SMH_API.Client, private route: ActivatedRoute) {
        this.route.queryParams.subscribe(params => {
            this.authCode = params['code'];

            if (this.authCode !== null && this.authCode !== "" && this.authCode !== undefined) {
                this.fbCodeRedirect.code = this.authCode;
                this.fbCodeRedirect.redirect_uri = this.redirect.redirect_uri;

                this.waClient.whatsAppGetToken(this.fbCodeRedirect).subscribe(response => {
                    this.isLoggedIn = true;
                    localStorage.setItem('waAccessToken', response['token']);
                    window.location.href = "http://localhost:4200/whatsapp/template";
                });
            }
        });
        if (localStorage.getItem("waAccessToken") !== "" && localStorage.getItem("waAccessToken") !== null) {
            this.isLoggedIn = true;
            if (window.location.href === "http://localhost:4200/whatsapp") {
                window.location.href = "http://localhost:4200/whatsapp/template";
            }
        } else {
            localStorage.setItem('waAccessToken', "");
        }
    }

    ngOnInit() {

    }

    onLoginClick() {
        this.waClient.whatsAppLoginPOST(this.redirect).subscribe(response => {
            window.location.href = response['url'];
        });
    }

    onLogoutClick() {
        this.isLoggedIn = false;
        localStorage.removeItem('waAccessToken');
    }
}
