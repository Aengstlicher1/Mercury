using YouTubeApi;

namespace Mercury.Models
{
    public class Playlist
    {
        public string? Title => Media.Title;
        public string? Description => Media.Description;
        public string? Artist => Media.Author;
        public required YouTube.Playlist Media { get; set; }

        public override string ToString()
        {
            return $"{Title} - {Artist} - Songs:{Media.VideoCount}";
        }
    }

    public static class PlaylistTools
    {
        public static async Task<List<Playlist>> SearchPlaylists(string query, int pages = 1)
        {
            var videos = await YouTube.SearchYouTubeMusic(query, YouTube.MusicSearchFilter.CommunityPlaylists);
            List<Playlist> playlists;

            playlists = videos.CurrentPage.ContentItems
                .Where(c => c.Content is YouTube.Playlist)
                .Select(p => p.Content as YouTube.Playlist)
                .Select(p => new Playlist() { Media = p! })
                .ToList();

            for (int i = 1; i < pages; i++)
            {
                var nextResults = await videos.GetNextPage();
                playlists.AddRange(nextResults.ContentItems
                    .Where(c => c.Content is YouTube.Playlist)
                    .Select(p => p.Content as YouTube.Playlist)
                    .Select(p => new Playlist() { Media = p! })
                    .ToList());
            }
            
            return playlists;
        }

        public static async Task<List<Song>> GetPlaylistSongs(Playlist playlist)
        {
            var videos = await YouTube.GetPlaylistVideos(playlist.Media.PlaylistId);
            
            List<Song> songs;

            songs = videos.CurrentPage.ContentItems
                .Where(c => c.Content is YouTube.Video)
                .Select(v => v.Content as YouTube.Video)
                .Select(s => new Song() { Media = s! })
                .ToList();

            while (true)
            {
                var vidResults = await videos.GetNextPage();
                songs.AddRange(vidResults.ContentItems
                    .Where(c => c.Content is YouTube.Video)
                    .Select(v => v.Content as YouTube.Video)
                    .Select(s => new Song() { Media = s! })
                    .ToList());

                if (videos.AllPagesFetched) break;
            }

            return songs;
        }
    }
}
