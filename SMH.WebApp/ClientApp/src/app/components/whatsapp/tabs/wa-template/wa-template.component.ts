import { Component, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { SMH_API } from 'src/app/api/SMH_API';

@Component({
  selector: 'app-wa-template',
  templateUrl: './wa-template.component.html',
  providers: [MessageService]
})
export class WaTemplateComponent implements OnInit {

  mediaType: string;

  recepientNb: string;

  templateName: string;

  templateLangCode: string;

  phoneNbId: string;

  wp: SMH_API.WhatsApp = new SMH_API.WhatsApp();

  constructor(private wpClient: SMH_API.Client, private messageService: MessageService) {
    this.mediaType = "";
    this.recepientNb = "";
    this.templateName = "";
    this.templateLangCode = "";
    this.phoneNbId = "";
  }

  ngOnInit(): void {
    if (localStorage.getItem("phoneNbId") !== null) {
      this.phoneNbId = localStorage.getItem("phoneNbId");
    }
  }

  onPostClick() {
    if (this.phoneNbId == "" || this.templateName == "" || this.templateLangCode == "" || this.recepientNb == "") {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fill in all required fields' });
      return;
    }

    localStorage.setItem('phoneNbId', this.phoneNbId);

    this.wp.access_token = localStorage.getItem('waAccessToken');
    this.wp.media_type = "template";
    this.wp.phone_number_id = this.phoneNbId;
    this.wp.recepient_number = this.recepientNb;
    this.wp.template_name = this.templateName;
    this.wp.template_language_code = this.templateLangCode;

    this.messageService.add({ severity: 'info', summary: 'Info', detail: 'Processing your request...' });
    this.wpClient.whatsAppSendMessage(this.wp).subscribe(response => {
      if (response["error"] === undefined) {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Request sent to WhatsApp' });
      } else {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'There was an error processing your request' });
      }
      this.templateName = "";
      this.templateLangCode = "";
      this.recepientNb = "";
    });
  }
}
