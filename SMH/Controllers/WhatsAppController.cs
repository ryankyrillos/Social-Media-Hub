using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.IO;
using SMH.Models;

namespace WebAPI_SocialMediaPosts.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WhatsAppController : Controller
    {
        private readonly IConfiguration config;
        public WhatsAppController(IConfiguration configuration)
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

        [HttpGet("Login", Name = "[Controller][Action]")]

        public async Task<IActionResult> Login(string code)
        {
            string url = "http://localhost:4200/whatsapp?code=" + code;
            return Redirect(url);
        }

        [HttpPost("Login", Name = "[Controller][Action]")]

        public async Task<ActionResult<string>> Login(Redirect redirect)
        {
            string app_id = config.GetValue<string>("WhatsApp:WhatsApp_App_ID");

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
            string url = "https://www.facebook.com/v14.0/dialog/oauth?client_id=" + app_id + "&redirect_uri=" + redirect.redirect_uri + "/&state=" + state + "&scope=whatsapp_business_management,whatsapp_business_messaging,public_profile";

            Url URL = new Url();
            URL.url = url;
            return Ok(URL);
        }

        [HttpPost("Token", Name = "[Controller][Action]")]

        public async Task<ActionResult<string>> GetToken(FacebookCodeRedirect wp_auth)
        {
            string app_id = config.GetValue<string>("WhatsApp:WhatsApp_App_ID");
            string app_secret = config.GetValue<string>("WhatsApp:WhatsApp_App_Secret");

            string? code = wp_auth.code;

            //get short-lived user access token
            string url = "https://graph.facebook.com/v14.0/oauth/access_token?client_id=" + app_id + "&redirect_uri=" + wp_auth.redirect_uri + "/&client_secret=" + app_secret + "&code=" + code;
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

        [HttpPost(Name = "[Controller][Action]")]

        public async Task<ActionResult<string>> SendMessage(WhatsApp wp)
        {
            ErrorMessage err = new ErrorMessage();
            string? token = wp.access_token;

            string? phone_number_id = wp.phone_number_id;
            string? recipient_number = wp.recepient_number;
            string? media_type = wp.media_type;

            if (media_type == "template")
            {
                var content = Task.Run(async () =>
                {
                    using (var http = new HttpClient())
                    {
                        string url = "https://graph.facebook.com/v14.0/" + phone_number_id + "/messages?access_token=" + token;

                        JObject o = new JObject
                        {
                            {"messaging_product", "whatsapp"},
                            {"to", recipient_number},
                            {"type", "template" },
                            {"template", new JObject { {"name", wp.template_name},{"language", new JObject { {"code", wp.template_language_code } } } } }
                        };
                        var content = new StringContent(o.ToString(), Encoding.UTF8, "application/json");
                        var httpResponse = await http.PostAsync(url, content);
                        var httpContent = await httpResponse.Content.ReadAsStringAsync();

                        return httpContent;
                    }
                });
                string result = content.Result;
                return Ok(result);
            }
            else if (media_type == "text")
            {
                string? text = wp.text;
                var contentResponse = Task.Run(async () =>
                {
                    using (var http = new HttpClient())
                    {
                        string url = "https://graph.facebook.com/v14.0/" + phone_number_id + "/messages";


                        JObject o = new JObject
                        {
                            {"access_token", token },
                            {"messaging_product", "whatsapp"},
                            {"recipient_type", "individual" },
                            {"to", recipient_number },
                            {"type", "text" },
                            {"text", new JObject {{"body", text}} }
                        };
                        var content = new StringContent(o.ToString(), Encoding.UTF8, "application/json");
                        var httpResponse = await http.PostAsync(url, content);
                        var httpContent = await httpResponse.Content.ReadAsStringAsync();

                        return httpContent;
                    }
                });
                string result = contentResponse.Result;
                return Ok(result);
            }
            else if (media_type == "image")
            {
                string? location = wp.media_location;

                if (location == "local")
                {
                    string? type = "";
                    string? format = wp.media_format;
                    if (format == "jpeg")
                    {
                        type = "image/jpeg";
                    }
                    else if (format == "png")
                    {
                        type = "image/png";
                    }
                    else
                    {
                        err.error = "media_format must be jpeg or png";
                        return BadRequest(err);
                    }
                    string? path = wp.media_path;
                    if(!System.IO.File.Exists(path))
                    {
                        ErrorMessage fileError = new ErrorMessage();
                        fileError.error = "File doesn't exist on local machine";
                        fileError.code = "1";
                        return Ok(fileError);
                    }
                    string[] path_parts = path.Split("\\");
                    string? imageFilename = path_parts[path_parts.Length - 1];

                    var contentResponse = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            string url = "https://graph.facebook.com/v14.0/" + phone_number_id + "/media";

                            var multipartFormContent = new MultipartFormDataContent();
                            var fileStreamContent = new StreamContent(System.IO.File.OpenRead(path));
                            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(type);

                            multipartFormContent.Add(fileStreamContent, "file", imageFilename);
                            multipartFormContent.Add(new StringContent(token), "access_token");
                            multipartFormContent.Add(new StringContent("whatsapp"), "messaging_product");

                            var httpResponse = await http.PostAsync(url, multipartFormContent);
                            var httpContent = await httpResponse.Content.ReadAsStringAsync();

                            return httpContent;
                        }
                    });

                    WhatsApp_Media? media = JsonConvert.DeserializeObject<WhatsApp_Media>(contentResponse.Result);
                    string? media_id = media.id;

                    string? caption = wp.text;
                    contentResponse = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            string url = "https://graph.facebook.com/v14.0/" + phone_number_id + "/messages";

                            JObject o = new JObject
                            {
                                {"access_token", token },
                                {"messaging_product", "whatsapp"},
                                {"recipient_type", "individual" },
                                {"to", recipient_number },
                                {"type", "image" },
                                {"image", new JObject {{"id", media_id}, {"caption", caption} } }
                            };
                            var content = new StringContent(o.ToString(), Encoding.UTF8, "application/json");
                            var httpResponse = await http.PostAsync(url, content);
                            var httpContent = await httpResponse.Content.ReadAsStringAsync();

                            return httpContent;
                        }
                    });
                    string result = contentResponse.Result;
                    return Ok(result);
                }
                else if (location == "internet")
                {
                    //Send an image from the internet using a URL
                    string? image_URL = wp.media_path;
                    string? caption = wp.text;
                    var contentResponse = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            string url = "https://graph.facebook.com/v14.0/" + phone_number_id + "/messages";


                            JObject o = new JObject
                            {
                                {"access_token", token },
                                {"messaging_product", "whatsapp"},
                                {"recipient_type", "individual" },
                                {"to", recipient_number },
                                {"type", "image" },
                                {"image", new JObject {{"link", image_URL}, {"caption", caption} } }
                            };
                            var content = new StringContent(o.ToString(), Encoding.UTF8, "application/json");
                            var httpResponse = await http.PostAsync(url, content);
                            var httpContent = await httpResponse.Content.ReadAsStringAsync();

                            return httpContent;
                        }
                    });
                    string? result = contentResponse.Result;
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
                string? location = wp.media_location;

                if (location == "internet")
                {
                    //Send video from internet using URL
                    string? video_URL = wp.media_path;
                    string? caption = wp.text;
                    var contentResponse = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            string url = "https://graph.facebook.com/v14.0/" + phone_number_id + "/messages";


                            JObject o = new JObject
                            {
                                {"access_token", token },
                                {"messaging_product", "whatsapp"},
                                {"recipient_type", "individual" },
                                {"to", recipient_number },
                                {"type", "video" },
                                {"video", new JObject {{"link", video_URL}, {"caption", caption} } }
                            };
                            var content = new StringContent(o.ToString(), Encoding.UTF8, "application/json");
                            var httpResponse = await http.PostAsync(url, content);
                            var httpContent = await httpResponse.Content.ReadAsStringAsync();

                            return httpContent;
                        }
                    });
                    string result = contentResponse.Result;
                    return Ok(result);
                }
                else if (location == "local")
                { 
                    string? type = "";
                    string? format = wp.media_format;
                    if (format == "mp4")
                    {
                        type = "video/mp4";
                    }
                    else if (format == "3gpp")
                    {
                        type = "video/3gpp";
                    }
                    else
                    {
                        err.error = "media_format must be mp4 or 3gpp";
                        return BadRequest(err);
                    }

                    string? path = wp.media_path;

                    if (!System.IO.File.Exists(path))
                    {
                        ErrorMessage fileError = new ErrorMessage();
                        fileError.error = "File doesn't exist on local machine";
                        fileError.code = "1";
                        return Ok(fileError);
                    }

                    //send video from local device
                    string[] path_parts = path.Split("\\");
                    string? videoFilename = path_parts[path_parts.Length - 1];

                    var contentResponse = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            string url = "https://graph.facebook.com/v14.0/" + phone_number_id + "/media";

                            var multipartFormContent = new MultipartFormDataContent();
                            var fileStreamContent = new StreamContent(System.IO.File.OpenRead(path));
                            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(type);

                            multipartFormContent.Add(fileStreamContent, "file", videoFilename);
                            multipartFormContent.Add(new StringContent(token), "access_token");
                            multipartFormContent.Add(new StringContent("whatsapp"), "messaging_product");


                            var httpResponse = await http.PostAsync(url, multipartFormContent);
                            var httpContent = await httpResponse.Content.ReadAsStringAsync();

                            return httpContent;
                        }
                    });
                    
                    WhatsApp_Media? media = JsonConvert.DeserializeObject<WhatsApp_Media>(contentResponse.Result);
                    string? media_id = media.id;
                    
                    string? caption = wp.text;
                    contentResponse = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            string url = "https://graph.facebook.com/v14.0/" + phone_number_id + "/messages";


                            JObject o = new JObject
                            {
                                {"access_token", token },
                                {"messaging_product", "whatsapp"},
                                {"recipient_type", "individual" },
                                {"to", recipient_number },
                                {"type", "video" },
                                {"video", new JObject {{"id", media_id}, {"caption", caption} } }
                            };
                            var content = new StringContent(o.ToString(), Encoding.UTF8, "application/json");
                            var httpResponse = await http.PostAsync(url, content);
                            var httpContent = await httpResponse.Content.ReadAsStringAsync();

                            return httpContent;
                        }
                    });
                    string result = contentResponse.Result;
                    return Ok(result);
                }
                else
                {
                    err.error = "media_location must be local or internet";
                    return BadRequest(err);
                }

            }
            else if (media_type == "sticker")
            {
                string? location = wp.media_location;

                if (location == "internet")
                {
                    string? sticker_URL = wp.media_path;
                    var contentResponse = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            string url = "https://graph.facebook.com/v14.0/" + phone_number_id + "/messages";
                            JObject o = new JObject
                            {
                                {"access_token", token },
                                {"messaging_product", "whatsapp"},
                                {"recipient_type", "individual" },
                                {"to", recipient_number },
                                {"type", "sticker" },
                                {"sticker", new JObject {{"link", sticker_URL} } }
                            };
                            var content = new StringContent(o.ToString(), Encoding.UTF8, "application/json");
                            var httpResponse = await http.PostAsync(url, content);
                            var httpContent = await httpResponse.Content.ReadAsStringAsync();

                            return httpContent;
                        }
                    });
                    string result = contentResponse.Result;
                    return Ok(result);
                }
                else if (location == "local")
                {
                    string? path = wp.media_path;

                    if (!System.IO.File.Exists(path))
                    {
                        ErrorMessage fileError = new ErrorMessage();
                        fileError.error = "File doesn't exist on local machine";
                        fileError.code = "1";
                        return Ok(fileError);
                    }

                    //send sticker from local device
                    string[] path_parts = path.Split("\\");
                    string? stickerFilename = path_parts[path_parts.Length - 1];

                    var contentResponse = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            string url = "https://graph.facebook.com/v14.0/" + phone_number_id + "/media";

                            var multipartFormContent = new MultipartFormDataContent();
                            var fileStreamContent = new StreamContent(System.IO.File.OpenRead(path));
                            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/webp");

                            multipartFormContent.Add(fileStreamContent, "file", stickerFilename);
                            multipartFormContent.Add(new StringContent(token), "access_token");
                            multipartFormContent.Add(new StringContent("whatsapp"), "messaging_product");


                            var httpResponse = await http.PostAsync(url, multipartFormContent);
                            var httpContent = await httpResponse.Content.ReadAsStringAsync();

                            return httpContent;
                        }
                    });
                    WhatsApp_Media? media = JsonConvert.DeserializeObject<WhatsApp_Media>(contentResponse.Result);
                    string? media_id = media.id;

                    contentResponse = Task.Run(async () =>
                    {
                        using (var http = new HttpClient())
                        {
                            string url = "https://graph.facebook.com/v14.0/" + phone_number_id + "/messages";

                            JObject o = new JObject
                            {
                                {"access_token", token },
                                {"messaging_product", "whatsapp"},
                                {"recipient_type", "individual" },
                                {"to", recipient_number },
                                {"type", "sticker" },
                                {"sticker", new JObject {{"id", media_id} } }
                            };
                            var content = new StringContent(o.ToString(), Encoding.UTF8, "application/json");
                            var httpResponse = await http.PostAsync(url, content);
                            var httpContent = await httpResponse.Content.ReadAsStringAsync();

                            return httpContent;
                        }
                    });
                    string result = contentResponse.Result;
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

class WhatsApp_Media
{
    public string? id;
}