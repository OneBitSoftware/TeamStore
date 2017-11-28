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
    using Microsoft.AspNetCore.Authorization;

    [Authorize]
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
            if (id == null || id < 1)
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
            if (id < 1) return NotFound();

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
                    string accessIpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString();

                    await _assetService.AddAssetToProjectAsync(createViewModel.ProjectId, asset, accessIpAddress);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Details), new { id = createViewModel.ProjectId });
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
                    string accessIpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString();

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
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Category, IsPublic")] Project project)
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

            if (await _permissionService.CurrentUserHasAccessAsync(id, _projectsService, Keeper.Enums.Role.Owner) == false) return Unauthorized();

            var shareProjectViewModel = ProjectFactory.ConvertForShare(project);

            return View(shareProjectViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share([Bind("ProjectId, Details, Role")]AccessChangeProjectViewModel shareProjectViewModel)
        {
            // TODO add bind

            // validate

            // Build user
            var remoteIpAddress = this.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var accessResult = await _permissionService.GrantAccessAsync(
                shareProjectViewModel.ProjectId,
                shareProjectViewModel.Details,
                shareProjectViewModel.Role,
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
        // TODO
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
                Keeper.Enums.Role.Owner,
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

            string accessIpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(accessIpAddress)) return BadRequest();

            try
            {

                var asset = await _assetService.GetAssetAsync(getPasswordViewModel.ProjectId, getPasswordViewModel.AssetId, accessIpAddress);
                var credential = asset as Credential;
                var decryptedPassword = _assetService.DecryptPassword(credential.Password);

                return new OkObjectResult(decryptedPassword);
            }
            catch (Exception ex)
            {
                // LOG through SERVICE TODO
                var t = new TelemetryClient();
                t.TrackException(ex);
                return BadRequest();
            }
        }

        // GET: Projects/6/UpdatePassword/5
        [HttpGet]
        public async Task<IActionResult> UpdatePassword(int projectId, int assetId)
        {
            // Validation
            if (projectId < 1) throw new ArgumentNullException(nameof(projectId));
            if (assetId < 1) throw new ArgumentNullException(nameof(assetId));

            // get project if current user has permission
            var project = await _projectsService.GetProject(projectId);
            if (project == null) return NotFound(); // return if no access or no such projectId

            // get the current user IP
            string accessIpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString();

            // retrieve the asset
            var asset = await _assetService.GetAssetAsync(project.Id, assetId, accessIpAddress);

            if (asset == null) return NotFound(); // return if no access or no such assetId

            // prepare view model
            var credential = asset as Credential;
            var viewModel = new UpdatePasswordViewModel();
            viewModel.ProjectId = project.Id;
            viewModel.ProjectTitle = project.Title;
            viewModel.AssetTitle = asset.Title;
            viewModel.AssetId = asset.Id;
            viewModel.Login = credential.Login;

            return View(viewModel);
        }

        // GET: Projects/6/ArchivePassword/5
        [HttpGet]
        public async Task<IActionResult> ArchivePassword(int projectId, int assetId)
        {
            if (projectId < 1) throw new ArgumentNullException(nameof(projectId));
            if (assetId < 1) throw new ArgumentNullException(nameof(assetId));

            // get project if current user has permission
            var project = await _projectsService.GetProject(projectId);
            if (project == null) return NotFound();

            // get the current user IP
            string accessIpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var asset = await _assetService.GetAssetAsync(project.Id, assetId, accessIpAddress);

            return View();
        }

        // POST: Projects/6/UpdatePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword([Bind("ProjectId, AssetId, Password")] UpdatePasswordViewModel updatePasswordViewModel)
        {
            if (updatePasswordViewModel == null)
            {
                throw new ArgumentNullException(nameof(updatePasswordViewModel));
            }

            if (ModelState.IsValid)
            {
                // check valid project, do not decrypt, checks access
                var project = await _projectsService.GetProject(updatePasswordViewModel.ProjectId, true);
                if (project == null) return NotFound();

                // get the current user IP
                string accessIpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString();

                // Perform the credential update
                await _assetService.UpdateAssetPasswordAsync(
                    updatePasswordViewModel.ProjectId,
                    updatePasswordViewModel.AssetId,
                    updatePasswordViewModel.Password,
                    accessIpAddress);
            }
            else
            {
                return BadRequest("Model is invalid.");
            }

            return RedirectToAction(nameof(Details), new { id = updatePasswordViewModel.ProjectId });
        }

        // POST: Projects/6/ArchivePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchivePassword([Bind("ProjectId, AssetId")] ArchivePasswordViewModel archivePasswordViewModel)
        {
            if (archivePasswordViewModel == null)
            {
                throw new ArgumentNullException(nameof(archivePasswordViewModel));
            }

            if (ModelState.IsValid)
            {
                // Check permissions in service

            }

            return View();
        }
    }
}
