import axios from "axios";

const api = axios.create({
    baseURL: "http://localhost:5097/api/plans"
});

export const getPlans = () => api.get("");

export const getPlan = (id) => api.get(`/${id}`);

export const createPlan = (data) => api.post("", data);

export const updatePlan = (id, data) =>
    api.put(`/${id}`, data);

export const deactivatePlan = (id) =>
    api.patch(`/${id}/deactivate`);

export default api;