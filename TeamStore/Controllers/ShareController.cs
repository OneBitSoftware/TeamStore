using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TeamStore.Interfaces;

namespace TeamStore.Controllers
{
    public class ShareController : Controller
    {
        private IProjectsService _projectsService { get; set; }

        public ShareController(IProjectsService projectsService)
        {
            _projectsService = projectsService ?? throw new ArgumentNullException(nameof(projectsService));
        }

        public IActionResult Index(int projectId)
        {
            var project = _projectsService.GetProject(projectId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }
    }
}
