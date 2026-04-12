using DeviceManagement.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceManagement.Api.Services;

public class DeviceAssignmentService : IDeviceAssignmentService
{
    private readonly ApplicationDbContext _db;

    public DeviceAssignmentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(bool Success, string? Error)> AssignToCurrentUserAsync(int deviceId, string userId, CancellationToken cancellationToken = default)
    {
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);
        if (device is null)
            return (false, "not_found");

        if (!string.IsNullOrEmpty(device.AssignedUserId) && device.AssignedUserId != userId)
            return (false, "already_assigned");

        device.AssignedUserId = userId;
        await _db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UnassignIfOwnedAsync(int deviceId, string userId, CancellationToken cancellationToken = default)
    {
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);
        if (device is null)
            return (false, "not_found");

        if (string.IsNullOrEmpty(device.AssignedUserId) || device.AssignedUserId != userId)
            return (false, "not_owner");

        device.AssignedUserId = null;
        await _db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
