import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { createPlan } from "../../api/planApi";

function CreatePlan() {

    const navigate = useNavigate();

    const [formData, setFormData] = useState({
        name: "",
        price: "",
        durationInDays: "",
        description: ""
    });

    const handleChange = (e) => {

        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const handleSubmit = async (e) => {

        e.preventDefault();

        try {

            await createPlan({
                ...formData,
                price: Number(formData.price),
                durationInDays: Number(formData.durationInDays)
            });

            navigate("/");

        } catch (error) {

            console.error(error);
            alert("Failed to create plan");
        }
    };

    return (
        <div className="container mt-5">

            <h2>Create Plan</h2>

            <form onSubmit={handleSubmit}>

                <div className="mb-3">
                    <label>Name</label>
                    <input
                        className="form-control"
                        name="name"
                        value={formData.name}
                        onChange={handleChange}
                        required
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
                        required
                    />
                </div>

                <div className="mb-3">
                    <label>Duration (Days)</label>
                    <input
                        type="number"
                        className="form-control"
                        name="durationInDays"
                        value={formData.durationInDays}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div className="mb-3">
                    <label>Description</label>
                    <textarea
                        className="form-control"
                        name="description"
                        rows="4"
                        value={formData.description}
                        onChange={handleChange}
                    />
                </div>

                <button
                    type="submit"
                    className="btn btn-success"
                >
                    Create Plan
                </button>

            </form>

        </div>
    );
}

export default CreatePlan;