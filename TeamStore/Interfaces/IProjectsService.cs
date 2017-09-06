namespace TeamStore.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TeamStore.Models;

    public interface IProjectsService
    {
        Task<List<Project>> GetProjects();
    }
}
