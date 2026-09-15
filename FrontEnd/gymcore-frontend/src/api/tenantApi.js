import apiClient from "./apiClient";

export const getTenants = () => apiClient.get("/tenants");
export const getTenantById = (id) => apiClient.get(`/tenants/${id}`);
