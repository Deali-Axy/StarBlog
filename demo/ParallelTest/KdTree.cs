using System.Diagnostics;

namespace ParallelTest;

public interface ILocation {
    public double Lng { get; init; }
    public double Lat { get; init; }
}

public interface ILocationNode {
    public ILocation Location { get; init; }
}

public class KdTreeNode<T> where T : ILocationNode {
    public T Value { get; set; }
    public KdTreeNode<T>? Left { get; set; }
    public KdTreeNode<T>? Right { get; set; }
}

/// <summary>
/// k-dimensional tree
/// </summary>
/// <typeparam name="T"></typeparam>
public class KdTree<T> where T : ILocationNode {
    private readonly IProgress<string> _progress = new Progress<string>(Console.WriteLine);
    private KdTreeNode<T>? _root;

    public void BuildTreeParallel2(List<T> items) {
        var sw = new Stopwatch();
        sw.Start();
        Console.WriteLine("Building KD tree...");
        _root = new KdTreeNode<T>();
        BuildTreeParallel2(_root, items, 0, items.Count - 1, 0);
        sw.Stop();
        Console.WriteLine($"KD tree construction complete. 耗时: {sw.Elapsed}");
    }

    private void BuildTreeParallel2(KdTreeNode<T> node, List<T> items, int start, int end, int depth) {
        if (start > end) return;

        var axis = depth % 2; // Assuming latitude and longitude as 2D coordinates

        // 复制一份新的 items 列表
        var sortedItems = items.ToList();

        sortedItems.Sort((a, b) =>
            axis == 0 ? a.Location.Lng.CompareTo(b.Location.Lng) : a.Location.Lat.CompareTo(b.Location.Lat));

        var medianIndex = start + (end - start) / 2;
        node.Value = sortedItems[medianIndex];

        if (start < medianIndex) {
            node.Left = new KdTreeNode<T>();
        }
        
        if (medianIndex < end) {
            node.Right = new KdTreeNode<T>();
        }
        
        Parallel.Invoke(
            () => {
                if (node.Left != null) {
                    BuildTreeParallel2(node.Left, sortedItems, start, medianIndex - 1, depth + 1);
                }
            },
            () => {
                if (node.Right != null) {
                    BuildTreeParallel2(node.Right, sortedItems, medianIndex + 1, end, depth + 1);
                }
            }
        );
    }

    public void BuildTreeParallel(List<T> items) {
        var sw = new Stopwatch();
        sw.Start();
        Console.WriteLine("Building KD tree...");
        _root = BuildTreeParallel(items, 0, items.Count - 1, 0);
        sw.Stop();
        Console.WriteLine($"KD tree construction complete. 耗时: {sw.Elapsed}");
    }

    private KdTreeNode<T>? BuildTreeParallel(List<T> items, int start, int end, int depth) {
        if (start > end)
            return null;

        var axis = depth % 2; // Assuming latitude and longitude as 2D coordinates

        // 复制一份新的 items 列表
        var sortedItems = items.ToList();

        sortedItems.Sort((a, b) => axis == 0
            ? a.Location.Lng.CompareTo(b.Location.Lng)
            : a.Location.Lat.CompareTo(b.Location.Lat));

        var medianIndex = start + (end - start) / 2;
        var node = new KdTreeNode<T> {
            Value = sortedItems[medianIndex]
        };

        var leftTask = Task.Run(() => BuildTreeParallel(sortedItems, start, medianIndex - 1, depth + 1));
        var rightTask = Task.Run(() => BuildTreeParallel(sortedItems, medianIndex + 1, end, depth + 1));

        node.Left = leftTask.Result;
        node.Right = rightTask.Result;

        return node;
    }

    public void BuildTree(List<T> items) {
        var sw = new Stopwatch();
        sw.Start();
        Console.WriteLine("Building KD tree...");
        _root = BuildTree(items, 0, items.Count - 1, 0);
        sw.Stop();
        Console.WriteLine($"KD tree construction complete. 耗时: {sw.Elapsed}");
    }

    private KdTreeNode<T>? BuildTree(List<T> items, int start, int end, int depth) {
        if (start > end)
            return null;

        var axis = depth % 2; // Assuming latitude and longitude as 2D coordinates


        items.Sort((a, b) => axis == 0
            ? a.Location.Lng.CompareTo(b.Location.Lng)
            : a.Location.Lat.CompareTo(b.Location.Lat)
        );

        var medianIndex = start + (end - start) / 2;
        var node = new KdTreeNode<T> {
            Value = items[medianIndex],
            Left = BuildTree(items, start, medianIndex - 1, depth + 1),
            Right = BuildTree(items, medianIndex + 1, end, depth + 1)
        };

        // 进度显示
        // _progress.Report($"Splitting on depth {depth}, axis {axis}, median index {medianIndex}");

        return node;
    }

