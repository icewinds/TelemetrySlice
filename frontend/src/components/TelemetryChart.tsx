import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import type { TelemetryEvent } from '../types/telemetry';

interface TelemetryChartProps {
  data: TelemetryEvent[];
  unit: string;
}

export const TelemetryChart = ({ data, unit }: TelemetryChartProps) => {
  const chartData = data.map((event) => ({
    time: new Date(event.recordedAt).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
    }),
    value: event.value,
    fullTime: new Date(event.recordedAt).toLocaleString(),
  }));

  return (
    <div className="chart-container">
      <h3>Telemetry Chart (Last 24 Hours)</h3>
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="time" />
          <YAxis label={{ value: `Temperature (${unit})`, angle: -90, position: 'insideLeft' }} />
          <Tooltip
            formatter={(value: number | undefined) => 
              value !== undefined ? [`${value.toFixed(2)} ${unit}`, 'Temperature'] : ['N/A', 'Temperature']
            }
            labelFormatter={(label, payload) => {
              if (payload && payload[0]) {
                return payload[0].payload.fullTime;
              }
              return label;
            }}
          />
          <Legend />
          <Line
            type="monotone"
            dataKey="value"
            stroke="#8884d8"
            strokeWidth={2}
            dot={{ r: 3 }}
            name="Temperature"
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
};
