import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import {
    getPlan,
    updatePlan
} from "../../api/planApi";

function EditPlan() {

    const { id } = useParams();

    const navigate = useNavigate();

    const [formData, setFormData] = useState({
        name: "",
        price: "",
        durationInDays: "",
        description: ""
    });

    useEffect(() => {

        loadPlan();

    }, []);

    const loadPlan = async () => {

        try {

            const response = await getPlan(id);

            setFormData(response.data);

        } catch (error) {

            console.error(error);
        }
    };

    const handleChange = (e) => {

        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const handleSubmit = async (e) => {

        e.preventDefault();

        try {

            await updatePlan(id, {
                name: formData.name,
                price: Number(formData.price),
                durationInDays: Number(formData.durationInDays),
                description: formData.description
            });

            navigate("/");

        } catch (error) {

            console.error(error);
            alert("Failed to update plan");
        }
    };

    return (
        <div className="container mt-5">

            <h2>Edit Plan</h2>

            <form onSubmit={handleSubmit}>

                <div className="mb-3">
                    <label>Name</label>
                    <input
                        className="form-control"
                        name="name"
                        value={formData.name}
                        onChange={handleChange}
                    />
                </div>

                <div className="mb-3">
                    <label>Price</label>
                    <input
                        type="number"
                        className="form-control"
                        name="price"
                        value={formData.price}
                        onChange={handleChange}
                    />
                </div>

                <div className="mb-3">
                    <label>Duration</label>
                    <input
                        type="number"
                        className="form-control"
                        name="durationInDays"
                        value={formData.durationInDays}
                        onChange={handleChange}
                    />
                </div>

                <div className="mb-3">
                    <label>Description</label>
                    <textarea
                        className="form-control"
                        rows="4"
                        name="description"
                        value={formData.description}
                        onChange={handleChange}
                    />
                </div>

                <button
                    className="btn btn-warning"
                    type="submit"
                >
                    Update Plan
                </button>

            </form>

        </div>
    );
}

export default EditPlan;