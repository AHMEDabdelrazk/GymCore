import apiClient from "./apiClient";

export const getMembers = () => apiClient.get("/members");

export const getMember = (id) => apiClient.get(`/members/${id}`);

export const createMember = (data) => apiClient.post("/members", data);

export const updateMember = (id, data) => apiClient.put(`/members/${id}`, data);

export const deactivateMember = (id) => apiClient.patch(`/members/${id}/deactivate`);

export default apiClient;