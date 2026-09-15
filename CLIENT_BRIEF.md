# CLIENT STATEMENT OF WORK & REQUIREMENTS BRIEF

**Client Organization:** Apex Fitness International Group (FitPulse Operations)  
**Project Identifier:** RFP-2026-ENG-GYMCORE-V2  
**Engagement Type:** Custom Enterprise SaaS Engineering  
**Version:** 2.0  
**Target Delivery Date:** Q3 2026  

---

## 1. Executive Summary & Problem Statement

Apex Fitness International currently operates a growing portfolio of premium fitness and health clubs across multiple metropolitan locations. Our current gym clubs run on disparate single-facility software with manual check-in lists, phone-based reservation desks, disconnected billing gateways, and zero inter-branch coordination. 

This fragmented setup has led to:
1. **Frequent Class Overbooking & Member Friction:** Group fitness classes (HIIT, Spin, Yoga) are frequently overbooked, with no automated waitlist queue or concurrency-safe booking engine.
2. **Revenue Leakage & Unenforced Memberships:** Members with expired cards or failed billing cards frequently access gym floors unnoticed because the front desk lacks real-time verification against gateway invoice webhooks.
3. **Lack of Auditability & Cross-Branch Visibility:** Headquarters cannot monitor branch KPI metrics (MRR, capacity utilization, daily attendance) or inspect administrative change logs when user permissions or subscription terms are altered.

We are commissioning your engineering team to architect and build **GymCore Enterprise Operations Platform**—a production-grade, multi-tenant cloud operations suite that unifies branch management, class scheduling, automated billing with external webhook ingestion, kiosk check-in, and tamper-evident auditing into a cohesive, high-performance solution.

---

## 2. Personas & Authorization Roles

The platform must enforce Role-Based Access Control (RBAC) with fine-grained claim policies across the following distinct user roles:

| Role | Operational Scope & Permissions |
| :--- | :--- |
| **SuperAdmin** | Global executive access across all gym branches/tenants. Can provision new branches, alter global policies, view all-branch consolidated financial MRR, and inspect complete immutable audit trails. |
| **Branch Manager** | Operational administrator scoped strictly to their assigned branch. Can manage staff, update membership tiers, schedule class timetables, review branch invoices, and monitor attendance metrics. |
| **Trainer / Coach** | Scoped to group classes and rosters. Can inspect upcoming sessions, view registered attendee rosters, and verify attendance. |
| **Front Desk Staff** | Scoped to front-of-house operations. Operates the member check-in kiosk terminal, registers walk-in members, and processes barcode/card access scans. |
| **Member** | End-user. Can browse class schedules, reserve spots in fitness sessions, view waitlist standings, cancel bookings, and review subscription billing status. |

---

## 3. Detailed Functional Requirements

### 3.1 Multi-Tenant Data Isolation
- The backend must support multiple physical gym branches (tenants) within a shared operational database.
- Every tenant request must resolve the tenant context dynamically (via `X-Tenant-ID` header or JWT claim `tenant_id`).
- All queries on tenant-scoped entities (`Member`, `MembershipPlan`, `ClassSession`, `ClassBooking`, `Invoice`, `CheckInRecord`) must be transparently and automatically filtered at the database context level (e.g., EF Core Global Query Filters).
- Global administrators must retain the capability to query across branches for enterprise reporting.

### 3.2 Concurrency-Safe Class Booking & Automated Waitlist Promotion
- Group fitness sessions have strictly enforced physical capacity limits (e.g., 4 to 25 spots).
- The booking engine must handle high concurrency safely:
  - If a member books when `ReservedSpots < Capacity`, the spot is confirmed atomically.
  - If the class is full, the booking is automatically placed on an ordered **Waitlist** with a tracked sequence position (`#1`, `#2`, etc.).
  - **Automated Waitlist Promotion:** When a confirmed attendee cancels their booking, the system must atomically decrement the reserved count, locate the earliest registered waitlisted member, promote them to `Confirmed`, assign the freed spot, and re-index the remaining waitlist queue.

### 3.3 Attendance & Kiosk Terminal Engine
- Digital terminal supporting member lookup by numeric ID or alphanumeric barcode string (e.g. `MEM-1001`).
- Upon scan, the engine must perform instantaneous verification against the member's profile and active subscription status:
  - **Access Granted (Green):** Member active with current subscription in `Active` status.
  - **Access Granted - Warning (Yellow):** Member subscription in `GracePeriod` (overdue payment notice displayed).
  - **Access Denied (Red):** Member profile deactivated, subscription `Expired`, or subscription `Canceled`.
