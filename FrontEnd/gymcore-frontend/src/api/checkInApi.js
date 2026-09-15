import apiClient from "./apiClient";

export const processCheckIn = (memberCodeOrId, accessMethod = "Kiosk_QR") =>
  apiClient.post("/checkin", { memberCodeOrId, accessMethod });

export const getRecentCheckIns = (limit = 50) =>
  apiClient.get("/checkin/recent", { params: { limit } });
