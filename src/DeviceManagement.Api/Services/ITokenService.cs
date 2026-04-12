using DeviceManagement.Api.Models;

namespace DeviceManagement.Api.Services;

public interface ITokenService
{
    string CreateToken(ApplicationUser user);
}
