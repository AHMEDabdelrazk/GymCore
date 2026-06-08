import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import DashboardLayout from "../../layouts/DashboardLayout";

import { getMember, updateMember } from "../../api/memberApi";

function EditMember() {

    const { id } = useParams();

    const navigate = useNavigate();

    const [formData, setFormData] = useState({
        fullName: "",
        email: "",
        phoneNumber: "",
        gender: "",
        membershipPlanId: ""
    });

    useEffect(() => {

        loadMember();

    }, []);

    const loadMember = async () => {

        const response =
            await getMember(id);

        setFormData(response.data);
    };

    const handleChange = (e) => {

        setFormData({
            ...formData,
            [e.target.name]:
                e.target.value
        });
    };

    const handleSubmit = async (e) => {

        e.preventDefault();

        await updateMember(id, formData);

        navigate("/members");
    };

    return (

        <DashboardLayout>

            <h2>Edit Member</h2>

            <form onSubmit={handleSubmit}>

                <input
                    className="form-control mb-3"
                    name="fullName"
                    value={formData.fullName}
                    onChange={handleChange}
                />

                <input
                    className="form-control mb-3"
                    name="email"
                    value={formData.email}
                    onChange={handleChange}
                />

                <input
                    className="form-control mb-3"
                    name="phoneNumber"
                    value={formData.phoneNumber}
                    onChange={handleChange}
                />

                <button
                    className="btn btn-warning"
                    type="submit"
                >
                    Update Member
                </button>

            </form>

        </DashboardLayout>
    );
}

export default EditMember;