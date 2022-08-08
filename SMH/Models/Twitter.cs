namespace WebAPI_SocialMediaPosts
{
    public class Twitter
    {
        public string? access_token { get; set; }
        public string? access_token_secret { get; set; }
        public string? media_type { get; set; } //text, image or video
        public string? text { get; set; } //when posting plain text or text with image or video
        public string? media_path { get; set; } //file path
    }
}
