
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicStreamingPrototype.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.API.Services
{
    public class SpotifyService : ISpotifyService
    {
        private readonly HttpClient _http;
        private readonly SpotifyOptions _opts;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SpotifyService> _logger;

        public SpotifyService(
            HttpClient http,
            IOptions<SpotifyOptions> opts,
            IMemoryCache cache,
            ILogger<SpotifyService> logger)
        {
            _http = http;
            _opts = opts.Value;
            _cache = cache;
            _logger = logger;

            if (_http.BaseAddress is null)
                _http.BaseAddress = new Uri("https://api.spotify.com/v1/");
        }

        #region ───────── Access Token ─────────
        public async Task<string> GetAccessTokenAsync()
        {
            return await _cache.GetOrCreateAsync("SpotifyToken", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(50);

                var req = new HttpRequestMessage(
                    HttpMethod.Post, "https://accounts.spotify.com/api/token");

                var creds = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{_opts.ClientId}:{_opts.ClientSecret}"));
                req.Headers.Authorization =
                    new AuthenticationHeaderValue("Basic", creds);
                req.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("grant_type","client_credentials")
                });

                var res = await _http.SendAsync(req);
                var body = await res.Content.ReadAsStringAsync();
                res.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(body);
                return doc.RootElement.GetProperty("access_token").GetString()!;
            })!;
        }
        #endregion

        #region ───────── Seed Playlists (search API) ─────────
        public async Task<IEnumerable<Playlist>> GetSomePlaylistsAsync(int take = 10)
        {
            var token = await GetAccessTokenAsync();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var res = await _http.GetAsync($"search?q=top&type=playlist&limit={take}");
            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("Spotify search API {Status}. Body: {Body}",
                                   res.StatusCode, body);
                return Enumerable.Empty<Playlist>();
            }

            using var doc = JsonDocument.Parse(body);
            var items = doc.RootElement
                                 .GetProperty("playlists")
                                 .GetProperty("items");

            return items.EnumerateArray()
                        .Where(e => e.ValueKind == JsonValueKind.Object)
                        .Select(p =>
                        {
                            // ── Kapak resmi (null-güvenli) ──
                            string? coverUrl = null;

                            if (p.TryGetProperty("images", out var imgs) &&
                                imgs.ValueKind == JsonValueKind.Array)
                            {
                                var firstImg = imgs.EnumerateArray()
                                                   .FirstOrDefault(img =>
                                                        img.ValueKind == JsonValueKind.Object &&
                                                        img.TryGetProperty("url", out _));

                                if (firstImg.ValueKind == JsonValueKind.Object &&
                                    firstImg.TryGetProperty("url", out var urlProp))
                                {
                                    coverUrl = urlProp.GetString();
                                }
                            }

                            // ── Açıklama (NOT-NULL için varsayılan) ──
                            string description = "Imported from Spotify";
                            if (p.TryGetProperty("description", out var desc) &&
                                desc.ValueKind == JsonValueKind.String &&
                                !string.IsNullOrWhiteSpace(desc.GetString()))
                            {
                                description = desc.GetString()!;
                            }

                            return new Playlist
                            {
                                SpotifyId = p.GetProperty("id").GetString()!,
                                Name = p.GetProperty("name").GetString()!,
                                Description = description,   // <— NULL olmayacak
                                CoverUrl = coverUrl       // nullable
                            };
                        })
                        .ToList();
        }
        #endregion

        #region ───────── Playlist Detail ─────────
        public async Task<object> GetPlaylistAsync(string playlistId)
        {
            var token = await GetAccessTokenAsync();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var res = await _http.GetAsync($"playlists/{playlistId}");
            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new ApplicationException($"Spotify API: {res.StatusCode} – {body}");

            return JsonSerializer.Deserialize<JsonElement>(body);
        }
        #endregion

        #region ───────── Track Detail ─────────
        public async Task<object> GetTrackAsync(string trackId)
        {
            var token = await GetAccessTokenAsync();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var res = await _http.GetAsync($"tracks/{trackId}");
            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new ApplicationException($"Spotify API: {res.StatusCode} – {body}");

            return JsonSerializer.Deserialize<JsonElement>(body);
        }
        #endregion

        #region ───────── Recommendations ─────────
        public async Task<object> GetRecommendationsAsync(string seedTrackId)
        {
            var token = await GetAccessTokenAsync();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var res = await _http.GetAsync(
                $"recommendations?seed_tracks={seedTrackId}&limit=10&market=TR");
            var body = await res.Content.ReadAsStringAsync();

            if (res.StatusCode == HttpStatusCode.NotFound)
                return Array.Empty<object>();

            if (!res.IsSuccessStatusCode)
                throw new ApplicationException($"Spotify API: {res.StatusCode} – {body}");

            return JsonSerializer.Deserialize<JsonElement>(body);
        }
        #endregion
    }
}
