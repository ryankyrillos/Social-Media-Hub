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
                    {label: 'Facebook', icon: 'pi pi-fw pi-facebook', routerLink: ['/facebook']},
                    {label: 'Instagram', icon: 'pi pi-fw pi-instagram', routerLink: ['/instagram']},
                    {label: 'Whatsapp', icon: 'pi pi-fw pi-whatsapp', routerLink: ['/whatsapp']},
                    {label: 'Twitter', icon: 'pi pi-fw pi-twitter', routerLink: ['/twitter']},
                    {label: 'Youtube', icon: 'pi pi-fw pi-youtube', routerLink: ['/youtube'], class: 'rotated-icon'},
                    {label: 'Tiktok', icon: 'tiktok', routerLink: ['/tiktok']},                    
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
