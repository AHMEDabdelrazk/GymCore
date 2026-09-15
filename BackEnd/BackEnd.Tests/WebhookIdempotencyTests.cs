using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GymCore.API.Data;
using GymCore.API.DTOs.Billing;
using GymCore.API.Models;
using GymCore.API.Models.Entities;
using GymCore.API.Services;
using GymCore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BackEnd.Tests;

public class WebhookIdempotencyTests
{
    private GymDbContext CreateInMemoryContext()
    {
        var tenantMock = new Mock<ITenantProvider>();
        tenantMock.Setup(t => t.GetCurrentTenantId()).Returns(1);
        tenantMock.Setup(t => t.IsGlobalAdmin()).Returns(true);

        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseInMemoryDatabase(databaseName: $"GymCore_Webhook_Test_{Guid.NewGuid():N}")
            .Options;

        return new GymDbContext(options, tenantMock.Object);
    }

    [Fact]
    public async Task ProcessWebhookEventAsync_DuplicateEvent_ShouldBeIgnoredIdempotently()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var configMock = new Mock<IConfiguration>();
        var service = new StripePaymentGatewayService(context, configMock.Object, NullLogger<StripePaymentGatewayService>.Instance);

        var member = new Member
        {
            Id = 1,
            TenantId = 1,
            FullName = "Billing Member",
            Email = "billing@gym.com",
            IsActive = true
        };
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var eventId = "evt_test_unique_123456";
        var stripeObject = new StripeObjectDto
        {
            Id = "pi_123456",
            MemberId = member.Id,
            Amount = 49.99m,
            Currency = "usd",
            Status = "paid"
        };

        // Act 1: First delivery
        var firstResult = await service.ProcessWebhookEventAsync(eventId, "invoice.payment_succeeded", stripeObject, "raw");

        // Act 2: Duplicate delivery (e.g. Stripe network retry)
        var secondResult = await service.ProcessWebhookEventAsync(eventId, "invoice.payment_succeeded", stripeObject, "raw");

        // Assert
        firstResult.Success.Should().BeTrue();
        firstResult.IsDuplicate.Should().BeFalse();

        secondResult.Success.Should().BeTrue();
        secondResult.IsDuplicate.Should().BeTrue();

        // Verify only 1 invoice was created
        var invoiceCount = await context.Invoices.CountAsync(i => i.MemberId == member.Id);
        invoiceCount.Should().Be(1);
    }

    [Fact]
    public void VerifyStripeSignature_WithValidHMAC_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var configMock = new Mock<IConfiguration>();
        var service = new StripePaymentGatewayService(context, configMock.Object, NullLogger<StripePaymentGatewayService>.Instance);

        var payload = "{\"id\":\"evt_123\"}";
        var secret = "whsec_test_secret_123";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var signedPayload = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload))).Replace("-", "").ToLowerInvariant();

        var signatureHeader = $"t={timestamp},v1={hash}";

        // Act
        var isValid = service.VerifyStripeSignature(payload, signatureHeader, secret);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyStripeSignature_WithInvalidSecret_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var configMock = new Mock<IConfiguration>();
        var service = new StripePaymentGatewayService(context, configMock.Object, NullLogger<StripePaymentGatewayService>.Instance);

        var payload = "{\"id\":\"evt_123\"}";
        var signatureHeader = "t=1600000000,v1=invalidhash123";

        // Act
        var isValid = service.VerifyStripeSignature(payload, signatureHeader, "wrong_secret");

        // Assert
        isValid.Should().BeFalse();
    }
}
