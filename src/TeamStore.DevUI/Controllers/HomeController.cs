using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ApplicationInsights;
using System.Security.Claims;
using TeamStore.ViewModels;
using TeamStore.Keeper.Interfaces;

namespace TeamStore.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private IProjectsService _projectsService { get; set; }

        private readonly IAssetService _assetService;

        public HomeController(IProjectsService projectsService, IAssetService assetService)
        {
            _projectsService = projectsService ?? throw new ArgumentNullException(nameof(projectsService));
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
        }

        public async Task<IActionResult> Index()
        {
            var homeViewModel = new HomeViewModel();

            homeViewModel.Projects = await _projectsService.GetProjects();

            return View(homeViewModel);
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetAssetResults()
        {
            string accessIpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(accessIpAddress)) return BadRequest();

            try
            {
                var assets = await _assetService.GetAssetResultsAsync();
                return new OkObjectResult(assets);
            }
            catch (Exception ex)
            {
                // LOG through SERVICE TODO
                var t = new TelemetryClient();
                t.TrackException(ex);
                return BadRequest();
            }
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
