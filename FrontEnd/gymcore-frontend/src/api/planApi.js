import apiClient from "./apiClient";

export const getPlans = () => apiClient.get("/plans");

export const getPlan = (id) => apiClient.get(`/plans/${id}`);

export const createPlan = (data) => apiClient.post("/plans", data);

export const updatePlan = (id, data) => apiClient.put(`/plans/${id}`, data);

export const deactivatePlan = (id) => apiClient.patch(`/plans/${id}/deactivate`);

export default apiClient;