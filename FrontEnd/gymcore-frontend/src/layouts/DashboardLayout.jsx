import { useEffect, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { getTenants } from "../api/tenantApi";

function DashboardLayout({ children }) {
  const { user, logoutUser, selectedTenantId, switchTenant } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [tenants, setTenants] = useState([]);

  useEffect(() => {
    loadTenants();
  }, []);

  const loadTenants = async () => {
    try {
      const res = await getTenants();
      setTenants(res.data);
    } catch (err) {
      // Fallback local tenants if backend offline
      setTenants([
        { id: 1, name: "Apex Downtown Flagship", code: "APX-DT" },
        { id: 2, name: "Apex Westside Performance Hub", code: "APX-WS" },
      ]);
    }
  };

  const handleLogout = () => {
    logoutUser();
    navigate("/login");
  };

  const isSuperAdmin = user?.role === "SuperAdmin";

  const allNavItems = [
    { path: "/dashboard", label: "Dashboard", icon: "bi-speedometer2", roles: ["SuperAdmin", "Trainer", "FrontDeskStaff"] },
    { path: "/members", label: "Members", icon: "bi-people-fill", roles: ["SuperAdmin", "FrontDeskStaff"] },
    { path: "/plans", label: "Plans", icon: "bi-card-checklist", roles: ["SuperAdmin"] },
    { path: "/classes", label: "Classes & Schedule", icon: "bi-calendar-event", roles: ["SuperAdmin", "Trainer"] },
    { path: "/checkin", label: "Check-In Kiosk", icon: "bi-qr-code-scan", roles: ["SuperAdmin", "FrontDeskStaff"] },
    { path: "/billing", label: "Billing & Webhooks", icon: "bi-credit-card-2-front", roles: ["SuperAdmin", "FrontDeskStaff"] },
    { path: "/audit", label: "Audit Logs", icon: "bi-shield-check", roles: ["SuperAdmin"] },
  ];

  const currentRole = user?.role || "SuperAdmin";
  const navItems = allNavItems.filter((item) => item.roles.includes(currentRole));

  const activeTenantName = tenants.find((t) => t.id.toString() === selectedTenantId.toString())?.name || "Downtown Flagship";

  return (
    <div className="dashboard-container d-flex" style={{ minHeight: "100vh" }}>
      {/* Sidebar */}
      <div
        className="sidebar text-white p-3 d-flex flex-column"
        style={{
          width: "260px",
          background: "linear-gradient(180deg, #111827 0%, #1f2937 100%)",
          borderRight: "1px solid #374151",
        }}
      >
        <div className="d-flex align-items-center mb-4 px-2">
          <i className="bi bi-activity fs-3 text-primary me-2"></i>
          <div>
            <h4 className="m-0 fw-bold tracking-tight">GymCore</h4>
            <span className="badge bg-primary-subtle text-primary border border-primary-subtle small" style={{ fontSize: "0.65rem" }}>
              ENTERPRISE v2.0
            </span>
          </div>
        </div>

        {/* Tenant / Branch Selector in Sidebar */}
        <div className="mb-4 p-2 rounded bg-dark border border-secondary border-opacity-25">
          <label className="form-label text-muted small mb-1">
            <i className="bi bi-geo-alt-fill text-danger me-1"></i> Active Gym Branch
          </label>
          {isSuperAdmin ? (
            <select
              className="form-select form-select-sm bg-secondary text-white border-0"
              value={selectedTenantId}
              onChange={(e) => {
                switchTenant(e.target.value);
                window.location.reload(); // Refresh tenant-scoped data
              }}
            >
              {tenants.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name}
                </option>
              ))}
            </select>
          ) : (
            <div className="text-white small fw-semibold d-flex align-items-center justify-content-between px-1 py-1">
              <span className="text-truncate" style={{ maxWidth: "160px" }}>{activeTenantName}</span>
              <span className="badge bg-secondary-subtle text-warning border border-secondary border-opacity-50 small" title="Branch is locked to your assigned facility">
                <i className="bi bi-lock-fill me-1"></i>Locked
              </span>
            </div>
          )}
        </div>

        {/* Navigation items filtered by role */}
        <ul className="nav flex-column gap-1 flex-grow-1">
          {navItems.map((item) => {
            const isActive = location.pathname.startsWith(item.path);
            return (
              <li key={item.path} className="nav-item">
                <Link
                  to={item.path}
                  className={`nav-link d-flex align-items-center px-3 py-2 rounded transition-all ${
                    isActive
                      ? "bg-primary text-white fw-bold shadow-sm"
                      : "text-light text-opacity-75 hover-bg-secondary"
                  }`}
                  style={{ textDecoration: "none" }}
                >
                  <i className={`bi ${item.icon} me-3 fs-5`}></i>
                  {item.label}
                </Link>
              </li>
            );
          })}
        </ul>

        {/* User Info & Quick Switch Footer */}
        <div className="mt-auto pt-3 border-top border-secondary border-opacity-25 px-2">
          <div className="d-flex align-items-center justify-content-between mb-2">
            <div>
              <div className="small fw-bold text-truncate" style={{ maxWidth: "140px" }}>
                {user?.fullName || "Gym Operator"}
              </div>
              <span className="badge bg-success small">{user?.role || "Administrator"}</span>
            </div>
            <button
              onClick={handleLogout}
              className="btn btn-sm btn-outline-danger"
              title="Logout"
            >
              <i className="bi bi-box-arrow-right"></i>
            </button>
          </div>
        </div>
      </div>

      {/* Main Content Area */}
      <div className="main-wrapper flex-grow-1 d-flex flex-column bg-light" style={{ minHeight: "100vh" }}>
        {/* Top Navbar */}
        <nav className="navbar navbar-expand navbar-light bg-white border-bottom px-4 py-2 shadow-sm">
          <div className="d-flex align-items-center">
            <span className="text-secondary small fw-medium">
              Multi-Tenant Cluster &bull;{" "}
              <span className="text-dark fw-semibold">
                {tenants.find((t) => t.id.toString() === selectedTenantId.toString())?.name || "Downtown Branch"}
              </span>
            </span>
          </div>

          <div className="ms-auto d-flex align-items-center gap-3">
            <span className="badge bg-secondary-subtle text-dark border px-2 py-1">
              <i className="bi bi-shield-lock-fill text-primary me-1"></i>
              Role: {user?.role || "SuperAdmin"}
            </span>

            <button onClick={handleLogout} className="btn btn-outline-danger btn-sm">
              <i className="bi bi-power me-1"></i> Logout
            </button>
          </div>
        </nav>

        {/* Content View */}
        <main className="content-area p-4 flex-grow-1">{children}</main>
      </div>
    </div>
  );
}

export default DashboardLayout;