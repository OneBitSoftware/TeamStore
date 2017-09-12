namespace TeamStore.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TeamStore.DataAccess;
    using TeamStore.Interfaces;
    using TeamStore.Models;

    public class ProjectsService : IProjectsService
    {
        private ApplicationDbContext DbContext { get; set; }
        private readonly IEncryptionService _encryptionService;

        public ProjectsService(ApplicationDbContext context, IEncryptionService encryptionService)
        {
            DbContext = context ?? throw new ArgumentNullException(nameof(context));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        public async Task<List<Project>> GetProjects()
        {
            // Validate access

            // Get projects with access TODO
            var projects = await DbContext.Projects.ToListAsync();
            foreach (var project in projects)
            {
                project.Title = _encryptionService.DecryptStringAsync(project.Title);
                project.Description = _encryptionService.DecryptStringAsync(project.Description);
            }

            return projects;
        }

        public async Task<Project> GetProject(int projectId)
        {
            // Validate access TODO

            // Get project
            var result =  await DbContext.Projects.Where(p => p.Id == projectId).FirstOrDefaultAsync();

            result.Title = _encryptionService.DecryptStringAsync(result.Title);
            result.Description = _encryptionService.DecryptStringAsync(result.Description);

            return result;
        }

        public async void CreateProject(Project project)
        {
            // Encrypt
            project.Title = _encryptionService.EncryptStringAsync(project.Title);
            project.Description = _encryptionService.EncryptStringAsync(project.Description);

            // Save
            await DbContext.Projects.AddAsync(project);
        }
    }
}
