using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Server.Data;

public static class DatabaseExtensions
{
    // Creates the SQLite file if missing and applies any pending migrations (schema + seed data).
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}