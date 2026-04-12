using DeviceManagement.Api.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DeviceManagement.Tests;

public sealed class DeviceManagementApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(DatabaseSeeder.IntegrationTestEnvironment);
    }
}
