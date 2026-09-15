using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymCore.API.Models;
using GymCore.API.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymCore.API.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(GymDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Ensure Database & Roles exist
        string[] roles = { UserRoles.SuperAdmin, UserRoles.BranchManager, UserRoles.Trainer, UserRoles.FrontDeskStaff, UserRoles.Member };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Tenants if none exist
        if (!await context.Tenants.AnyAsync())
        {
            var downtownTenant = new Tenant
            {
                Id = 1,
                Name = "Apex Downtown Flagship",
                Code = "APX-DT",
                Address = "100 Grand Avenue, Financial District",
                PhoneNumber = "+1 (555) 019-2831",
                TimeZone = "America/New_York",
                IsActive = true
            };

            var westsideTenant = new Tenant
            {
                Id = 2,
                Name = "Apex Westside Performance Hub",
                Code = "APX-WS",
                Address = "450 Olympic Blvd, Westside",
                PhoneNumber = "+1 (555) 028-4920",
                TimeZone = "America/Los_Angeles",
                IsActive = true
            };

            context.Tenants.AddRange(downtownTenant, westsideTenant);
            await context.SaveChangesAsync();
        }

        // 3. Seed Users
        var adminEmail = "admin@gymcore.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Chief Administrator",
                TenantId = 1,
                Role = UserRoles.SuperAdmin,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!@#");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, UserRoles.SuperAdmin);
            }
        }

        var trainerEmail = "trainer@gymcore.com";
        var trainerUser = await userManager.FindByEmailAsync(trainerEmail);
        if (trainerUser == null)
        {
            trainerUser = new ApplicationUser
            {
                UserName = trainerEmail,
                Email = trainerEmail,
                FullName = "Coach Marcus Vance",
                TenantId = 1,
                Role = UserRoles.Trainer,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(trainerUser, "Trainer123!@#");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(trainerUser, UserRoles.Trainer);
            }
        }

        var frontDeskEmail = "frontdesk@gymcore.com";
        var deskUser = await userManager.FindByEmailAsync(frontDeskEmail);
        if (deskUser == null)
        {
            deskUser = new ApplicationUser
            {
                UserName = frontDeskEmail,
                Email = frontDeskEmail,
                FullName = "Emily Reception Desk",
                TenantId = 1,
                Role = UserRoles.FrontDeskStaff,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(deskUser, "FrontDesk123!@#");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(deskUser, UserRoles.FrontDeskStaff);
            }
        }

        // 4. Seed Membership Plans
        if (!await context.MembershipPlans.IgnoreQueryFilters().AnyAsync(p => p.Tier == "Elite"))
        {
            var plans = new List<MembershipPlan>
            {
                new MembershipPlan { TenantId = 1, Name = "Bronze Starter", Tier = "Basic", Price = 39.99m, DurationInDays = 30, MaxClassesPerWeek = 2, Description = "Standard gym floor & locker room access with 2 classes per week.", IsActive = true },
                new MembershipPlan { TenantId = 1, Name = "Silver Performance", Tier = "Pro", Price = 69.99m, DurationInDays = 30, MaxClassesPerWeek = 5, Description = "Full access to gym, saunas, and 5 weekly high-intensity group classes.", IsActive = true },
                new MembershipPlan { TenantId = 1, Name = "Gold All-Access VIP", Tier = "Elite", Price = 119.99m, DurationInDays = 30, MaxClassesPerWeek = 14, Description = "Unlimited 24/7 access, VIP towel service, private recovery pods, and unlimited classes.", IsActive = true },
                new MembershipPlan { TenantId = 2, Name = "Westside All-In Hub", Tier = "Pro", Price = 79.99m, DurationInDays = 30, MaxClassesPerWeek = 7, Description = "Full access to Olympic lifting platforms and outdoor turf area.", IsActive = true }
            };

            context.MembershipPlans.AddRange(plans);
            await context.SaveChangesAsync();
        }

        // 5. Seed Members & Subscriptions
        if (!await context.Members.IgnoreQueryFilters().AnyAsync())
        {
            var plan1 = await context.MembershipPlans.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.TenantId == 1 && p.Tier == "Elite")
                        ?? await context.MembershipPlans.IgnoreQueryFilters().FirstAsync(p => p.TenantId == 1);
            var plan2 = await context.MembershipPlans.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.TenantId == 1 && p.Tier == "Pro")
                        ?? plan1;
            var plan3 = await context.MembershipPlans.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.TenantId == 1 && p.Tier == "Basic")
                        ?? plan1;
            var planWest = await context.MembershipPlans.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.TenantId == 2)
                           ?? plan1;

            var members = new List<Member>
            {
                new Member { TenantId = 1, MemberCode = "MEM-1001", FullName = "Alex Johnson", Email = "alex.j@example.com", PhoneNumber = "555-0101", DateOfBirth = new DateTime(1992, 4, 15), Gender = "Male", Address = "12 Harbor Way", MembershipPlanId = plan1.Id, IsActive = true, JoinDate = DateTime.UtcNow.AddMonths(-6) },
                new Member { TenantId = 1, MemberCode = "MEM-1002", FullName = "Sarah Connor", Email = "sarah.c@example.com", PhoneNumber = "555-0102", DateOfBirth = new DateTime(1989, 7, 22), Gender = "Female", Address = "88 Cyber St", MembershipPlanId = plan2.Id, IsActive = true, JoinDate = DateTime.UtcNow.AddMonths(-4) },
                new Member { TenantId = 1, MemberCode = "MEM-1003", FullName = "Michael Chang", Email = "michael.c@example.com", PhoneNumber = "555-0103", DateOfBirth = new DateTime(1995, 11, 3), Gender = "Male", Address = "304 Pine Rd", MembershipPlanId = plan1.Id, IsActive = true, JoinDate = DateTime.UtcNow.AddMonths(-2) },
                new Member { TenantId = 1, MemberCode = "MEM-1004", FullName = "David Miller (Expired)", Email = "david.m@example.com", PhoneNumber = "555-0104", DateOfBirth = new DateTime(1985, 2, 10), Gender = "Male", Address = "19 Elm Lane", MembershipPlanId = plan3.Id, IsActive = true, JoinDate = DateTime.UtcNow.AddMonths(-12) },
                new Member { TenantId = 1, MemberCode = "MEM-1005", FullName = "Elena Rostova (Grace Period)", Email = "elena.r@example.com", PhoneNumber = "555-0105", DateOfBirth = new DateTime(1997, 8, 19), Gender = "Female", Address = "77 Willow Ave", MembershipPlanId = plan2.Id, IsActive = true, JoinDate = DateTime.UtcNow.AddMonths(-3) },
                new Member { TenantId = 2, MemberCode = "MEM-2001", FullName = "Marcus Brody", Email = "marcus.b@example.com", PhoneNumber = "555-0201", DateOfBirth = new DateTime(1990, 5, 30), Gender = "Male", Address = "500 Ocean Blvd", MembershipPlanId = planWest.Id, IsActive = true, JoinDate = DateTime.UtcNow.AddMonths(-5) }
            };

            context.Members.AddRange(members);
            await context.SaveChangesAsync();

            // Subscriptions
            var subs = new List<MemberSubscription>
            {
                new MemberSubscription { TenantId = 1, MemberId = members[0].Id, MembershipPlanId = plan1.Id, StartDateUtc = DateTime.UtcNow.AddDays(-10), EndDateUtc = DateTime.UtcNow.AddDays(20), Status = SubscriptionStatus.Active, PricePaid = plan1.Price, AutoRenew = true },
                new MemberSubscription { TenantId = 1, MemberId = members[1].Id, MembershipPlanId = plan2.Id, StartDateUtc = DateTime.UtcNow.AddDays(-5), EndDateUtc = DateTime.UtcNow.AddDays(25), Status = SubscriptionStatus.Active, PricePaid = plan2.Price, AutoRenew = true },
                new MemberSubscription { TenantId = 1, MemberId = members[2].Id, MembershipPlanId = plan1.Id, StartDateUtc = DateTime.UtcNow.AddDays(-1), EndDateUtc = DateTime.UtcNow.AddDays(29), Status = SubscriptionStatus.Active, PricePaid = plan1.Price, AutoRenew = true },
                new MemberSubscription { TenantId = 1, MemberId = members[3].Id, MembershipPlanId = plan3.Id, StartDateUtc = DateTime.UtcNow.AddDays(-60), EndDateUtc = DateTime.UtcNow.AddDays(-30), Status = SubscriptionStatus.Expired, PricePaid = plan3.Price, AutoRenew = false },
                new MemberSubscription { TenantId = 1, MemberId = members[4].Id, MembershipPlanId = plan2.Id, StartDateUtc = DateTime.UtcNow.AddDays(-32), EndDateUtc = DateTime.UtcNow.AddDays(-2), Status = SubscriptionStatus.GracePeriod, PricePaid = plan2.Price, AutoRenew = true },
                new MemberSubscription { TenantId = 2, MemberId = members[5].Id, MembershipPlanId = planWest.Id, StartDateUtc = DateTime.UtcNow.AddDays(-15), EndDateUtc = DateTime.UtcNow.AddDays(15), Status = SubscriptionStatus.Active, PricePaid = planWest.Price, AutoRenew = true }
            };

            context.MemberSubscriptions.AddRange(subs);
            await context.SaveChangesAsync();
        }

        // 6. Seed Class Types and Scheduled Sessions
        if (!await context.ClassTypes.IgnoreQueryFilters().AnyAsync())
        {
            var hiitType = new ClassType { TenantId = 1, Name = "HIIT MetCon Blast", Category = "HIIT", Description = "High intensity functional interval training to burn fat and build stamina.", DefaultCapacity = 4, DefaultDurationMinutes = 45 };
            var yogaType = new ClassType { TenantId = 1, Name = "Power Vinyasa Flow", Category = "Yoga", Description = "Dynamic athletic flow building core stability and deep mobility.", DefaultCapacity = 15, DefaultDurationMinutes = 60 };
            var spinType = new ClassType { TenantId = 1, Name = "Cardio Spin Sprint", Category = "Spin", Description = "High-energy rhythm cycling with resistance intervals.", DefaultCapacity = 10, DefaultDurationMinutes = 50 };

            context.ClassTypes.AddRange(hiitType, yogaType, spinType);
            await context.SaveChangesAsync();

            var coach = await userManager.FindByEmailAsync("trainer@gymcore.com");
            var trainerId = coach?.Id ?? "trainer-1";

            // Create a HIIT session that is 3/4 booked to test booking & waitlist concurrency
            var hiitSession = new ClassSession
            {
                TenantId = 1,
                ClassTypeId = hiitType.Id,
                TrainerId = trainerId,
                RoomName = "Studio A - Conditioning",
                StartTimeUtc = DateTime.UtcNow.AddHours(2),
                EndTimeUtc = DateTime.UtcNow.AddHours(2).AddMinutes(45),
                Capacity = 4,
                ReservedSpots = 3
            };

            var yogaSession = new ClassSession
            {
                TenantId = 1,
                ClassTypeId = yogaType.Id,
                TrainerId = trainerId,
                RoomName = "Studio B - Zen Loft",
                StartTimeUtc = DateTime.UtcNow.AddHours(5),
                EndTimeUtc = DateTime.UtcNow.AddHours(6),
                Capacity = 15,
                ReservedSpots = 1
            };

            context.ClassSessions.AddRange(hiitSession, yogaSession);
            await context.SaveChangesAsync();

            var m1 = await context.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.MemberCode == "MEM-1001")
                     ?? await context.Members.IgnoreQueryFilters().FirstOrDefaultAsync();
            var m2 = await context.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.MemberCode == "MEM-1002")
                     ?? m1;
            var m3 = await context.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.MemberCode == "MEM-1003")
                     ?? m1;

            if (m1 != null && m2 != null && m3 != null)
            {

            var bookings = new List<ClassBooking>
            {
                new ClassBooking { TenantId = 1, ClassSessionId = hiitSession.Id, MemberId = m1.Id, Status = BookingStatus.Confirmed, BookedAtUtc = DateTime.UtcNow.AddHours(-1) },
                new ClassBooking { TenantId = 1, ClassSessionId = hiitSession.Id, MemberId = m2.Id, Status = BookingStatus.Confirmed, BookedAtUtc = DateTime.UtcNow.AddMinutes(-45) },
                new ClassBooking { TenantId = 1, ClassSessionId = hiitSession.Id, MemberId = m3.Id, Status = BookingStatus.Confirmed, BookedAtUtc = DateTime.UtcNow.AddMinutes(-30) },
                new ClassBooking { TenantId = 1, ClassSessionId = yogaSession.Id, MemberId = m1.Id, Status = BookingStatus.Confirmed, BookedAtUtc = DateTime.UtcNow.AddMinutes(-15) }
            };

            context.ClassBookings.AddRange(bookings);

            // Seed sample check-in history
            var checkIns = new List<CheckInRecord>
            {
                new CheckInRecord { TenantId = 1, MemberId = m1.Id, Status = CheckInStatus.Success, AccessMethod = "Kiosk_QR", CheckInTimeUtc = DateTime.UtcNow.AddHours(-2) },
                new CheckInRecord { TenantId = 1, MemberId = m2.Id, Status = CheckInStatus.Success, AccessMethod = "Kiosk_QR", CheckInTimeUtc = DateTime.UtcNow.AddHours(-1) },
                new CheckInRecord { TenantId = 1, MemberId = m3.Id, Status = CheckInStatus.Success, AccessMethod = "RFID_Badge", CheckInTimeUtc = DateTime.UtcNow.AddMinutes(-40) }
            };

            context.CheckInRecords.AddRange(checkIns);

            // Seed sample invoices
            var invoices = new List<Invoice>
            {
                new Invoice { TenantId = 1, MemberId = m1.Id, InvoiceNumber = "INV-2026-001", Amount = 119.99m, Currency = "USD", Status = InvoiceStatus.Paid, DueDateUtc = DateTime.UtcNow.AddDays(-10), PaidAtUtc = DateTime.UtcNow.AddDays(-10), StripePaymentIntentId = "pi_1N0001" },
                new Invoice { TenantId = 1, MemberId = m2.Id, InvoiceNumber = "INV-2026-002", Amount = 69.99m, Currency = "USD", Status = InvoiceStatus.Paid, DueDateUtc = DateTime.UtcNow.AddDays(-5), PaidAtUtc = DateTime.UtcNow.AddDays(-5), StripePaymentIntentId = "pi_1N0002" },
                new Invoice { TenantId = 1, MemberId = m3.Id, InvoiceNumber = "INV-2026-003", Amount = 119.99m, Currency = "USD", Status = InvoiceStatus.Paid, DueDateUtc = DateTime.UtcNow.AddDays(-1), PaidAtUtc = DateTime.UtcNow.AddDays(-1), StripePaymentIntentId = "pi_1N0003" }
            };

                context.Invoices.AddRange(invoices);
                await context.SaveChangesAsync();
            }
        }
    }
}
