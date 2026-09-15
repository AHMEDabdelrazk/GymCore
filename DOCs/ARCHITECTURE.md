# GymCore Enterprise Architecture Specification

**System Name:** GymCore Enterprise Operations Platform  
**Target Framework:** .NET 8 (ASP.NET Core Web API) & React 19 (Vite)  
**Security Standard:** HMAC-SHA256, JWT Bearer, RBAC, EF Core Global Query Filters  

---

## 1. High-Level Architectural Topology

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                             React 19 Frontend                               │
│  - Multi-Tenant Branch Switcher (Injects X-Tenant-ID Header)                │
│  - Real-Time Operations KPI Dashboard                                       │
│  - Concurrency Class Booking & Auto-Promoting Waitlist Console              │
│  - Attendance & Kiosk Barcode Scanner Terminal                              │
│  - Stripe Webhook Simulator & Idempotency Testing Console                   │
│  - Audit Trail & JSON Property Diff Viewer                                  │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │ HTTP / REST (JWT Bearer + X-Tenant-ID)
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         ASP.NET Core 8 Web API Layer                        │
│  - Authentication & RBAC Policy Authorization                               │
│  - Tenant Resolution Provider (Scoped ITenantProvider)                      │
│  - HMAC-SHA256 Webhook Cryptographic Signature Verification                │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │
         ┌─────────────────────────────┼─────────────────────────────┐
         ▼                             ▼                             ▼
┌─────────────────┐           ┌─────────────────┐           ┌─────────────────┐
│ Class Booking   │           │ Stripe Webhook  │           │ Background      │
│ & Waitlist      │           │ Idempotency     │           │ Subscription    │
│ Service         │           │ Processor       │           │ Lifecycle Worker│
└────────┬────────┘           └────────┬────────┘           └────────┬────────┘
         │                             │                             │
         └─────────────────────────────┼─────────────────────────────┘
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Entity Framework Core 8 Data Layer                       │
│  - Global Query Filters: HasQueryFilter(e => e.TenantId == CurrentTenantId) │
│  - Optimistic Concurrency Tokens on ClassSession (RowVersion)               │
│  - Automatic Audit Interceptor: Captures Entity Property Diffs              │
│  - Dual Database Provider: Portable In-Memory (Review) & SQL Server (Prod)  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Multi-Tenancy Architecture

GymCore employs a **Shared Database, Shared Schema with Discriminator Column (`TenantId`)** strategy. 

### Tenant Resolution Pipeline
1. Inbound HTTP requests pass through the ASP.NET Core middleware pipeline.
2. `ITenantProvider` inspects:
   - Header: `X-Tenant-ID`
   - JWT Claim: `tenant_id`
   - User Role: If `SuperAdmin`, the provider grants cross-tenant visibility.
3. In `GymDbContext.OnModelCreating`, global query filters are registered on all `ITenantEntity` models:
   ```csharp
   modelBuilder.Entity<Member>()
       .HasQueryFilter(e => _tenantProvider == null || _tenantProvider.IsGlobalAdmin() || e.TenantId == CurrentTenantId);
   ```
4. On entity insertion, `GymDbContext.SaveChangesAsync` automatically stamps `TenantId` if omitted.

---

## 3. Concurrency-Safe Class Booking & Automated Waitlist Flow

```
[ Member Requests Booking ]
           │
           ▼
    [ Class Full? ]
       │        │
      No       Yes
       │        │
       ▼        ▼
 [ ReservedSpots < Capacity ]      [ Add to Waitlist with Position #N ]
 [ Status: Confirmed        ]      [ Status: Waitlisted               ]
           │
           ▼
 [ Member Later Cancels Confirmed Booking ]
           │
           ▼
 [ Decrement ReservedSpots ]
           │
           ▼
 [ Query Earliest Waitlisted Member (OrderBy WaitlistPosition) ]
           │
      Found Waitlist Member?
       │        │
      Yes       No
       │        │
       ▼        ▼
 [ Promote Member to Confirmed ]    [ Spot Remains Open ]
 [ Clear WaitlistPosition      ]
 [ Re-index Remaining Waitlist ]
```

---

## 4. Stripe Webhook Processing & Idempotency Safeguards

To prevent replay attacks and duplicate invoicing during network retries:
1. **Cryptographic Validation:** The webhook controller reads the raw request payload and `Stripe-Signature` header, re-computing HMAC-SHA256 with the configured shared webhook secret using fixed-time equality (`CryptographicOperations.FixedTimeEquals`).
2. **Idempotency Check:** The `StripePaymentGatewayService` queries the `ProcessedWebhookEvents` table for `EventId` (unique index):
   - If found: Logs replay event and returns `isDuplicate = true, status = success` (HTTP 200 OK) without re-applying business mutations.
   - If new: Executes domain mutations (`invoice.payment_succeeded`, updates subscription expiry, generates invoice) and saves `ProcessedWebhookEvent` within the same transaction.

---

## 5. Automated Background Worker

- `SubscriptionLifecycleWorker` inherits `BackgroundService`.
- Runs on a periodic schedule.
- Employs `IServiceScopeFactory` to resolve scoped database contexts safely.
- Executes `IgnoreQueryFilters()` to audit subscriptions across all tenants.
- Transitions past-due subscriptions:
  - `EndDateUtc < Now` ➔ `GracePeriod`
  - `EndDateUtc < Now - 5 Days` ➔ `Expired`

---

## 6. Automated Test Suite Layout

The `BackEnd.Tests` xUnit project validates core engineering invariants:
1. **`MultiTenancyIsolationTests.cs`**:
   - Asserts Tenant 1 cannot observe Tenant 2 members.
   - Asserts Global Admin can query cross-tenant datasets.
   - Asserts automatic tenant stamping on insert.
2. **`ClassBookingConcurrencyTests.cs`**:
   - Asserts capacity ceilings are respected.
   - Asserts correct waitlist position assignment.
   - Asserts automatic promotion of waitlisted members upon cancellation.
3. **`WebhookIdempotencyTests.cs`**:
   - Asserts duplicate webhook delivery is safely deduplicated.
   - Asserts HMAC signature validation passes for authentic signatures and fails for tampered payloads.
4. **`SubscriptionLifecycleTests.cs`**:
   - Asserts automated transition from Active to GracePeriod and Expired.
   - Asserts digital check-in terminal grants access to active members and rejects expired members.
