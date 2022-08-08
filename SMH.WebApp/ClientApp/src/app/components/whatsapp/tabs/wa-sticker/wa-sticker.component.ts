import { Component, OnInit } from '@angular/core';
import { SMH_API } from 'src/app/api/SMH_API';
import { MessageService } from 'primeng/api';

interface Location {
  label: string,
  location: string
}

@Component({
  selector: 'app-wa-sticker',
  templateUrl: './wa-sticker.component.html',
  providers: [MessageService]
})
export class WaStickerComponent implements OnInit {

  mediaType: string;

  locations: Location[];

  stickerLocation: Location;

  recepientNb: string;

  stickerPath: string;

  phoneNbId: string;

  wp: SMH_API.WhatsApp = new SMH_API.WhatsApp();

  constructor(private wpClient: SMH_API.Client, private messageService: MessageService) {
    this.locations = [
      { label: 'Local', location: "local" },
      { label: 'Internet', location: "internet" }
    ];
    this.mediaType = "";
    this.stickerLocation = this.locations[0];
    this.recepientNb = "";
    this.stickerPath = "";
    this.phoneNbId = "";
  }

  ngOnInit(): void {
    if (localStorage.getItem("phoneNbId") !== null) {
      this.phoneNbId = localStorage.getItem("phoneNbId");
    }
  }

  onPostClick() {
    if (this.phoneNbId == "" || this.recepientNb === "" || this.stickerPath === "") {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fill in all required fields' });
      return;
    }

    localStorage.setItem('phoneNbId', this.phoneNbId);

    this.wp.access_token = localStorage.getItem('waAccessToken');
    this.wp.media_type = "sticker";
    this.wp.media_path = this.stickerPath.replace(/\\/g, "\\\\");
    this.wp.phone_number_id = this.phoneNbId;
    this.wp.recepient_number = this.recepientNb;
    this.wp.media_location = this.stickerLocation.location;

    this.messageService.add({ severity: 'info', summary: 'Info', detail: 'Processing your request...' });
    this.wpClient.whatsAppSendMessage(this.wp).subscribe(response => {
      if (response["error"] === undefined) {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Request sent to WhatsApp' });
      } else if (response["error"] === "File doesn't exist on local machine") {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'File not found on current path' });
      } else {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'There was an error processing your request' });
      }
      this.stickerPath = "";
      this.recepientNb = "";
    });
  }
}
