import apiClient from "./apiClient";

export const getDashboardMetrics = () => apiClient.get("/dashboardanalytics/metrics");
