namespace WebAPI_SocialMediaPosts
{
    public class Instagram
    {
        public string? facebook_page_id { get; set; }
        public string? page_token { get; set; }
        public string? media_type { get; set; } //image or video
        public string? caption { get; set; }
        public string? media_url { get; set; }
    }
}
