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
public sealed class PositionsController : ControllerBase
{
    private readonly IPositionService _positionService;

    public PositionsController(IPositionService positionService)
    {
        _positionService = positionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PositionSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PositionFilterParams filters)
    {
        var result = await _positionService.GetAllAsync(filters);
        return Ok(result);
    }

    [HttpGet("options")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PositionOptionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptions([FromQuery] PositionOptionFilterParams filters)
    {
        var result = await _positionService.GetOptionsAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _positionService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePositionRequest request)
    {
        var result = await _positionService.CreateAsync(request);
        if (!result.Success)
        {
            if (result.Message.Contains("already in use", StringComparison.OrdinalIgnoreCase))
                return Conflict(result);

            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.PositionId },
            result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePositionRequest request)
    {
        var result = await _positionService.UpdateAsync(id, request);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);
            if (result.Message.Contains("already in use", StringComparison.OrdinalIgnoreCase))
                return Conflict(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _positionService.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
