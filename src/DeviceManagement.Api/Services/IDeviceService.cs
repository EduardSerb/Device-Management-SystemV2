using DeviceManagement.Api.Contracts;

namespace DeviceManagement.Api.Services;

public interface IDeviceService
{
    Task<IReadOnlyList<DeviceListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DeviceDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DeviceDetailDto> CreateAsync(CreateDeviceRequest request, CancellationToken cancellationToken = default);
    Task<DeviceDetailDto?> UpdateAsync(int id, UpdateDeviceRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsDuplicateAsync(string name, string manufacturer, int? excludeId, CancellationToken cancellationToken = default);
}
