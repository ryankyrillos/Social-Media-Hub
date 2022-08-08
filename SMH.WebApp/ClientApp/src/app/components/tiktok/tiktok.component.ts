import { Component, OnInit, ViewChild, ChangeDetectorRef, ElementRef } from '@angular/core';
import { Customer, Representative } from '../../api/customer';
import { CustomerService } from '../../service/customerservice';
import { Product } from '../../api/product';
import { ProductService } from '../../service/productservice';
import { Table } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api'

@Component({
    templateUrl: './tiktok.component.html',
    providers: [MessageService, ConfirmationService],
    styleUrls: ['../../../assets/demo/badges.scss'],
})
export class TiktokComponent implements OnInit {
    ngOnInit() {
        
    }
}
