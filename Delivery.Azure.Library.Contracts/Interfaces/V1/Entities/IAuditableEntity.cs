using System;

namespace Delivery.Azure.Library.Contracts.Interfaces.V1.Entities
{
    /// <summary>
    ///     Adds a column to specify who inserted the new entity
    /// </summary>
    public interface IAuditableEntity
    {
        /// <summary>
        ///     Indicates the user name of the person or process who inserted the entity
        /// </summary>
        string InsertedBy { get; set; }

        /// <summary>
        ///     Entities should only only have reads and inserts (not updates or deletes) in order to preserve audit history
        ///     The insertion datetime is used to check when a record was created or 'updated' (inserted into the database)
        /// </summary>
        DateTimeOffset InsertionDateTime { get; set; }
    }
}