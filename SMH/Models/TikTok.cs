namespace WebAPI_SocialMediaPosts
{
    public class TikTok
    {
        public string? access_token { get; set; }
        
        public string? video_path { get; set; }
        public string? open_id { get; set; }
        public string? video_format { get; set; } //mp4 or webm
    }
}
