import apiClient from "./apiClient";

export const register = (data) => apiClient.post("/auth/register", data);

export const login = (data) => apiClient.post("/auth/login", data);

export default apiClient;