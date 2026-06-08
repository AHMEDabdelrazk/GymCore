import axios from "axios";

const api = axios.create({
    baseURL: "http://localhost:5097/api/auth"
});

export const register = (data) =>
    api.post("/register", data);

export const login = (data) =>
    api.post("/login", data);

export default api;