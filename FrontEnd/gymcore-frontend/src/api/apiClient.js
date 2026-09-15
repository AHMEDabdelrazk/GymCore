import axios from "axios";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL 
  ? `${import.meta.env.VITE_API_BASE_URL.replace(/\/+$/, '')}/api`
  : "http://localhost:5097/api";

const apiClient = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    "Content-Type": "application/json",
  },
});

// Interceptor to attach JWT token and current Tenant ID header
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  const tenantId = localStorage.getItem("selectedTenantId") || "1";
  config.headers["X-Tenant-ID"] = tenantId;

  return config;
});

export default apiClient;
