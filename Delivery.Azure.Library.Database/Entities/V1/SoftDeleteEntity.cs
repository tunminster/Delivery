using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Azure.Library.Database.Entities.V1
{
    public abstract class SoftDeleteEntity : Entity, ISoftDeleteEntity
    {
#pragma warning disable CS8618
        public virtual bool IsDeleted { get; set; }
#pragma warning restore CS8618

        public override string ToString()
        {
            return $"{GetType().Name}: {nameof(Id)}: {Id.Format()}, {nameof(ExternalId)}: {ExternalId.Format()}, {nameof(IsDeleted)}: {IsDeleted.Format()}";
        }
    }
}