namespace SMH.Models
{
    public class FacebookCodeRedirect
    {
        public string? redirect_uri { get; set; }
        public string? code { get; set; } //authorization code user gets after logging into Facebook
    }
}
