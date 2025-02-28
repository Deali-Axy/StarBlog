using Dumpify;
using ParallelTest;

Console.WriteLine("Hello, World!");

Console.WriteLine("生成随机坐标");
var coordinates = GenerateRandomCoordinates(20000);
var stores = coordinates.Select(e => new Store($"store-{Guid.NewGuid()}", e)).ToList();
var kdTree = new KdTree<Store>();
kdTree.BuildTreeParallel2(stores);

FindNode(kdTree, stores[0]);

return;

void FindNode(KdTree<Store> tree, Store item) {
    Console.WriteLine($"查找 {item.Name} ，坐标 lng={item.Location.Lng}, lat={item.Location.Lat}");
    var node = tree.FindNearestNode(item.Location);
    if (node == null) {
        Console.WriteLine("找不到");
        return;
    }

    Console.WriteLine($"找到最近node，{node.Value.Name}, lng={node.Value.Location.Lng}, lat={node.Value.Location.Lat} ");
}

List<Location> GenerateRandomCoordinates(int numPoints) {
    var rand = new Random();

    // 定义范围
    const double minLongitude = -180.0;
    const double maxLongitude = 180.0;
    const double minLatitude = -90.0;
    const double maxLatitude = 90.0;

    var data = new List<Location>();

    Parallel.For(0, numPoints, i => {
        var lon = rand.NextDouble() * (maxLongitude - minLongitude) + minLongitude;
        var lat = rand.NextDouble() * (maxLatitude - maxLatitude) + minLatitude;
        lock (data) {
            data.Add(new Location(lon, lat));
        }
    });
    return data;
}

internal record Location(double Lng, double Lat) : ILocation;

internal record Store(string Name, ILocation Location) : ILocationNode;