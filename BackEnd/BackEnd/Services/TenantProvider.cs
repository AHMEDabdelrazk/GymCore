using System.Security.Claims;
using GymCore.API.Models;
using GymCore.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GymCore.API.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private int? _forcedTenantId;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetTenantId(int tenantId)
    {
        _forcedTenantId = tenantId;
    }

    public int GetCurrentTenantId()
    {
        if (_forcedTenantId.HasValue)
            return _forcedTenantId.Value;

        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return 1; // Default tenant fallback for background jobs or unit tests

        // 1. Check custom header "X-Tenant-ID"
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader) &&
            int.TryParse(tenantHeader, out var parsedHeaderTenant) && parsedHeaderTenant > 0)
        {
            return parsedHeaderTenant;
        }

        // 2. Check JWT Claims
        var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantClaim) && int.TryParse(tenantClaim, out var parsedClaimTenant) && parsedClaimTenant > 0)
        {
            return parsedClaimTenant;
        }

        return 1; // Default to Tenant 1
    }

    public bool IsGlobalAdmin()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return false;

        return context.User.IsInRole(UserRoles.SuperAdmin);
    }
}
