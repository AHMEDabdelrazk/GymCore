import { useEffect, useState } from "react";
import DashboardLayout from "../../layouts/DashboardLayout";
import { processCheckIn, getRecentCheckIns } from "../../api/checkInApi";

function CheckInKiosk() {
  const [memberCode, setMemberCode] = useState("");
  const [accessMethod, setAccessMethod] = useState("Kiosk_QR");
  const [result, setResult] = useState(null);
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadHistory();
  }, []);

  const loadHistory = async () => {
    try {
      const res = await getRecentCheckIns();
      setHistory(res.data);
    } catch (err) {
      console.error("Failed to load check-in history", err);
    }
  };

  const handleScan = async (codeToScan) => {
    const targetCode = codeToScan || memberCode;
    if (!targetCode) {
      alert("Please enter a Member Code or scan barcode!");
      return;
    }

    try {
      setLoading(true);
      const res = await processCheckIn(targetCode, accessMethod);
      setResult(res.data);
      setMemberCode("");
      await loadHistory();
    } catch (err) {
      setResult({
        accessGranted: false,
        message: err.response?.data?.error || "Error processing check-in.",
        timestampUtc: new Date().toISOString(),
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <DashboardLayout>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="fw-bold mb-1">Attendance & Kiosk Terminal</h2>
          <p className="text-muted mb-0">
            Real-time digital access validation with automated subscription eligibility checks.
          </p>
        </div>
      </div>

      <div className="row g-4">
        {/* Scanner Terminal */}
        <div className="col-lg-6">
          <div className="card shadow-sm border-0 bg-white mb-4">
            <div className="card-header bg-dark text-white py-3 d-flex justify-content-between align-items-center">
              <span className="fw-bold">
                <i className="bi bi-upc-scan me-2 text-warning"></i>Kiosk Barcode / QR Scanner
              </span>
              <span className="badge bg-success">Terminal Online</span>
            </div>

            <div className="card-body p-4">
              <form
                onSubmit={(e) => {
                  e.preventDefault();
                  handleScan();
                }}
              >
                <div className="mb-3">
                  <label className="form-label fw-semibold">Scan Barcode or Enter Member Code</label>
                  <div className="input-group input-group-lg">
                    <span className="input-group-text bg-light">
                      <i className="bi bi-qr-code text-muted"></i>
                    </span>
                    <input
                      type="text"
                      className="form-control"
                      placeholder="e.g. MEM-1001"
                      value={memberCode}
                      onChange={(e) => setMemberCode(e.target.value)}
                      autoFocus
                    />
                    <button className="btn btn-primary px-4 fw-bold" type="submit" disabled={loading}>
                      {loading ? "Scanning..." : "Check In"}
                    </button>
                  </div>
                </div>

                <div className="mb-4">
                  <label className="form-label small text-muted">Access Device Method</label>
                  <div className="d-flex gap-3">
                    <div className="form-check">
                      <input
                        className="form-check-input"
                        type="radio"
                        name="method"
                        id="methodQR"
                        value="Kiosk_QR"
                        checked={accessMethod === "Kiosk_QR"}
                        onChange={(e) => setAccessMethod(e.target.value)}
                      />
                      <label className="form-check-label small" htmlFor="methodQR">
                        <i className="bi bi-qr-code me-1"></i> QR Code Kiosk
                      </label>
                    </div>
                    <div className="form-check">
                      <input
                        className="form-check-input"
                        type="radio"
                        name="method"
                        id="methodRFID"
                        value="RFID_Badge"
                        checked={accessMethod === "RFID_Badge"}
                        onChange={(e) => setAccessMethod(e.target.value)}
                      />
                      <label className="form-check-label small" htmlFor="methodRFID">
                        <i className="bi bi-badge-ad me-1"></i> RFID Turnstile
                      </label>
                    </div>
                    <div className="form-check">
                      <input
                        className="form-check-input"
                        type="radio"
                        name="method"
                        id="methodManual"
                        value="Manual_Desk"
                        checked={accessMethod === "Manual_Desk"}
                        onChange={(e) => setAccessMethod(e.target.value)}
                      />
                      <label className="form-check-label small" htmlFor="methodManual">
                        <i className="bi bi-person-badge me-1"></i> Front Desk Manual
                      </label>
                    </div>
                  </div>
                </div>
              </form>

              {/* Quick Simulation Test Buttons */}
              <div className="p-3 bg-light rounded border">
                <div className="small fw-bold text-muted mb-2">
                  <i className="bi bi-lightning-charge-fill text-warning me-1"></i>One-Click Verification Test Scenarios:
                </div>
                <div className="d-flex flex-wrap gap-2">
                  <button
                    onClick={() => handleScan("MEM-1001")}
                    className="btn btn-sm btn-outline-success"
                  >
                    Active VIP (MEM-1001)
                  </button>
                  <button
                    onClick={() => handleScan("MEM-1004")}
                    className="btn btn-sm btn-outline-danger"
                  >
                    Expired Plan (MEM-1004)
                  </button>
                  <button
                    onClick={() => handleScan("MEM-1005")}
                    className="btn btn-sm btn-outline-warning text-dark"
                  >
                    Grace Period (MEM-1005)
                  </button>
                </div>
              </div>

              {/* Instant Scan Result Box */}
              {result && (
                <div
                  className={`mt-4 p-4 rounded text-center border-3 border ${
                    result.accessGranted
                      ? result.subscriptionStatus === "GracePeriod"
                        ? "bg-warning-subtle border-warning text-dark"
                        : "bg-success-subtle border-success text-success"
                      : "bg-danger-subtle border-danger text-danger"
                  }`}
                >
                  <div className="display-5 mb-2">
                    {result.accessGranted ? (
                      result.subscriptionStatus === "GracePeriod" ? (
                        <i className="bi bi-exclamation-triangle-fill text-warning"></i>
                      ) : (
                        <i className="bi bi-check-circle-fill text-success"></i>
                      )
                    ) : (
                      <i className="bi bi-x-circle-fill text-danger"></i>
                    )}
                  </div>

                  <h3 className="fw-bold mb-1">
                    {result.accessGranted
                      ? result.subscriptionStatus === "GracePeriod"
                        ? "ACCESS GRANTED (GRACE PERIOD)"
                        : "ACCESS GRANTED"
                      : "ACCESS DENIED"}
                  </h3>

                  <p className="fs-5 mb-2 fw-medium">{result.message}</p>

                  {result.memberName && (
                    <div className="small text-muted">
                      Member: <strong>{result.memberName}</strong> &bull; Plan:{" "}
                      <strong>{result.planName || "General"}</strong> &bull; Status:{" "}
                      <span className="badge bg-dark">{result.subscriptionStatus}</span>
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Live Attendance History Stream */}
        <div className="col-lg-6">
          <div className="card shadow-sm border-0 bg-white">
            <div className="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center">
              <h5 className="card-title m-0 fw-bold">
                <i className="bi bi-clock-history text-primary me-2"></i>Live Check-In Activity Log
              </h5>
              <button onClick={loadHistory} className="btn btn-sm btn-outline-secondary">
                <i className="bi bi-arrow-clockwise"></i> Refresh
              </button>
            </div>

            <div className="card-body p-0">
              <div className="table-responsive" style={{ maxHeight: "460px" }}>
                <table className="table table-hover align-middle mb-0">
                  <thead className="table-light sticky-top">
                    <tr>
                      <th>Time</th>
                      <th>Member</th>
                      <th>Method</th>
                      <th>Result</th>
                    </tr>
                  </thead>
                  <tbody>
                    {history.length > 0 ? (
                      history.map((h) => (
                        <tr key={h.id}>
                          <td className="small text-muted">
                            {new Date(h.checkInTimeUtc).toLocaleTimeString([], {
                              hour: "2-digit",
                              minute: "2-digit",
                              second: "2-digit",
                            })}
                          </td>
                          <td className="fw-semibold">{h.memberName}</td>
                          <td>
                            <span className="badge bg-secondary-subtle text-dark border">
                              {h.accessMethod}
                            </span>
                          </td>
                          <td>
                            {h.status === "Success" ? (
                              <span className="badge bg-success">Access Granted</span>
                            ) : (
                              <div>
                                <span className="badge bg-danger">Denied</span>
                                <div className="small text-danger" style={{ fontSize: "0.7rem" }}>
                                  {h.denialReason}
                                </div>
                              </div>
                            )}
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan="4" className="text-center py-4 text-muted">
                          No check-ins recorded yet today.
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

export default CheckInKiosk;
