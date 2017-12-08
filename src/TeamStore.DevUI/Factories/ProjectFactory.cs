namespace TeamStore.Factories
{
    using System;
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
            projectViewModel.ProjectTitle = project.Title;

            return projectViewModel;
        }

        public static ProjectAssetViewModel ConvertAsset(Asset asset)
        {
            var assetViewModel = new ProjectAssetViewModel();

            switch (asset.GetType().Name)
            {
                case "Note":
                    var assetNote = asset as Note;
                    assetViewModel.DisplayTitle = assetNote.Title;
                    break;
                case "Credential":
                    var assetCredential = asset as Credential;
                    assetViewModel.DisplayTitle = assetCredential.Title;
                    assetViewModel.Login = assetCredential.Login;
                    break;
                default:
                    return null;
            }

            assetViewModel.AssetId = asset.Id;
            assetViewModel.Created = asset.Created != null ? asset.Created.ToString() : "Never";
            assetViewModel.CreatedBy = asset.CreatedBy != null ? asset.CreatedBy.ToString() : string.Empty;
            assetViewModel.ModifiedBy = asset.ModifiedBy != null ? asset.ModifiedBy?.DisplayName?.ToString() : string.Empty;
            assetViewModel.IsArchived = asset.IsArchived;
            assetViewModel.Notes = asset.Notes;

            if (asset.Modified != null && asset.Modified == DateTime.MinValue)
            {
                assetViewModel.Modified = "Never";
            }
            else
            {
                assetViewModel.Modified = asset.Modified != null ? asset.Modified.ToString() : "Never";
            }

            if (assetViewModel.DisplayTitle == null) assetViewModel.DisplayTitle = string.Empty;

            return assetViewModel;
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

            projectViewModel.AssetsList = project.Assets.Where(a=>a.IsArchived == false).Select(asset =>
            {
                var assetViewModel = new ProjectAssetViewModel();

                switch (asset.GetType().Name)
                {
                    case "Note":
                        var assetNote = asset as Note;
                        assetViewModel.DisplayTitle = assetNote.Title;
                        break;
                    case "Credential":
                        var assetCredential = asset as Credential;
                        assetViewModel.DisplayTitle = assetCredential.Title;
                        assetViewModel.Login = assetCredential.Login;
                        break;
                    default:
                        return null;
                }

                assetViewModel.AssetId = asset.Id;
                assetViewModel.Created = asset.Created != null ? asset.Created.ToString() : "Never";
                assetViewModel.CreatedBy = asset.CreatedBy != null ? asset.CreatedBy.ToString() : string.Empty;
                assetViewModel.ModifiedBy = asset.ModifiedBy != null ? asset.ModifiedBy?.DisplayName?.ToString() : string.Empty;
                assetViewModel.IsArchived = asset.IsArchived;
                assetViewModel.Notes = asset.Notes;

                if (asset.Modified != null && asset.Modified == DateTime.MinValue)
                {
                    assetViewModel.Modified = "Never";
                }
                else
                {
                    assetViewModel.Modified = asset.Modified != null ? asset.Modified.ToString() : "Never";
                }

                if (assetViewModel.DisplayTitle == null) assetViewModel.DisplayTitle = string.Empty;

                return assetViewModel;
            });

            return projectViewModel;
        }

        private static AccessIdentifierViewModel BuildAccessIdentityViewModel(AccessIdentifier accessIdentifier)
        {
            var newAccessIdentifierViewModel = new AccessIdentifierViewModel();
            var appIdentity = (ApplicationUser)accessIdentifier.Identity;

            newAccessIdentifierViewModel.DisplayName = appIdentity.DisplayName;
            newAccessIdentifierViewModel.Role = accessIdentifier.Role;
            newAccessIdentifierViewModel.Upn = appIdentity.Upn;
            newAccessIdentifierViewModel.LastModified = accessIdentifier.Modified == DateTime.MinValue ? accessIdentifier.Created : accessIdentifier.Modified;

            return newAccessIdentifierViewModel;
        }

        public static CreateCredentialViewModel GetCredentialViewModel(Project project)
        {
            var viewModel = new CreateCredentialViewModel();
            viewModel.ProjectId = project.Id;
            viewModel.ProjectTitle = project.Title;

            return viewModel;
        }

        public static CreateNoteViewModel GetNoteViewModel(Project project)
        {
            var viewModel = new CreateNoteViewModel();
            viewModel.ProjectId = project.Id;
            viewModel.ProjectTitle = project.Title;

            return viewModel;
        }
    }
}
