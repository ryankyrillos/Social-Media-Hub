import { Component, OnInit } from '@angular/core';
import { SMH_API } from 'src/app/api/SMH_API';
import { MessageService } from 'primeng/api';

interface Type {
  label: string,
  code: string
}

@Component({
  selector: 'app-fb-image',
  templateUrl: './fb-image.component.html',
  providers: [MessageService]
})
export class FbImageComponent implements OnInit {

  locations: Type[];

  imageLocation: Type;

  pageId: string;

  imageUrl: string;

  text: string;

  fb: SMH_API.Facebook = new SMH_API.Facebook();

  fbAuth: SMH_API.FacebookAuth = new SMH_API.FacebookAuth();

  constructor(private fbClient: SMH_API.Client, private messageService: MessageService) {
    this.locations = [
      { label: 'Internet', code: 'internet' },
      { label: 'Local Device', code: 'local' },
    ];
    this.imageLocation = this.locations[0];
    this.pageId = "";
    this.imageUrl = "";
    this.text = "";
    this.fbClient = fbClient;
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
    this.fb.media_type = "image";
    this.fb.media_location = this.imageLocation.code;
    this.fb.media_path = this.imageUrl;

    this.messageService.add({ severity: 'info', summary: 'Info', detail: 'Processing your request...' });
    this.fbClient.facebookPost(this.fb).subscribe(response => {
      if (response["error"] === undefined) {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Request sent to Facebook' });
      } else if (response["error"] === "File doesn't exist on local machine") {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'File not found on current path' });
      } else {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'There was an error processing your request' });
      }
      this.text = "";
      this.imageUrl = "";
    });
  }

  onPostClick() {
    if (this.imageUrl === "" || this.pageId === "") {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fill in all required fields' });
      return;
    }

    this.fbAuth.page_id = this.pageId;
    this.fbAuth.long_access_token = localStorage.getItem("userAccessToken");

    var obj;
    if (localStorage.getItem("pageAccessToken") === null || localStorage.getItem("pageId") !== this.pageId) {

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
