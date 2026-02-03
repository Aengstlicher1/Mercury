using Mercury.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mercury.Services
{
    public static class LyricsService
    {
        private static readonly HttpClient _httpClient = new();
        private const string BaseUrl = "https://lrclib.net/api";

        public static async Task<LyricsResult?> GetLyricsAsync(Song song)
        {
            var url = $"{BaseUrl}/get?artist_name={Uri.EscapeDataString(song.Artist)}&track_name={Uri.EscapeDataString(song.Title)}";

            if (song.Duration.TotalSeconds > 0)
                url += $"&duration={Convert.ToInt32(song.Duration.TotalSeconds)}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LyricsResult>(jsonString);
        }
    }

    public class LyricsResult
    {
        [JsonPropertyName("plainLyrics")]
        public string? PlainLyrics { get; set; }

        [JsonPropertyName("syncedLyrics")]
        public string? SyncedLyrics { get; set; }  // LRC format with timestamps

        [JsonPropertyName("trackName")]
        public string? TrackName { get; set; }

        [JsonPropertyName("artistName")]
        public string? ArtistName { get; set; }

        [JsonPropertyName("albumName")]
        public string? AlbumName { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }
    }
}
