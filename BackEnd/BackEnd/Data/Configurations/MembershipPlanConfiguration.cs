using GymCore.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymCore.API.Data.Configurations;

public class MembershipPlanConfiguration
    : IEntityTypeConfiguration<MembershipPlan>
{
    public void Configure(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasData(
            new MembershipPlan
            {
                Id = 1,
                Name = "Basic Plan",
                Price = 300,
                DurationInDays = 30,
                Description = "Access to gym equipment",
                IsActive = true
            },
            new MembershipPlan
            {
                Id = 2,
                Name = "Standard Plan",
                Price = 500,
                DurationInDays = 60,
                Description = "Equipment + Classes",
                IsActive = true
            }
        );
    }
}