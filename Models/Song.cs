using YouTubeApi;


namespace Mercury.Models
{
    public class Song
    {
        public required YouTube.Video Media { get; set; }

        public string Title => Media.Title;
        public string Author => Media.Author.Split('•').First();
        public string Url => Media.Url;
        public string Album => Media.Author.Split('•').Last();
        public TimeSpan Duration => Media.Duration;

        public string FormatedDurarion => Media.Duration.TotalHours >= 1d ? Media.Duration.ToString(@"hh\:mm\:ss") : Media.Duration.ToString(@"m\:ss");
    }
}
