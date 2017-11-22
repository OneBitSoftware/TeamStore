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
    using TeamStore.DevUI.ViewModels;
    using Microsoft.ApplicationInsights;

    public class ProjectsController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly IProjectsService _projectsService;
        private readonly IGraphService _graphService;
        private readonly IAssetService _assetService;
        private readonly IApplicationIdentityService _applicationIdentityService;

        public ProjectsController(
            IPermissionService permissionService,
            IProjectsService projectsService,
            IAssetService assetService,
            IGraphService graphService,
            IApplicationIdentityService applicationIdentityService
            )
        {
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _projectsService = projectsService ?? throw new ArgumentNullException(nameof(projectsService));
            _graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            _applicationIdentityService = applicationIdentityService ?? throw new ArgumentNullException(nameof(applicationIdentityService));
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            // TODO: return ProjectViewModel not DB Project
            return View(await _projectsService.GetProjects(false));
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectsService.GetProject(id.Value);
            await _assetService.LoadAssets(project);
            
            if (project == null)
            {
                return NotFound();
            }

            var projectViewModel = ProjectFactory.Convert(project);

            return View(projectViewModel);
        }

        // GET: Projects/CreateCredential/5
        public async Task<IActionResult> CreateCredential(int id)
        {
            var project = await _projectsService.GetProject(id);

            if (project == null)
            {
                return NotFound();
            }

            var projectViewModel = ProjectFactory.GetCredentialViewModel(project);

            return View(projectViewModel);
        }

        // POST: Projects/CreateCredential
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCredential([Bind("ProjectId,Login,Body,Password,Title")] CreateCredentialViewModel createViewModel)
        {
            if (createViewModel.ProjectId < 0)
            {
                return NotFound();
            }

            if (ModelState.IsValid == true)
            {
                try
                {
                    var asset = new Credential();

                    asset.Title = createViewModel.Title;
                    asset.Notes = createViewModel.Notes;
                    asset.Login = createViewModel.Login;
                    asset.Password = createViewModel.Password;

                    // get IP
                    string accessIpAddress = string.Empty;
                    if (HttpContext != null)
                    {
                        accessIpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                    }

                    await _assetService.AddAssetToProjectAsync(createViewModel.ProjectId, asset, accessIpAddress);
                }
                catch (DbUpdateConcurrencyException)
                {
                        throw;
                }
                return RedirectToAction(nameof(Details), new { id = createViewModel.ProjectId } );
            }

            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> CreateNote(int id, [Bind("ProjectId,Title,Notes")] CreateNoteViewModel createNoteViewModel)
        {
            if (createNoteViewModel.ProjectId < 0)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var asset = new Note();

                    asset.Title = createNoteViewModel.Title;
                    asset.Notes = createNoteViewModel.Notes;

                    // get IP
                    string accessIpAddress = string.Empty;
                    if (HttpContext != null)
                    {
                        accessIpAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                    }

                    await _assetService.AddAssetToProjectAsync(createNoteViewModel.ProjectId, asset, accessIpAddress);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Details), new { id = createNoteViewModel.ProjectId });
            }

            return RedirectToAction(nameof(Index));
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(AccessChangeProjectViewModel shareProjectViewModel, int id)
        {
            // Build user
            var remoteIpAddress = this.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var accessResult = await _permissionService.GrantAccessAsync(
                id,
                shareProjectViewModel.Details,
                "Owner",
                remoteIpAddress,
                _projectsService);

            shareProjectViewModel.Result = accessResult.Success;

            if (string.IsNullOrWhiteSpace(accessResult.Message) == false)
            {
                shareProjectViewModel.Details = accessResult.Message;
            }

            return View(shareProjectViewModel);
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

                    throw;
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

        // TODO: need to change this to HttpPost called from AJAX and validate the antiforgerytoken
        public async Task<IActionResult> RevokeAccess(int id, string upn)
        {
            var remoteIpAddress = this.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var accessResult = await _permissionService.RevokeAccessAsync(
                id,
                upn,
                "Owner",
                remoteIpAddress,
                _projectsService);

            return RedirectToAction(nameof(Details), new { id = id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetPassword([FromBody] GetPasswordViewModel getPasswordViewModel)
        {
            // Given this is a sensitive method, we don't give the correct error
            if (ModelState.IsValid == false) return BadRequest();
            if (getPasswordViewModel.AssetId < 0) return BadRequest();
            if (getPasswordViewModel.ProjectId < 0) return BadRequest();
            if (string.IsNullOrWhiteSpace(getPasswordViewModel.FormDigest)) return BadRequest();

            var remoteIpAddress = this.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) return BadRequest();

            try
            {

                var asset = await _assetService.GetAssetAsync(getPasswordViewModel.ProjectId, getPasswordViewModel.AssetId, remoteIpAddress);
                var credential = asset as Credential;
                var decryptedPassword = _assetService.DecryptPassword(credential.Password);

                return new OkObjectResult(decryptedPassword);
            }
            catch (Exception ex)
            {
                // LOG
                var t = new TelemetryClient();
                t.TrackException(ex);
                return BadRequest();
            }
        }
    }
}
