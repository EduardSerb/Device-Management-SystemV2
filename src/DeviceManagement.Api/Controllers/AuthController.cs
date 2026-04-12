using DeviceManagement.Api.Contracts;
using DeviceManagement.Api.Models;
using DeviceManagement.Api.Options;
using DeviceManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DeviceManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly ITokenService _tokens;
    private readonly JwtOptions _jwt;

    public AuthController(
        UserManager<ApplicationUser> users,
        ITokenService tokens,
        IOptions<JwtOptions> jwt)
    {
        _users = users;
        _tokens = tokens;
        _jwt = jwt.Value;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var existing = await _users.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Conflict(new { message = "Email is already registered." });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            FullName = request.FullName.Trim(),
            RoleName = request.RoleName.Trim(),
            Location = request.Location.Trim()
        };

        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        var token = _tokens.CreateToken(user);
        return Ok(new LoginResponse(token, DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes), user.Id, user.Email ?? string.Empty));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _users.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized(new { message = "Invalid credentials." });

        var valid = await _users.CheckPasswordAsync(user, request.Password);
        if (!valid)
            return Unauthorized(new { message = "Invalid credentials." });

        var token = _tokens.CreateToken(user);
        return Ok(new LoginResponse(token, DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes), user.Id, user.Email ?? string.Empty));
    }
}
