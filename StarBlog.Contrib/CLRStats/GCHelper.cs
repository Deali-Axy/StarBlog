namespace StarBlog.Contrib.CLRStats;

internal static class GCHelper {
    private static long _prevGen0CollectCount;

    private static long _prevGen1CollectCount;

    private static long _prevGen2CollectCount;

    private static readonly int _maxGen = GC.MaxGeneration;

    public static long Gen0CollectCount {
        get {
            var count = GC.CollectionCount(0);
            var prevCount = _prevGen0CollectCount;
            Interlocked.Exchange(ref _prevGen0CollectCount, count);
            return count - prevCount;
        }
    }

    public static long Gen1CollectCount {
        get {
            if (_maxGen < 1) {
                return 0;
            }

            var count = GC.CollectionCount(1);
            var prevCount = _prevGen1CollectCount;
            Interlocked.Exchange(ref _prevGen1CollectCount, count);
            return count - prevCount;
        }
    }

    public static long Gen2CollectCount {
        get {
            if (_maxGen < 2) {
                return 0;
            }

            var count = GC.CollectionCount(2);
            var prevCount = _prevGen2CollectCount;
            Interlocked.Exchange(ref _prevGen2CollectCount, count);
            return count - prevCount;
        }
    }

    public static long TotalMemory => GC.GetTotalMemory(false);
}