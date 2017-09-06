using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamStore.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<Asset> Assets { get; set; }

    }
}
