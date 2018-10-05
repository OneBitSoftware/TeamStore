namespace TeamStore.Keeper.Models
{
    using System;

    public class Event
    {
        /// <summary>
        /// Primary key for the Event entity
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The type of event performed
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Details about the Azure AD Object Id or the primary key of the user who performed the event.
        /// </summary>
        public string ActedByUser { get; set; }

        /// <summary>
        /// The database primary key of the user who performed the event.
        /// </summary>
        public string ActedByUserId { get; set; }

        /// <summary>
        /// If this is an access grant/revoke event, this stores the primary key of the added or removed user/group
        /// </summary>
        public int? TargetUserId { get; set; }

        /// <summary>
        /// The database primary key of the asset related to the operation
        /// </summary>
        public int? AssetId { get; set; }

        /// <summary>
        /// The title of the asset related to the operation
        /// </summary>
        public string AssetTitle { get; set; }

        /// <summary>
        /// The login of the asset related to the operation
        /// </summary>
        public string AssetLogin { get; set; }

        /// <summary>
        /// The database primary key of the project related to the operation
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>
        /// The title of the project related to the operation
        /// </summary>
        public string ProjectTitle { get; set; }

        /// <summary>
        /// The time of the event.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The IP address of the machine from which the event originated.
        /// </summary>
        public string RemoteIpAddress { get; set; }

        /// <summary>
        /// Stores details of the value before a change
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// Stores details of the new value
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// Custom data about the event
        /// </summary>
        public string Data { get; set; }
    }
}
