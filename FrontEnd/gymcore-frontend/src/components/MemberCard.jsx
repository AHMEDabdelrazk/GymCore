import { Link } from "react-router-dom";

function MemberCard({ member, onDeactivate }) {
  const getSubBadge = (status) => {
    switch (status) {
      case "Active":
        return <span className="badge bg-success">Sub: Active</span>;
      case "GracePeriod":
        return <span className="badge bg-warning text-dark">Sub: Grace Period</span>;
      case "Expired":
        return <span className="badge bg-danger">Sub: Expired</span>;
      case "Canceled":
        return <span className="badge bg-secondary">Sub: Canceled</span>;
      default:
        return <span className="badge bg-info">Sub: Active</span>;
    }
  };

  return (
    <div className="card shadow-sm h-100 border-0 bg-white">
      <div className="card-body">
        <div className="d-flex justify-content-between align-items-start mb-2">
          <div>
            <h5 className="fw-bold mb-1">{member.fullName}</h5>
            <span className="badge bg-light text-dark border font-monospace">
              <i className="bi bi-upc me-1"></i>
              {member.memberCode || `MEM-${member.id}`}
            </span>
          </div>

          <div className="d-flex flex-column align-items-end gap-1">
            <span className={member.isActive ? "badge bg-success" : "badge bg-secondary"}>
              {member.isActive ? "Profile Active" : "Inactive"}
            </span>
            {getSubBadge(member.subscriptionStatus)}
          </div>
        </div>

        <hr className="my-2" />

        <p className="small mb-1">
          <strong>Email:</strong> {member.email}
        </p>

        <p className="small mb-1">
          <strong>Phone:</strong> {member.phoneNumber}
        </p>

        <p className="small mb-3">
          <strong>Plan:</strong> {member.membershipPlanName || "Standard"}
        </p>

        <div className="d-flex gap-2">
          <Link className="btn btn-sm btn-outline-primary" to={`/members/${member.id}`}>
            View
          </Link>

          <Link className="btn btn-sm btn-outline-warning text-dark" to={`/members/edit/${member.id}`}>
            Edit
          </Link>

          {member.isActive && (
            <button
              className="btn btn-sm btn-outline-danger ms-auto"
              onClick={() => onDeactivate(member.id)}
            >
              Deactivate
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export default MemberCard;