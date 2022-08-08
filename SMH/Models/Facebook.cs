namespace WebAPI_SocialMediaPosts
{
    public class Facebook
    {
        public string? page_id { get; set; }
        public string page_token { get; set; }
        public string? media_type { get; set; } //text, image or video
        public string? text { get; set; } //when posting plain text or description for image or video
        public string? media_path { get; set; } //file path or url
        public string? media_location { get; set; } //local or internet
    }
}
