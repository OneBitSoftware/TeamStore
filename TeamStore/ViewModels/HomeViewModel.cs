namespace TeamStore.ViewModels
{
    using System.Collections.Generic;
    using TeamStore.Models;

    public class HomeViewModel
    {
        public HomeViewModel()
        {
            Projects = new List<Project>();
        }

        public List<Project> Projects { get; set; }
    }
}
