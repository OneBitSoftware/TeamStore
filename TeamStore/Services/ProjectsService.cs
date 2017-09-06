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

        public ProjectsService(ApplicationDbContext context)
        {
            DbContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Project>> GetProjects()
        {
            // Get projects with access
            return await DbContext.Projects.ToListAsync();
        }

        public async Task<Project> GetProject(int projectId)
        {
            // Validate access
            return await DbContext.Projects.Where(p => p.Id == projectId).FirstOrDefaultAsync();
        }
    }
}
