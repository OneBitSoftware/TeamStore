using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamStore.DevUI.ViewModels
{
    public class ProjectAssetViewModel
    {
        public int AssetId { get; set; }
        public bool IsArchived { get; set; }
        public string DisplayTitle { get; set; }
        public string Login { get; set; }
        public string Domain { get; set; }
        public string CreatedBy { get; set; }
        public string Created { get; set; }
        public string ModifiedBy { get; set; }
        public string Modified { get; set; }

        public override string ToString()
        {
            return $"{DisplayTitle} {Created} {CreatedBy} {Modified} {ModifiedBy}";
        }
    }
}
