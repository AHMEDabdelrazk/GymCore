import { Link, useNavigate } from "react-router-dom";
import { useAuth }
    from "../context/AuthContext";

function DashboardLayout({ children }) {

    const { logoutUser } = useAuth();

    const navigate = useNavigate();

    const handleLogout = () => {

        logoutUser();

        navigate("/");
    };

    return (
    <div className="dashboard-container">

        <div className="sidebar">

            <h3 className="mb-4 p-3">
                GymCore
            </h3>

            <ul className="nav flex-column px-3">

                <li className="nav-item mb-2">
                    <Link
                        className="nav-link text-white"
                        to="/plans"
                    >
                        📋 Plans
                    </Link>
                </li>

                <li className="nav-item mb-2">
                    <Link
                        className="nav-link text-white"
                        to="/members"
                    >
                        🧘🏿 Members
                    </Link>
                </li>

                <li className="nav-item mb-2">
                    <Link
                        className="nav-link"
                        style={{ color: 'black' }}
                    >
                        👨‍💻 Admins
                    </Link>
                </li>

                <li className="nav-item mb-2" >
                    <Link
                        className="nav-link"
                        style={{ color: 'black' }}
                    >
                        🏃 Trainer
                    </Link>
                </li>

                <li className="nav-item mb-2">
                    <Link
                        className="nav-link"
                        style={{ color: 'black' }}
                    >
                        ⚙️ Settings
                    </Link>
                </li>

            </ul>

        </div>

        <div className="main-wrapper">

            <nav className="navbar navbar-light bg-white border-bottom px-4">

                <h4 className="mb-0">
                    GymCore Dashboard
                </h4>

                <button
                    className="btn btn-outline-danger"
                    onClick={handleLogout}
                >
                    Logout
                </button>

            </nav>

            <main className="content-area">

                {children}

            </main>

        </div>

    </div>
);
}

export default DashboardLayout;