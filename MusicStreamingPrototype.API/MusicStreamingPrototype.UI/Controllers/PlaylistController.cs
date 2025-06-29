using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStreamingPrototype.UI.Models;
using MusicStreamingPrototype.UI.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MusicStreamingPrototype.UI.Controllers
{
    [Authorize]
    public class PlaylistController : Controller
    {
        private readonly IApiClient _api;
        public PlaylistController(IApiClient api) => _api = api;

        
        [HttpGet]
        public IActionResult Create()
            => View(new PlaylistViewModel());

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlaylistViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new Playlist
            {
                SpotifyId = Guid.NewGuid().ToString(),
                Name = vm.Name,
                Description = vm.Description
            };
            await _api.CreatePlaylistAsync(dto);
            return RedirectToAction("Index", "Home");
        }

        
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var playlist = await _api.GetPlaylistByIdAsync(id);
            if (playlist == null) return NotFound();

            
            var tracks = (await _api.GetTracksInPlaylistAsync(id)).ToList();

            
            var archivedIds = await _api.GetArchivedTrackIdsAsync();

            
            var archivedTracks = new List<Track>();
            foreach (var trackId in archivedIds)
            {
                var track = await _api.GetTrackByIdAsync(trackId);
                if (track != null) archivedTracks.Add(track);
            }

            var vm = new PlaylistDetailsViewModel
            {
                Playlist = playlist,
                Tracks = tracks,
                ArchivedTracks = archivedTracks
            };
            return View(vm);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTrack(int playlistId, int trackId)
        {
            await _api.AddTrackToPlaylistAsync(playlistId, trackId);
            return RedirectToAction(nameof(Details), new { id = playlistId });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTrack(int playlistId, int trackId)
        {
            await _api.RemoveTrackFromPlaylistAsync(playlistId, trackId);
            return RedirectToAction(nameof(Details), new { id = playlistId });
        }
    }
}
