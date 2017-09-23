namespace TeamStore.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using TeamStore.ViewModels;
    using TeamStore.Factories;
    using TeamStore.Keeper.Interfaces;
    using TeamStore.Keeper.Models;

    public class ProjectsController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly IProjectsService _projectsService;
        private readonly IGraphService _graphService;
        private readonly IApplicationIdentityService _applicationIdentityService;

        public ProjectsController(
            IPermissionService permissionService,
            IProjectsService projectsService,
            IGraphService graphService,
            IApplicationIdentityService applicationIdentityService
            )
        {
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _projectsService = projectsService ?? throw new ArgumentNullException(nameof(projectsService));
            _graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
            _applicationIdentityService = applicationIdentityService ?? throw new ArgumentNullException(nameof(applicationIdentityService));
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            // TODO: return ProjectViewModel not DB Project
            return View(await _projectsService.GetProjects());
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectsService.GetProject(id.Value);

            if (project == null)
            {
                return NotFound();
            }

            var projectViewModel = ProjectFactory.Convert(project);

            return View(projectViewModel);
        }

        // GET: Projects/CreateCredential
        public IActionResult CreateCredential()
        {
            return View();
        }

        // POST: Projects/CreateCredential
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCredential(int id, [Bind("Id,Title,Description,Category")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //_context.Update(project);
                    //await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/CreateNote
        public IActionResult CreateNote()
        {
            return View();
        }

        // POST: Projects/CreateNote
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNote(int id, [Bind("Id,Title,Description,Category")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //_context.Update(project);
                    //await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Category")] Project project)
        {
            if (ModelState.IsValid)
            {
                await _projectsService.CreateProject(project);
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        [HttpGet]
        public async Task<IActionResult> Share(int id)
        {
            var project = await _projectsService.GetProject(id);

            if (project == null)
            {
                return NotFound();
            }

            if (await _permissionService.CurrentUserHasAccessAsync(id, _projectsService, "Owner") == false) return Unauthorized();

            var shareProjectViewModel = ProjectFactory.ConvertForShare(project);

            return View(shareProjectViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Share(ShareProjectViewModel shareProjectViewModel, int id)
        {
            // Build user
            var remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            await _permissionService.GrantAccessAsync(
                id,
                shareProjectViewModel.ShareDetails,
                "Owner",
                HttpContext.Connection.RemoteIpAddress.ToString(),
                _projectsService);

            return View();
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectsService.GetProject(id.Value);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Category")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //_context.Update(project);
                    //await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectsService.GetProject(id.Value);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //var project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == id);
            //_context.Projects.Remove(project);
            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            var retrievedProject = _projectsService.GetProject(id);
            if (retrievedProject != null) return true;
            return false;
        }
    }
}
