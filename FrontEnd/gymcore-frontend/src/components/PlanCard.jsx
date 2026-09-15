import { Link } from "react-router-dom";

function PlanCard({ plan, onDeactivate }) {

    return (

        <div className="card shadow-sm h-100">

            <div className="card-body">

                <div className="d-flex justify-content-between">

                    <h4>
                        {plan.name}
                    </h4>

                    <span
                        className={
                            plan.isActive
                                ? "badge bg-success"
                                : "badge bg-danger"
                        }
                    >
                        {plan.isActive
                            ? "Active"
                            : "Inactive"}
                    </span>

                </div>

                <hr />

                <div className="plan-price">

                    {plan.price} EGP

                </div>

                <p className="text-muted">

                    {plan.durationInDays} Days

                </p>

                <p>

                    {plan.description}

                </p>

                <div className="d-flex gap-2 mt-3">

                    <Link
                        className="btn btn-primary"
                        to={`/plans/${plan.id}`}
                    >
                        View
                    </Link>

                    <Link
                        className="btn btn-warning"
                        to={`/plans/edit/${plan.id}`}
                    >
                        Edit
                    </Link>

                    <button
                        className="btn btn-outline-danger"
                        onClick={() =>
                            onDeactivate(plan.id)
                        }
                    >
                        Disable
                    </button>

                </div>

            </div>

        </div>
    );
}

export default PlanCard;