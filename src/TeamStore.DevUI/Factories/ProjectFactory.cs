namespace TeamStore.Factories
{
    using System.Linq;
    using TeamStore.DevUI.ViewModels;
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
            projectViewModel.Title = project.Title;
            projectViewModel.Category = project.Category;
            projectViewModel.Description = project.Description;
            projectViewModel.AccessList = project.AccessIdentifiers.Select(ai =>
            {
                var appIdentity = (ApplicationUser)ai.Identity;
                return $"{appIdentity.DisplayName} ({appIdentity.Upn})";
            });

            projectViewModel.AssetsList = project.Assets.Select(a =>
            {
                var assetViewModel = new ProjectAssetViewModel();

                switch (a.GetType().Name)
                {
                    case "Note":
                        var assetNote = a as Note;
                        assetViewModel.DisplayTitle = assetNote.Title;
                        break;
                    case "Credential":
                        var assetCredential = a as Credential;
                        assetViewModel.DisplayTitle = assetCredential.Login;
                        break;
                    default:
                        return null;
                }

                assetViewModel.Created = a.Created.ToString();
                assetViewModel.CreatedBy = a.CreatedBy.ToString();
                assetViewModel.Modified = a.Modified.ToString();
                assetViewModel.ModifiedBy = a.ModifiedBy.ToString();
                assetViewModel.IsEnabled = a.IsEnabled;

                return assetViewModel;
            });

            return projectViewModel;
        }
    }
}
