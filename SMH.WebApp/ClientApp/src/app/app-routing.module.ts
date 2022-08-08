import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { HomepageComponent } from './components/homepage/homepage.component';
import { FacebookComponent } from './components/facebook/facebook.component';
import { InstagramComponent } from './components/instagram/instagram.component';
import { YoutubeComponent } from './components/youtube/youtube.component';
import { WhatsappComponent } from './components/whatsapp/whatsapp.component';
import { TwitterComponent } from './components/twitter/twitter.component';
import { AppMainComponent } from './app.main.component';
import { FbTextComponent } from './components/facebook/tabs/fb-text/fb-text.component';
import { FbImageComponent } from './components/facebook/tabs/fb-image/fb-image.component';
import { FbVideoComponent } from './components/facebook/tabs/fb-video/fb-video.component';
import { IgImageComponent } from './components/instagram/tabs/ig-image/ig-image.component';
import { IgVideoComponent } from './components/instagram/tabs/ig-video/ig-video.component';
import { WaTemplateComponent } from './components/whatsapp/tabs/wa-template/wa-template.component';
import { WaTextComponent } from './components/whatsapp/tabs/wa-text/wa-text.component';
import { WaImageComponent } from './components/whatsapp/tabs/wa-image/wa-image.component';
import { WaVideoComponent } from './components/whatsapp/tabs/wa-video/wa-video.component';
import { WaStickerComponent } from './components/whatsapp/tabs/wa-sticker/wa-sticker.component';
import { TwTextComponent } from './components/twitter/tabs/tw-text/tw-text.component';
import { TwImageComponent } from './components/twitter/tabs/tw-image/tw-image.component';
import { TwVideoComponent } from './components/twitter/tabs/tw-video/tw-video.component';

@NgModule({
    imports: [
        RouterModule.forRoot([
            {
                path: '', component: AppMainComponent,
                children: [
                    { path: '', component: HomepageComponent },
                    {
                        path: 'facebook', component: FacebookComponent, children: [
                            { path: 'text', component: FbTextComponent },
                            { path: 'image', component: FbImageComponent },
                            { path: 'video', component: FbVideoComponent }
                        ]
                    },
                    {
                        path: 'instagram', component: InstagramComponent, children: [
                            { path: 'image', component: IgImageComponent },
                            { path: 'video', component: IgVideoComponent }
                        ]
                    },
                    {
                        path: 'whatsapp', component: WhatsappComponent, children: [
                            { path: 'template', component: WaTemplateComponent },
                            { path: 'text', component: WaTextComponent },
                            { path: 'image', component: WaImageComponent },
                            { path: 'video', component: WaVideoComponent },
                            { path: 'sticker', component: WaStickerComponent }
                        ]
                    },
                    {
                        path: 'twitter', component: TwitterComponent, children: [
                            { path: 'text', component: TwTextComponent },
                            { path: 'image', component: TwImageComponent },
                            { path: 'video', component: TwVideoComponent }
                        ]
                    },
                    { path: 'youtube', component: YoutubeComponent },
                ],
            },
            { path: '**', redirectTo: 'pages/notfound' },
        ],
            { scrollPositionRestoration: 'enabled', anchorScrolling: 'enabled' })
    ],
    exports: [RouterModule]
})
export class AppRoutingModule {
}
