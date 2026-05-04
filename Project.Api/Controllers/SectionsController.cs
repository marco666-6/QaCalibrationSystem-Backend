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
public sealed class SectionsController : ControllerBase
{
    private readonly ISectionService _sectionService;

    public SectionsController(ISectionService sectionService)
    {
        _sectionService = sectionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SectionSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] SectionFilterParams filters)
    {
        var result = await _sectionService.GetAllAsync(filters);
        return Ok(result);
    }

    [HttpGet("options")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SectionOptionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptions([FromQuery] SectionOptionFilterParams filters)
    {
        var result = await _sectionService.GetOptionsAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SectionDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _sectionService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SectionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SectionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SectionDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateSectionRequest request)
    {
        var result = await _sectionService.CreateAsync(request);
        if (!result.Success)
        {
            if (result.Message.Contains("already in use", StringComparison.OrdinalIgnoreCase))
                return Conflict(result);

            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.SectionId },
            result);
    }

    [HttpPost("multi-add")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SectionDto>>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SectionDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SectionDto>>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateMany([FromBody] CreateSectionsRequest request)
    {
        var result = await _sectionService.CreateManyAsync(request);
        if (!result.Success)
        {
            if (result.Message.Contains("already in use", StringComparison.OrdinalIgnoreCase))
                return Conflict(result);

            return BadRequest(result);
        }

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SectionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SectionDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<SectionDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSectionRequest request)
    {
        var result = await _sectionService.UpdateAsync(id, request);
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
        var result = await _sectionService.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("bulk-delete")]
    [ProducesResponseType(typeof(ApiResponse<BulkDeleteResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BulkDeleteResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BulkDeleteResultDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMany([FromBody] BulkDeleteSectionsRequest request)
    {
        var result = await _sectionService.DeleteManyAsync(request);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }
}
