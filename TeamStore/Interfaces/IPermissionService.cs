namespace TeamStore.Interfaces
{
    using System.Threading.Tasks;
    using TeamStore.Models;

    public interface IPermissionService
    {
        /// <summary>
        /// Checks if the current ApplicationUser has the passed role access to the specified project 
        /// </summary>
        /// <param name="projectId">The Id of the project to check for access</param>
        /// <param name="projectsService">An instance of IProjectService to resolve projects</param>
        /// <param name="role">The role/level of access to check</param>
        /// <returns>A Task of bool, True if the current user has the role access to the specified project</returns>
        Task<bool> CurrentUserHasAccessAsync(int projectId, IProjectsService projectsService, string role);

        /// <summary>
        /// Checks if the current ApplicationUser has the passed role access to the specified project 
        /// </summary>
        /// <param name="project">The instance of the project to check for access</param>
        /// <param name="projectsService">An instance of IProjectService to resolve projects</param>
        /// <param name="role">The role/level of access to check</param>
        /// <returns>True if the current user has the role access to the specified project</returns>
        bool CurrentUserHasAccess(Project project, IProjectsService projectsService, string role);

        /// <summary>
        /// Grants access to a project
        /// </summary>
        /// <param name="projectId">The Id of the project</param>
        /// <param name="azureAdObjectIdentifier">The identifier of the identity for which access will be granted to</param>
        /// <param name="role">The role/level of access that will be granted</param>
        /// <param name="grantingUser">The ApplicationUser granting the access</param>
        /// <param name="remoteIpAddress">The IP address of the incoming request</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>A Task object</returns>
        Task GrantAccessAsync(
            int projectId,
            string azureAdObjectIdentifier,
            string role,
            ApplicationUser grantingUser,
            string remoteIpAddress,
            IProjectsService projectsService
            );

        /// <summary>
        /// Revokes access to a project
        /// </summary>
        /// <param name="projectId">The Id of the project</param>
        /// <param name="azureAdObjectIdentifier">The identifier of the identity for which access will be granted to</param>
        /// <param name="role">The role/level of access that will be granted</param>
        /// <param name="revokingUser">The ApplicationUser revoking the access</param>
        /// <param name="remoteIpAddress">The IP address of the incoming request</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>A Task object</returns>
        Task RevokeAccessAsync(
            int projectId,
            string azureAdObjectIdentifier,
            string role,
            ApplicationUser revokingUser,
            string remoteIpAddress,
            IProjectsService projectsService
            );
    }
}
