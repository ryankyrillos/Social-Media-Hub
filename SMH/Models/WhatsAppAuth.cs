namespace SMH.Models
{
    public class WhatsAppAuth
    {
        public string? final_url_auth { get; set; } //to parse authorization code from
        public string? redirect_uri { get; set; }
    }
}
