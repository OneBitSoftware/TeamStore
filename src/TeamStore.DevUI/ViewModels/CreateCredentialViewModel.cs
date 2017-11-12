using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamStore.DevUI.ViewModels
{
    public class CreateCredentialViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string Title { get; set; }
        public string Login { get; set; }
        public string Domain { get; set; }
        public string Password { get; set; }
    }
}
