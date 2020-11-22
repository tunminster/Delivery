using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Azure.Library.Contracts.Interfaces.V1.Entities;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Azure.Library.Database.Entities.V1
{
    public abstract class Entity : IEntity
    {
        public override string ToString()
        {
            return $"{GetType().Name}: {nameof(Id)}: {Id.Format()}, {nameof(ExternalId)}: {ExternalId.Format()}";
        }

#pragma warning disable CS8618
        /// <summary>
        ///     The internal id which is used for primary and foreign keys
        /// </summary>
        [Required]
        [Key]
        public virtual int Id { get; set; }

        /// <summary>
        ///     Represents an identifier that can be shared with other people safely
        /// </summary>
        /// <remarks>For a policy this would be the policy id which is shown on emails and documents and should be human-readable</remarks>
        [Column(TypeName = "NVARCHAR(40)")]
        [StringLength(maximumLength: 40)]
        [Required]
        public virtual string ExternalId { get; set; }
#pragma warning restore CS8618
    }
}