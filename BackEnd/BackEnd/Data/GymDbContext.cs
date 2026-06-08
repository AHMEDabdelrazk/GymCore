using GymCore.API.Models;
using GymCore.API.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymCore.API.Data;

public class GymDbContext
    : IdentityDbContext<ApplicationUser>
{
    public GymDbContext(
        DbContextOptions<GymDbContext> options)
        : base(options)
    {
    }

    public DbSet<MembershipPlan> MembershipPlans { get; set; }

    public DbSet<Member> Members { get; set; }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MembershipPlan>()
            .Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<MembershipPlan>()
            .Property(x => x.Name)
            .HasMaxLength(100);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(GymDbContext).Assembly);
    }
}