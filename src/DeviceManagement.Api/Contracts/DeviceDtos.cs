using System.ComponentModel.DataAnnotations;
using DeviceManagement.Api.Models;

namespace DeviceManagement.Api.Contracts;

public record UserSummaryDto(string Id, string Name, string Role, string Location);

public record DeviceListItemDto(
    int Id,
    string Name,
    string Manufacturer,
    DeviceType Type,
    string OS,
    UserSummaryDto? AssignedTo);

public record DeviceDetailDto(
    int Id,
    string Name,
    string Manufacturer,
    DeviceType Type,
    string OS,
    string OSVersion,
    string Processor,
    string RamAmount,
    string Description,
    UserSummaryDto? AssignedTo);

public class CreateDeviceRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Manufacturer { get; set; } = string.Empty;

    [Required]
    public DeviceType Type { get; set; }

    [Required]
    public string OS { get; set; } = string.Empty;

    [Required]
    public string OSVersion { get; set; } = string.Empty;

    [Required]
    public string Processor { get; set; } = string.Empty;

    [Required]
    public string RamAmount { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;
}

public class UpdateDeviceRequest : CreateDeviceRequest;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string FullName,
    [Required] string RoleName,
    [Required] string Location);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record LoginResponse(string Token, DateTime ExpiresAtUtc, string UserId, string Email);

public record GenerateDescriptionResponse(string Description);
