
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using MusicStreamingPrototype.UI.Models;

namespace MusicStreamingPrototype.UI.Services
{
    public class ApiClient : IApiClient
    {
        private const string SessionKey = "JwtToken";
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        public ApiClient(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        public void StoreTokenInSession(string token)
        {
            _ctx.HttpContext?.Session.SetString(SessionKey, token);
        }

        private void AddAuthHeader()
        {
            
            _http.DefaultRequestHeaders.Authorization = null;

            var token = _ctx.HttpContext?.User.FindFirstValue("Jwt")
                        ?? _ctx.HttpContext?.Session.GetString(SessionKey);

            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            var res = await _http.PostAsJsonAsync("api/v1/auth/register",
                             new { Username = username, Password = password });
            return res.IsSuccessStatusCode;
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            var res = await _http.PostAsJsonAsync("api/v1/auth/login",
                             new { Username = username, Password = password });
            if (!res.IsSuccessStatusCode)
                return null;

            var dict = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (dict != null && dict.TryGetValue("token", out var token))
            {
                StoreTokenInSession(token);
                return token;
            }
            return null;
        }

        public async Task<IEnumerable<Playlist>> GetPlaylistsAsync()
        {
            AddAuthHeader();
            
            var response = await _http.GetAsync("api/v1/playlists");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                
                return new List<Playlist>();
            }
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<List<Playlist>>();
            return list ?? new List<Playlist>();
        }

        public async Task<Playlist?> GetPlaylistByIdAsync(int playlistId)
        {
            AddAuthHeader();
            var res = await _http.GetAsync($"api/v1/playlists/{playlistId}");
            if (!res.IsSuccessStatusCode)
                return null;
            return await res.Content.ReadFromJsonAsync<Playlist>();
        }

        public async Task<Playlist> CreatePlaylistAsync(Playlist playlist)
        {
            AddAuthHeader();
            var res = await _http.PostAsJsonAsync("api/v1/playlists", playlist);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Playlist>()
                   ?? throw new InvalidOperationException("Playlist deserialize edilemedi.");
        }

        public async Task<IEnumerable<Track>> GetTracksInPlaylistAsync(int playlistId)
        {
            AddAuthHeader();
            var uri = $"api/v1/playlists/{playlistId}/tracks";
            var response = await _http.GetAsync(uri);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return new List<Track>();
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<List<Track>>();
            return list ?? new List<Track>();
        }

        public async Task<IEnumerable<Track>> SearchTracksAsync(string query)
        {
            AddAuthHeader();
            var uri = $"api/v1/tracks/search?query={Uri.EscapeDataString(query)}";
            var list = await _http.GetFromJsonAsync<List<Track>>(uri);
            return list ?? new List<Track>();
        }

        public async Task<IEnumerable<int>> GetArchivedTrackIdsAsync()
        {
            AddAuthHeader();
            var response = await _http.GetAsync("api/v1/archive");
            if (!response.IsSuccessStatusCode)
                return new List<int>();
            return await response.Content.ReadFromJsonAsync<List<int>>()
                   ?? new List<int>();
        }

        public async Task<bool> AddToArchiveAsync(int trackId)
        {
            AddAuthHeader();
            var res = await _http.PostAsJsonAsync("api/v1/archive",
                             new { trackId });
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveFromArchiveAsync(int trackId)
        {
            AddAuthHeader();
            var res = await _http.DeleteAsync($"api/v1/archive/{trackId}");
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> AddTrackToPlaylistAsync(int playlistId, int trackId)
        {
            AddAuthHeader();
            var res = await _http.PostAsJsonAsync(
                $"api/v1/playlists/{playlistId}/tracks",
                new { trackId });
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveTrackFromPlaylistAsync(int playlistId, int trackId)
        {
            AddAuthHeader();
            var res = await _http.DeleteAsync(
                $"api/v1/playlists/{playlistId}/tracks/{trackId}");
            return res.IsSuccessStatusCode;
        }

        public async Task<Track?> GetTrackByIdAsync(int trackId)
        {
            AddAuthHeader();
            var res = await _http.GetAsync($"api/v1/tracks/{trackId}");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<Track>();
        }
    }
}
