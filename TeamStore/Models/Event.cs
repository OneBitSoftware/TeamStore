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

        /// <summary>
        /// Stores the logged in user who performed the event.
        /// </summary>
        public ApplicationUser ActedByUser { get; set; }

        public int? AssetForeignKey { get; set; }

        [ForeignKey("AssetForeignKey")]
        public Asset Asset { get; set; }

        /// <summary>
        /// The time of the event.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The IP address of the machine from which the event originated.
        /// </summary>
        public string RemoteIpAddress { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Data { get; set; }
    }
}
