import axios from 'axios';
import type { Customer, Device, TelemetryEvent, TelemetryInsights, TelemetryEventSubmit } from '../types/telemetry';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5177/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const telemetryApi = {
  // Get all customers
  async getCustomers(): Promise<Customer[]> {
    const response = await api.get<Customer[]>('/customers');
    return response.data;
  },

  // Get devices for a customer
  async getDevices(customerId: string): Promise<Device[]> {
    const response = await api.get<Device[]>(`/devices/${customerId}`);
    return response.data;
  },

  // Get telemetry events for a device
  async getTelemetry(customerId: string, deviceId: string, hours: number = 24): Promise<TelemetryEvent[]> {
    const response = await api.get<TelemetryEvent[]>(`/telemetry/${customerId}/${deviceId}`, {
      params: { hours },
    });
    return response.data;
  },

  // Get insights for a device
  async getInsights(customerId: string, deviceId: string, hours: number = 24): Promise<TelemetryInsights> {
    const response = await api.get<TelemetryInsights>(`/telemetry/${customerId}/${deviceId}/insights`, {
      params: { hours },
    });
    return response.data;
  },

  // Submit telemetry event
  async submitTelemetry(event: TelemetryEventSubmit): Promise<{ message: string; eventId: string; isDuplicate: boolean }> {
    const response = await api.post('/telemetry', event);
    return response.data;
  },

  // Health check
  async healthCheck(): Promise<{ status: string; timestamp: string }> {
    const response = await api.get('/health');
    return response.data;
  },
};
