using DeviceManagement.Api.Contracts;

namespace DeviceManagement.Api.Services;

public interface IDeviceSearchService
{
    Task<IReadOnlyList<DeviceListItemDto>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
