import { Component, OnInit } from '@angular/core';
import { faTiktok } from '@fortawesome/free-brands-svg-icons'; 

@Component({
    templateUrl: './homepage.component.html',
})
export class HomepageComponent implements OnInit {

    faTikTok = faTiktok;

    ngOnInit() {
        
    }
}