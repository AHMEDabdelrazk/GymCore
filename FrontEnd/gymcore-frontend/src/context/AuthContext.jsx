import { createContext, useContext, useState } from "react";

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const savedUser = localStorage.getItem("user");
    return savedUser ? JSON.parse(savedUser) : null;
  });

  const [selectedTenantId, setSelectedTenantId] = useState(() => {
    return localStorage.getItem("selectedTenantId") || "1";
  });

  const loginUser = (data) => {
    localStorage.setItem("token", data.token);
    localStorage.setItem("user", JSON.stringify(data));
    const tenantId = data.tenantId ? data.tenantId.toString() : "1";
    localStorage.setItem("selectedTenantId", tenantId);
    setSelectedTenantId(tenantId);
    setUser(data);
  };

  const logoutUser = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    localStorage.removeItem("selectedTenantId");
    setUser(null);
    setSelectedTenantId("1");
  };

  const switchTenant = (newTenantId) => {
    const idStr = newTenantId.toString();
    localStorage.setItem("selectedTenantId", idStr);
    setSelectedTenantId(idStr);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        selectedTenantId,
        switchTenant,
        loginUser,
        logoutUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);