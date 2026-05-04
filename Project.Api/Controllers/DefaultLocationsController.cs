using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Common;
using Project.Application.DTOs;
using Project.Application.Interfaces;

namespace Project.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "SuperAdmin,Admin")]
public sealed class DefaultLocationsController : ControllerBase
{
    private readonly IDefaultLocationService _defaultLocationService;

    public DefaultLocationsController(IDefaultLocationService defaultLocationService)
    {
        _defaultLocationService = defaultLocationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DefaultLocationSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] DefaultLocationFilterParams filters)
    {
        var result = await _defaultLocationService.GetAllAsync(filters);
        return Ok(result);
    }

    [HttpGet("options")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DefaultLocationOptionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptions([FromQuery] DefaultLocationOptionFilterParams filters)
    {
        var result = await _defaultLocationService.GetOptionsAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DefaultLocationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DefaultLocationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _defaultLocationService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DefaultLocationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DefaultLocationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDefaultLocationRequest request)
    {
        var result = await _defaultLocationService.CreateAsync(request);
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.DefaultLocationId },
            result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DefaultLocationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DefaultLocationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<DefaultLocationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDefaultLocationRequest request)
    {
        var result = await _defaultLocationService.UpdateAsync(id, request);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _defaultLocationService.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
