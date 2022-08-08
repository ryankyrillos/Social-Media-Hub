namespace WebAPI_SocialMediaPosts
{
    public class WhatsApp
    {
        public string? access_token { get; set; }
        public string? phone_number_id { get; set; } //the id of the phone number you want to send messages from
        public string? media_type { get; set; } //template, text, image, video or sticker

        public string? template_name { get; set; }
        public string? template_language_code { get; set; }
        public string? text { get; set; } //when sending plain text or text with image or video
        public string? media_path { get; set; } //file path or url
        public string? media_location { get; set; } //local or internet
        public string? media_format { get; set; } //png, jpeg, mp4, ...
        public string? recepient_number { get; set; }

    }
}
