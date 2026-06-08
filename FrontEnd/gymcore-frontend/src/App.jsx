import { BrowserRouter, Routes, Route } from "react-router-dom";

import PlanList from "./pages/plans/PlanList";
import CreatePlan from "./pages/plans/CreatePlan";
import EditPlan from "./pages/plans/EditPlan";
import PlanDetails from "./pages/plans/PlanDetails";
import MemberList from "./pages/members/MemberList";
import CreateMember from "./pages/members/CreateMember";
import EditMember from "./pages/members/EditMember";
import MemberDetails from "./pages/members/MemberDetails";
import Login from "./pages/auth/Login";
import Register from "./pages/auth/Register";
import ProtectedRoute from "./components/ProtectedRoute";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/plans" element={<ProtectedRoute><PlanList /></ProtectedRoute>} />
        <Route path="/plans/create" element={<ProtectedRoute><CreatePlan /></ProtectedRoute>} />
        <Route path="/plans/:id" element={<ProtectedRoute><PlanDetails /></ProtectedRoute>} />
        <Route path="/plans/edit/:id" element={<ProtectedRoute><EditPlan /></ProtectedRoute>} />
        <Route path="/members" element={<ProtectedRoute><MemberList /></ProtectedRoute>} />
        <Route path="/members/create" element={<ProtectedRoute><CreateMember /></ProtectedRoute>} />
        <Route path="/members/edit/:id" element={<ProtectedRoute><EditMember /></ProtectedRoute>} />
        <Route path="/members/:id" element={<ProtectedRoute><MemberDetails /></ProtectedRoute>} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;