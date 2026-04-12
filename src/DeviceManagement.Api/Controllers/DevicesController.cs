using System.Security.Claims;
using DeviceManagement.Api.Contracts;
using DeviceManagement.Api.Data;
using DeviceManagement.Api.Models;
using DeviceManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _devices;
    private readonly IDeviceSearchService _search;
    private readonly IDeviceAssignmentService _assignment;
    private readonly IDeviceDescriptionGenerator _descriptionGenerator;
    private readonly ApplicationDbContext _db;

    public DevicesController(
        IDeviceService devices,
        IDeviceSearchService search,
        IDeviceAssignmentService assignment,
        IDeviceDescriptionGenerator descriptionGenerator,
        ApplicationDbContext db)
    {
        _devices = devices;
        _search = search;
        _assignment = assignment;
        _descriptionGenerator = descriptionGenerator;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeviceListItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _devices.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<DeviceListItemDto>>> Search([FromQuery] string? q, CancellationToken cancellationToken)
    {
        var items = await _search.SearchAsync(q ?? string.Empty, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeviceDetailDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var device = await _devices.GetByIdAsync(id, cancellationToken);
        return device is null ? NotFound() : Ok(device);
    }

    [HttpPost]
    public async Task<ActionResult<DeviceDetailDto>> Create([FromBody] CreateDeviceRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!HasAllFields(request))
            return BadRequest(new { message = "All fields must have values." });

        if (await _devices.ExistsDuplicateAsync(request.Name, request.Manufacturer, null, cancellationToken))
            return Conflict(new { message = "A device with this name and manufacturer already exists." });

        var created = await _devices.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DeviceDetailDto>> Update(int id, [FromBody] UpdateDeviceRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!HasAllFields(request))
            return BadRequest(new { message = "All fields must have values." });

        if (await _devices.ExistsDuplicateAsync(request.Name, request.Manufacturer, id, cancellationToken))
            return Conflict(new { message = "A device with this name and manufacturer already exists." });

        var updated = await _devices.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _devices.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _assignment.AssignToCurrentUserAsync(id, userId, cancellationToken);
        return result.Error switch
        {
            "not_found" => NotFound(),
            "already_assigned" => Conflict(new { message = "Device is already assigned to another user." }),
            _ => NoContent()
        };
    }

    [HttpPost("{id:int}/unassign")]
    public async Task<IActionResult> Unassign(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _assignment.UnassignIfOwnedAsync(id, userId, cancellationToken);
        return result.Error switch
        {
            "not_found" => NotFound(),
            "not_owner" => StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only unassign devices assigned to you." }),
            _ => NoContent()
        };
    }

    [HttpPost("{id:int}/generate-description")]
    public async Task<ActionResult<GenerateDescriptionResponse>> GenerateDescription(int id, CancellationToken cancellationToken)
    {
        var device = await _db.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (device is null)
            return NotFound();

        try
        {
            var text = await _descriptionGenerator.GenerateAsync(device, cancellationToken);
            var tracked = await _db.Devices.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            if (tracked is not null)
            {
                tracked.Description = text;
                await _db.SaveChangesAsync(cancellationToken);
            }

            return Ok(new GenerateDescriptionResponse(text));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static bool HasAllFields(CreateDeviceRequest request)
    {
        static bool NonEmpty(string? s) => !string.IsNullOrWhiteSpace(s);

        return NonEmpty(request.Name)
               && NonEmpty(request.Manufacturer)
               && NonEmpty(request.OS)
               && NonEmpty(request.OSVersion)
               && NonEmpty(request.Processor)
               && NonEmpty(request.RamAmount)
               && NonEmpty(request.Description);
    }
}
