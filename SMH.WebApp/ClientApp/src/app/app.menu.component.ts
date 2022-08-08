import { Component, OnInit } from '@angular/core';
import { AppMainComponent } from './app.main.component';

@Component({
    selector: 'app-menu',
    template: `
        <div class="layout-menu-container">
            <ul class="layout-menu" role="menu" (keydown)="onKeydown($event)">
                <li app-menu class="layout-menuitem-category" *ngFor="let item of model; let i = index;" [item]="item" [index]="i" [root]="true" role="none">
                    <div class="layout-menuitem-root-text" [attr.aria-label]="item.label">{{item.label}}</div>
                    <ul role="menu">
                        <li app-menuitem *ngFor="let child of item.items" [item]="child" [index]="i" role="none"></li>
                    </ul>
                </li>
            </ul>
        </div>
    `
})
export class AppMenuComponent implements OnInit {

    model: any[];

    constructor(public appMain: AppMainComponent) { }

    ngOnInit() {
        this.model = [
            {
                label: 'Home',
                items:[
                    {label: 'Home Page',icon: 'pi pi-fw pi-home', routerLink: ['/']}
                ]
            },
            {
                label: 'Social Media',
                items: [
                    {label: 'Facebook', icon: 'pi pi-fw pi-facebook', routerLink: [localStorage.getItem("userAccessToken") !== "" && localStorage.getItem("userAccessToken") !== null ? '/facebook/text' : '/facebook']},
                    {label: 'Instagram', icon: 'pi pi-fw pi-instagram', routerLink: [localStorage.getItem("igUserAccessToken") !== "" && localStorage.getItem("igUserAccessToken") !== null ? '/instagram/image' : '/instagram']},
                    {label: 'WhatsApp', icon: 'pi pi-fw pi-whatsapp', routerLink: [localStorage.getItem("waAccessToken") !== "" && localStorage.getItem("waAccessToken") !== null ? '/whatsapp/template' : '/whatsapp']},
                    {label: 'Twitter', icon: 'pi pi-fw pi-twitter', routerLink: [localStorage.getItem('twIsLoggedIn') === "true"  ? '/twitter/text' : '/twitter']},
                    {label: 'YouTube', icon: 'pi pi-fw pi-youtube', routerLink: ['/youtube'], class: 'rotated-icon'},           
                ]
            }      
        ];
    }

    onKeydown(event: KeyboardEvent) {
        const nodeElement = (<HTMLDivElement> event.target);
        if (event.code === 'Enter' || event.code === 'Space') {
            nodeElement.click();
            event.preventDefault();
        }
    }
}
