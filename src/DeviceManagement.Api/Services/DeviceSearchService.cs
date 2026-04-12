using System.Text;
using System.Text.RegularExpressions;
using DeviceManagement.Api.Contracts;
using DeviceManagement.Api.Data;
using DeviceManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceManagement.Api.Services;

public partial class DeviceSearchService : IDeviceSearchService
{
    private const int WeightName = 100;
    private const int WeightManufacturer = 60;
    private const int WeightProcessor = 40;
    private const int WeightRam = 20;

    private readonly ApplicationDbContext _db;

    public DeviceSearchService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DeviceListItemDto>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var tokens = Tokenize(query);
        if (tokens.Count == 0)
        {
            return await LoadAllListItemsAsync(cancellationToken);
        }

        var devices = await _db.Devices
            .AsNoTracking()
            .Include(d => d.AssignedUser)
            .ToListAsync(cancellationToken);

        var scored = devices
            .Select(d => (Device: d, Score: ScoreDevice(d, tokens)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Device.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Device.Id)
            .Select(x => MapListItem(x.Device))
            .ToList();

        return scored;
    }

    private async Task<IReadOnlyList<DeviceListItemDto>> LoadAllListItemsAsync(CancellationToken cancellationToken)
    {
        var devices = await _db.Devices
            .AsNoTracking()
            .Include(d => d.AssignedUser)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        return devices.Select(MapListItem).ToList();
    }

    private static int ScoreDevice(Device device, IReadOnlyList<string> tokens)
    {
        var nameNorm = NormalizeField(device.Name);
        var manNorm = NormalizeField(device.Manufacturer);
        var procNorm = NormalizeField(device.Processor);
        var ramNorm = NormalizeField(device.RamAmount);

        var total = 0;
        foreach (var token in tokens)
        {
            total += ScoreField(nameNorm, token, WeightName);
            total += ScoreField(manNorm, token, WeightManufacturer);
            total += ScoreField(procNorm, token, WeightProcessor);
            total += ScoreField(ramNorm, token, WeightRam);
        }

        return total;
    }

    private static int ScoreField(string normalizedHaystack, string normalizedToken, int fieldWeight)
    {
        if (normalizedHaystack.Length == 0 || normalizedToken.Length == 0)
            return 0;

        if (normalizedHaystack == normalizedToken)
            return fieldWeight * 3;

        if (ContainsWordOrSubstring(normalizedHaystack, normalizedToken))
            return fieldWeight * 2;

        return 0;
    }

    private static bool ContainsWordOrSubstring(string haystack, string token)
    {
        if (haystack.Contains(token, StringComparison.Ordinal))
            return true;

        var words = haystack.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Any(w => w.Equals(token, StringComparison.Ordinal));
    }

    private static string NormalizeField(string value)
    {
        var lower = value.Trim().ToLowerInvariant();
        var noPunct = PunctuationRegex().Replace(lower, " ");
        return CollapseSpaces(noPunct);
    }

    private static List<string> Tokenize(string query)
    {
        var n = NormalizeField(query);
        if (string.IsNullOrEmpty(n))
            return [];

        return n.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static string CollapseSpaces(string input)
    {
        var sb = new StringBuilder(input.Length);
        var space = false;
        foreach (var ch in input)
        {
            if (char.IsWhiteSpace(ch))
            {
                space = true;
            }
            else
            {
                if (space && sb.Length > 0)
                    sb.Append(' ');
                space = false;
                sb.Append(ch);
            }
        }

        return sb.ToString();
    }

    [GeneratedRegex(@"[\p{P}\p{S}]+", RegexOptions.Compiled)]
    private static partial Regex PunctuationRegex();

    private static UserSummaryDto? MapUser(ApplicationUser? user)
    {
        if (user is null)
            return null;
        return new UserSummaryDto(user.Id, user.FullName, user.RoleName, user.Location);
    }

    private static DeviceListItemDto MapListItem(Device d) =>
        new(d.Id, d.Name, d.Manufacturer, d.Type, d.OS, MapUser(d.AssignedUser));
}
