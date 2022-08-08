using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using SMH.Models;

namespace WebAPI_SocialMediaPosts.Controllers
{
    [ApiController]
    [Route("[controller]")]


    public class FacebookController : Controller
    {
        
        private readonly IConfiguration config;
        public FacebookController(IConfiguration configuration)
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

        [HttpPost("Login")]

        public async Task<IActionResult> Login(Redirect redirect)
        {
            string app_id = config.GetValue<string>("Facebook:Facebook_AppId");
            string app_secret = config.GetValue<string>("Facebook:Facebook_AppSecret");
            
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
            string url = "https://www.facebook.com/v14.0/dialog/oauth?client_id=" + app_id + "&redirect_uri=" + redirect.redirect_uri + "/&state=" + state + "&scope=pages_show_list,pages_read_engagement,pages_manage_posts,public_profile";

            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
            return Ok();
        }


        [HttpPost("Page_Token")]

        public async Task<ActionResult<string>> GetPageToken(FacebookAuth fb_auth)
        {
            string app_id = config.GetValue<string>("Facebook:Facebook_AppId");
            string app_secret = config.GetValue<string>("Facebook:Facebook_AppSecret");

            string? final_url = fb_auth.final_url_auth;
            Uri targetUri = new Uri(final_url);
            string? code = System.Web.HttpUtility.ParseQueryString(targetUri.Query).Get("code");

            //get short-lived user access token
            string url = "https://graph.facebook.com/v14.0/oauth/access_token?client_id=" + app_id + "&redirect_uri=" + fb_auth.redirect_uri + "/&client_secret=" + app_secret + "&code=" + code;
            var content = getHTTPContent(url);
            FBToken? token = JsonConvert.DeserializeObject<FBToken>(content.Result);
            string? access = token.access_token;

            //get long-lived user access token
            url = "https://graph.facebook.com/oauth/access_token?grant_type=fb_exchange_token&client_id=" + app_id + "&client_secret=" + app_secret + "&fb_exchange_token=" + access;
            content = getHTTPContent(url);

            token = JsonConvert.DeserializeObject<FBToken>(content.Result);
            string? long_access = token.access_token;

            string? pageID = fb_auth.page_id;
            //get page access token
            url = "https://graph.facebook.com/" + pageID + "?fields=access_token&access_token=" + long_access;
            content = getHTTPContent(url);
            FBPageToken? page_token = JsonConvert.DeserializeObject<FBPageToken>(content.Result);
            string? page_access = page_token.access_token;
            Token token1 = new Token();
            token1.token = page_access;
            return Ok(token1);
        }

        [HttpPost]
        public async Task<ActionResult<string>> Post(Facebook fb)
        {
            ErrorMessage err = new ErrorMessage();
            string page_access = fb.page_token;
            string? pageID = fb.page_id;
            string? media_type = fb.media_type;
           
            if (media_type == "text")
            {
                //post text on FB page
                string? text = fb.text;
                string url = "https://graph.facebook.com/" + pageID + "/feed?access_token=" + page_access;
                var content = Task.Run(async () =>
                {
                    using (var http = new HttpClient())
                    {
                        var values = new Dictionary<string, string>();
                        {
                            values.Add("message", text);
                            var content = new FormUrlEncodedContent(values);
                            var httpResponse = await http.PostAsync(url, content);
                            var _httpContent = await httpResponse.Content.ReadAsStringAsync();
                            return _httpContent;
                        }
                    }
                });
                string result = content.Result;
                return Ok(result);
            }
            else if (media_type == "image")
            {
                //post picture on FB page
                string? location = fb.media_location;

                if (location == "local")
                {
                    string? picture_path = fb.media_path;
                    string? caption = fb.text;
                    string[] path_parts = picture_path.Split("\\");
                    string? pictureFilename = path_parts[path_parts.Length - 1];

                    var content = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            string url = "https://graph.facebook.com/" + pageID + "/photos?access_token=" + page_access;

                            var multipartFormContent = new MultipartFormDataContent();
                            var fileStreamContent = new StreamContent(System.IO.File.OpenRead(picture_path));

                            multipartFormContent.Add(new StringContent(caption), "caption");
                            multipartFormContent.Add(fileStreamContent, "source", pictureFilename);

                            var httpResponse = await http.PostAsync(url, multipartFormContent);
                            var httpContent = await httpResponse.Content.ReadAsStringAsync();

                            return httpContent;
                        }
                    });
                    string result = content.Result;
                    return Ok(result);
                }
                else if (location == "internet")
                {
                    string? picture_path = fb.media_path;
                    string? caption = fb.text;
                    string url = "https://graph.facebook.com/" + pageID + "/photos?access_token=" + page_access;
                    var content = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            var values = new Dictionary<string, string>();

                            values.Add("url", picture_path);
                            values.Add("caption", caption);
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
                    err.error = "media_location must be local or internet";
                    return BadRequest(err);
                }

            }
            else if (media_type == "video")
            {
                //post video on FB page
                string? location = fb.media_location;

                string? video_path = fb.media_path;
                string? description = fb.text;

                if (location == "local")
                {
                    string[] path_parts = video_path.Split("\\");
                    string? videoFilename = path_parts[path_parts.Length - 1];

                    var content = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            string url = "https://graph-video.facebook.com/v14.0/" + pageID + "/videos?access_token=" + page_access;

                            var multipartFormContent = new MultipartFormDataContent();
                            var fileStreamContent = new StreamContent(System.IO.File.OpenRead(video_path));

                            multipartFormContent.Add(new StringContent(description), "description");
                            multipartFormContent.Add(fileStreamContent, "source", videoFilename);

                            var httpResponse = await http.PostAsync(url, multipartFormContent);
                            var httpContent = await httpResponse.Content.ReadAsStringAsync();

                            return httpContent;
                        }
                    });
                    string result = content.Result;
                    return Ok(result);
                }
                else if (location == "internet")
                {
                    //Post video from url
                    var content = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            string url = "https://graph-video.facebook.com/v14.0/" + pageID + "/videos?access_token=" + page_access;

                            var values = new Dictionary<string, string>();

                            values.Add("file_url", video_path);
                            values.Add("description", description);
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
                    err.error = "media_location must be local or internet";
                    return BadRequest(err);
                }
            }
            else
            {
                err.error = "Invalid media type";
                return BadRequest(err);
            }
        }
    }
    
}

class FBToken
{
    public string? access_token;
    string? token_type;
    string? expires_in;
}

class FBPageToken
{
    public string? access_token;
    string? id;
}
