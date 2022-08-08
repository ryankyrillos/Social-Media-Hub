import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { SMH_API } from 'src/app/api/SMH_API';

@Component({
	templateUrl: './instagram.component.html',
	styles: [`:host ::ng-deep .p-multiselect {
		min-width: 15rem;
	}

	:host ::ng-deep .multiselect-custom-virtual-scroll .p-multiselect {
		min-width: 20rem;
	}

	:host ::ng-deep .multiselect-custom .p-multiselect-label,  {
		padding-top: 0.75rem;
		padding-bottom: 0.75rem;

	}

	:host ::ng-deep .multiselect-custom .country-item.country-item-value {
		padding: .25rem .5rem;
		border-radius: 3px;
		display: inline-flex;
		margin-right: .5rem;
		background-color: var(--primary-color);
		color: var(--primary-color-text);
	}

	:host ::ng-deep .multiselect-custom .country-item.country-item-value img.flag {
		width: 17px;
	}

	:host ::ng-deep .multiselect-custom .country-item {
		display: flex;
		align-items: center;
	}

	:host ::ng-deep .multiselect-custom .country-item img.flag {
		width: 18px;
		margin-right: .5rem;
	}

	:host ::ng-deep .multiselect-custom .country-placeholder {
		padding: 0.25rem;
	}

    `]
})
export class InstagramComponent implements OnInit {
	items: MenuItem[] = [
		{ label: 'Image', icon: 'pi pi-fw pi-image', routerLink: ['/instagram/image'] },
		{ label: 'Video', icon: 'pi pi-fw pi-video', routerLink: ['/instagram/video'] }
	];

	activeItem: MenuItem = this.items[0];

	isLoggedIn: boolean = false;

	authCode: string;

	fbCodeRedirect: SMH_API.FacebookCodeRedirect = new SMH_API.FacebookCodeRedirect();

	redirect: SMH_API.Redirect = new SMH_API.Redirect({
		redirect_uri: "http://localhost:4200/instagram",
	});

	constructor(private igClient: SMH_API.Client, private route: ActivatedRoute) {
		this.route.queryParams.subscribe(params => {
			this.authCode = params['code'];

			if (this.authCode !== null && this.authCode !== "" && this.authCode !== undefined) {
				this.fbCodeRedirect.code = this.authCode;
				this.fbCodeRedirect.redirect_uri = this.redirect.redirect_uri;

				this.igClient.instagramGetAccessToken(this.fbCodeRedirect).subscribe(response => {
					this.isLoggedIn = true;
					localStorage.setItem('igUserAccessToken', response['token']);
					window.location.href = "http://localhost:4200/instagram/image";
				});
			}
		});
		if (localStorage.getItem("igUserAccessToken") !== "" && localStorage.getItem("igUserAccessToken") !== null) {
			this.isLoggedIn = true;
			if (window.location.href === "http://localhost:4200/instagram") {
				window.location.href = "http://localhost:4200/instagram/image";
			}
		} else {
			localStorage.setItem('igUserAccessToken', "");
		}
		localStorage.setItem('fbPageAccessToken', "");
	}

	ngOnInit() {

	}

	onLoginClick() {
		this.igClient.instagramLogin(this.redirect).subscribe(response => {
			window.location.href = response['url'];
		});
	}

	onLogoutClick() {
		this.isLoggedIn = false;
		localStorage.removeItem('igUserAccessToken');
	}
}
