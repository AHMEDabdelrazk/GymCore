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

        var isAuthenticated = context.User?.Identity?.IsAuthenticated == true;
        var isGlobalAdmin = IsGlobalAdmin();

        // 1. If authenticated as a non-admin, strictly lock to their assigned JWT tenant claim.
        // Non-admins CANNOT override their branch via X-Tenant-ID header.
        if (isAuthenticated && !isGlobalAdmin)
        {
            var userTenantClaim = context.User?.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(userTenantClaim) && int.TryParse(userTenantClaim, out var lockedTenant) && lockedTenant > 0)
            {
                return lockedTenant;
            }
        }

        // 2. If SuperAdmin or unauthenticated (e.g. kiosk terminal / external callback), check X-Tenant-ID header
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader) &&
            int.TryParse(tenantHeader, out var parsedHeaderTenant) && parsedHeaderTenant > 0)
        {
            return parsedHeaderTenant;
        }

        // 3. Check JWT Claims as fallback
        var claimTenant = context.User?.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(claimTenant) && int.TryParse(claimTenant, out var parsedClaimTenant) && parsedClaimTenant > 0)
        {
            return parsedClaimTenant;
        }

        return 1; // Default to Tenant 1
    }

    public bool IsGlobalAdmin()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null || context.User == null)
            return false;

        return context.User.IsInRole(UserRoles.SuperAdmin);
    }

    public string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public string? GetCurrentUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
            ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }

    public string? GetClientIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return null;

        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded) && !string.IsNullOrEmpty(forwarded))
        {
            return forwarded.ToString().Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}
