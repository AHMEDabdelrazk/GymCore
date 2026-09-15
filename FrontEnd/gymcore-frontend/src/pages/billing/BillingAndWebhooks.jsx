import { useEffect, useState } from "react";
import DashboardLayout from "../../layouts/DashboardLayout";
import { getInvoices, simulateStripeWebhook } from "../../api/billingApi";
import { getMembers } from "../../api/memberApi";

function BillingAndWebhooks() {
  const [invoices, setInvoices] = useState([]);
  const [members, setMembers] = useState([]);
  const [selectedMemberId, setSelectedMemberId] = useState("");
  const [webhookResult, setWebhookResult] = useState(null);
  const [simulatedEventId, setSimulatedEventId] = useState(`evt_test_${Math.floor(Math.random() * 100000)}`);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [invRes, memRes] = await Promise.all([getInvoices(), getMembers()]);
      setInvoices(invRes.data);
      setMembers(memRes.data);
      if (memRes.data.length > 0) {
        setSelectedMemberId(memRes.data[0].id.toString());
      }
    } catch (err) {
      console.error("Failed to load billing data", err);
    } finally {
      setLoading(false);
    }
  };

  const handleFireWebhook = async (eventType, fixedEventId = null) => {
    if (!selectedMemberId) {
      alert("Please select a member first!");
      return;
    }

    const eventIdToUse = fixedEventId || simulatedEventId;

    try {
      setLoading(true);
      const res = await simulateStripeWebhook(parseInt(selectedMemberId), eventType, 69.99);
      setWebhookResult({
        ...res.data,
        eventType,
      });
      await loadData();
    } catch (err) {
      setWebhookResult({
        status: "failed",
        message: err.response?.data?.error || "Error executing webhook simulation.",
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <DashboardLayout>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="fw-bold mb-1">Billing, Invoices & Stripe Webhooks</h2>
          <p className="text-muted mb-0">
            Payment processing, invoice tracking, and idempotent Stripe webhook ingestion.
          </p>
        </div>
      </div>

      <div className="row g-4">
        {/* Left: Stripe Webhook Simulator Console */}
        <div className="col-lg-5">
          <div className="card shadow-sm border-0 bg-white mb-4">
            <div className="card-header bg-dark text-white py-3 d-flex justify-content-between align-items-center">
              <span className="fw-bold">
                <i className="bi bi-stripe text-info me-2"></i>Stripe Webhook Simulator
              </span>
              <span className="badge bg-primary">HMAC-SHA256</span>
            </div>

            <div className="card-body p-4">
              <p className="text-muted small">
                Test the backend's cryptographic verification and idempotent event pipeline. Events with identical IDs are detected and safely deduplicated.
              </p>

              <div className="mb-3">
                <label className="form-label small fw-bold">Select Target Member:</label>
                <select
                  className="form-select form-select-sm"
                  value={selectedMemberId}
                  onChange={(e) => setSelectedMemberId(e.target.value)}
                >
                  {members.map((m) => (
                    <option key={m.id} value={m.id}>
                      {m.fullName} (Sub: {m.subscriptionStatus})
                    </option>
                  ))}
                </select>
              </div>

              <div className="mb-3">
                <label className="form-label small fw-bold">Event ID (for Idempotency Testing):</label>
                <div className="input-group input-group-sm">
                  <input
                    type="text"
                    className="form-control font-monospace"
                    value={simulatedEventId}
                    onChange={(e) => setSimulatedEventId(e.target.value)}
                  />
                  <button
                    className="btn btn-outline-secondary"
                    onClick={() => setSimulatedEventId(`evt_test_${Math.floor(Math.random() * 100000)}`)}
                    title="Generate New ID"
                  >
                    <i className="bi bi-shuffle"></i>
                  </button>
                </div>
                <div className="form-text small" style={{ fontSize: "0.75rem" }}>
                  Keep the same ID and send twice to test <strong>idempotent deduplication</strong>.
                </div>
              </div>

              <div className="d-grid gap-2 mb-3">
                <button
                  onClick={() => handleFireWebhook("invoice.payment_succeeded")}
                  className="btn btn-success"
                  disabled={loading}
                >
                  <i className="bi bi-check2-circle me-1"></i> Simulate <code>payment_succeeded</code>
                </button>

                <button
                  onClick={() => handleFireWebhook("invoice.payment_failed")}
                  className="btn btn-danger"
                  disabled={loading}
                >
                  <i className="bi bi-x-circle me-1"></i> Simulate <code>payment_failed</code>
                </button>

                <button
                  onClick={() => handleFireWebhook("invoice.payment_succeeded", simulatedEventId)}
                  className="btn btn-outline-warning text-dark"
                  disabled={loading}
                >
                  <i className="bi bi-arrow-repeat me-1"></i> Send Duplicate Event (Idempotency Test)
                </button>
              </div>

              {webhookResult && (
                <div
                  className={`p-3 rounded small border ${
                    webhookResult.status === "success"
                      ? webhookResult.isDuplicate
                        ? "bg-warning-subtle border-warning text-dark"
                        : "bg-success-subtle border-success text-success"
                      : "bg-danger-subtle border-danger text-danger"
                  }`}
                >
                  <div className="fw-bold mb-1">
                    {webhookResult.isDuplicate
                      ? "⚠️ IDEMPOTENCY SUCCESS: Duplicate Event Detected & Ignored"
                      : "✅ WEBHOOK PROCESSED SUCCESSFULLY"}
                  </div>
                  <div className="font-monospace text-dark" style={{ fontSize: "0.8rem" }}>
                    Event ID: {webhookResult.eventId}
                  </div>
                  <div>Message: {webhookResult.message}</div>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Right: Invoices Table */}
        <div className="col-lg-7">
          <div className="card shadow-sm border-0 bg-white">
            <div className="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center">
              <h5 className="card-title m-0 fw-bold">
                <i className="bi bi-receipt text-primary me-2"></i>Branch Invoices Ledger
              </h5>
              <button onClick={loadData} className="btn btn-sm btn-outline-secondary">
                <i className="bi bi-arrow-clockwise"></i> Refresh
              </button>
            </div>

            <div className="card-body p-0">
              <div className="table-responsive">
                <table className="table table-hover align-middle mb-0">
                  <thead className="table-light">
                    <tr>
                      <th>Invoice #</th>
                      <th>Member</th>
                      <th>Amount</th>
                      <th>Status</th>
                      <th>Due Date</th>
                    </tr>
                  </thead>
                  <tbody>
                    {invoices.length > 0 ? (
                      invoices.map((inv) => (
                        <tr key={inv.id}>
                          <td className="fw-bold font-monospace small">{inv.invoiceNumber}</td>
                          <td>{inv.memberName}</td>
                          <td className="fw-semibold">
                            ${Number(inv.amount).toFixed(2)} {inv.currency}
                          </td>
                          <td>
                            {inv.status === "Paid" ? (
                              <span className="badge bg-success">Paid</span>
                            ) : inv.status === "Pending" ? (
                              <span className="badge bg-warning text-dark">Pending</span>
                            ) : (
                              <span className="badge bg-danger">Failed</span>
                            )}
                          </td>
                          <td className="small text-muted">
                            {new Date(inv.dueDateUtc).toLocaleDateString()}
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan="5" className="text-center py-4 text-muted">
                          No invoices generated for this branch yet.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    </DashboardLayout>
  );
}

export default BillingAndWebhooks;
