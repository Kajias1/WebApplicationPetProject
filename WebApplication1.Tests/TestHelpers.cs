using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Tests;

public static class TestHelpers
{
    public static AppDbContext CreateDbContext()
    {
        // Keep the connection open for the lifetime of the context,
        // since ":memory:" databases disappear when the connection closes.
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();   // creates the schema
        return db;
    }
}