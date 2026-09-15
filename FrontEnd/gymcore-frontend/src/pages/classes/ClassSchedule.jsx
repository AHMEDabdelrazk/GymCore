import { useEffect, useState } from "react";
import DashboardLayout from "../../layouts/DashboardLayout";
import { getUpcomingSessions, bookClassSession, cancelClassBooking, getSessionRoster } from "../../api/classApi";
import { getMembers } from "../../api/memberApi";

function ClassSchedule() {
  const [sessions, setSessions] = useState([]);
  const [members, setMembers] = useState([]);
  const [selectedSession, setSelectedSession] = useState(null);
  const [selectedMemberId, setSelectedMemberId] = useState("");
  const [roster, setRoster] = useState([]);
  const [activeRosterSessionId, setActiveRosterSessionId] = useState(null);
  const [alertMsg, setAlertMsg] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [sessionsRes, membersRes] = await Promise.all([
        getUpcomingSessions(),
        getMembers(),
      ]);
      setSessions(sessionsRes.data);
      setMembers(membersRes.data);
      if (membersRes.data.length > 0) {
        setSelectedMemberId(membersRes.data[0].id.toString());
      }
    } catch (err) {
      console.error("Failed to load class schedule data", err);
    } finally {
      setLoading(false);
    }
  };

  const handleBookSpot = async (sessionId) => {
    if (!selectedMemberId) {
      alert("Please select a member first!");
      return;
    }

    try {
      const res = await bookClassSession(sessionId, parseInt(selectedMemberId));
      setAlertMsg({
        type: res.data.status === "Confirmed" ? "success" : "warning",
        text: `Result: ${res.data.status}! ${res.data.message}`,
      });
      await loadData();
      if (activeRosterSessionId === sessionId) {
        handleViewRoster(sessionId);
      }
    } catch (err) {
      setAlertMsg({
        type: "danger",
        text: err.response?.data?.error || "Failed to book session.",
      });
    }
  };

  const handleCancelBooking = async (bookingId, sessionId) => {
    try {
      await cancelClassBooking(bookingId);
      setAlertMsg({
        type: "info",
        text: "Booking canceled. Top waitlisted member was automatically promoted to Confirmed!",
      });
      await loadData();
      if (activeRosterSessionId) {
        handleViewRoster(activeRosterSessionId);
      }
    } catch (err) {
      setAlertMsg({
        type: "danger",
        text: "Failed to cancel booking.",
      });
    }
  };

  const handleViewRoster = async (sessionId) => {
    try {
      setActiveRosterSessionId(sessionId);
      const res = await getSessionRoster(sessionId);
      setRoster(res.data);
    } catch (err) {
      console.error("Failed to load roster", err);
    }
  };

  return (
    <DashboardLayout>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="fw-bold mb-1">Class Schedule & Booking</h2>
          <p className="text-muted mb-0">
            High-concurrency slot reservations with automated waitlist promotion.
          </p>
        </div>

        {/* Member selector for booking demo */}
        <div className="d-flex align-items-center gap-2 bg-white p-2 rounded shadow-sm border">
          <label className="small fw-bold text-muted mb-0">Booking as:</label>
          <select
            className="form-select form-select-sm"
            style={{ width: "200px" }}
            value={selectedMemberId}
            onChange={(e) => setSelectedMemberId(e.target.value)}
          >
            {members.map((m) => (
              <option key={m.id} value={m.id}>
                {m.fullName} ({m.memberCode})
              </option>
            ))}
          </select>
        </div>
      </div>

      {alertMsg && (
        <div className={`alert alert-${alertMsg.type} alert-dismissible fade show shadow-sm`} role="alert">
          <i className="bi bi-info-circle-fill me-2"></i>
          {alertMsg.text}
          <button type="button" className="btn-close" onClick={() => setAlertMsg(null)}></button>
        </div>
      )}

      {/* Class Sessions List */}
      <div className="row g-4 mb-4">
        {sessions.map((session) => {
          const isFull = session.availableSpots <= 0;
          return (
            <div key={session.id} className="col-12 col-md-6 col-lg-4">
              <div className="card h-100 shadow-sm border-0 bg-white">
                <div className="card-header bg-white border-bottom d-flex justify-content-between align-items-center py-3">
                  <span className="badge bg-dark">{session.category}</span>
                  <span className={`badge ${isFull ? "bg-danger" : "bg-success"}`}>
                    {isFull ? "FULL (Waitlist Open)" : `${session.availableSpots} SPOTS LEFT`}
                  </span>
                </div>

                <div className="card-body">
                  <h5 className="card-title fw-bold text-dark">{session.classTypeName}</h5>
                  <div className="text-muted small mb-3">
                    <i className="bi bi-geo-alt me-1 text-danger"></i> {session.roomName} &bull;{" "}
                    <i className="bi bi-person me-1 text-primary"></i> {session.trainerName}
                  </div>

                  <div className="d-flex align-items-center gap-2 mb-3">
                    <i className="bi bi-clock text-muted"></i>
                    <span className="small text-dark fw-medium">
                      {new Date(session.startTimeUtc).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })} -{" "}
                      {new Date(session.endTimeUtc).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}
                    </span>
                  </div>

                  {/* Progress bar of spots */}
                  <div className="mb-3">
                    <div className="d-flex justify-content-between small text-muted mb-1">
                      <span>Reserved: {session.reservedSpots}/{session.capacity}</span>
                      {session.waitlistCount > 0 && (
                        <span className="text-warning fw-bold">Waitlist: {session.waitlistCount}</span>
                      )}
                    </div>
                    <div className="progress" style={{ height: "8px" }}>
                      <div
                        className={`progress-bar ${isFull ? "bg-danger" : "bg-primary"}`}
                        role="progressbar"
                        style={{ width: `${Math.min(100, (session.reservedSpots / session.capacity) * 100)}%` }}
                      ></div>
                    </div>
                  </div>

                  <div className="d-flex gap-2">
                    <button
                      onClick={() => handleBookSpot(session.id)}
                      className={`btn btn-sm flex-grow-1 ${isFull ? "btn-warning" : "btn-primary"}`}
                    >
                      {isFull ? "Join Waitlist" : "Reserve Spot"}
                    </button>
                    <button
                      onClick={() => handleViewRoster(session.id)}
                      className="btn btn-sm btn-outline-secondary"
                      title="View Roster"
                    >
                      <i className="bi bi-people"></i> Roster
                    </button>
                  </div>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Roster & Auto-Promotion Demo Drawer */}
      {activeRosterSessionId && (
        <div className="card shadow border-0 bg-white mb-4">
          <div className="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center">
            <h5 className="card-title m-0 fw-bold">
              <i className="bi bi-list-check text-primary me-2"></i>
              Class Roster & Concurrency Auto-Promotion (Session #{activeRosterSessionId})
            </h5>
            <button
              onClick={() => setActiveRosterSessionId(null)}
              className="btn btn-sm btn-outline-secondary"
            >
              Close
            </button>
          </div>
          <div className="card-body">
            <p className="text-muted small mb-3">
              <strong>Testing Tip:</strong> Cancel any <code>Confirmed</code> reservation below to observe the backend automatically promote the earliest <code>Waitlisted</code> member into the confirmed spot in real time!
            </p>

            <div className="table-responsive">
              <table className="table table-hover align-middle mb-0">
                <thead className="table-light">
                  <tr>
                    <th>Member</th>
                    <th>Email</th>
                    <th>Booking Status</th>
                    <th>Waitlist Pos</th>
                    <th>Registered At</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  {roster.length > 0 ? (
                    roster.map((r) => (
                      <tr key={r.bookingId}>
                        <td className="fw-semibold">{r.memberName}</td>
                        <td className="text-muted">{r.memberEmail}</td>
                        <td>
                          {r.status === "Confirmed" ? (
                            <span className="badge bg-success">Confirmed</span>
                          ) : (
                            <span className="badge bg-warning text-dark">Waitlisted</span>
                          )}
                        </td>
                        <td>{r.waitlistPosition ? `#${r.waitlistPosition}` : "—"}</td>
                        <td className="small text-muted">
                          {new Date(r.bookedAtUtc).toLocaleTimeString()}
                        </td>
                        <td>
                          <button
                            onClick={() => handleCancelBooking(r.bookingId, activeRosterSessionId)}
                            className="btn btn-sm btn-outline-danger"
                          >
                            <i className="bi bi-x-circle me-1"></i> Cancel Spot
                          </button>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan="6" className="text-center py-3 text-muted">
                        No active bookings for this session yet.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </DashboardLayout>
  );
}

export default ClassSchedule;
