import { Component, OnInit } from '@angular/core';
import { SMH_API } from 'src/app/api/SMH_API';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-ig-video',
  templateUrl: './ig-video.component.html',
  providers: [MessageService]
})
export class IgVideoComponent implements OnInit {

  caption: string;

  mediaUrl: string;

  fbPageID: string;

  ig: SMH_API.Instagram = new SMH_API.Instagram();

  igAuth: SMH_API.InstagramAuth = new SMH_API.InstagramAuth();

  constructor(private igClient: SMH_API.Client, private messageService: MessageService) {
    this.caption = "";
    this.mediaUrl = "";
  }

  ngOnInit(): void {
    if (localStorage.getItem("fbPageId") !== null) {
      this.fbPageID = localStorage.getItem("fbPageId");
    }
  }

  sendPostRequest() {
    localStorage.setItem("fbPageId", this.fbPageID);
    this.ig.page_token = localStorage.getItem('fbPageAccessToken');
    this.ig.facebook_page_id = this.fbPageID;
    this.ig.caption = this.caption;
    this.ig.media_type = "video";
    this.ig.media_url = this.mediaUrl;

    this.messageService.add({ severity: 'info', summary: 'Info', detail: 'Processing your request. It may take time depending on your video\'s file size' });
    this.igClient.instagramPost(this.ig).subscribe(response => {
      if (response["error"] === undefined) {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Request sent to Facebook' });
      } else {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'There was an error processing your request' });
      }
      this.mediaUrl = "";
      this.caption = "";
    });
  }

  onPostClick() {
    if (this.mediaUrl === "" || this.fbPageID === "") {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fill in all required fields' });
      return;
    }

    this.igAuth.facebook_page_id = this.fbPageID;
    this.igAuth.long_access_token = localStorage.getItem("igUserAccessToken");

    var obj;
    if (localStorage.getItem("fbPageAccessToken") === null || localStorage.getItem("fbPageAccessToken") === "" || localStorage.getItem("fbPageId") !== this.fbPageID) {

      this.igClient.instagramGetPageToken(this.igAuth).subscribe(response => {
        obj = JSON.stringify(response);
        obj = JSON.parse(obj);

        if(obj["token"] === null)
        {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please check your Facebook page ID' });
        }
        else
        {
          localStorage.setItem('fbPageAccessToken', obj["token"]);
          this.sendPostRequest();
        }
      });

    } else {

      this.sendPostRequest();

    }
  }
}
