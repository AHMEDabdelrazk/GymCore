namespace GymCore.API.Services.Interfaces;

public interface ITenantProvider
{
    int GetCurrentTenantId();
    bool IsGlobalAdmin();
    string? GetCurrentUserId() => null;
    string? GetCurrentUserEmail() => null;
    string? GetClientIpAddress() => null;
}
