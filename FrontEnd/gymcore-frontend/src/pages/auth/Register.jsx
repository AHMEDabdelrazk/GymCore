import { useState } from "react";

import { Link } from "react-router-dom";

import { useNavigate }
    from "react-router-dom";

import AuthLayout
    from "../../layouts/AuthLayout";

import { register }
    from "../../api/authApi";


function Register() {

    const [error, setError] = useState("");

    const [success, setSuccess] = useState("");
    
    const navigate = useNavigate();

    const [formData, setFormData] =
        useState({
            fullName: "",
            email: "",
            password: ""
        });

    const password = formData.password;

    const handleChange = (e) => {

        setFormData({
            ...formData,
            [e.target.name]:
                e.target.value
        });
    };

    const handleSubmit = async (e) => {

    e.preventDefault();

    setError("");
    setSuccess("");

    try {

        await register(formData);

        setSuccess(
            "Account created successfully. You can now login."
        );

        setTimeout(() => {

            navigate("/");

        }, 1500);

    } catch (err) {

        const data =
            err.response?.data;

        if (typeof data === "string") {

            setError(data);

            return;
        }

        if (Array.isArray(data)) {

            setError(
                data
                    .map(x => x.description)
                    .join(", ")
            );

            return;
        }

        setError(
            "Registration failed"
        );
    }
    };

    return (

        <AuthLayout>

            <div className="auth-card">

                <h2>Create Account</h2>

                {error && (

                    <div className="alert alert-danger">

                        {error}

                    </div>

                )}

                {success && (

                    <div className="alert alert-success">

                        {success}

                    </div>

                )}

                <form
                    onSubmit={handleSubmit}
                >

                    <input
                        className="form-control mb-3"
                        placeholder="Full Name"
                        name="fullName"
                        value={formData.fullName}
                        onChange={handleChange}
                    />

                    <input
                        className="form-control mb-3"
                        placeholder="Email"
                        name="email"
                        value={formData.email}
                        onChange={handleChange}
                    />

                    <input
                        type="password"
                        className="form-control mb-3"
                        placeholder="Password"
                        name="password"
                        value={formData.password}
                        onChange={handleChange}
                    />

                    {/* (password.length < 8) && (password.length > 0) */}
                    
                        {password && password.length < 8 && (

                            <div className="text-danger mt-1">

                                Password is too short

                            </div>

                        )}

                        {password.length >= 8 && (

                            <div className="text-success mt-1">

                                Strong password

                            </div>

                        )}

                    <button
                        type="submit"
                        className="btn btn-success w-100"
                    >
                        Register
                    </button>

                </form>

                <p className="mt-3 text-center">

                    Already have an account?

                    <Link
                        to="/"
                        className="ms-2"
                    >
                        Login
                    </Link>

                </p>

            </div>

        </AuthLayout>
    );
}

export default Register;