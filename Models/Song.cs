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

        public string FormatedDurarion => Media.Duration.TotalHours >= 1d ? Media.Duration.ToString(@"h\:mm\:ss") : Media.Duration.ToString(@"m\:ss");
    }


    public static class SongTools
    {
        public static async Task<List<Song>> GetSongs(string query)
        {
            var videos = await YouTube.SearchYouTubeMusic(query, YouTube.MusicSearchFilter.Songs);
            List<Song> songs = videos.CurrentPage.ContentItems
                .Where(c => c.Content is YouTube.Video)
                .Select(v => v.Content as YouTube.Video)
                .Select(s => new Song() { Media = s! })
                .ToList();
            return songs;
        }

        public static async Task<List<Song>> GetPlaylistSongs(string url)
        {
            var Id = YouTube.GetPlaylistId(url);
            var videos = await YouTube.GetPlaylistVideos(Id);
            List<Song> songs = videos.CurrentPage.ContentItems
                .Where(c => c.Content is YouTube.Video)
                .Select(v => v.Content as YouTube.Video)
                .Select(s => new Song() { Media = s! })
                .ToList();
            return songs;

        }
    }
}
