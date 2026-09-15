import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import DashboardLayout from "../../layouts/DashboardLayout";
import { getDashboardMetrics } from "../../api/dashboardApi";

function DashboardHome() {
  const [metrics, setMetrics] = useState({
    activeMembersCount: 0,
    monthlyRecurringRevenue: 0,
    todayCheckInsCount: 0,
    classCapacityUtilizationPercentage: 0,
    upcomingClassesCount: 0,
    recentActivities: [],
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadMetrics();
  }, []);

  const loadMetrics = async () => {
    try {
      setLoading(true);
      const res = await getDashboardMetrics();
      setMetrics(res.data);
    } catch (err) {
      console.error("Failed to load dashboard metrics", err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <DashboardLayout>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="fw-bold mb-1">Operations Overview</h2>
          <p className="text-muted mb-0">Real-time metrics and activity across the branch.</p>
        </div>
        <div className="d-flex gap-2">
          <Link to="/checkin" className="btn btn-outline-primary">
            <i className="bi bi-qr-code-scan me-1"></i> Launch Kiosk
          </Link>
          <Link to="/classes" className="btn btn-primary">
            <i className="bi bi-calendar-plus me-1"></i> Class Timetable
          </Link>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="row g-3 mb-4">
        <div className="col-12 col-sm-6 col-xl-3">
          <div className="card shadow-sm border-0 border-start border-primary border-4 p-3 bg-white">
            <div className="d-flex justify-content-between align-items-center">
              <div>
                <div className="text-muted small fw-medium">ACTIVE MEMBERS</div>
                <div className="fs-3 fw-bold text-dark mt-1">{metrics.activeMembersCount}</div>
              </div>
              <div className="rounded-circle bg-primary-subtle p-3 text-primary">
                <i className="bi bi-people-fill fs-4"></i>
              </div>
            </div>
            <div className="mt-2 text-muted small">
              <span className="text-success fw-bold"><i className="bi bi-arrow-up-right"></i> Scoped</span> to current branch
            </div>
          </div>
        </div>

        <div className="col-12 col-sm-6 col-xl-3">
          <div className="card shadow-sm border-0 border-start border-success border-4 p-3 bg-white">
            <div className="d-flex justify-content-between align-items-center">
              <div>
                <div className="text-muted small fw-medium">MONTHLY REVENUE (MRR)</div>
                <div className="fs-3 fw-bold text-success mt-1">
                  ${Number(metrics.monthlyRecurringRevenue).toFixed(2)}
                </div>
              </div>
              <div className="rounded-circle bg-success-subtle p-3 text-success">
                <i className="bi bi-currency-dollar fs-4"></i>
              </div>
            </div>
            <div className="mt-2 text-muted small">
              <span className="badge bg-success-subtle text-success">Active Subscriptions</span>
            </div>
          </div>
        </div>

        <div className="col-12 col-sm-6 col-xl-3">
          <div className="card shadow-sm border-0 border-start border-info border-4 p-3 bg-white">
            <div className="d-flex justify-content-between align-items-center">
              <div>
                <div className="text-muted small fw-medium">TODAY'S CHECK-INS</div>
                <div className="fs-3 fw-bold text-dark mt-1">{metrics.todayCheckInsCount}</div>
              </div>
              <div className="rounded-circle bg-info-subtle p-3 text-info">
                <i className="bi bi-door-open-fill fs-4"></i>
              </div>
            </div>
            <div className="mt-2 text-muted small">
              <span className="text-info fw-bold">Live attendance</span>
            </div>
          </div>
        </div>

        <div className="col-12 col-sm-6 col-xl-3">
          <div className="card shadow-sm border-0 border-start border-warning border-4 p-3 bg-white">
            <div className="d-flex justify-content-between align-items-center">
              <div>
                <div className="text-muted small fw-medium">CLASS UTILIZATION</div>
                <div className="fs-3 fw-bold text-dark mt-1">
                  {metrics.classCapacityUtilizationPercentage}%
                </div>
              </div>
              <div className="rounded-circle bg-warning-subtle p-3 text-warning">
                <i className="bi bi-pie-chart-fill fs-4"></i>
              </div>
            </div>
            <div className="mt-2 text-muted small">
              <span>{metrics.upcomingClassesCount} scheduled sessions</span>
            </div>
          </div>
        </div>
      </div>

      {/* Two Column Layout: Quick Actions & Live Stream */}
      <div className="row g-4">
        {/* Left: Quick Launch & Architecture Badges */}
        <div className="col-lg-6">
          <div className="card shadow-sm border-0 bg-white mb-4">
            <div className="card-header bg-white border-bottom py-3">
              <h5 className="card-title m-0 fw-bold">
                <i className="bi bi-cpu-fill text-primary me-2"></i>Enterprise Architectural Features
              </h5>
            </div>
            <div className="card-body">
              <div className="list-group list-group-flush">
                <div className="list-group-item d-flex align-items-center px-0 py-3">
                  <div className="rounded p-2 bg-primary-subtle text-primary me-3">
                    <i className="bi bi-diagram-3-fill fs-5"></i>
                  </div>
                  <div className="flex-grow-1">
                    <div className="fw-bold">Multi-Tenancy & Query Isolation</div>
                    <div className="text-muted small">
                      EF Core Global Query Filters isolate data strictly by branch header (<code>X-Tenant-ID</code>).
                    </div>
                  </div>
                  <span className="badge bg-success">Active</span>
                </div>

                <div className="list-group-item d-flex align-items-center px-0 py-3">
                  <div className="rounded p-2 bg-warning-subtle text-warning me-3">
                    <i className="bi bi-hourglass-split fs-5"></i>
                  </div>
                  <div className="flex-grow-1">
                    <div className="fw-bold">Concurrency-Safe Booking & Waitlist</div>
                    <div className="text-muted small">
                      Atomic slot reservations prevent overbooking; cancellations auto-promote top waitlist members.
                    </div>
                  </div>
                  <span className="badge bg-success">Active</span>
                </div>

                <div className="list-group-item d-flex align-items-center px-0 py-3">
                  <div className="rounded p-2 bg-info-subtle text-info me-3">
                    <i className="bi bi-credit-card-fill fs-5"></i>
                  </div>
                  <div className="flex-grow-1">
                    <div className="fw-bold">Stripe Webhooks & Idempotency Guard</div>
                    <div className="text-muted small">
                      HMAC-SHA256 signature verification and unique event tracking prevent duplicate billing replay attacks.
                    </div>
                  </div>
                  <span className="badge bg-success">Active</span>
                </div>

                <div className="list-group-item d-flex align-items-center px-0 py-3">
                  <div className="rounded p-2 bg-secondary-subtle text-secondary me-3">
                    <i className="bi bi-journal-code fs-5"></i>
                  </div>
                  <div className="flex-grow-1">
                    <div className="fw-bold">Audit Trail & Background Jobs</div>
                    <div className="text-muted small">
                      Background worker handles lifecycle expiry; EF Core interceptor persists property JSON diffs.
                    </div>
                  </div>
                  <span className="badge bg-success">Active</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Right: Real-time Activity Stream */}
        <div className="col-lg-6">
          <div className="card shadow-sm border-0 bg-white">
            <div className="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center">
              <h5 className="card-title m-0 fw-bold">
                <i className="bi bi-broadcast text-danger me-2"></i>Live Activity Feed
              </h5>
              <button onClick={loadMetrics} className="btn btn-sm btn-outline-secondary">
                <i className="bi bi-arrow-clockwise"></i> Refresh
              </button>
            </div>
            <div className="card-body">
              {metrics.recentActivities && metrics.recentActivities.length > 0 ? (
                <div className="timeline">
                  {metrics.recentActivities.map((act, index) => (
                    <div key={index} className="d-flex mb-3 pb-2 border-bottom border-light">
                      <div className="me-3">
                        {act.type === "CheckIn" ? (
                          <span className="badge rounded-pill bg-success-subtle text-success p-2">
                            <i className="bi bi-door-open fs-6"></i>
                          </span>
                        ) : (
                          <span className="badge rounded-pill bg-primary-subtle text-primary p-2">
                            <i className="bi bi-credit-card fs-6"></i>
                          </span>
                        )}
                      </div>
                      <div className="flex-grow-1">
                        <div className="small fw-semibold text-dark">{act.description}</div>
                        <div className="text-muted small" style={{ fontSize: "0.75rem" }}>
                          {new Date(act.timestampUtc).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-4 text-muted">
                  <i className="bi bi-inbox fs-2 mb-2 d-block"></i>
                  No recent activities recorded yet.
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </DashboardLayout>
  );
}

export default DashboardHome;
