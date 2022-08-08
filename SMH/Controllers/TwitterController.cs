using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace WebAPI_SocialMediaPosts.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TwitterController : Controller
    {
        private readonly IConfiguration config;
        public TwitterController(IConfiguration configuration)
        {
            config = configuration;
        }

        //Tweetinvi: "Create a client for your app"
        private static TwitterClient? appClient;

        //Tweetinvi: "Start the authentication process"
        private static IAuthenticationRequest? authenticationRequest;

        [HttpGet("Login")]

        public async Task<IActionResult> Get()
        {
            string API_key = config.GetValue<string>("Twitter:Twitter_API_KEY");
            string API_key_secret = config.GetValue<string>("Twitter:Twitter_API_KEY_SECRET");

            appClient = new TwitterClient(API_key, API_key_secret);
            authenticationRequest = await appClient.Auth.RequestAuthenticationUrlAsync(); 
            //Tweetinvi: "Go to the URL so that Twitter authenticates the user and gives him a PIN code."
            Process.Start(new ProcessStartInfo(authenticationRequest.AuthorizationURL)
            {
                UseShellExecute = true
            });
            return Ok();
        }

        [HttpPost("Tokens")]

        public async Task<IActionResult> GetAccessToken(TwitterPin pin)
        {   
            //Tweetivi: "With this pin code it is now possible to get the credentials back from Twitter"
            var userCredentials = await appClient.Auth.RequestCredentialsFromVerifierCodeAsync(pin.pin_code, authenticationRequest);
            TwitterToken token = new TwitterToken();
            
            //get access token
            token.access_token = userCredentials.AccessToken;
            //get access token secret
            token.access_token_secret = userCredentials.AccessTokenSecret;

            return Ok(token);
        }

        [HttpPost]

        public async Task<IActionResult> Post(Twitter twitter)
        {
            string API_key = config.GetValue<string>("Twitter:Twitter_API_KEY");
            string API_key_secret = config.GetValue<string>("Twitter:Twitter_API_KEY_SECRET");

            var userClient = new TwitterClient(API_key, API_key_secret, twitter.access_token, twitter.access_token_secret);
            var user = await userClient.Users.GetAuthenticatedUserAsync(); //to authenticate user

            string? type = twitter.media_type;
            if (type == "text")
            {
                string? text = twitter.text;
                var tweet = await userClient.Tweets.PublishTweetAsync(text); //for text-based tweet
                return Ok();
            }
            else if (type == "video")
            {
                string? text = twitter.text;
                string? videoPath = twitter.media_path;

                var videoBinary = System.IO.File.ReadAllBytes(videoPath);
                var uploadedVideo = await userClient.Upload.UploadTweetVideoAsync(videoBinary);

                //Tweetinvi: "IMPORTANT: you need to wait for Twitter to process the video"
                await userClient.Upload.WaitForMediaProcessingToGetAllMetadataAsync(uploadedVideo);

                var tweetWithVideo = await userClient.Tweets.PublishTweetAsync(new PublishTweetParameters(text)
                {
                    Medias = { uploadedVideo }
                });
                return Ok();
            }
            else if (type == "image")
            {
                string? text = twitter.text;
                string? picturePath = twitter.media_path;
                
                var pictureBinary = System.IO.File.ReadAllBytes(picturePath);
                var uploadedPicture = await userClient.Upload.UploadTweetImageAsync(pictureBinary);

                var tweetWithPicture = await userClient.Tweets.PublishTweetAsync(new PublishTweetParameters(text)
                {
                    Medias = { uploadedPicture }
                });
                return Ok();
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

class TwitterToken
{
    public string? access_token { get; set; }
    public string? access_token_secret { get; set; }
}
