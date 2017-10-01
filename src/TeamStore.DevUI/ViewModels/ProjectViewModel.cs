using System.Collections.Generic;
using TeamStore.DevUI.ViewModels;

namespace TeamStore.ViewModels
{
    public class ProjectViewModel
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public IEnumerable<AccessIdentifierViewModel> AccessList { get; set; }
        public IEnumerable<ProjectAssetViewModel> AssetsList { get; set; }

        public bool RevokeAccessSuccess { get; set; }
        public string RevokeAccessMessage { get; set; }
    }
}
