using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TeamStore.Models
{
    public class AccessIdentifier
    {
        public string Identifier { get; set; }
        public string Role { get; set; }
        public int ProjectForeignKey { get; set; }

        [ForeignKey("ProjectForeignKey")]
        public Project Project { get; set; }
    }
}
