import { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import DashboardLayout from "../../layouts/DashboardLayout";
import PlanCard from "../../components/PlanCard";

import {
    getPlans,
    deactivatePlan
} from "../../api/planApi";

function PlanList() {

    const [plans, setPlans] = useState([]);
    const [searchTerm, setSearchTerm] = useState("");

    useEffect(() => {

        loadPlans();

    }, []);

    const loadPlans = async () => {

        try {

            const response = await getPlans();

            setPlans(response.data);

        }
        catch (error) {

            console.error(error);
        }
    };

    const handleDeactivate = async (id) => {

        const confirmed =
            window.confirm(
                "Deactivate this plan?"
            );

        if (!confirmed)
            return;

        try {

            await deactivatePlan(id);

            loadPlans();

        }
        catch (error) {

            console.error(error);
        }
    };

    const activePlans =
        plans.filter(x => x.isActive).length;

    const inactivePlans =
        plans.filter(x => !x.isActive).length;

    const filteredPlans =
        plans.filter(plan =>
            plan.name
                .toLowerCase()
                .includes(
                    searchTerm.toLowerCase()
                )
        );

    return (

        <DashboardLayout>

            <div className="alert alert-primary">

                Welcome back Admin 👋

            </div>
            <div className="d-flex justify-content-between align-items-center mb-4">

                <div>

                    <h2>
                        Membership Plans
                    </h2>

                    <p 
                        style={{ color: 'white' }} >
                        Manage gym subscriptions
                    </p>

                </div>

                <Link
                    to="/plans/create"
                    className="btn btn-success"
                >
                    <i className="bi bi-plus-circle"></i>

                    {" "}New Plan
                </Link>

            </div>

            <div className="mb-4">

                <input
                    type="text"
                    className="form-control"
                    placeholder="Search plans..."
                    value={searchTerm}
                    onChange={(e) =>
                        setSearchTerm(
                            e.target.value
                        )
                    }
                />

            </div>

           <div className="row mb-4">

                <div className="col-md-4">

                    <div className="card stats-card shadow-sm">

                        <div className="card-body">

                            <h6>Total Plans</h6>

                            <h1 style={{ color: 'black' }}>{plans.length}</h1>

                        </div>

                    </div>

                </div>

                <div className="col-md-4">

                    <div className="card stats-card shadow-sm">

                        <div className="card-body">

                            <h6>Active Plans</h6>

                            <h1 style={{ color: 'black' }}>{activePlans}</h1>

                        </div>

                    </div>

                </div>

                <div className="col-md-4">

                    <div className="card stats-card shadow-sm">

                        <div className="card-body">

                            <h6>Inactive Plans</h6>

                            <h1 style={{ color: 'black' }}>{inactivePlans}</h1>

                        </div>

                    </div>

                </div>

            </div>

            <div className="row">

                {filteredPlans.map(plan => (

                    <div
                        key={plan.id}
                        className="col-md-4 mb-4"
                    >

                        <PlanCard
                            plan={plan}
                            onDeactivate={handleDeactivate}
                        />

                    </div>

                ))}

            </div>

        </DashboardLayout>
    );
}

export default PlanList;