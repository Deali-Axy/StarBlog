// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
//
// Console.WriteLine("Hello, World!");
//
// var title = "（未修改）在-NET-Core中使用MongoDB明细教程(1)-驱动基础及文档插入";
// var title2 = "在-NET-Core中使用MongoDB明细教程(1)-驱动基础及文档插入";
// var title3 = "自己动手开发简单消息队列（异步任务队列）：Python实现";
//
// const string pattern = @"^（(.+)）(.+)$";
//
// var result = Regex.Match(title, pattern);
//
// foreach (var item in result.Groups) {
//     Console.WriteLine(item);
// }
//
// Console.WriteLine(Regex.Match(title, pattern).Success);
// Console.WriteLine(Regex.Match(title2, pattern).Success);
// Console.WriteLine(Regex.Match(title3, pattern).Success);
//
//
// void TestRe(string content, string pattern) {
//     Console.WriteLine($"{content}, {Regex.IsMatch(content, pattern)}");
// }
//
// const string patternCh = "^((?![a-zA-Z]).)*[\u4e00-\u9fbb]((?![a-zA-Z]).)*$";
//
//
// TestRe("代理", patternCh);
// TestRe("代理hello代理", patternCh);
// TestRe("代理hello", patternCh);


Console.WriteLine(Regex.Replace("1docker 代理 :-!?/haha1", "[^a-zA-Z0-9\\s]+", ""));