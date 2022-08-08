import { Component, OnInit } from '@angular/core';
import { SMH_API } from 'src/app/api/SMH_API';
import { MessageService } from 'primeng/api';

interface Format {
  label: string,
  format: string,
  location: string
}

@Component({
  selector: 'app-wa-image',
  templateUrl: './wa-image.component.html',
  providers: [MessageService]
})
export class WaImageComponent implements OnInit {

  mediaType: string;

  formats: Format[];

  imageFormat: Format;

  recepientNb: string;

  imagePath: string;

  text: string;

  phoneNbId: string;

  wp: SMH_API.WhatsApp = new SMH_API.WhatsApp();

  constructor(private wpClient: SMH_API.Client, private messageService: MessageService) {
    this.formats = [
      { label: 'PNG (Local)', format: 'png', location: "local" },
      { label: 'JPEG (Local)', format: 'jpeg', location: "local" },
      { label: 'URL (Internet)', format: 'url', location: "internet" },
    ];
    this.mediaType = "";
    this.imageFormat = this.formats[0];
    this.recepientNb = "";
    this.imagePath = "";
    this.text = "";
    this.phoneNbId = "";
  }

  ngOnInit(): void {
    if (localStorage.getItem("phoneNbId") !== null) {
      this.phoneNbId = localStorage.getItem("phoneNbId");
    }
  }

  onPostClick() {
    if (this.phoneNbId == "" || this.recepientNb === "" || this.imagePath === "") {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fill in all required fields' });
      return;
    }

    localStorage.setItem('phoneNbId', this.phoneNbId);

    this.wp.access_token = localStorage.getItem('waAccessToken');
    this.wp.media_type = "image";
    this.wp.media_path = this.imagePath.replace(/\\/g, "\\\\");
    this.wp.phone_number_id = this.phoneNbId;
    this.wp.recepient_number = this.recepientNb;
    this.wp.text = this.text;
    this.wp.media_location = this.imageFormat.location;
    this.wp.media_format = this.imageFormat.format;

    this.messageService.add({ severity: 'info', summary: 'Info', detail: 'Processing your request...' });
    this.wpClient.whatsAppSendMessage(this.wp).subscribe(response => {
      if (response["error"] === undefined) {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Request sent to WhatsApp' });
      } else if (response["error"] === "File doesn't exist on local machine") {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'File not found on current path' });
      } else {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'There was an error processing your request' });
      }
      this.text = "";
      this.imagePath = "";
      this.recepientNb = "";
    });
  }
}
