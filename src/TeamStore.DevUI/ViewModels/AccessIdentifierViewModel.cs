using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamStore.Keeper.Enums;

namespace TeamStore.DevUI.ViewModels
{
    public class AccessIdentifierViewModel
    {
        public string DisplayName { get; set; }
        public Role Role { get; set; }
        public string Upn { get; set; }
        public DateTime LastModified { get; set; }
    }
}
