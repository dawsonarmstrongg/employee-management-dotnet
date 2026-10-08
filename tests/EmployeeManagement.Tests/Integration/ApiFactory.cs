using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace EmployeeManagement.Tests.Integration;

// WebApplicationFactory<Program> boots the real app (Program.cs), including EF Core migrations
// and seed data, against a private temporary SQLite file so tests never touch the developer's
// employees.db. Each test class gets its own factory instance (see IClassFixture usage), so
// test classes are isolated from each other and can run in parallel.
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public string DbPath { get; } = Path.Combine(Path.GetTempPath(), $"qa-employees-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            // Appended after appsettings.json, so this override wins.
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Pooling=False releases the file when each connection closes. With pooling on, an idle
                // pooled connection keeps it locked and the cleanup in Dispose can't delete it.
                ["ConnectionStrings:EmployeeDb"] = $"Data Source={DbPath};Pooling=False",
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
        {
            return;
        }

        TryDelete(DbPath);
        TryDelete(DbPath + "-shm");
        TryDelete(DbPath + "-wal");
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup only; a locked temp file is not worth failing the test run over.
        }
    }
}
