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
    public class UserHomeController : Controller
    {
        private readonly IApiClient _api;
        public UserHomeController(IApiClient api) => _api = api;

        
        public async Task<IActionResult> Index(string query = "")
        {
            
            IEnumerable<Track> results = string.IsNullOrEmpty(query)
                ? Enumerable.Empty<Track>()
                : await _api.SearchTracksAsync(query);

            
            var archive = await _api.GetArchivedTrackIdsAsync();

            var vm = new UserHomeViewModel
            {
                Query = query,
                SearchResults = results,
                ArchivedTrackIds = archive
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddToArchive(int trackId)
        {
            await _api.AddToArchiveAsync(trackId);
            return RedirectToAction(nameof(Index), new { query = "" });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromArchive(int trackId)
        {
            await _api.RemoveFromArchiveAsync(trackId);
            return RedirectToAction(nameof(Index), new { query = "" });
        }
    }
}
