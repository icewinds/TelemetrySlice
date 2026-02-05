export interface TelemetryEvent {
  eventId: string;
  recordedAt: string;
  receivedAt: string;
  type: string;
  value: number;
  unit: string;
}

export interface Device {
  customerId: string;
  deviceId: string;
  label: string;
  location: string;
}

export interface Customer {
  customerId: string;
  name: string;
}

export interface TelemetryInsights {
  latest: number | null;
  min: number | null;
  average: number | null;
  max: number | null;
  count: number;
  unit: string;
}

export interface TelemetryEventSubmit {
  customerId: string;
  deviceId: string;
  eventId: string;
  recordedAt: string;
  type: string;
  value: number;
  unit: string;
}
