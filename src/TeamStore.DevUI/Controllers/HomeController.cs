using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ApplicationInsights;
using System.Security.Claims;
using TeamStore.Factories;
using TeamStore.ViewModels;
using TeamStore.Keeper.Interfaces;

namespace TeamStore.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IProjectsService _projectsService;
        private readonly IAssetService _assetService;

        public HomeController(IProjectsService projectsService, IAssetService assetService)
        {
            _projectsService = projectsService ?? throw new ArgumentNullException(nameof(projectsService));
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var homeViewModel = new HomeViewModel();

            // CR 12/12/2017 This ViewModel returns a database Project. 
            // It should go through a factory for mapping purposes 
            homeViewModel.Projects = await _projectsService.GetProjects();

            return View(homeViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetAssetResults(string searchToken)
        {
            if (String.IsNullOrWhiteSpace(searchToken))
            {
                return BadRequest();
            }

            string accessIpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(accessIpAddress))
            {
                return BadRequest();
            }

            try
            {
                var assets = await _assetService.GetAssetResultsAsync(searchToken);
                return new OkObjectResult(AssetFactory.ConvertAssetSearch(assets));
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
