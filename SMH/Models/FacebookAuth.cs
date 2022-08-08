namespace WebAPI_SocialMediaPosts
{
    public class FacebookAuth
    {
        public string? final_url_auth { get; set; } //to parse authorization code from
        public string? page_id { get; set; }
        public string? redirect_uri { get; set; }
    }
}
