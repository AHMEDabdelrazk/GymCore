import { Link } from "react-router-dom";

function MemberCard({
    member,
    onDeactivate
}) {

    return (

        <div className="card shadow-sm h-100">

            <div className="card-body">

                <div className="d-flex justify-content-between">

                    <h5>
                        {member.fullName}
                    </h5>

                    <span
                        className={
                            member.isActive
                                ? "badge bg-success"
                                : "badge bg-danger"
                        }
                    >
                        {member.isActive
                            ? "Active"
                            : "Inactive"}
                    </span>

                </div>

                <hr />

                <p>
                    <strong>Email:</strong>
                    {" "}
                    {member.email}
                </p>

                <p>
                    <strong>Phone:</strong>
                    {" "}
                    {member.phoneNumber}
                </p>

                <p>
                    <strong>Plan:</strong>
                    {" "}
                    {member.membershipPlanName}
                </p>

                <div className="mt-3">

                    <Link
                        className="btn btn-primary me-2"
                        to={`/members/${member.id}`}
                    >
                        View
                    </Link>

                    <Link
                        className="btn btn-warning me-2"
                        to={`/members/edit/${member.id}`}
                    >
                        Edit
                    </Link>

                    <button
                        className="btn btn-danger"
                        onClick={() =>
                            onDeactivate(member.id)
                        }
                    >
                        Deactivate
                    </button>

                </div>

            </div>

        </div>
    );
}

export default MemberCard;