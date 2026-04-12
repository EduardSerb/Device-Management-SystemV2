using DeviceManagement.Api.Contracts;
using DeviceManagement.Api.Data;
using DeviceManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceManagement.Api.Services;

public class DeviceService : IDeviceService
{
    private readonly ApplicationDbContext _db;

    public DeviceService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DeviceListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var devices = await _db.Devices
            .AsNoTracking()
            .Include(d => d.AssignedUser)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        return devices.Select(MapListItem).ToList();
    }

    public async Task<DeviceDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var device = await _db.Devices
            .AsNoTracking()
            .Include(d => d.AssignedUser)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        return device is null ? null : MapDetail(device);
    }

    public async Task<DeviceDetailDto> CreateAsync(CreateDeviceRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Device
        {
            Name = request.Name.Trim(),
            Manufacturer = request.Manufacturer.Trim(),
            Type = request.Type,
            OS = request.OS.Trim(),
            OSVersion = request.OSVersion.Trim(),
            Processor = request.Processor.Trim(),
            RamAmount = request.RamAmount.Trim(),
            Description = request.Description.Trim()
        };

        _db.Devices.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var created = await _db.Devices
            .AsNoTracking()
            .Include(d => d.AssignedUser)
            .FirstAsync(d => d.Id == entity.Id, cancellationToken);

        return MapDetail(created);
    }

    public async Task<DeviceDetailDto?> UpdateAsync(int id, UpdateDeviceRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Devices.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (entity is null)
            return null;

        entity.Name = request.Name.Trim();
        entity.Manufacturer = request.Manufacturer.Trim();
        entity.Type = request.Type;
        entity.OS = request.OS.Trim();
        entity.OSVersion = request.OSVersion.Trim();
        entity.Processor = request.Processor.Trim();
        entity.RamAmount = request.RamAmount.Trim();
        entity.Description = request.Description.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        var updated = await _db.Devices
            .AsNoTracking()
            .Include(d => d.AssignedUser)
            .FirstAsync(d => d.Id == id, cancellationToken);

        return MapDetail(updated);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Devices.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (entity is null)
            return false;

        _db.Devices.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<bool> ExistsDuplicateAsync(string name, string manufacturer, int? excludeId, CancellationToken cancellationToken = default)
    {
        var n = name.Trim();
        var m = manufacturer.Trim();
        return _db.Devices.AnyAsync(
            d => d.Name == n && d.Manufacturer == m && (!excludeId.HasValue || d.Id != excludeId.Value),
            cancellationToken);
    }

    private static UserSummaryDto? MapUser(ApplicationUser? user)
    {
        if (user is null)
            return null;
        return new UserSummaryDto(user.Id, user.FullName, user.RoleName, user.Location);
    }

    private static DeviceListItemDto MapListItem(Device d) =>
        new(d.Id, d.Name, d.Manufacturer, d.Type, d.OS, MapUser(d.AssignedUser));

    private static DeviceDetailDto MapDetail(Device d) =>
        new(
            d.Id,
            d.Name,
            d.Manufacturer,
            d.Type,
            d.OS,
            d.OSVersion,
            d.Processor,
            d.RamAmount,
            d.Description,
            MapUser(d.AssignedUser));
}
