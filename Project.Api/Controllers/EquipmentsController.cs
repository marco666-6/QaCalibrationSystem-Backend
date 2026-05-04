using System.Security.Claims;
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
public sealed class EquipmentsController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;

    public EquipmentsController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EquipmentSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] EquipmentFilterParams filters)
    {
        var result = await _equipmentService.GetAllAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _equipmentService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateEquipmentRequest request)
    {
        var result = await _equipmentService.CreateAsync(request, GetActorUsername(), GetActorEmployeeCode());
        if (!result.Success)
        {
            if (result.Message.Contains("already in use", StringComparison.OrdinalIgnoreCase))
                return Conflict(result);

            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEquipmentRequest request)
    {
        var result = await _equipmentService.UpdateAsync(id, request, GetActorUsername(), GetActorEmployeeCode());
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
        var result = await _equipmentService.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("bulk-delete")]
    [ProducesResponseType(typeof(ApiResponse<BulkDeleteResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BulkDeleteResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BulkDeleteResultDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMany([FromBody] BulkDeleteEquipmentsRequest request)
    {
        var result = await _equipmentService.DeleteManyAsync(request);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("bulk-section-change")]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateResultDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BulkChangeSection([FromBody] BulkSectionChangeRequest request)
    {
        var result = await _equipmentService.BulkChangeSectionAsync(request, GetActorUsername(), GetActorEmployeeCode());
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("bulk-pic-change")]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateResultDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BulkChangePic([FromBody] BulkPicChangeRequest request)
    {
        var result = await _equipmentService.BulkChangePicAsync(request, GetActorUsername(), GetActorEmployeeCode());
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("bulk-status-change")]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdateResultDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BulkChangeStatus([FromBody] BulkStatusChangeRequest request)
    {
        var result = await _equipmentService.BulkChangeStatusAsync(request, GetActorUsername(), GetActorEmployeeCode());
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<EquipmentImportResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EquipmentImportResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import([FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<EquipmentImportResultDto>.Fail("An Excel file is required."));

        await using var stream = file.OpenReadStream();
        var result = await _equipmentService.ImportAsync(stream, file.FileName, GetActorUsername(), GetActorEmployeeCode());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("import-template")]
    public async Task<IActionResult> DownloadTemplate()
    {
        var file = await _equipmentService.GetImportTemplateAsync();
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] EquipmentFilterParams filters)
    {
        var file = await _equipmentService.ExportAsync(filters);
        return File(file.Content, file.ContentType, file.FileName);
    }

    private string? GetActorUsername() =>
        User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;

    private string? GetActorEmployeeCode()
    {
        var claim = User.FindFirst("employee_code")?.Value;
        return string.IsNullOrWhiteSpace(claim) ? null : claim;
    }
}
