import { useState, useEffect } from 'react';
import './App.css';
import { telemetryApi } from './services/api';
import type { Customer, Device, TelemetryEvent, TelemetryInsights } from './types/telemetry';
import { TelemetryChart } from './components/TelemetryChart';
import { TelemetryTable } from './components/TelemetryTable';
import { InsightsPanel } from './components/InsightsPanel';

function App() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [selectedCustomerId, setSelectedCustomerId] = useState<string>('');
  const [devices, setDevices] = useState<Device[]>([]);
  const [selectedDevice, setSelectedDevice] = useState<Device | null>(null);
  const [telemetryData, setTelemetryData] = useState<TelemetryEvent[]>([]);
  const [insights, setInsights] = useState<TelemetryInsights | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [autoRefresh, setAutoRefresh] = useState(false);

  // Load customers on mount
  useEffect(() => {
    loadCustomers();
  }, []);

  // Load devices when customer changes
  useEffect(() => {
    if (selectedCustomerId) {
      loadDevices(selectedCustomerId);
    } else {
      setDevices([]);
      setSelectedDevice(null);
      setTelemetryData([]);
      setInsights(null);
    }
  }, [selectedCustomerId]);

  // Load telemetry when device changes
  useEffect(() => {
    if (selectedDevice) {
      loadTelemetryData();
    } else {
      setTelemetryData([]);
      setInsights(null);
    }
  }, [selectedDevice]);

  // Auto-refresh telemetry data
  useEffect(() => {
    let interval: number | null = null;
    if (autoRefresh && selectedDevice) {
      interval = window.setInterval(() => {
        loadTelemetryData();
      }, 10000); // Refresh every 10 seconds
    }
    return () => {
      if (interval) window.clearInterval(interval);
    };
  }, [autoRefresh, selectedDevice]);

  const loadCustomers = async () => {
    try {
      const data = await telemetryApi.getCustomers();
      setCustomers(data);
      if (data.length > 0) {
        setSelectedCustomerId(data[0].customerId);
      }
    } catch (err) {
      setError('Failed to load customers');
      console.error(err);
    }
  };

  const loadDevices = async (customerId: string) => {
    try {
      setLoading(true);
      setError(null);
      const data = await telemetryApi.getDevices(customerId);
      setDevices(data);
      setSelectedDevice(null);
    } catch (err) {
      setError('Failed to load devices');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const loadTelemetryData = async () => {
    if (!selectedDevice) return;

    try {
      setLoading(true);
      setError(null);
      const [telemetry, insightsData] = await Promise.all([
        telemetryApi.getTelemetry(selectedDevice.customerId, selectedDevice.deviceId),
        telemetryApi.getInsights(selectedDevice.customerId, selectedDevice.deviceId),
      ]);
      setTelemetryData(telemetry);
      setInsights(insightsData);
    } catch (err) {
      setError('Failed to load telemetry data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeviceSelect = (device: Device) => {
    setSelectedDevice(device);
  };

  return (
    <div className="app">
      <header className="app-header">
        <h1>🔧 Telemetry Slice - Multi-Tenant IoT Platform</h1>
        <p className="subtitle">Device Monitoring Dashboard</p>
      </header>

      <div className="app-content">
        <aside className="sidebar">
          <div className="control-panel">
            <h2>Customer Selection</h2>
            <select
              value={selectedCustomerId}
              onChange={(e) => setSelectedCustomerId(e.target.value)}
              className="customer-select"
            >
              <option value="">-- Select Customer --</option>
              {customers.map((customer) => (
                <option key={customer.customerId} value={customer.customerId}>
                  {customer.name} ({customer.customerId})
                </option>
              ))}
            </select>

            {selectedCustomerId && (
              <>
                <h2>Devices</h2>
                {loading && !selectedDevice ? (
                  <p className="loading">Loading devices...</p>
                ) : devices.length === 0 ? (
                  <p className="no-data">No devices found</p>
                ) : (
                  <ul className="device-list">
                    {devices.map((device) => (
                      <li
                        key={device.deviceId}
                        className={selectedDevice?.deviceId === device.deviceId ? 'active' : ''}
                        onClick={() => handleDeviceSelect(device)}
                      >
                        <strong>{device.label}</strong>
                        <small>{device.location}</small>
                        <small className="device-id">{device.deviceId}</small>
                      </li>
                    ))}
                  </ul>
                )}

                {selectedDevice && (
                  <div className="auto-refresh">
                    <label>
                      <input
                        type="checkbox"
                        checked={autoRefresh}
                        onChange={(e) => setAutoRefresh(e.target.checked)}
                      />
                      Auto-refresh (10s)
                    </label>
                  </div>
                )}
              </>
            )}
          </div>
        </aside>

        <main className="main-content">
          {error && <div className="error-message">{error}</div>}

          {!selectedDevice ? (
            <div className="welcome-message">
              <h2>Welcome to Telemetry Slice</h2>
              <p>Select a customer and device from the sidebar to view telemetry data</p>
              <div className="features">
                <div className="feature">
                  <h3>✓ Multi-Tenant Isolation</h3>
                  <p>Each customer's data is completely isolated</p>
                </div>
                <div className="feature">
                  <h3>✓ Real-Time Insights</h3>
                  <p>View latest readings and statistics</p>
                </div>
                <div className="feature">
                  <h3>✓ Historical Data</h3>
                  <p>Access last 24 hours of telemetry</p>
                </div>
              </div>
            </div>
          ) : (
            <>
              <div className="device-header">
                <h2>{selectedDevice.label}</h2>
                <p>
                  {selectedDevice.location} • {selectedDevice.deviceId}
                </p>
              </div>

              {insights && <InsightsPanel insights={insights} />}

              {loading && telemetryData.length === 0 ? (
                <div className="loading">Loading telemetry data...</div>
              ) : telemetryData.length === 0 ? (
                <div className="no-data">No telemetry data available for the last 24 hours</div>
              ) : (
                <>
                  <TelemetryChart data={telemetryData} unit={insights?.unit || 'C'} />
                  <TelemetryTable data={telemetryData} />
                </>
              )}
            </>
          )}
        </main>
      </div>
    </div>
  );
}

export default App;

