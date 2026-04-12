namespace DeviceManagement.Api.Services;

public interface IDeviceAssignmentService
{
    Task<(bool Success, string? Error)> AssignToCurrentUserAsync(int deviceId, string userId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UnassignIfOwnedAsync(int deviceId, string userId, CancellationToken cancellationToken = default);
}
