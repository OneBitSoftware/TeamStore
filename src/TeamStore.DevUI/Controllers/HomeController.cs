using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeamStore.ViewModels;
using TeamStore.Keeper.Interfaces;

namespace TeamStore.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private IProjectsService _projectsService { get; set; }

        public HomeController(IProjectsService projectsService)
        {
            _projectsService = projectsService ?? throw new ArgumentNullException(nameof(projectsService));
        }

        public async Task<IActionResult> Index()
        {
            var homeViewModel = new HomeViewModel();

            // CR 12/12/2017 This ViewModel returns a database Project. 
            // It should go through a factory for mapping purposes 
            homeViewModel.Projects = await _projectsService.GetProjects();

            return View(homeViewModel);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
