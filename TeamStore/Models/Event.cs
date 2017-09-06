using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using TeamStore.Enums;

namespace TeamStore.Models
{
    public class Event
    {
        public int Id { get; set; }

        public EventType Type { get; set; }

        public ApplicationUser User { get; set; }

        public int AssetForeignKey { get; set; }

        [ForeignKey("AssetForeignKey")]
        public Asset Asset { get; set; }

        public DateTime DateTime { get; set; }

        public string OldValue { get; set; }
        public string NewValue { get; set; }

    }
}
