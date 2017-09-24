namespace TeamStore.Factories
{
    using TeamStore.Keeper.Models;
    using TeamStore.ViewModels;

    public static class ProjectFactory
    {
        public static ShareProjectViewModel ConvertForShare(Project project)
        {
            var projectViewModel = new ShareProjectViewModel();
            projectViewModel.ProjectId = project.Id;
            projectViewModel.Title = project.Title;

            return projectViewModel;
        }

        public static ProjectViewModel Convert(Project project)
        {
            var projectViewModel = new ProjectViewModel();
            projectViewModel.Id = project.Id;

            return projectViewModel;
        }
    }
}
