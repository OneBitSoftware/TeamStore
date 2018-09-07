using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TeamStore.Keeper.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading;
using System.Text;
using TeamStore.Keeper.Models;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;

namespace TeamStore.DevUI.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly IProjectsService _projectsService;
        private readonly IAssetService _assetService;
        private readonly IApplicationIdentityService _applicationIdentityService;
        private readonly IConfiguration _configuration;

        public AdminController(
            IPermissionService permissionService,
            IProjectsService projectsService,
            IApplicationIdentityService applicationIdentityService,
            IAssetService assetService,
            IConfiguration configuration
            )
        {
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _projectsService = projectsService ?? throw new ArgumentNullException(nameof(projectsService));
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            _applicationIdentityService = applicationIdentityService ?? throw new ArgumentNullException(nameof(applicationIdentityService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IActionResult> Index()
        {
            if (await _applicationIdentityService.IsCurrentUserAdmin() == false)
            {
                return Unauthorized();
            }

            return View();
        }

        public async Task<JsonResult> ExportDatabase()
        {
            var currentUser = await _applicationIdentityService.GetCurrentUser();

            if (currentUser == null) return new JsonResult("Unathorised");

            if (await _applicationIdentityService.IsCurrentUserAdmin() == false)
            {
                return new JsonResult("Unathorised");
            }

            var allProjects = await _projectsService.GetProjects();

            foreach (var project in allProjects)
            {
                await _assetService.LoadAssetsAsync(project);
                foreach (var asset in project.Assets)
                {
                    if (asset.GetType() == typeof(Credential))
                    {
                        var credential = asset as Credential;
                        credential.Password = _assetService.DecryptPassword(credential.Password);
                    }
                }
            }

            return Json(allProjects, new Newtonsoft.Json.JsonSerializerSettings()
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            });
        }

        [HttpGet]
        public async Task<IActionResult> ImportDatabase()
        {
            if (await _applicationIdentityService.IsCurrentUserAdmin() == false)
            {
                return Unauthorized();
            }

            return View();
        }

        /// <summary>
        /// This is very experimental and not for production purposes.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ImportDatabase(ICollection<IFormFile> files)
        {
            if (await _applicationIdentityService.IsCurrentUserAdmin() == false)
            {
                return Unauthorized();
            }

            long size = files.Sum(f => f.Length);

            string accessIpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString();

            // full path to file in temp location
            var filePath = Path.GetTempFileName();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var reader = new StreamReader(
                        formFile.OpenReadStream(),
                        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
                        detectEncodingFromByteOrderMarks: true))
                    {
                        var fileContents = await reader.ReadToEndAsync();

                        if (fileContents.Length > 0)
                        {
                            var projects = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Project>>(
                                fileContents, new JsonSerializerSettings
                                {
                                    TypeNameHandling = TypeNameHandling.All
                                });

                            foreach (var project in projects)
                            {
                                // the import file is decrypted, so we need to encrypt them
                                foreach (var asset in project.Assets)
                                {
                                    _assetService.EncryptAsset(asset);
                                }
                                var createResult = await _projectsService.ImportProject(project, accessIpAddress);
                            }
                        }
                        else
                        {
                            return new OkObjectResult("Failed.");
                        }
                    }

                }
            }


            return Ok("Import successfull");
        }

        public async Task<IActionResult> CreateMeAsAdmin()
        {
            var currentUser = await _applicationIdentityService.GetCurrentUser();

            if (currentUser == null) return Unauthorized();

            List<string> items = new List<string>();
            var systemAdministratorsConfiguration = _configuration.GetSection("SystemAdministrators");

            if (systemAdministratorsConfiguration == null) return Unauthorized();

            systemAdministratorsConfiguration.Bind(items);

            if (items.Contains(currentUser.Upn) == false)
            {
                return Unauthorized();
            }

            var setResult = await _applicationIdentityService.SetSystemAdministrator(currentUser);

            if (setResult)
            {
                return new OkObjectResult("Done");
            }
            else
                return new OkObjectResult("Failed.");
        }
    }
}