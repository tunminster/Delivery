using System;
using System.Collections.Generic;

namespace Delivery.Azure.Library.Contracts.Interfaces.V1.Contracts
{
    /// <summary>
    ///     Contains contract definitions which are required when storing data in cosmos db
    /// </summary>
    public interface IDocumentContract<TData>
    {
        Guid? Id { get; set; }

        string? PartitionKey { get; }

        int Version { get; set; }

        string? InsertedBy { get; set; }

        DateTimeOffset? InsertionDate { get; set; }

        DateTimeOffset? ValidFromDate { get; set; }

        DateTimeOffset? ValidToDate { get; set; }

        List<TData> Data { get; set; }
    }
}