import apiClient from "./apiClient";

export const getUpcomingSessions = (fromDate) => 
  apiClient.get("/classes/sessions", { params: { fromDate } });

export const bookClassSession = (sessionId, memberId) => 
  apiClient.post(`/classes/sessions/${sessionId}/book`, { memberId });

export const cancelClassBooking = (bookingId) => 
  apiClient.post(`/classes/bookings/${bookingId}/cancel`);

export const getSessionRoster = (sessionId) => 
  apiClient.get(`/classes/sessions/${sessionId}/roster`);
