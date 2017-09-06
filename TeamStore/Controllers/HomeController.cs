using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamStore.Models;
using System.Security.Claims;
using TeamStore.ViewModels;
using TeamStore.Services;
using TeamStore.Interfaces;

namespace TeamStore.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IProjectsService ProjectsService { get; set; }

        public HomeController(IProjectsService projectsService)
        {
            ProjectsService = projectsService ?? throw new ArgumentNullException(nameof(projectsService));
        }

        public async Task<IActionResult> Index()
        {
            var homeViewModel = new HomeViewModel();

            homeViewModel.Projects = await ProjectsService.GetProjects();

            return View(homeViewModel);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
