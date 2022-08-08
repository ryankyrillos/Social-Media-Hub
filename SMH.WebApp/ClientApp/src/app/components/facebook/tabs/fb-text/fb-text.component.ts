import { Component, OnInit } from '@angular/core';
import { SMH_API } from 'src/app/api/SMH_API';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-fb-text',
  templateUrl: './fb-text.component.html',
  providers: [MessageService]
})
export class FbTextComponent implements OnInit {

  pageId: string;

  text: string;

  fb: SMH_API.Facebook = new SMH_API.Facebook();

  fbAuth: SMH_API.FacebookAuth = new SMH_API.FacebookAuth();

  constructor(private fbClient: SMH_API.Client, private messageService: MessageService) {
    this.pageId = "";
    this.text = "";
  }

  ngOnInit(): void {
    if (localStorage.getItem("pageId") !== null) {
      this.pageId = localStorage.getItem("pageId");
    }
  }

  sendPostRequest() {
    localStorage.setItem('pageId', this.pageId);
    this.fb.page_token = localStorage.getItem('pageAccessToken');
    this.fb.page_id = this.pageId;
    this.fb.text = this.text;
    this.fb.media_type = "text";

    this.messageService.add({ severity: 'info', summary: 'Info', detail: 'Processing your request...' });
    this.fbClient.facebookPost(this.fb).subscribe(response => {
      if (response["error"] === undefined) {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Request sent to Facebook' });
      } else {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'There was an error processing your request' });
      }
      this.text = "";
    });
  }

  onPostClick() {
    if (this.pageId == "" || this.text == "") {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fill in all required fields' });
      return;
    }

    this.fbAuth.page_id = this.pageId;
    this.fbAuth.long_access_token = localStorage.getItem("userAccessToken");

    var obj;
    if (localStorage.getItem("pageAccessToken") === "" || localStorage.getItem("pageId") !== this.pageId) {

      this.fbClient.facebookGetPageToken(this.fbAuth).subscribe(response => {
        obj = JSON.stringify(response);
        obj = JSON.parse(obj);

        if(obj["token"] === null)
        {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please check your Facebook page ID' });
        }
        else
        {
          localStorage.setItem('pageAccessToken', obj["token"]);
          this.sendPostRequest();
        }
      });

    } else {
      this.sendPostRequest();
    }
  }
}
