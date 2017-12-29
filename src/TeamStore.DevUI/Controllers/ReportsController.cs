using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TeamStore.Keeper.Interfaces;
using TeamStore.DevUI.ViewModels;
using TeamStore.DevUI.ViewModels.Reports;
using TeamStore.Factories;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace TeamStore.DevUI.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IProjectsService _projectsService;
        private readonly IApplicationIdentityService _applicationIdentityService;

        public ReportsController(
            IApplicationIdentityService applicationIdentityService,
            IProjectsService projectsService
            )
        {
            _projectsService = projectsService ?? throw new ArgumentNullException(nameof(projectsService));
            _applicationIdentityService = applicationIdentityService ?? throw new ArgumentNullException(nameof(applicationIdentityService));
        }

        /// <summary>
        /// Check if the current user is a SystemAdministrator
        /// </summary>
        /// <returns>True if the current user is a SystemAdministrator.</returns>
        private async Task<bool> ValidateSystemAdministrator()
        {
            return await _applicationIdentityService.IsCurrentUserAdmin();
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            if (await ValidateSystemAdministrator() == false)
            {
                return Unauthorized();
            }

            var reportsListViewModel = new ReportsViewModel();
            reportsListViewModel.Reports.Add("Credential Access", "CredentialAccess");
            reportsListViewModel.Reports.Add("Archived Items", "ArchivedItems");
            reportsListViewModel.Reports.Add("User Logins", "UserLogins");

            return View(reportsListViewModel);
        }

        public async Task<IActionResult> ArchivedItems()
        {
            if (await ValidateSystemAdministrator() == false)
            {
                return Unauthorized();
            }

            var viewModel = new ArchivedItemsReportViewModel();

            var projects = await _projectsService.GetProjects(false, true);

            return View(viewModel);
        }
    }
}