    public KdTreeNode<T>? FindNearestNode(ILocation location) {
        return FindNearestNode(_root, location, 0, null);
    }

    private KdTreeNode<T>? FindNearestNode(KdTreeNode<T>? node, ILocation location, int depth, KdTreeNode<T>? best) {
        if (node == null) return best;

        var bestDistance = best != null
            ? DistanceCalculator.CalculateDistance(location, best.Value.Location)
            : double.PositiveInfinity;
        var currentNodeDistance = DistanceCalculator.CalculateDistance(location, node.Value.Location);

        if (currentNodeDistance < bestDistance)
            best = node;

        var axis = depth % 2;
        var axisDistance =
            axis == 0 ? location.Lng - node.Value.Location.Lng : location.Lat - node.Value.Location.Lat;

        var nearChild = axisDistance < 0 ? node.Left : node.Right;
        var farChild = axisDistance < 0 ? node.Right : node.Left;

        var nearest = FindNearestNode(nearChild, location, depth + 1, best);

        if (nearest != null) {
            var nearestDistance = DistanceCalculator.CalculateDistance(location, nearest.Value.Location);
            if (nearestDistance < bestDistance)
                best = nearest;
        }

        if (Math.Abs(axisDistance) < bestDistance) {
            var farthest = FindNearestNode(farChild, location, depth + 1, best);
            if (farthest != null) {
                var farthestDistance = DistanceCalculator.CalculateDistance(location, farthest.Value.Location);
                if (farthestDistance < bestDistance)
                    best = farthest;
            }
        }

        return best;
    }

    public KdTreeNode<T>? FindNearestNodeParallel(ILocation location) {
        return FindNearestNodeParallel(_root, location, 0, null);
    }

    private KdTreeNode<T>? FindNearestNodeParallel(KdTreeNode<T>? node, ILocation location, int depth,
        KdTreeNode<T>? best) {
        if (node == null) return best;

        var bestDistance = best != null
            ? DistanceCalculator.CalculateDistance(location, best.Value.Location)
            : double.PositiveInfinity;
        var currentNodeDistance = DistanceCalculator.CalculateDistance(location, node.Value.Location);

        if (currentNodeDistance < bestDistance)
            best = node;

        var axis = depth % 2;
        var axisDistance =
            axis == 0 ? location.Lng - node.Value.Location.Lng : location.Lat - node.Value.Location.Lat;

        var nearChild = axisDistance < 0 ? node.Left : node.Right;
        var farChild = axisDistance < 0 ? node.Right : node.Left;

        var nearestTask = Task.Run(() => FindNearestNodeParallel(nearChild, location, depth + 1, best));
        var nearest = nearestTask.Result;

        if (nearest != null) {
            var nearestDistance = DistanceCalculator.CalculateDistance(location, nearest.Value.Location);
            if (nearestDistance < bestDistance)
                best = nearest;
        }

        if (Math.Abs(axisDistance) < bestDistance) {
            var farthestTask = Task.Run(() => FindNearestNodeParallel(farChild, location, depth + 1, best));
            var farthest = farthestTask.Result;
            if (farthest != null) {
                var farthestDistance = DistanceCalculator.CalculateDistance(location, farthest.Value.Location);
                if (farthestDistance < bestDistance)
                    best = farthest;
            }
        }

        return best;
    }
}

public static class DistanceCalculator {
    // Radius of the Earth in kilometers
    private const double EarthRadius = 6371;

    /// <summary>
    /// 使用 Haversine 公式计算两点之间距离（以公里为单位）
    /// </summary>
    /// <param name="location1"></param>
    /// <param name="location2"></param>
    /// <returns></returns>
    public static double CalculateDistance(ILocation location1, ILocation location2) {
        var lat1 = DegreeToRadian(location1.Lat);
        var lon1 = DegreeToRadian(location1.Lng);
        var lat2 = DegreeToRadian(location2.Lat);
        var lon2 = DegreeToRadian(location2.Lng);

        var dlon = lon2 - lon1;
        var dlat = lat2 - lat1;

        var a = Math.Pow(Math.Sin(dlat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        var distance = EarthRadius * c;
        return distance;
    }

    private static double DegreeToRadian(double degree) {
        return degree * Math.PI / 180;
    }
}