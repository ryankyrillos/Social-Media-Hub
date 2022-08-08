import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { HomepageComponent } from './components/homepage/homepage.component';
import { FacebookComponent } from './components/facebook/facebook.component';
import { InstagramComponent } from './components/instagram/instagram.component';
import { YoutubeComponent } from './components/youtube/youtube.component';
import { TiktokComponent } from './components/tiktok/tiktok.component';
import { WhatsappComponent } from './components/whatsapp/whatsapp.component';
import { TwitterComponent } from './components/twitter/twitter.component';
import { AppMainComponent } from './app.main.component';
import { LoginComponent } from './components/login/login.component';
import { FbTextComponent } from './components/facebook/tabs/fb-text/fb-text.component';
import { FbImageComponent } from './components/facebook/tabs/fb-image/fb-image.component';
import { FbVideoComponent } from './components/facebook/tabs/fb-video/fb-video.component';

@NgModule({
    imports: [
        RouterModule.forRoot([
            {
                path: '', component: AppMainComponent,
                children: [
                    {path: '', component: HomepageComponent},
                    {path: 'facebook', component: FacebookComponent, children: [
                        {path: 'text', component: FbTextComponent},
                        {path: 'image', component: FbImageComponent},
                        {path: 'video', component: FbVideoComponent}
                    ]},
                    {path: 'instagram', component: InstagramComponent},
                    {path: 'whatsapp', component: WhatsappComponent},
                    {path: 'twitter', component: TwitterComponent},
                    {path: 'youtube', component: YoutubeComponent},
                    {path: 'tiktok', component: TiktokComponent},
                ],
            },
            {path: 'login', component: LoginComponent},
            {path: '**', redirectTo: 'pages/notfound'},
        ], {scrollPositionRestoration: 'enabled', anchorScrolling:'enabled'})
    ],
    exports: [RouterModule]
})
export class AppRoutingModule {
}
