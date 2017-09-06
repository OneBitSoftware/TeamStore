namespace TeamStore.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TeamStore.Models;

    public interface IProjectsService
    {
        Task<Project> GetProject(int projectId);

        Task<List<Project>> GetProjects();

    }
}
