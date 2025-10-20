using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CSuiteViewWPF.Services
{
    /// <summary>
    /// File-based performance logger for tracking filter operations in Release builds.
    /// Logs to C:\Temp\CSuiteView_FilterPerformance.log for debugging performance issues.
    /// </summary>
    public class FilterPerformanceLogger
    {
        private static readonly string LogFilePath = @"C:\Temp\CSuiteView_FilterPerformance.log";
        private static readonly object _lock = new object();
        private static bool _isEnabled = true;

        /// <summary>
        /// Enable or disable logging (useful for production)
        /// </summary>
        public static bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        /// <summary>
        /// Initializes the log file (clears previous session)
        /// </summary>
        public static void Initialize()
        {
            if (!_isEnabled) return;

            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(LogFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create new log file with header
                lock (_lock)
                {
                    File.WriteAllText(LogFilePath,
                        $"=== CSuiteView Filter Performance Log ===\n" +
                        $"Session started: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\n" +
                        $"========================================\n\n");
                }
            }
            catch
            {
                // Silently fail if we can't write to log (don't crash the app)
                _isEnabled = false;
            }
        }

        /// <summary>
        /// Logs a performance metric with timing information
        /// </summary>
        public static void Log(string operation, long durationMs, string details = "")
        {
            if (!_isEnabled) return;

            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var message = string.IsNullOrEmpty(details)
                    ? $"[{timestamp}] {operation}: {durationMs}ms\n"
                    : $"[{timestamp}] {operation}: {durationMs}ms - {details}\n";

                lock (_lock)
                {
                    File.AppendAllText(LogFilePath, message);
                }

                // Also write to Debug output if debugger is attached
                if (Debugger.IsAttached)
                {
                    Debug.WriteLine($"[PERF] {operation}: {durationMs}ms - {details}");
                }
            }
            catch
            {
                // Silently fail
            }
        }

        /// <summary>
        /// Logs a performance metric with row count
        /// </summary>
        public static void LogWithRowCount(string operation, long durationMs, int rowCount, string additionalDetails = "")
        {
            var details = $"{rowCount:N0} rows";
            if (!string.IsNullOrEmpty(additionalDetails))
            {
                details += $", {additionalDetails}";
            }
            Log(operation, durationMs, details);
        }

        /// <summary>
        /// Logs a warning for slow operations
        /// </summary>
        public static void LogWarning(string operation, long durationMs, long thresholdMs, string details = "")
        {
            if (!_isEnabled) return;

            if (durationMs > thresholdMs)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var message = $"[{timestamp}] ⚠️ WARNING: {operation} took {durationMs}ms (threshold: {thresholdMs}ms) - {details}\n";

                try
                {
                    lock (_lock)
                    {
                        File.AppendAllText(LogFilePath, message);
                    }

                    if (Debugger.IsAttached)
                    {
                        Debug.WriteLine($"[PERF WARNING] {operation}: {durationMs}ms (>{thresholdMs}ms) - {details}");
                    }
                }
                catch
                {
                    // Silently fail
                }
            }
        }

        /// <summary>
        /// Logs a section header for better organization
        /// </summary>
        public static void LogSection(string sectionName)
        {
            if (!_isEnabled) return;

            try
            {
                var message = $"\n--- {sectionName} ---\n";
                lock (_lock)
                {
                    File.AppendAllText(LogFilePath, message);
                }
            }
            catch
            {
                // Silently fail
            }
        }

        /// <summary>
        /// Gets the full log file path
        /// </summary>
        public static string GetLogFilePath()
        {
            return LogFilePath;
        }

        /// <summary>
        /// Reads and returns the entire log contents
        /// </summary>
        public static string ReadLog()
        {
            if (!File.Exists(LogFilePath))
                return "Log file not found. Performance logging may be disabled or no operations have been logged yet.";

            try
            {
                lock (_lock)
                {
                    return File.ReadAllText(LogFilePath);
                }
            }
            catch (Exception ex)
            {
                return $"Error reading log file: {ex.Message}";
            }
        }

        /// <summary>
        /// Clears the current log file
        /// </summary>
        public static void ClearLog()
        {
            if (!_isEnabled) return;

            try
            {
                lock (_lock)
                {
                    if (File.Exists(LogFilePath))
                    {
                        File.Delete(LogFilePath);
                    }
                    Initialize();
                }
            }
            catch
            {
                // Silently fail
            }
        }

        /// <summary>
        /// Helper class for timing operations with automatic logging
        /// </summary>
        public class Timer : IDisposable
        {
            private readonly Stopwatch _stopwatch;
            private readonly string _operation;
            private readonly string _details;
            private readonly long _warningThresholdMs;

            public Timer(string operation, string details = "", long warningThresholdMs = -1)
            {
                _operation = operation;
                _details = details;
                _warningThresholdMs = warningThresholdMs;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _stopwatch.Stop();

                if (_warningThresholdMs > 0)
                {
                    LogWarning(_operation, _stopwatch.ElapsedMilliseconds, _warningThresholdMs, _details);
                }
                else
                {
                    Log(_operation, _stopwatch.ElapsedMilliseconds, _details);
                }
            }

            public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;
        }
    }
}
