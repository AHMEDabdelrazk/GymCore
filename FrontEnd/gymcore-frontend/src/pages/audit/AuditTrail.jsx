import { useEffect, useState } from "react";
import DashboardLayout from "../../layouts/DashboardLayout";
import { getAuditLogs } from "../../api/auditApi";

function AuditTrail() {
  const [logs, setLogs] = useState([]);
  const [selectedLog, setSelectedLog] = useState(null);
  const [entityFilter, setEntityFilter] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadLogs();
  }, [entityFilter]);

  const loadLogs = async () => {
    try {
      setLoading(true);
      const res = await getAuditLogs(entityFilter || undefined);
      setLogs(res.data);
    } catch (err) {
      console.error("Failed to load audit logs", err);
    } finally {
      setLoading(false);
    }
  };

  const parseJson = (jsonString) => {
    if (!jsonString) return null;
    try {
      return JSON.parse(jsonString);
    } catch {
      return jsonString;
    }
  };

  return (
    <DashboardLayout>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="fw-bold mb-1">System Audit Trail</h2>
          <p className="text-muted mb-0">
            Immutable, tamper-evident change logs automatically recorded by the EF Core interceptor.
          </p>
        </div>

        <div className="d-flex align-items-center gap-2">
          <select
            className="form-select form-select-sm"
            style={{ width: "180px" }}
            value={entityFilter}
            onChange={(e) => setEntityFilter(e.target.value)}
          >
            <option value="">All Entities</option>
            <option value="Member">Member</option>
            <option value="MembershipPlan">MembershipPlan</option>
            <option value="ClassBooking">ClassBooking</option>
            <option value="Invoice">Invoice</option>
          </select>
          <button onClick={loadLogs} className="btn btn-sm btn-outline-secondary">
            <i className="bi bi-arrow-clockwise"></i>
          </button>
        </div>
      </div>

      <div className="card shadow-sm border-0 bg-white">
        <div className="card-body p-0">
          <div className="table-responsive">
            <table className="table table-hover align-middle mb-0">
              <thead className="table-light">
                <tr>
                  <th>Timestamp</th>
                  <th>Action</th>
                  <th>Entity</th>
                  <th>Record ID</th>
                  <th>Tenant</th>
                  <th>Changes Diff</th>
                </tr>
              </thead>
              <tbody>
                {logs.length > 0 ? (
                  logs.map((log) => (
                    <tr key={log.id}>
                      <td className="small text-muted font-monospace">
                        {new Date(log.timestampUtc).toLocaleString()}
                      </td>
                      <td>
                        <span
                          className={`badge ${
                            log.action === "Create"
                              ? "bg-success"
                              : log.action === "Update"
                              ? "bg-primary"
                              : "bg-danger"
                          }`}
                        >
                          {log.action}
                        </span>
                      </td>
                      <td className="fw-semibold text-dark">{log.entityName}</td>
                      <td className="font-monospace small">#{log.entityId}</td>
                      <td>
                        <span className="badge bg-secondary-subtle text-dark border">
                          Branch #{log.tenantId || 1}
                        </span>
                      </td>
                      <td>
                        <button
                          onClick={() => setSelectedLog(log)}
                          className="btn btn-sm btn-outline-info"
                        >
                          <i className="bi bi-eye me-1"></i> View Diff
                        </button>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan="6" className="text-center py-4 text-muted">
                      No audit records found matching criteria.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* JSON Diff Modal / Inspection Drawer */}
      {selectedLog && (
        <div className="modal show d-block" style={{ backgroundColor: "rgba(0,0,0,0.5)" }} tabIndex="-1">
          <div className="modal-dialog modal-lg modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header bg-dark text-white">
                <h5 className="modal-title">
                  <i className="bi bi-file-diff me-2 text-info"></i>
                  State Diff: {selectedLog.action} on {selectedLog.entityName} #{selectedLog.entityId}
                </h5>
                <button
                  type="button"
                  className="btn-close btn-close-white"
                  onClick={() => setSelectedLog(null)}
                ></button>
              </div>
              <div className="modal-body">
                <div className="row g-3">
                  <div className="col-md-6">
                    <h6 className="fw-bold text-danger">Previous State (Old Values)</h6>
                    <pre className="p-3 bg-light rounded small border font-monospace" style={{ maxHeight: "250px", overflowY: "auto" }}>
                      {selectedLog.oldValuesJson
                        ? JSON.stringify(parseJson(selectedLog.oldValuesJson), null, 2)
                        : "(None - Newly Created Entity)"}
                    </pre>
                  </div>
                  <div className="col-md-6">
                    <h6 className="fw-bold text-success">New State (Modified Values)</h6>
                    <pre className="p-3 bg-light rounded small border font-monospace" style={{ maxHeight: "250px", overflowY: "auto" }}>
                      {selectedLog.newValuesJson
                        ? JSON.stringify(parseJson(selectedLog.newValuesJson), null, 2)
                        : "(None - Entity Deleted)"}
                    </pre>
                  </div>
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setSelectedLog(null)}
                >
                  Close
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </DashboardLayout>
  );
}

export default AuditTrail;
