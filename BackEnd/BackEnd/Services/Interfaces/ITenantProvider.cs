namespace GymCore.API.Services.Interfaces;

public interface ITenantProvider
{
    int GetCurrentTenantId();
    bool IsGlobalAdmin();
}
