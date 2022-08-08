using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using SMH.Models;

namespace WebAPI_SocialMediaPosts.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TikTokController : Controller
    {
        private readonly IConfiguration config;
        public TikTokController(IConfiguration configuration)
        {
            config = configuration;
        }
        public static string tiktok_refresh_access_token(string client_key, string refresh_token)
        {
            var content = Task.Run(async () =>
            {
                using (var http = new HttpClient())
                {
                    string? url = "https://open-api.tiktok.com/oauth/refresh_token/";

                    var values = new Dictionary<string, string>();

                    values.Add("client_key", client_key);
                    values.Add("grant_type", "refresh_token");
                    values.Add("refresh_token", refresh_token);
                    var content = new FormUrlEncodedContent(values);
                    var httpResponse = await http.PostAsync(url, content);
                    var httpContent = await httpResponse.Content.ReadAsStringAsync();

                    return httpContent;
                }
            });

            TikTok_Token? response = JsonConvert.DeserializeObject<TikTok_Token>(content.Result);

            string? token = response.data.access_token;
            return token;
        }


        [HttpPost("Login")]

        public async Task<IActionResult> Login(Redirect redirect)
        {
            string client_key = config.GetValue<string>("TikTok:TikTok_Client_Key");

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
            string url = "https://www.tiktok.com/auth/authorize/";

            url += "?client_key=" + client_key;
            url += "&scope=user.info.basic,video.list,video.upload";
            url += "&response_type=code";
            url += "&redirect_uri=" + redirect.redirect_uri;
            url += "&state=" + state;

            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });

            return Ok();
        }

        [HttpPost("Tokens")]

        public async Task<IActionResult> GetTokens(TikTokAuth tiktok_auth)
        {
            string client_key = config.GetValue<string>("TikTok:TikTok_Client_Key");
            string? client_secret = config.GetValue<string>("TikTok:TikTok_Client_Secret");

            string? finalUrl = tiktok_auth.final_url_auth;
            Uri targetUri = new Uri(finalUrl);
            string? code = System.Web.HttpUtility.ParseQueryString(targetUri.Query).Get("code");

            //get access token and refresh token
            var content = Task.Run(async () =>
            {
                using (var http = new HttpClient())
                {
                    string url = "https://open-api.tiktok.com/oauth/access_token/";

                    var values = new Dictionary<string, string>();

                    values.Add("client_key", client_key);
                    values.Add("client_secret", client_secret);
                    values.Add("code", code);
                    values.Add("grant_type", "authorization_code");
                    var content = new FormUrlEncodedContent(values);
                    var httpResponse = await http.PostAsync(url, content);
                    var httpContent = await httpResponse.Content.ReadAsStringAsync();

                    return httpContent;
                }
            });
            string? result = content.Result;
            return Ok(result);
        }

        [HttpPost("Refresh_Access_Token")]

        public async Task<IActionResult> Post(TikTok_Refresh_Token refresh)
        {
            string client_key = config.GetValue<string>("TikTok:TikTok_Client_Key");
            Token token = new Token();
            token.token = tiktok_refresh_access_token(client_key, refresh.refresh_token);
            return Ok(token);
        }

        [HttpPost]

        public async Task<IActionResult> PostVideo(TikTok tiktok)
        { 
            string? video_path = tiktok.video_path;
            var videoBinary = System.IO.File.ReadAllBytes(video_path);
            string? open_id = tiktok.open_id;

            string? format = tiktok.video_format;
            string video_type = "";
            if (format == "mp4")
            {
                video_type = "video/mp4";
            }
            else if (format == "webm")
            {
                video_type = "video/webm";
            }
            else
            {
                ErrorMessage err = new ErrorMessage();
                err.error = "video_format must be mp4 or webm";
                return BadRequest(err);
            }
            string[] path_parts = video_path.Split("\\");
            string? videoFilename = path_parts[path_parts.Length - 1];

            var content = Task.Run(async () =>
            {
                using (var http = new HttpClient())
                {
                    var multipartFormContent = new MultipartFormDataContent();
                    var fileStreamContent = new StreamContent(System.IO.File.OpenRead(video_path));
                    fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(video_type);

                    multipartFormContent.Add(fileStreamContent, "video", videoFilename);

                    string url = "https://open-api.tiktok.com/share/video/upload/?access_token=" + tiktok.access_token + "&open_id=" + open_id;

                    var httpResponse = await http.PostAsync(url, multipartFormContent);
                    var httpContent = await httpResponse.Content.ReadAsStringAsync();

                    return httpContent;
                }
            });
            string result = content.Result;
            return Ok(result);
        }
    }

}

class TikTok_Token
{
    public TikTok_Data? data;
    public string? message;
}

class TikTok_Data
{
    public string? open_id;
    public string? scope;
    public string? access_token;
    public Int64? expires_in;
    public string? refresh_token;
    public Int64? refresh_expires_in;
    string? captcha;
    string? desc_url;
    string? description;
    string? log_id;
}