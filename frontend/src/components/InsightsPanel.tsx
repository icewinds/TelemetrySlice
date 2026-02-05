import type { TelemetryInsights } from '../types/telemetry';

interface InsightsPanelProps {
  insights: TelemetryInsights;
}

export const InsightsPanel = ({ insights }: InsightsPanelProps) => {
  return (
    <div className="insights-panel">
      <h3>Insights (Last 24 Hours)</h3>
      <div className="insights-grid">
        <div className="insight-card">
          <div className="insight-label">Latest Reading</div>
          <div className="insight-value">
            {insights.latest !== null ? `${insights.latest.toFixed(2)} ${insights.unit}` : 'N/A'}
          </div>
        </div>
        <div className="insight-card">
          <div className="insight-label">Minimum</div>
          <div className="insight-value">
            {insights.min !== null ? `${insights.min.toFixed(2)} ${insights.unit}` : 'N/A'}
          </div>
        </div>
        <div className="insight-card">
          <div className="insight-label">Average</div>
          <div className="insight-value">
            {insights.average !== null ? `${insights.average.toFixed(2)} ${insights.unit}` : 'N/A'}
          </div>
        </div>
        <div className="insight-card">
          <div className="insight-label">Maximum</div>
          <div className="insight-value">
            {insights.max !== null ? `${insights.max.toFixed(2)} ${insights.unit}` : 'N/A'}
          </div>
        </div>
        <div className="insight-card">
          <div className="insight-label">Total Events</div>
          <div className="insight-value">{insights.count}</div>
        </div>
      </div>
    </div>
  );
};
