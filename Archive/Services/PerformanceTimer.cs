using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSuiteViewWPF.Services
{
    /// <summary>
    /// Utility class for measuring and logging performance of filter operations.
    /// Tracks timing statistics and helps identify performance bottlenecks.
    /// </summary>
    public class PerformanceTimer : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _operationName;
        private readonly Action<PerformanceResult>? _onComplete;
        private static readonly List<PerformanceResult> _history = new List<PerformanceResult>();
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Creates a new performance timer for the specified operation
        /// </summary>
        /// <param name="operationName">Name of the operation being timed</param>
        /// <param name="onComplete">Optional callback when timing completes</param>
        public PerformanceTimer(string operationName, Action<PerformanceResult>? onComplete = null)
        {
            _operationName = operationName ?? "Unknown Operation";
            _onComplete = onComplete;
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Stops the timer and records the result
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();
            
            var result = new PerformanceResult
            {
                OperationName = _operationName,
                ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds,
                Timestamp = DateTime.Now
            };

            // Add to history
            lock (_lockObject)
            {
                _history.Add(result);
                
                // Keep only last 1000 entries
                if (_history.Count > 1000)
                {
                    _history.RemoveAt(0);
                }
            }

            // Log to debug output
            Debug.WriteLine($"[PERF] {_operationName}: {_stopwatch.ElapsedMilliseconds}ms");

            // Call completion callback
            _onComplete?.Invoke(result);
        }

        /// <summary>
        /// Creates a timer and automatically starts it
        /// </summary>
        public static PerformanceTimer Start(string operationName, Action<PerformanceResult>? onComplete = null)
        {
            return new PerformanceTimer(operationName, onComplete);
        }

        /// <summary>
        /// Gets all performance history
        /// </summary>
        public static IReadOnlyList<PerformanceResult> GetHistory()
        {
            lock (_lockObject)
            {
                return _history.ToList();
            }
        }

        /// <summary>
        /// Gets performance statistics for a specific operation
        /// </summary>
        public static PerformanceStats GetStats(string operationName)
        {
            lock (_lockObject)
            {
                var results = _history.Where(r => r.OperationName == operationName).ToList();
                
                if (results.Count == 0)
                {
                    return new PerformanceStats
                    {
                        OperationName = operationName,
                        Count = 0,
                        AverageMs = 0,
                        MinMs = 0,
                        MaxMs = 0,
                        MedianMs = 0
                    };
                }

                var times = results.Select(r => r.ElapsedMilliseconds).OrderBy(t => t).ToList();

                return new PerformanceStats
                {
                    OperationName = operationName,
                    Count = results.Count,
                    AverageMs = times.Average(),
                    MinMs = times.Min(),
                    MaxMs = times.Max(),
                    MedianMs = times[times.Count / 2]
                };
            }
        }

        /// <summary>
        /// Gets a summary of all operations
        /// </summary>
        public static string GetSummary()
        {
            lock (_lockObject)
            {
                if (_history.Count == 0)
                {
                    return "No performance data collected yet.";
                }

                var grouped = _history.GroupBy(r => r.OperationName);
                var summary = new System.Text.StringBuilder();
                
                summary.AppendLine("Performance Summary:");
                summary.AppendLine("===================");
                
                foreach (var group in grouped.OrderBy(g => g.Key))
                {
                    var stats = GetStats(group.Key);
                    summary.AppendLine($"\n{stats.OperationName}:");
                    summary.AppendLine($"  Count: {stats.Count}");
                    summary.AppendLine($"  Average: {stats.AverageMs:F2}ms");
                    summary.AppendLine($"  Min: {stats.MinMs}ms");
                    summary.AppendLine($"  Max: {stats.MaxMs}ms");
                    summary.AppendLine($"  Median: {stats.MedianMs}ms");
                }

                return summary.ToString();
            }
        }

        /// <summary>
        /// Clears all performance history
        /// </summary>
        public static void ClearHistory()
        {
            lock (_lockObject)
            {
                _history.Clear();
            }
        }

        /// <summary>
        /// Gets recent performance results (last N operations)
        /// </summary>
        public static IReadOnlyList<PerformanceResult> GetRecent(int count = 10)
        {
            lock (_lockObject)
            {
                return _history.TakeLast(count).ToList();
            }
        }
    }

    /// <summary>
    /// Represents the result of a single performance measurement
    /// </summary>
    public class PerformanceResult
    {
        public string OperationName { get; set; } = string.Empty;
        public long ElapsedMilliseconds { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"{OperationName}: {ElapsedMilliseconds}ms at {Timestamp:HH:mm:ss.fff}";
        }
    }

    /// <summary>
    /// Statistical summary of performance measurements for an operation
    /// </summary>
    public class PerformanceStats
    {
        public string OperationName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AverageMs { get; set; }
        public long MinMs { get; set; }
        public long MaxMs { get; set; }
        public long MedianMs { get; set; }

        /// <summary>
        /// Returns true if average performance is within acceptable limits (<100ms for filters)
        /// </summary>
        public bool IsAcceptable => AverageMs < 100;

        public override string ToString()
        {
            return $"{OperationName}: Avg={AverageMs:F2}ms, Min={MinMs}ms, Max={MaxMs}ms (n={Count})";
        }
    }

    /// <summary>
    /// Extension methods for easy performance timing with using statements
    /// </summary>
    public static class PerformanceTimerExtensions
    {
        /// <summary>
        /// Times an action and returns the elapsed milliseconds
        /// </summary>
        public static long TimeAction(string operationName, Action action)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            
            Debug.WriteLine($"[PERF] {operationName}: {sw.ElapsedMilliseconds}ms");
            
            return sw.ElapsedMilliseconds;
        }

        /// <summary>
        /// Times a function and returns both the result and elapsed milliseconds
        /// </summary>
        public static (T result, long elapsedMs) TimeFunc<T>(string operationName, Func<T> func)
        {
            var sw = Stopwatch.StartNew();
            var result = func();
            sw.Stop();
            
            Debug.WriteLine($"[PERF] {operationName}: {sw.ElapsedMilliseconds}ms");
            
            return (result, sw.ElapsedMilliseconds);
        }
    }
}
