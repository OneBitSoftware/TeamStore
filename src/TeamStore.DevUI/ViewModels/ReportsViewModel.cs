namespace TeamStore.DevUI.ViewModels
{
    using System.Collections.Generic;

    public class ReportsViewModel
    {
        public ReportsViewModel()
        {
            Reports = new Dictionary<string, string>();
        }

        /// <summary>
        /// Holds a list of available reports and their controller action
        /// </summary>
        public IDictionary<string, string> Reports { get; set; }
    }
}
