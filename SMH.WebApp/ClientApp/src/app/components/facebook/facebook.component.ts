import { Component } from '@angular/core';
import {TabMenuModule} from 'primeng/tabmenu';
import {MenuItem} from 'primeng/api';

@Component({
    templateUrl: './facebook.component.html'
})
export class FacebookComponent {
    items: MenuItem[];

    activeItem: MenuItem;

    ngOnInit() {
        this.items = [
            {label: 'Text', icon: 'pi pi-fw pi-book', routerLink: ['/facebook/text']},
            {label: 'Image', icon: 'pi pi-fw pi-image', routerLink: ['/facebook/image']},
            {label: 'Video', icon: 'pi pi-fw pi-video', routerLink: ['/facebook/video']}
        ];

        this.activeItem = this.items[0];
    }
}
