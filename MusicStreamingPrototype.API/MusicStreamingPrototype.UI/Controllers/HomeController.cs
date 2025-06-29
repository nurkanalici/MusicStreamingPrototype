using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStreamingPrototype.UI.Models;
using MusicStreamingPrototype.UI.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStreamingPrototype.UI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IApiClient _api;
        public HomeController(IApiClient api) => _api = api;

        
        [HttpGet("/")]
        [HttpGet("Home")]
        [HttpGet("Home/Index")]
        public async Task<IActionResult> Index(string? query)
        {
            var vm = new UserHomeViewModel
            {
                Query = query ?? string.Empty,
                
                SearchResults = string.IsNullOrEmpty(query)
                    ? Enumerable.Empty<Track>()
                    : await _api.SearchTracksAsync(query),
                ArchivedTrackIds = await _api.GetArchivedTrackIdsAsync(),
                Playlists = await _api.GetPlaylistsAsync()
            };
            return View(vm);
        }

        
        [HttpPost("Home/Archive/Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToArchive(int trackId, string? query)
        {
            await _api.AddToArchiveAsync(trackId);
            return RedirectToAction(nameof(Index), new { query });
        }

        
        [HttpPost("Home/Archive/Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromArchive(int trackId, string? query)
        {
            await _api.RemoveFromArchiveAsync(trackId);
            return RedirectToAction(nameof(Index), new { query });
        }

        
        [HttpGet("Home/Privacy")]
        public IActionResult Privacy() => View();
    }
}
