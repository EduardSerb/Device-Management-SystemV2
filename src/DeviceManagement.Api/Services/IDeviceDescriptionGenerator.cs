using DeviceManagement.Api.Models;

namespace DeviceManagement.Api.Services;

public interface IDeviceDescriptionGenerator
{
    Task<string> GenerateAsync(Device device, CancellationToken cancellationToken = default);
}
