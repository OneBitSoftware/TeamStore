using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamStore.Models
{
    public class Project
    {
        public Project()
        {
            Assets = new List<Asset>();
            AccessIdentifiers = new List<AccessIdentifier>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public List<Asset> Assets { get; set; }
        public List<AccessIdentifier> AccessIdentifiers { get; set; }
        public bool IsArchived { get; set; }
    }
}
