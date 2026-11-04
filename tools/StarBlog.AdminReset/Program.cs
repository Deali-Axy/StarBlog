using Microsoft.Data.Sqlite;
using System.Net.Http.Json;

if (args.Contains("--self-test")) {
    SelfTests.Run();
    return;
}
if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("请在 Linux API 容器内运行此工具。");
var check = args.Contains("--check");
using var db = new SqliteConnection(new SqliteConnectionStringBuilder {
    DataSource = "/app/data/app.db", Mode = check ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWrite
}.ToString());
db.Open();
using var users = db.CreateCommand();
users.CommandText = "SELECT id,name FROM user ORDER BY name";
var accounts = new Dictionary<string, string>();
using (var reader = users.ExecuteReader()) {
    while (reader.Read()) accounts.Add(reader.GetString(1), reader.GetString(0));
}
Console.WriteLine("管理员账号：" + string.Join(", ", accounts.Keys));
if (check) return;
if (Console.IsInputRedirected || Console.IsOutputRedirected)
    throw new InvalidOperationException("必须在你自己的交互终端中运行，请使用 docker exec -it。");
var defaultName = accounts.Count == 1 ? accounts.Keys.Single() : null;
Console.Write(defaultName is null ? "当前用户名：" : $"当前用户名 [{defaultName}]：");
var username = Console.ReadLine()?.Trim();
if (string.IsNullOrEmpty(username)) username = defaultName;
if (username is null || !accounts.TryGetValue(username, out var id))
    throw new InvalidOperationException("该用户不存在，未修改数据。");
Console.Write($"新的用户名（回车保留 {username}）：");
var newUsername = Console.ReadLine()?.Trim();
if (string.IsNullOrEmpty(newUsername)) newUsername = username;
AdminAccount.ValidateName(newUsername);
using (var existing = db.CreateCommand()) {
    existing.CommandText = "SELECT COUNT(*) FROM user WHERE name=$name AND id<>$id";
    existing.Parameters.AddWithValue("$name", newUsername);
    existing.Parameters.AddWithValue("$id", id);
    if ((long)existing.ExecuteScalar()! != 0) throw new InvalidOperationException("新用户名已存在，未修改数据。");
}
var password = ReadSecret("新密码（8–256 字符；直接回车保留原密码，输入不显示）：");
if (password.Length > 0 && password.Length is < 8 or > 256)
    throw new InvalidOperationException("密码长度不符合要求，未修改数据。");
if (password.Length > 0 && password != ReadSecret("再次输入新密码："))
    throw new InvalidOperationException("两次密码不同，未修改数据。");
if (newUsername == username && password.Length == 0) {
    Console.WriteLine("没有修改账号或密码，已退出。");
    return;
}
var directory = "/app/data/admin-reset-backups";
Directory.CreateDirectory(directory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
File.SetUnixFileMode(directory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
var backupPath = Path.Combine(directory, $"before-reset-{DateTime.UtcNow:yyyyMMddTHHmmssfffZ}-{Guid.NewGuid():N}.db");
using (new FileStream(backupPath, new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.ReadWrite, UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite })) { }
using (var backup = new SqliteConnection($"Data Source={backupPath}")) {
    backup.Open();
    db.BackupDatabase(backup);
}
AdminAccount.Update(db, id, newUsername, password.Length == 0 ? null : password);
Console.WriteLine($"管理员用户名：{newUsername}");
Console.WriteLine("数据库备份：" + backupPath);
if (password.Length > 0) {
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
    try {
        using var response = await client.PostAsJsonAsync("http://127.0.0.1:8080/api/v1/auth/tokens", new { username = newUsername, password });
        Console.WriteLine(response.IsSuccessStatusCode ? "账号更新成功，已通过新用户名和新密码的实际登录验证。" : "账号已更新，但登录验证未通过，请检查 API 状态。");
    }
    catch (HttpRequestException) { Console.WriteLine("账号已更新，但暂时无法连接 API 验证登录。"); }
    catch (TaskCanceledException) { Console.WriteLine("账号已更新，但 API 登录验证超时。"); }
}
else Console.WriteLine("用户名已更新，请使用新用户名和原密码登录。");
Console.WriteLine("后台：https://blog.deali.cn/admin/");

static string ReadSecret(string prompt) {
    Console.Write(prompt);
    var value = new System.Text.StringBuilder();
    for (;;) {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return value.ToString(); }
        if (key.Key == ConsoleKey.Backspace) { if (value.Length > 0) value.Length--; continue; }
        if (!char.IsControl(key.KeyChar) && value.Length < 257) value.Append(key.KeyChar);
    }
}
