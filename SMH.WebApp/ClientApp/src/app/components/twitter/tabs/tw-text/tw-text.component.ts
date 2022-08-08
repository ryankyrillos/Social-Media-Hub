import { Component, OnInit } from '@angular/core';
import { SMH_API } from 'src/app/api/SMH_API';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-tw-text',
  templateUrl: './tw-text.component.html',
  providers: [MessageService]
})
export class TwTextComponent implements OnInit {

  text: string;

  pinCode: string;

  tw: SMH_API.Twitter = new SMH_API.Twitter();

  twPin: SMH_API.TwitterPin = new SMH_API.TwitterPin();

  constructor(private twClient: SMH_API.Client, private messageService: MessageService) {
    this.text = "";
    this.pinCode = "";
  }

  ngOnInit(): void {

  }

  async onPostClick() {
    this.pinCode = localStorage.getItem('twitterPinCode');

    if (this.text == "" || this.pinCode == "") {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fill in all required fields' });
      return;
    }

    this.twPin.pin_code = this.pinCode;

    if (localStorage.getItem("twitterAccessToken") === "" || localStorage.getItem("twitterSecretAccessToken") === "") {

      var myHeaders = new Headers();
      myHeaders.append("Content-Type", "application/json");

      var raw = JSON.stringify({
        "pin_code": this.pinCode
      });

      var requestOptions = {
        method: 'POST',
        headers: myHeaders,
        body: raw
      };

      await fetch("https://localhost:7130/Twitter/Tokens", requestOptions)
        .then(response => response.text())
        .then(result => {
          var obj = JSON.parse(result);
          if (obj["error"] === undefined) {
            localStorage.setItem('twitterAccessToken', obj["access_token"]);
            localStorage.setItem('twitterSecretAccessToken', obj["access_token_secret"]);
          }
          else {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Your Pin Code is invalid' });
          }
        })
        .catch(error => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Your Pin Code is invalid' });
        });
    }

    if (localStorage.getItem("twitterAccessToken") === "" || localStorage.getItem("twitterSecretAccessToken") === "") {
      return;
    }

    this.tw.access_token = localStorage.getItem('twitterAccessToken');
    this.tw.access_token_secret = localStorage.getItem('twitterSecretAccessToken');
    this.tw.media_type = "text";
    this.tw.text = this.text;

    this.messageService.add({ severity: 'info', summary: 'Info', detail: 'Processing your request...' });
    this.twClient.twitterPost(this.tw).subscribe(response => {
      if (response == null || response["error"] === undefined) {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Request sent to Twitter' });
      } else {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'There was an error processing your request' });
      }
    });
  }
}
