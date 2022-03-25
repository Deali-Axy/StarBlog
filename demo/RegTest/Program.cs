// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;

Console.WriteLine("Hello, World!");

var title = "（未修改）在-NET-Core中使用MongoDB明细教程(1)-驱动基础及文档插入";

var result= Regex.Match(title,@"（(.+)）(.+)");

foreach (var item in result.Groups) {
    Console.WriteLine(item);
}