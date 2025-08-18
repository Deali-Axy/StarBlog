using System;
using System.Threading;

namespace StarBlog.Contrib.CLRStats;

public class CLRStatsModel {
    private static readonly ServerRequest serverRequest = new ServerRequest();
    private static readonly ApplicationRequest applicationRequest = new ApplicationRequest();

    public ServerRequest Server => serverRequest;
    public ApplicationRequest Application => applicationRequest;
}

public class ApplicationRequest {
    private static readonly CPUStatsRequest cPUStatsRequest = new CPUStatsRequest();
    private static readonly GCStatsRequest gCStatsRequest = new GCStatsRequest();

    public CPUStatsRequest CPU => cPUStatsRequest;

    public GCStatsRequest GC => gCStatsRequest;

    public ThreadStatsRequest Thread => new ThreadStatsRequest();
}

public class ServerRequest {
    public string MachineName => Environment.MachineName;

    public string SystemDateTime => DateTime.Now.ToString();
}

public class CPUStatsRequest {
    public double UsagePercent => CpuHelper.UsagePercent;
}

public class GCStatsRequest {
    public long Gen0CollectCount => GCHelper.Gen0CollectCount;

    public long Gen1CollectCount => GCHelper.Gen1CollectCount;

    public long Gen2CollectCount => GCHelper.Gen2CollectCount;

    public long HeapMemory => GCHelper.TotalMemory;

    public string HeapMemoryFormat => $"{(HeapMemory / (1024 * 1024))} M";

    public bool IsServerGC => System.Runtime.GCSettings.IsServerGC;
}

public class ThreadStatsRequest {
    public ThreadStatsRequest() {
        ThreadPool.GetAvailableThreads(out var availableWorkerThreads, out var availableCompletionPortThreads);
        ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);
        MaxCompletionPortThreads = maxCompletionPortThreads;
        MaxWorkerThreads = maxWorkerThreads;
        AvailableCompletionPortThreads = availableCompletionPortThreads;
        AvailableWorkerThreads = availableWorkerThreads;
    }

    public int AvailableCompletionPortThreads { get; private set; }

    public int AvailableWorkerThreads { get; private set; }

    public int UsedCompletionPortThreads => MaxCompletionPortThreads - AvailableCompletionPortThreads;

    public int UsedWorkerThreads => MaxWorkerThreads - AvailableWorkerThreads;

    public int UsedThreadCount => System.Diagnostics.Process.GetCurrentProcess().Threads.Count;

    public int MaxCompletionPortThreads { get; private set; }

    public int MaxWorkerThreads { get; private set; }
}