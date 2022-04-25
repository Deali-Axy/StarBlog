// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;

Console.WriteLine("Hello, World!");

var title = "（未修改）在-NET-Core中使用MongoDB明细教程(1)-驱动基础及文档插入";
var title2 = "在-NET-Core中使用MongoDB明细教程(1)-驱动基础及文档插入";
var title3 = "自己动手开发简单消息队列（异步任务队列）：Python实现";

const string pattern = @"^（(.+)）(.+)$";

var result = Regex.Match(title, pattern);

foreach (var item in result.Groups) {
    Console.WriteLine(item);
}

Console.WriteLine(Regex.Match(title, pattern).Success);
Console.WriteLine(Regex.Match(title2, pattern).Success);
Console.WriteLine(Regex.Match(title3, pattern).Success);