- Every scan must generate a persistent `CheckInRecord` capturing timestamp, access method (`Kiosk_QR`, `RFID_Badge`, `Manual_Desk`), and denial reason if rejected.

### 3.4 External Billing Integration & Idempotent Webhook Processing
- The platform must integrate with external payment gateways (Stripe-compatible architecture).
- Expose a secure webhook receiver endpoint (`POST /api/webhooks/stripe`) that:
  1. Validates cryptographic HMAC-SHA256 signatures (`Stripe-Signature` header against shared secret).
  2. Enforces **Idempotency**: External payment gateways frequently retry webhook events over transient network dropouts. The engine must track processed event identifiers (`EventId`) in a persistent deduplication table and gracefully return HTTP 200 without double-billing or duplicate subscription extensions.
  3. Dispatches domain lifecycle events:
     - `invoice.payment_succeeded`: Automatically activates or renews member subscription for the plan period, marks invoice `Paid`, and logs payment intent.
     - `invoice.payment_failed`: Marks invoice `Failed` and places member subscription in `GracePeriod`.
     - `customer.subscription.deleted`: Cancels member subscription.
- Provide a built-in simulation console for QA and reviewer testing without requiring live external gateway credentials.

### 3.5 Automated Background Subscription Worker
- A continuous background service (`IHostedService` / `BackgroundService`) that periodically inspects subscription timelines:
  - Flags subscriptions past their end date into `GracePeriod`.
  - Transitions subscriptions past the grace period window into `Expired`.
  - Generates automated renewal invoices.

### 3.6 Tamper-Evident Audit Trail
- All create, update, and delete mutations across domain entities must automatically be intercepted.
- Persist an immutable `AuditLog` capturing:
  - Timestamp (UTC)
  - Acting User ID & Email
  - Tenant ID
  - Action (`Create`, `Update`, `Delete`, `CheckIn`)
  - Target Entity Name & Primary Key
  - JSON serialization of Property Diffs (Previous State vs Modified State).

---

## 4. Deliverables & Technical Acceptance Criteria

The final submission package must strictly adhere to the following standards:

1. **Source Code Cleanliness & Zero-PII Hygiene:**
   - The delivery package must contain **source code only**.
   - Absolutely **NO** compiled binaries (`bin/`, `obj/`, `Debug/`, `Release/`).
   - Absolutely **NO** package cache directories (`node_modules/`, `.vite/`, packages).
   - Absolutely **NO** IDE/developer caches (`.vs/`, `.vscode/`, `*.user`, `*.suo`).
   - Absolutely **NO** Git commit metadata, personal email addresses, local absolute filesystem paths, or repository remote URLs.
   - Include automated cleanup scripts (`clean_for_submission.ps1` and `clean_for_submission.sh`).

2. **Backend Deliverables (ASP.NET Core 8 Web API):**
   - Clean architecture with controllers, services, repositories, DTOs, and EF Core DbContext.
   - Support for both SQL Server (production) and portable In-Memory database mode (for instant reviewer evaluation).
   - Rich database seed initializer pre-populating multi-branch tenants, user roles, active/expired memberships, scheduled sessions, and check-in logs.
   - OpenAPI / Swagger documentation with Bearer authentication and tenant header configurations.

3. **Frontend Deliverables (React.js + Vite):**
   - Responsive modern operations dashboard.
   - Real-time branch/tenant switcher dynamically scoping all queries.
   - Operational KPI overview cards (MRR, Active Members, Daily Check-Ins, Class Utilization).
   - Interactive Class Booking with live capacity meters and waitlist promotion.
   - Live Check-In Terminal simulator with instant visual badge feedback.
   - Billing & Stripe Webhook testing console.
   - Interactive Audit Trail explorer with JSON diff inspector.
   - 1-Click demo logins for all primary personas (`SuperAdmin`, `Trainer`, `FrontDeskStaff`).

4. **Automated Test Suite (xUnit):**
   - Comprehensive test project (`BackEnd.Tests`) containing:
     - `MultiTenancyIsolationTests`: Proving tenant data segregation and query filter enforcement.
     - `ClassBookingConcurrencyTests`: Proving capacity boundaries, waitlist ordering, and auto-promotion upon cancellation.
     - `WebhookIdempotencyTests`: Proving duplicate webhook event deduplication and HMAC signature validation.
     - `SubscriptionLifecycleTests`: Proving automated status transitions and access validation.
