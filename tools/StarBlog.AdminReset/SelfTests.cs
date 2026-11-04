using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;

internal static class SelfTests {
    internal static void Run() {
        using var db = new SqliteConnection("Data Source=:memory:");
        db.Open();
        using var create = db.CreateCommand();
        create.CommandText = "CREATE TABLE user(id TEXT PRIMARY KEY,name TEXT UNIQUE,password TEXT); INSERT INTO user VALUES ('original-id','admin','original-hash'),('other-id','occupied','other-hash');";
        create.ExecuteNonQuery();
        var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));
        AdminAccount.Update(db, "original-id", "new-admin", password);
        var renamed = Read(db);
        Require(renamed.Id == "original-id" && renamed.Name == "new-admin", "Rename must preserve the user ID");
        Require(new PasswordHasher<object>().VerifyHashedPassword(new object(), renamed.Hash, password) != PasswordVerificationResult.Failed,
            "Combined rename and reset must produce a compatible Identity password hash");
        AdminAccount.Update(db, "original-id", "another-admin", null);
        Require(Read(db).Hash == renamed.Hash, "Rename alone must preserve the existing password hash");
        var before = Read(db);
        ExpectRejected(() => AdminAccount.Update(db, "original-id", "occupied", password));
        Require(Read(db) == before, "Duplicate names must leave both fields unchanged");
        ExpectRejected(() => AdminAccount.Update(db, "original-id", "x", password));
        ExpectRejected(() => AdminAccount.Update(db, "original-id", "valid-name", "short"));
        ExpectRejected(() => AdminAccount.Update(db, "missing-id", "valid-name", password));
        Require(Read(db) == before, "Invalid input and missing users must leave the account unchanged");
        var newPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));
        AdminAccount.Update(db, "original-id", before.Name, newPassword);
        Require(Read(db).Name == before.Name, "Reset alone must preserve the username");
        var hasher = new PasswordHasher<object>();
        Require(hasher.VerifyHashedPassword(new object(), Read(db).Hash, newPassword) != PasswordVerificationResult.Failed,
            "The new password must authenticate");
        Require(hasher.VerifyHashedPassword(new object(), Read(db).Hash, password) == PasswordVerificationResult.Failed,
            "The previous password must stop authenticating after a reset");
        Console.WriteLine("Self-tests passed: combined rename/reset, stable ID, rename only, reset only, duplicate rollback and invalid input.");
    }
    private static (string Id, string Name, string Hash) Read(SqliteConnection db) {
        using var command = db.CreateCommand();
        command.CommandText = "SELECT id,name,password FROM user WHERE id='original-id'";
        using var reader = command.ExecuteReader();
        reader.Read();
        return (reader.GetString(0), reader.GetString(1), reader.GetString(2));
    }
    private static void Require(bool condition, string message) {
        if (!condition) throw new Exception(message);
    }
    private static void ExpectRejected(Action action) {
        try { action(); }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException) { return; }
        throw new Exception("Invalid account update was accepted");
    }
}
