import { useState } from "react";

import { Link } from "react-router-dom";

import { useNavigate }
    from "react-router-dom";

import AuthLayout
    from "../../layouts/AuthLayout";

import { login }
    from "../../api/authApi";

import { useAuth }
    from "../../context/AuthContext";

function Login() {

    const [error, setError] = useState("");

    const navigate = useNavigate();

    const { loginUser } = useAuth();

    const [formData, setFormData] =
        useState({
            email: "",
            password: ""
        });

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

        try {

            const response =
                await login(formData);

            loginUser(
                response.data
            );

            navigate("/plans");

        } catch {

            setError(
                "Invalid email or password"
            );
        }
};

    return (

        <AuthLayout>

            <div className="auth-card">

                <h2>Login</h2>

                {error && (

                    <div className="alert alert-danger">

                        {error}

                    </div>

                )}

                <form
                    onSubmit={handleSubmit}
                >

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

                    <button
                        type="submit"
                        className="btn btn-success w-100"
                    >
                        Login
                    </button>

                </form>

                <p className="mt-3 text-center">

                    Don't have an account?

                    <Link
                        to="/register"
                        className="ms-2"
                    >
                        Register
                    </Link>

                </p>

            </div>

        </AuthLayout>
    );
}

export default Login;