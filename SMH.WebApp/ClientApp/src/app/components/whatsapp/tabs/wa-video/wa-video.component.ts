import { Component, OnInit } from '@angular/core';
import { SMH_API } from 'src/app/api/SMH_API';
import { MessageService } from 'primeng/api';

interface Format {
  label: string,
  format: string,
  location: string
}

@Component({
  selector: 'app-wa-video',
  templateUrl: './wa-video.component.html',
  providers: [MessageService]
})
export class WaVideoComponent implements OnInit {

  mediaType: string;

  formats: Format[];

  videoFormat: Format;

  recepientNb: string;

  videoPath: string;

  text: string;

  phoneNbId: string;

  wp: SMH_API.WhatsApp = new SMH_API.WhatsApp();

  constructor(private wpClient: SMH_API.Client, private messageService: MessageService) {
    this.formats = [
      { label: 'MP4 (Local)', format: 'mp4', location: "local" },
      { label: '3GPP (Local)', format: '3gpp', location: "local" },
      { label: 'URL (Internet)', format: 'url', location: "internet" },
    ];
    this.mediaType = "";
    this.videoFormat = this.formats[0];
    this.recepientNb = "";
    this.videoPath = "";
    this.text = "";
    this.phoneNbId = "";
  }

  ngOnInit(): void {
    if (localStorage.getItem("phoneNbId") !== null) {
      this.phoneNbId = localStorage.getItem("phoneNbId");
    }
  }

  onPostClick() {
    if (this.phoneNbId == "" || this.recepientNb === "" || this.videoPath === "") {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fill in all required fields' });
      return;
    }

    localStorage.setItem('phoneNbId', this.phoneNbId);

    this.wp.access_token = localStorage.getItem('waAccessToken');
    this.wp.media_type = "video";
    this.wp.media_path = this.videoPath.replace(/\\/g, "\\\\");
    this.wp.phone_number_id = this.phoneNbId;
    this.wp.recepient_number = this.recepientNb;
    this.wp.text = this.text;
    this.wp.media_location = this.videoFormat.location;
    this.wp.media_format = this.videoFormat.format;

    this.messageService.add({ severity: 'info', summary: 'Info', detail: 'Processing your request. It may take time depending on your video\'s file size' });
    this.wpClient.whatsAppSendMessage(this.wp).subscribe(response => {
      if (response["error"] === undefined) {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Request sent to WhatsApp' });
      } else if (response["error"] === "File doesn't exist on local machine") {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'File not found on current path' });
      } else {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'There was an error processing your request' });
      }
      this.videoPath = "";
      this.text = "";
      this.recepientNb = "";
    });
  }
}
