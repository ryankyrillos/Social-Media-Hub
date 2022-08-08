namespace WebAPI_SocialMediaPosts
{
    public class InstagramAuth
    {
        public string? final_url_auth { get; set; } //to parse authorization code from
        public string? facebook_page_id { get; set; } //id of facebook page connected to the instagram account
        public string? redirect_uri { get; set; }
    }
}
