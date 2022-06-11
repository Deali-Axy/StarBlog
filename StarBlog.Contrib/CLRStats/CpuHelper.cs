using System.Diagnostics;

namespace StarBlog.Contrib.CLRStats;

internal static class CpuHelper {
    private const int interval = 1000;
    private static double _usagePercent;
    private static readonly int _processorCount = Environment.ProcessorCount;
    private static readonly Process process = Process.GetCurrentProcess();

    public static double UsagePercent => _usagePercent;

    static CpuHelper() {
        Task.Factory.StartNew(async () => {
            var _prevCpuTime = process.TotalProcessorTime.TotalMilliseconds;
            while (true) {
                var prevCpuTime = _prevCpuTime;
                var currentCpuTime = process.TotalProcessorTime;
                var usagePercent = (currentCpuTime.TotalMilliseconds - prevCpuTime) / interval / _processorCount * 100;
                Interlocked.Exchange(ref _prevCpuTime, currentCpuTime.TotalMilliseconds);
                Interlocked.Exchange(ref _usagePercent, usagePercent);
                await Task.Delay(interval);
            }
        }, TaskCreationOptions.LongRunning);
    }
}