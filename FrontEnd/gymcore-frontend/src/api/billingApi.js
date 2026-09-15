import apiClient from "./apiClient";

export const getInvoices = () => apiClient.get("/invoices");

export const createInvoice = (memberId, amount, currency = "USD") =>
  apiClient.post("/invoices", { memberId, amount, currency });

export const simulateStripeWebhook = (memberId, eventType = "invoice.payment_succeeded", amount = 69.99) =>
  apiClient.post("/webhooks/stripe/simulate", { memberId, eventType, amount });
