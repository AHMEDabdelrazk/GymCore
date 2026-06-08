import axios from "axios";

const api = axios.create({
    baseURL: "http://localhost:5097/api/members"
});

export const getMembers = () => api.get("");

export const getMember = (id) =>
    api.get(`/${id}`);

export const createMember = (data) =>
    api.post("", data);

export const updateMember = (id, data) =>
    api.put(`/${id}`, data);

export const deactivateMember = (id) =>
    api.patch(`/${id}/deactivate`);

export default api;