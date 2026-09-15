namespace GymCore.API.Models.Interfaces;

/// <summary>
/// Marks an entity as scoped to a specific tenant/gym branch for multi-tenant isolation.
/// </summary>
public interface ITenantEntity
{
    int TenantId { get; set; }
}
