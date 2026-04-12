using Microsoft.AspNetCore.Identity;

namespace DeviceManagement.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}
