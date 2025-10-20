using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSuiteViewWPF.Services
{
    /// <summary>
    /// Monitors and reports performance metrics for the filtering system.
    /// Useful for debugging and ensuring sub-100ms filter performance.
    /// </summary>
    public class FilterPerformanceMonitor
    {
        private readonly List<PerformanceMetric> _metrics;
        private readonly int _maxMetricsToKeep;

        public FilterPerformanceMonitor(int maxMetricsToKeep = 100)
        {
            _metrics = new List<PerformanceMetric>();
            _maxMetricsToKeep = maxMetricsToKeep;
        }

        /// <summary>
        /// Records a performance metric
        /// </summary>
        public void RecordMetric(string operation, long durationMs, int rowCount, string? details = null)
        {
            var metric = new PerformanceMetric
            {
                Timestamp = DateTime.Now,
                Operation = operation,
                DurationMs = durationMs,
                RowCount = rowCount,
                Details = details
            };

            _metrics.Add(metric);

            // Keep only the most recent metrics
            if (_metrics.Count > _maxMetricsToKeep)
            {
                _metrics.RemoveAt(0);
            }

            // Log slow operations
            if (durationMs > 100)
            {
                Debug.WriteLine($"[PERFORMANCE WARNING] {operation} took {durationMs}ms (target: <100ms) - {details}");
            }
        }

        /// <summary>
        /// Gets statistics for a specific operation type
        /// </summary>
        public OperationStats GetStats(string operation)
        {
            var operationMetrics = _metrics.Where(m => m.Operation == operation).ToList();

            if (!operationMetrics.Any())
            {
                return new OperationStats { Operation = operation };
            }

            return new OperationStats
            {
                Operation = operation,
                Count = operationMetrics.Count,
                AverageDurationMs = operationMetrics.Average(m => m.DurationMs),
                MinDurationMs = operationMetrics.Min(m => m.DurationMs),
                MaxDurationMs = operationMetrics.Max(m => m.DurationMs),
                AverageRowCount = (int)operationMetrics.Average(m => m.RowCount)
            };
        }

        /// <summary>
        /// Gets all recent metrics
        /// </summary>
        public IReadOnlyList<PerformanceMetric> GetRecentMetrics(int count = 10)
        {
            return _metrics.TakeLast(count).ToList().AsReadOnly();
        }

        /// <summary>
        /// Clears all recorded metrics
        /// </summary>
        public void Clear()
        {
            _metrics.Clear();
        }

        /// <summary>
        /// Generates a performance report
        /// </summary>
        public string GenerateReport()
        {
            if (!_metrics.Any())
                return "No performance metrics recorded.";

            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Filter Performance Report ===");
            report.AppendLine($"Total operations: {_metrics.Count}");
            report.AppendLine();

            var operationTypes = _metrics.Select(m => m.Operation).Distinct().ToList();

            foreach (var operation in operationTypes)
            {
                var stats = GetStats(operation);
                report.AppendLine($"Operation: {operation}");
                report.AppendLine($"  Count: {stats.Count}");
                report.AppendLine($"  Average: {stats.AverageDurationMs:F2}ms");
                report.AppendLine($"  Min: {stats.MinDurationMs}ms");
                report.AppendLine($"  Max: {stats.MaxDurationMs}ms");
                report.AppendLine($"  Avg Rows: {stats.AverageRowCount:N0}");
                report.AppendLine();
            }

            return report.ToString();
        }
    }

    /// <summary>
    /// Represents a single performance metric
    /// </summary>
    public class PerformanceMetric
    {
        public DateTime Timestamp { get; set; }
        public string Operation { get; set; } = string.Empty;
        public long DurationMs { get; set; }
        public int RowCount { get; set; }
        public string? Details { get; set; }
    }

    /// <summary>
    /// Statistical summary for an operation type
    /// </summary>
    public class OperationStats
    {
        public string Operation { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AverageDurationMs { get; set; }
        public long MinDurationMs { get; set; }
        public long MaxDurationMs { get; set; }
        public int AverageRowCount { get; set; }
    }
}
