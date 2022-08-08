using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using SMH.Models;

namespace WebAPI_SocialMediaPosts.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InstagramController : Controller
    {
        private readonly IConfiguration config;
        public InstagramController(IConfiguration configuration)
        {
            config = configuration;
        }

        public static async Task<string> getHTTPContent(string url)
        {
            using (var http = new HttpClient())
            {
                var httpResponse = await http.GetAsync(url);
                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return httpContent;
            }
        }

        [HttpPost("Login", Name = "[Controller][Action]")]

        public async Task<ActionResult<string>> Login(Redirect redirect)
        {
            string app_id = config.GetValue<string>("Facebook:Facebook_AppId");

            const string src = "abcdefghijklmnopqrstuvwxyz0123456789";
            var sb = new StringBuilder();
            Random rand = new Random();
            for (var i = 0; i < 16; i++)
            {
                var c = src[rand.Next(0, src.Length)];
                sb.Append(c);
            }
            string state = sb.ToString();

            //let user log in to get authorization code
            string url = "https://www.facebook.com/v14.0/dialog/oauth?client_id=" + app_id + "&redirect_uri=" + redirect.redirect_uri + "/&state=" + state + "&scope=pages_show_list,pages_read_engagement,pages_manage_posts,public_profile,instagram_basic,instagram_content_publish";

            Url URL = new Url();
            URL.url = url;
            return Ok(URL);
        }


        [HttpPost("Access_Token", Name = "[Controller][Action]")]

        public async Task<ActionResult<string>> GetAccessToken(FacebookCodeRedirect fbCodeRedirect)
        {
            string app_id = config.GetValue<string>("Facebook:Facebook_AppId");
            string app_secret = config.GetValue<string>("Facebook:Facebook_AppSecret");

            string? code = fbCodeRedirect.code;
            //get short-lived user access token
            string url = "https://graph.facebook.com/v14.0/oauth/access_token?client_id=" + app_id + "&redirect_uri=" + fbCodeRedirect.redirect_uri + "/&client_secret=" + app_secret + "&code=" + code;
            var content = getHTTPContent(url);
            
            FBToken? token = JsonConvert.DeserializeObject<FBToken>(content.Result);
            string? access = token.access_token;

            //get long-lived user access token
            url = "https://graph.facebook.com/oauth/access_token?grant_type=fb_exchange_token&client_id=" + app_id + "&client_secret=" + app_secret + "&fb_exchange_token=" + access;
            content = getHTTPContent(url);

            token = JsonConvert.DeserializeObject<FBToken>(content.Result);
            string? long_access_token = token.access_token;
            
            Token token1 = new Token();
            token1.token = long_access_token;
            return Ok(token1);
        }

        [HttpPost("Page_Token", Name = "[Controller][Action]")]

        public async Task<ActionResult<string>> GetPageToken(InstagramAuth igAuth)
        {
            string? pageID = igAuth.facebook_page_id;
            //get page access token
            var url = "https://graph.facebook.com/" + pageID + "?fields=access_token&access_token=" + igAuth.long_access_token;
            var content = getHTTPContent(url);
            FBPageToken? page_token = JsonConvert.DeserializeObject<FBPageToken>(content.Result);
            string? page_access = page_token.access_token;
            Token token = new Token();
            token.token = page_access;
            return Ok(token);
        }

        [HttpPost(Name = "[Controller][Action]")]
        public async Task<ActionResult<string>> Post(Instagram ig)
        {
            string? pageID = ig.facebook_page_id;
            string? page_access = ig.page_token;
            //get connected IG user ID
            string url = "https://graph.facebook.com/v14.0/" + pageID + "?fields=instagram_business_account&access_token=" + page_access;
            var content = getHTTPContent(url);
            InstagramResponse? IG_Response = JsonConvert.DeserializeObject<InstagramResponse>(content.Result);
            string? IG_ID = IG_Response.instagram_business_account["id"];

            string? type = ig.media_type;

            if (type == "image")
            {
                //post image to IG
                string? image_path = ig.media_url;
                string? caption = ig.caption;
                content = Task.Run(async () =>
                {
                    using (var http = new HttpClient())
                    {
                        url = "https://graph.facebook.com/v14.0/" + IG_ID + "/media";

                        var values = new Dictionary<string, string>();

                        values.Add("image_url", image_path);
                        values.Add("caption", caption);
                        values.Add("access_token", page_access);
                        var content = new FormUrlEncodedContent(values);
                        var httpResponse = await http.PostAsync(url, content);
                        var httpContent = await httpResponse.Content.ReadAsStringAsync();

                        return httpContent;
                    }
                });

                //get image container ID after creating the container
                string image_container_ID = JsonConvert.DeserializeObject<IG_Container>(content.Result).id;

                content = Task.Run(async () =>
                {
                    using (var http = new HttpClient())
                    {
                        url = "https://graph.facebook.com/v14.0/" + IG_ID + "/media_publish";
                        var values = new Dictionary<string, string>();

                        values.Add("creation_id", image_container_ID);
                        values.Add("access_token", page_access);
                        var content = new FormUrlEncodedContent(values);
                        var httpResponse = await http.PostAsync(url, content);
                        var httpContent = await httpResponse.Content.ReadAsStringAsync();

                        return httpContent;
                    }
                });
                string result = content.Result;
                return Ok(result);
            }
            else if (type == "video")
            {
                //post video to IG
                string? video_path = ig.media_url;
                string? caption = ig.caption;
                content = Task.Run(async () =>
                {
                    using (var http = new HttpClient())
                    {
                        url = "https://graph.facebook.com/v14.0/" + IG_ID + "/media";

                        var values = new Dictionary<string, string>();
                        values.Add("media_type", "VIDEO");
                        values.Add("video_url", video_path);
                        values.Add("caption", caption);
                        values.Add("access_token", page_access);
                        var content = new FormUrlEncodedContent(values);
                        var httpResponse = await http.PostAsync(url, content);
                        var httpContent = await httpResponse.Content.ReadAsStringAsync();

                        return httpContent;
                    }
                });

                //get image container ID after creating the container
                string video_container_ID = JsonConvert.DeserializeObject<IG_Container>(content.Result).id;

                //wait for creation of the container to be finished
                string? status = "";

                while (status != "FINISHED")
                {
                    content = getHTTPContent("https://graph.facebook.com/v14.0/" + video_container_ID + "?fields=status_code&access_token=" + page_access);
                    IG_Video? vid = JsonConvert.DeserializeObject<IG_Video>(content.Result);
                    status = vid.status_code;
                    Thread.Sleep(30 * 1000);
                }


                content = Task.Run(async () =>
                {
                    using (var http = new HttpClient())
                    {
                        url = "https://graph.facebook.com/v14.0/" + IG_ID + "/media_publish";
                        var values = new Dictionary<string, string>();

                        values.Add("creation_id", video_container_ID);
                        values.Add("access_token", page_access);
                        var content = new FormUrlEncodedContent(values);
                        var httpResponse = await http.PostAsync(url, content);
                        var httpContent = await httpResponse.Content.ReadAsStringAsync();

                        return httpContent;
                    }
                });
                string result = content.Result;
                return Ok(result);
            }
            else
            {
                ErrorMessage err = new ErrorMessage();
                err.error = "Invalid media type";
                return BadRequest(err);
            }
        }
    }
}


class InstagramResponse
{
    public Dictionary<string, string>? instagram_business_account = new Dictionary<string, string>();
    string? id;
}

class IG_Container
{
    public string? id;
}

class IG_Video
{
    public string? status_code;
    string? id;
}