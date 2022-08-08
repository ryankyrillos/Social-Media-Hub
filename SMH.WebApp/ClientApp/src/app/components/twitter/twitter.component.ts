import { Component, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { SMH_API } from 'src/app/api/SMH_API';

@Component({
    templateUrl: './twitter.component.html'
})
export class TwitterComponent implements OnInit {

    items: MenuItem[] = [
        { label: 'Text', icon: 'pi pi-fw pi-book', routerLink: ['/twitter/text'] },
        { label: 'Image', icon: 'pi pi-fw pi-image', routerLink: ['/twitter/image'] },
        { label: 'Video', icon: 'pi pi-fw pi-video', routerLink: ['/twitter/video'] }
    ];

    activeItem: MenuItem = this.items[0];

    isLoggedIn: boolean = false;

    pinCode: string = '';

    constructor(private twClient: SMH_API.Client) {
        if (localStorage.getItem('twIsLoggedIn') === 'true') {
            this.isLoggedIn = true;
            this.activeItem = this.items[0]
            if (window.location.href === "http://localhost:4200/twitter") {
                window.location.href = "http://localhost:4200/twitter/text";
            }
        } else {
            localStorage.setItem('twitterAccessToken', "");
            localStorage.setItem('twitterSecretAccessToken', "");
        }
        localStorage.setItem('twitterPinCode', this.pinCode);
    }

    ngOnInit() {

    }

    onPinChange(newValue) {
        this.pinCode = newValue;
        localStorage.setItem('twitterPinCode', this.pinCode);
    }

    onLoginClick() {
        this.twClient.twitterGet().subscribe(response => {
            this.isLoggedIn = true;
            localStorage.setItem('twIsLoggedIn', "true");
            window.location.reload();
        });
        localStorage.setItem('twitterAccessToken', "");
        localStorage.setItem('twitterSecretAccessToken', "");
    }

    onLogoutClick() {
        this.isLoggedIn = false;
        localStorage.removeItem('twitterAccessToken');
        localStorage.removeItem('twitterSecretAccessToken');
        localStorage.removeItem('twIsLoggedIn');
    }

}
