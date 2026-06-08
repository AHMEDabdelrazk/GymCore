import { Link } from "react-router-dom";

function AuthLayout({ children }) {

    return (
        <div
            className="auth-container"
        >
            <div className="auth-left">

                <div>

                    <h1>
                        GymCore
                    </h1>

                    <p>
                        Modern Gym Management System
                    </p>

                </div>

            </div>

            <div className="auth-right">

                {children}

            </div>

        </div>
    );
}

export default AuthLayout;