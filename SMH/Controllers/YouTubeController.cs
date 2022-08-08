using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace WebAPI_SocialMediaPosts.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class YouTubeController : Controller
    {
        private readonly IConfiguration config;
        public YouTubeController(IConfiguration configuration)
        {
            config = configuration;
        }

        [HttpPost]
        public async Task<ActionResult<string>> PostVideo(YouTube yt)
        {
            UploadVideo.MainFunction(yt.video_path, yt.title, yt.description, config);
            return Ok();
        }
    }
}

class UploadVideo
{
    [STAThread]
    public static void MainFunction(string? path, string? title, string? description, IConfiguration config)
    {
        try
        {
            new UploadVideo().Run(path, title, description, config).Wait();
        }
        catch (AggregateException ex)
        {
            foreach (var e in ex.InnerExceptions)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }
    }

    private async Task Run(string path, string title, string description, IConfiguration config)
    {
        UserCredential credential;
        ClientSecrets clientSecrets = new ClientSecrets();
        clientSecrets.ClientId = config.GetValue<string>("YouTube:YouTube_Client_Id");
        clientSecrets.ClientSecret = config.GetValue<string>("YouTube:YouTube_Client_Secret");
        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            clientSecrets,
            //From the developer website:
            // "This OAuth 2.0 access scope allows an application to upload files to the
            // authenticated user's YouTube channel, but doesn't allow other types of access."
            new[] { YouTubeService.Scope.YoutubeUpload },
            "user",
            CancellationToken.None
        );

        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
        });

        var video = new Video();
        video.Snippet = new VideoSnippet();
        video.Snippet.Title = title;
        video.Snippet.Description = description;
        //video.Snippet.Tags = new string[] { "tag1", "tag2" };
        //video.Snippet.CategoryId = "22"; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
        video.Status = new VideoStatus();
        video.Status.PrivacyStatus = "public"; // "unlisted" or "private" or "public"
        var filePath = path; // From developer website: "Replace with path to actual movie file."

        using (var fileStream = new FileStream(filePath, FileMode.Open))
        {
            var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
            videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
            videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;

            await videosInsertRequest.UploadAsync();
        }
    }

    void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
    {
        switch (progress.Status)
        {
            case UploadStatus.Uploading:
                Console.WriteLine("{0} bytes sent.", progress.BytesSent);
                break;

            case UploadStatus.Failed:
                Console.WriteLine("An error prevented the upload from completing.\n{0}", progress.Exception);
                break;
        }
    }

    void videosInsertRequest_ResponseReceived(Video video)
    {
        Console.WriteLine("Video id '{0}' was successfully uploaded.", video.Id);
    }
}