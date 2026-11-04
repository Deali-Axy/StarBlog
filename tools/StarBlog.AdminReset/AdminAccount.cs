using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;

internal static class AdminAccount {
    internal static void ValidateName(string name) {
        if (string.IsNullOrWhiteSpace(name) || name.Length is < 3 or > 64 || name.Any(char.IsControl))
            throw new ArgumentException("用户名需要 3–64 个字符，不能含控制字符。");
    }

    internal static void Update(SqliteConnection connection, string id, string name, string? password) {
        ValidateName(name);
        if (password is not null && password.Length is < 8 or > 256)
            throw new ArgumentException("密码需要 8–256 个字符。");
        var hash = password is null ? null : new PasswordHasher<object>().HashPassword(new object(), password);
        using var transaction = connection.BeginTransaction();
        using var update = connection.CreateCommand();
        update.Transaction = transaction;
        update.CommandText = "UPDATE user SET name=$name,password=COALESCE($password,password) WHERE id=$id";
        update.Parameters.AddWithValue("$name", name);
        update.Parameters.AddWithValue("$password", (object?)hash ?? DBNull.Value);
        update.Parameters.AddWithValue("$id", id);
        try {
            if (update.ExecuteNonQuery() != 1) throw new InvalidOperationException("用户不存在，取消更新。");
            transaction.Commit();
        }
        catch (SqliteException error) when (error.SqliteErrorCode == 19) {
            throw new InvalidOperationException("新用户名已存在，账号和密码均未修改。", error);
        }
    }
}
