namespace TeamStore.Factories
{
    using System.Linq;
    using TeamStore.DevUI.ViewModels;
    using TeamStore.Keeper.Models;
    using TeamStore.ViewModels;

    // These method names need to be renamed to follow a consistent pattern
    public static class ProjectFactory
    {
        public static AccessChangeProjectViewModel ConvertForShare(Project project)
        {
            var projectViewModel = new AccessChangeProjectViewModel();
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
            projectViewModel.AccessList = project.AccessIdentifiers.Select(
                (ai) => BuildAccessIdentityViewModel(ai)
            );

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
                assetViewModel.IsArchived = a.IsArchived;

                return assetViewModel;
            });

            return projectViewModel;
        }

        private static AccessIdentifierViewModel BuildAccessIdentityViewModel(AccessIdentifier accessIdentifier)
        {
            var newAccessIdentifierViewModel = new AccessIdentifierViewModel();
            var appIdentity = (ApplicationUser)accessIdentifier.Identity;

            newAccessIdentifierViewModel.DisplayName = appIdentity.DisplayName;
            newAccessIdentifierViewModel.Upn = appIdentity.Upn;

            return newAccessIdentifierViewModel;
        }

        public static CreateCredentialViewModel GetCredentialViewModel(Project project)
        {
            var viewModel = new CreateCredentialViewModel();
            viewModel.ProjectId = project.Id;
            viewModel.ProjectTitle = project.Title;

            return viewModel;
        }
    }
}
