import apiClient from "./apiClient";

export const getAuditLogs = (entityName, limit = 50) =>
  apiClient.get("/audit", { params: { entityName, limit } });
