import type { TelemetryEvent } from '../types/telemetry';

interface TelemetryTableProps {
  data: TelemetryEvent[];
}

export const TelemetryTable = ({ data }: TelemetryTableProps) => {
  // Sort by recorded time, most recent first
  const sortedData = [...data].sort(
    (a, b) => new Date(b.recordedAt).getTime() - new Date(a.recordedAt).getTime()
  );

  return (
    <div className="table-container">
      <h3>Telemetry Events</h3>
      <div className="table-wrapper">
        <table className="telemetry-table">
          <thead>
            <tr>
              <th>Recorded At</th>
              <th>Type</th>
              <th>Value</th>
              <th>Event ID</th>
              <th>Received At</th>
            </tr>
          </thead>
          <tbody>
            {sortedData.map((event) => (
              <tr key={event.eventId}>
                <td>{new Date(event.recordedAt).toLocaleString()}</td>
                <td>
                  <span className="badge">{event.type}</span>
                </td>
                <td>
                  <strong>
                    {event.value.toFixed(2)} {event.unit}
                  </strong>
                </td>
                <td>
                  <code>{event.eventId}</code>
                </td>
                <td className="secondary-text">{new Date(event.receivedAt).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
