namespace TeamStore.Keeper.Interfaces
{
    using System.Threading.Tasks;
    using TeamStore.Keeper.Enums;
    using TeamStore.Keeper.Models;

    public interface IPermissionService
    {
        /// <summary>
        /// Checks if the current ApplicationUser has any access to the specified project 
        /// </summary>
        /// <param name="projectId">The Id of the project to check for access</param>
        /// <param name="projectsService">An instance of IProjectService to resolve projects</param>
        /// <returns>A Task of bool, True if the current user has the role access to the specified project</returns>
        Task<bool> CurrentUserHasAccessAsync(int projectId, IProjectsService projectsService);

        /// <summary>
        /// Checks if the current ApplicationUser has the passed role access to the specified project 
        /// </summary>
        /// <param name="projectId">The Id of the project to check for access</param>
        /// <param name="projectsService">An instance of IProjectService to resolve projects</param>
        /// <param name="role">The role/level of access to check</param>
        /// <returns>A Task of bool, True if the current user has the role access to the specified project</returns>
        Task<bool> CurrentUserHasAccessAsync(int projectId, IProjectsService projectsService, Role role);

        ///// <summary>
        ///// Checks if the current ApplicationUser has any access to the specified project 
        ///// </summary>
        ///// <param name="project">The instance of the project to check for access</param>
        ///// <param name="projectsService">An instance of IProjectService to resolve projects</param>
        ///// <returns>True if the current user has the role access to the specified project</returns>
        //Task<bool> CurrentUserHasAccess(Project project, IProjectsService projectsService);

        ///// <summary>
        ///// Checks if the current ApplicationUser has the passed role access to the specified project 
        ///// </summary>
        ///// <param name="project">The instance of the project to check for access</param>
        ///// <param name="projectsService">An instance of IProjectService to resolve projects</param>
        ///// <param name="role">The role/level of access to check</param>
        ///// <returns>True if the current user has the role access to the specified project</returns>
        //Task<bool> CurrentUserHasAccess(Project project, IProjectsService projectsService, Role role);

        /// <summary>
        /// Grants access to a project. Checks if the calling user has access to give access.
        /// </summary>
        /// <param name="projectId">The Id of the project</param>
        /// <param name="upn">The UPN identifier of the identity for which access will be granted to</param>
        /// <param name="role">The role/level of access that will be granted</param>
        /// <param name="remoteIpAddress">The IP address of the incoming request</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>A Task object with an AccessChangeResult representing the result</returns>
        Task<AccessChangeResult> GrantAccessAsync(
            int projectId,
            string upn,
            Role role,
            string remoteIpAddress,
            IProjectsService projectsService
            );

        /// <summary>
        /// Revokes access to a project
        /// </summary>
        /// <param name="projectId">The Id of the project</param>
        /// <param name="upn">The UPN of the identity for which access will be granted to</param>
        /// <param name="role">The role/level of access that will be granted</param>
        /// <param name="revokingUser">The ApplicationUser revoking the access</param>
        /// <param name="remoteIpAddress">The IP address of the incoming request</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>A Task object with an AccessChangeResult representing the result</returns>
        Task<AccessChangeResult> RevokeAccessAsync(
            int projectId,
            string upn,
            Role role,
            string remoteIpAddress,
            IProjectsService projectsService
            );

        /// <summary>
        /// Checks if an ApplicationUser has the requested role against a project
        /// </summary>
        /// <param name="project">The Project to check</param>
        /// <param name="targetUser">The ApplicationUser to check</param>
        /// <param name="role">The role of interest</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>True if the user has the specified role, false if not.</returns>
        bool CheckAccess(Project project, ApplicationUser targetUser, Role role, IProjectsService projectsService);

        /// <summary>
        /// Checks if an ApplicationUser has the requested role against a project
        /// </summary>
        /// <param name="project">The Project to check</param>
        /// <param name="targetUserUpn">The Upn of the ApplicationUser to check</param>
        /// <param name="role">The role of interest</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>True if the user has the specified role, false if not.</returns>
        bool CheckAccess(Project project, string targetUserUpn, Role role, IProjectsService projectsService);

        /// <summary>
        /// Checks if an ApplicationUser has the requested role against a project
        /// </summary>
        /// <param name="projectId">The ID of the Project to check</param>
        /// <param name="targetUser">The ApplicationUser to check</param>
        /// <param name="role">The role of interest</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>A task of True if the user has the specified role, false if not.</returns>
        Task<bool> CheckAccessAsync(int projectId, ApplicationUser targetUser, Role role, IProjectsService projectsService);
    }
}
