// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

var rand = new Random();


var randomCoordinates = GenerateRandomCoordinates(10000000);

// 打印结果
Console.WriteLine(randomCoordinates.Count);
// for (int i = 0; i < randomCoordinates.Count; i++) {
//     Console.WriteLine(
//         $"Point {i + 1}: Longitude = {randomCoordinates[i].Item1}, Latitude = {randomCoordinates[i].Item2}");
// }

List<(double, double)> GenerateRandomCoordinates(int numPoints) {
    // 定义范围
    const double minLongitude = -180.0;
    const double maxLongitude = 180.0;
    const double minLatitude = -90.0;
    const double maxLatitude = 90.0;

    var coordinates = new List<(double, double)>();
    Parallel.For(0, numPoints, i => {
        var lon = rand.NextDouble() * (maxLongitude - minLongitude) + minLongitude;
        var lat = rand.NextDouble() * (maxLatitude - maxLatitude) + minLatitude;
        lock (coordinates) {
            coordinates.Add((lon, lat));
        }
    });
    return coordinates;
}