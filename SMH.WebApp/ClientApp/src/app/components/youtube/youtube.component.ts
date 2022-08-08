import { Component, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { SMH_API } from 'src/app/api/SMH_API';
import { MessageService } from 'primeng/api';

@Component({
    templateUrl: './youtube.component.html',
    providers: [MessageService]
})
export class YoutubeComponent implements OnInit {

    videoPath: string;

    title: string;

    description: string;

    yt: SMH_API.YouTube = new SMH_API.YouTube();

    ngOnInit() {

    }

    constructor(private ytClient: SMH_API.Client, private messageService: MessageService) {
        this.videoPath = "";
        this.title = "";
        this.description = "";
    }

    onPostClick() {
        if (this.videoPath === "") {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fill in all required fields' });
            return;
        }

        this.yt.video_path = this.videoPath.replace(/\\/g, "\\\\");
        this.yt.title = this.title;
        this.yt.description = this.description;

        this.messageService.add({ severity: 'info', summary: 'Info', detail: 'Processing your request. It may take time depending on your video\'s file size' });
        this.ytClient.youTubePostVideo(this.yt).subscribe(response => {
            if ( response == null || response["error"] === undefined) {
                this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Request sent to YouTube' });
            } else {
                this.messageService.add({ severity: 'error', summary: 'Error', detail: 'There was an error processing your request' });
            }
            this.videoPath = "";
            this.title = "";
            this.description = "";
        });
    }
}
