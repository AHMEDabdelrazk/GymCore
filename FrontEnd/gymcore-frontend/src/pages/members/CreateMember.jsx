import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import { createMember } from "../../api/memberApi";
import { getPlans } from "../../api/planApi";

import DashboardLayout from "../../layouts/DashboardLayout";

function CreateMember() {

    const navigate = useNavigate();

    const [plans, setPlans] = useState([]);

    const [formData, setFormData] = useState({
        fullName: "",
        email: "",
        phoneNumber: "",
        dateOfBirth: "",
        gender: "Male",
        address: "",
        membershipPlanId: ""
    });

    useEffect(() => {

        loadPlans();

    }, []);

    const loadPlans = async () => {

        const response = await getPlans();

        setPlans(response.data);
    };

    const handleChange = (e) => {

        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const handleSubmit = async (e) => {

        e.preventDefault();

        await createMember({
            ...formData,
            membershipPlanId:
                Number(formData.membershipPlanId)
        });

        navigate("/members");
    };

    return (

        <DashboardLayout>

            <h2 className="mb-4">
                Create Member
            </h2>

            <form onSubmit={handleSubmit}>

                <div className="row">

                    <div className="col-md-6 mb-3">

                        <label>Full Name</label>

                        <input
                            className="form-control"
                            name="fullName"
                            value={formData.fullName}
                            onChange={handleChange}
                            required
                        />

                    </div>

                    <div className="col-md-6 mb-3">

                        <label>Email</label>

                        <input
                            type="email"
                            className="form-control"
                            name="email"
                            value={formData.email}
                            onChange={handleChange}
                        />

                    </div>

                    <div className="col-md-6 mb-3">

                        <label>Phone Number</label>

                        <input
                            className="form-control"
                            name="phoneNumber"
                            value={formData.phoneNumber}
                            onChange={handleChange}
                        />

                    </div>

                    <div className="col-md-6 mb-3">

                        <label>Date Of Birth</label>

                        <input
                            type="date"
                            className="form-control"
                            name="dateOfBirth"
                            value={formData.dateOfBirth}
                            onChange={handleChange}
                        />

                    </div>

                    <div className="col-md-6 mb-3">

                        <label>Gender</label>

                        <select
                            className="form-control"
                            name="gender"
                            value={formData.gender}
                            onChange={handleChange}
                        >
                            <option>Male</option>
                            <option>Female</option>
                        </select>

                    </div>

                    <div className="col-md-6 mb-3">

                        <label>Membership Plan</label>

                        <select
                            className="form-control"
                            name="membershipPlanId"
                            value={formData.membershipPlanId}
                            onChange={handleChange}
                            required
                        >
                            <option value="">
                                Select Plan
                            </option>

                            {plans.map(plan => (

                                <option
                                    key={plan.id}
                                    value={plan.id}
                                >
                                    {plan.name}
                                </option>

                            ))}

                        </select>

                    </div>

                    <div className="col-12 mb-3">

                        <label>Address</label>

                        <textarea
                            className="form-control"
                            rows="3"
                            name="address"
                            value={formData.address}
                            onChange={handleChange}
                        />

                    </div>

                </div>

                <button
                    className="btn btn-success"
                    type="submit"
                >
                    Save Member
                </button>

            </form>

        </DashboardLayout>
    );
}

export default CreateMember;