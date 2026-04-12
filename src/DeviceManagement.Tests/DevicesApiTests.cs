using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DeviceManagement.Api.Contracts;
using DeviceManagement.Api.Models;

namespace DeviceManagement.Tests;

public class DevicesApiTests : IClassFixture<DeviceManagementApiFactory>
{
    private readonly HttpClient _client;

    public DevicesApiTests(DeviceManagementApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Devices_require_authentication()
    {
        var response = await _client.GetAsync("/api/devices");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Crud_and_search_flow_works()
    {
        var email = $"test-{Guid.NewGuid():N}@test.local";
        var register = new RegisterRequest(email, "Passw0rd!", "Test User", "QA", "Remote");
        var reg = await _client.PostAsJsonAsync("/api/auth/register", register);
        reg.EnsureSuccessStatusCode();

        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Passw0rd!"));
        login.EnsureSuccessStatusCode();
        var loginBody = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Token);

        var create = new CreateDeviceRequest
        {
            Name = "Test Phone",
            Manufacturer = "Acme",
            Type = DeviceType.Phone,
            OS = "Android",
            OSVersion = "15",
            Processor = "Test CPU",
            RamAmount = "8GB",
            Description = "Unit test device"
        };

        var createdResponse = await _client.PostAsJsonAsync("/api/devices", create);
        createdResponse.EnsureSuccessStatusCode();
        var created = await createdResponse.Content.ReadFromJsonAsync<DeviceDetailDto>();
        Assert.NotNull(created);
        Assert.Equal("Test Phone", created.Name);

        var byId = await _client.GetFromJsonAsync<DeviceDetailDto>($"/api/devices/{created.Id}");
        Assert.NotNull(byId);
        Assert.Equal("Acme", byId.Manufacturer);

        var list = await _client.GetFromJsonAsync<List<DeviceListItemDto>>("/api/devices");
        Assert.NotNull(list);
        Assert.Contains(list, d => d.Id == created.Id);

        create.Description = "Updated";
        var updated = await _client.PutAsJsonAsync($"/api/devices/{created.Id}", new UpdateDeviceRequest
        {
            Name = create.Name,
            Manufacturer = create.Manufacturer,
            Type = create.Type,
            OS = create.OS,
            OSVersion = create.OSVersion,
            Processor = create.Processor,
            RamAmount = create.RamAmount,
            Description = create.Description
        });
        updated.EnsureSuccessStatusCode();

        var search = await _client.GetFromJsonAsync<List<DeviceListItemDto>>("/api/devices/search?q=acme%20test");
        Assert.NotNull(search);
        Assert.Contains(search, d => d.Id == created.Id);

        var deleted = await _client.DeleteAsync($"/api/devices/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var missing = await _client.GetAsync($"/api/devices/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }
}
