# GymCore Enterprise — Operations & Club Management Platform

[![Framework](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19.0-blue.svg)](https://react.dev/)
[![Tests](https://img.shields.io/badge/Tests-xUnit%20%7C%20FluentAssertions-green.svg)](https://xunit.net/)
[![Multi-Tenancy](https://img.shields.io/badge/Architecture-Multi--Tenant%20RBAC-orange.svg)]()

> **Enterprise Gym & Health Club Operations Platform** built for high-throughput club networks. Features multi-tenant branch partitioning, concurrency-safe class scheduling with automated waitlist promotion, digital kiosk access validation, Stripe webhook ingestion with HMAC-SHA256 signature verification and idempotency protection, automated background workers, and immutable change-diff audit trails.

---

## 1. Key Engineering Highlights

| Capability | Technical Implementation Details |
| :--- | :--- |
| **Multi-Tenancy** | Shared database with EF Core **Global Query Filters** (`HasQueryFilter`) partitioning data by gym branch (`TenantId`). Dynamic resolution via `X-Tenant-ID` header or JWT claims with SuperAdmin cross-tenant query bypass. |
| **Class Concurrency & Auto-Waitlist** | Atomic slot reservations with optimistic concurrency tokens. When a confirmed attendee cancels, the engine automatically promotes the earliest waitlisted member to confirmed, updates remaining spots, and re-indexes the queue. |
| **Stripe Webhooks & Idempotency** | Cryptographic HMAC-SHA256 signature verification (`Stripe-Signature`). Dedicated `ProcessedWebhookEvents` table enforcing event idempotency to prevent duplicate charges or double subscription extensions during network retries. |
| **Kiosk Check-In Validation** | Digital access engine supporting barcode/QR scans. Validates profile and subscription state in real time: Green (Active Access), Yellow (Grace Period Alert), Red (Expired / Deactivated). |
| **Background Lifecycle Worker** | `IHostedService` / `BackgroundService` executing periodic audits across all tenants to transition past-due memberships into `GracePeriod` and `Expired`. |
| **Tamper-Evident Audit Trail** | EF Core interceptor automatically calculating property-level diffs (Old vs New JSON values) on all mutations, persisting immutable audit logs. |
| **Automated Test Suite** | Comprehensive xUnit + FluentAssertions + Moq test suite covering multi-tenant isolation, booking concurrency, webhook idempotency, and lifecycle status transitions. |

---

## 2. Quick Start & Execution

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (for Frontend)

### 2.1 Backend (ASP.NET Core Web API)
```bash
cd BackEnd/BackEnd
dotnet run
```
- **API URL:** `http://localhost:5097`
- **Swagger Documentation:** `http://localhost:5097/swagger`
- *Note:* The application defaults to **In-Memory database mode** (`"UseInMemoryDatabase": true` in `appsettings.json`) with rich seed data automatically initialized on startup. To use SQL Server, set `"UseInMemoryDatabase": false`.

### 2.2 Frontend (React + Vite)
```bash
cd FrontEnd/gymcore-frontend
npm install
npm run dev
```
- **Web App URL:** `http://localhost:5173`

---

## 3. Demo Personas & Pre-Seeded Credentials

The database seeder automatically configures accounts for immediate evaluation:

| Role | Email | Password | Access Scope |
| :--- | :--- | :--- | :--- |
| **SuperAdmin** | `admin@gymcore.com` | `Admin123!@#` | Full system access, cross-tenant audit trails, global configuration. |
| **Head Trainer** | `trainer@gymcore.com` | `Trainer123!@#` | Class session scheduling, attendee roster inspection. |
| **Front Desk** | `frontdesk@gymcore.com` | `FrontDesk123!@#` | Attendance kiosk operations, member registrations. |

*(Note: The login screen includes convenient 1-click login buttons for instant role switching without manual typing).*

---

## 4. Running the Automated Test Suite

The solution includes an independent xUnit test project (`BackEnd.Tests`):
```bash
cd BackEnd
dotnet test
```

### Test Coverage Highlights:
- **`MultiTenancyIsolationTests`**: Proves Tenant 1 queries cannot observe Tenant 2 entities under EF Core query filters, and confirms SuperAdmin cross-tenant capabilities.
- **`ClassBookingConcurrencyTests`**: Tests that booking beyond capacity places members on the waitlist, and canceling a confirmed spot automatically promotes the top waitlisted member.
- **`WebhookIdempotencyTests`**: Verifies that duplicate webhook deliveries with identical event IDs are deduplicated and processed safely without double invoicing, and verifies cryptographic HMAC signature checking.
- **`SubscriptionLifecycleTests`**: Verifies that the background worker shifts past-due subscriptions to GracePeriod/Expired, and confirms that the check-in service grants or denies access accordingly.

---

## 5. Clean Packaging for Submission (Zero Build Artifacts & PII)

To comply strictly with Outlier's source-only submission policy, run the included sanitization script before creating your archive:

### On Windows (PowerShell):
```powershell
.\clean_for_submission.ps1
```

### On Linux / macOS / Git Bash:
```bash
chmod +x ./clean_for_submission.sh
./clean_for_submission.sh
```

This utility automatically purges:
- All `.NET` build artifacts (`bin/`, `obj/`, `Debug/`, `Release/`)
- All Visual Studio and IDE files (`.vs/`, `.vscode/`, `*.user`, `*.suo`)
- All Node.js caches (`node_modules/`, `dist/`, `.vite/`)
- Strips any developer personal paths or machine metadata.

*(When creating your final zip archive for Outlier, exclude the `.git` directory so that no local git history, usernames, or remote URLs are included).*
