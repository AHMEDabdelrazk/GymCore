using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GymCore.API.Models;
using GymCore.API.Models.Entities;
using GymCore.API.Models.Interfaces;
using GymCore.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GymCore.API.Data;

public class GymDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ITenantProvider? _tenantProvider;

    public GymDbContext(DbContextOptions<GymDbContext> options, ITenantProvider? tenantProvider = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<MembershipPlan> MembershipPlans { get; set; } = null!;
    public DbSet<Member> Members { get; set; } = null!;
    public DbSet<MemberSubscription> MemberSubscriptions { get; set; } = null!;
    public DbSet<ClassType> ClassTypes { get; set; } = null!;
    public DbSet<ClassSession> ClassSessions { get; set; } = null!;
    public DbSet<ClassBooking> ClassBookings { get; set; } = null!;
    public DbSet<CheckInRecord> CheckInRecords { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    public int CurrentTenantId => _tenantProvider?.GetCurrentTenantId() ?? 1;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Precision & Length Configurations
        modelBuilder.Entity<MembershipPlan>()
            .Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<MembershipPlan>()
            .Property(x => x.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<MemberSubscription>()
            .Property(x => x.PricePaid)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Invoice>()
            .Property(x => x.Amount)
            .HasColumnType("decimal(18,2)");

        // Webhook Idempotency: Unique constraint on EventId
        modelBuilder.Entity<ProcessedWebhookEvent>()
            .HasIndex(x => x.EventId)
            .IsUnique();

        // Concurrency token on ClassSession to prevent overbooking races
        modelBuilder.Entity<ClassSession>()
            .Property(x => x.RowVersion)
            .IsRowVersion();

        // Multi-tenant Global Query Filters
        // Automatically isolates queries by TenantId unless the caller is a Global Admin
        modelBuilder.Entity<Member>()
            .HasQueryFilter(e => _tenantProvider == null || _tenantProvider.IsGlobalAdmin() || e.TenantId == CurrentTenantId);

        modelBuilder.Entity<MembershipPlan>()
            .HasQueryFilter(e => _tenantProvider == null || _tenantProvider.IsGlobalAdmin() || e.TenantId == CurrentTenantId);

        modelBuilder.Entity<MemberSubscription>()
            .HasQueryFilter(e => _tenantProvider == null || _tenantProvider.IsGlobalAdmin() || e.TenantId == CurrentTenantId);

        modelBuilder.Entity<ClassType>()
            .HasQueryFilter(e => _tenantProvider == null || _tenantProvider.IsGlobalAdmin() || e.TenantId == CurrentTenantId);

        modelBuilder.Entity<ClassSession>()
            .HasQueryFilter(e => _tenantProvider == null || _tenantProvider.IsGlobalAdmin() || e.TenantId == CurrentTenantId);

        modelBuilder.Entity<ClassBooking>()
            .HasQueryFilter(e => _tenantProvider == null || _tenantProvider.IsGlobalAdmin() || e.TenantId == CurrentTenantId);

        modelBuilder.Entity<CheckInRecord>()
            .HasQueryFilter(e => _tenantProvider == null || _tenantProvider.IsGlobalAdmin() || e.TenantId == CurrentTenantId);

        modelBuilder.Entity<Invoice>()
            .HasQueryFilter(e => _tenantProvider == null || _tenantProvider.IsGlobalAdmin() || e.TenantId == CurrentTenantId);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Automatically stamp TenantId on added ITenantEntity entities if not set
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId <= 0)
            {
                entry.Entity.TenantId = CurrentTenantId;
            }
        }

        // 2. Capture audit logs for entity state diffs (excluding AuditLog itself)
        var auditEntries = CaptureAuditEntries();

        var result = await base.SaveChangesAsync(cancellationToken);

        // 3. Persist audit entries if any were generated
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private List<AuditLog> CaptureAuditEntries()
    {
        var auditLogs = new List<AuditLog>();
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
                        e.Entity is not ProcessedWebhookEvent &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => "Unknown"
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[property.Metadata.Name] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        oldValues[property.Metadata.Name] = property.OriginalValue;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[property.Metadata.Name] = property.OriginalValue;
                            newValues[property.Metadata.Name] = property.CurrentValue;
                        }
                        break;
                }
            }

            var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? "0";
            var tenantId = (entry.Entity as ITenantEntity)?.TenantId ?? CurrentTenantId;

            auditLogs.Add(new AuditLog
            {
                TenantId = tenantId,
                Action = action,
                EntityName = entry.Entity.GetType().Name,
                EntityId = primaryKey,
                OldValuesJson = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null,
                NewValuesJson = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null,
                TimestampUtc = DateTime.UtcNow
            });
        }

        return auditLogs;
    }
}