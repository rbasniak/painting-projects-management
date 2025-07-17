using System.ComponentModel.DataAnnotations;

namespace rbkApiModules.Commons.Core;

public abstract class TenantEntity : BaseEntity
{
    protected TenantEntity() : base()
    {

    }

    [MaxLength(32)]
    public string? TenantId { get; protected set; }

    public bool HasTenant => !String.IsNullOrEmpty(TenantId);

    public bool HasNoTenant => String.IsNullOrEmpty(TenantId);
}
