using FluentValidation;
using Microsoft.Extensions.Logging;
using Project.Application.Common;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Entities;

namespace Project.Application.Services;

public sealed class SectionService : ISectionService
{
    private readonly ISectionRepository _sectionRepo;
    private readonly IValidator<CreateSectionRequest> _createValidator;
    private readonly IValidator<UpdateSectionRequest> _updateValidator;
    private readonly ILogger<SectionService> _logger;

    public SectionService(
        ISectionRepository sectionRepo,
        IValidator<CreateSectionRequest> createValidator,
        IValidator<UpdateSectionRequest> updateValidator,
        ILogger<SectionService> logger)
    {
        _sectionRepo = sectionRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<SectionSummaryDto>>> GetAllAsync(SectionFilterParams filters)
    {
        var (items, totalCount) = await _sectionRepo.GetAllAsync(filters);
        var result = PagedResult<SectionSummaryDto>.Create(
            items.Select(MapToSummaryDto),
            totalCount,
            filters);

        return ApiResponse<PagedResult<SectionSummaryDto>>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<SectionOptionDto>>> GetOptionsAsync(SectionOptionFilterParams filters)
    {
        var items = await _sectionRepo.GetOptionsAsync(filters);
        return ApiResponse<IEnumerable<SectionOptionDto>>.Ok(items.Select(MapToOptionDto));
    }

    public async Task<ApiResponse<SectionDto>> GetByIdAsync(int sectionId)
    {
        var entity = await _sectionRepo.GetByIdAsync(sectionId);
        if (entity is null)
            return ApiResponse<SectionDto>.NotFound($"Section with ID {sectionId} was not found.");

        return ApiResponse<SectionDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<SectionDto>> CreateAsync(CreateSectionRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ApiResponse<SectionDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage));

        var sectionCode = request.SectionCode.Trim();
        if (await _sectionRepo.CodeExistsAsync(sectionCode))
            return ApiResponse<SectionDto>.Fail($"Section code '{sectionCode}' is already in use.");

        var entity = new Section
        {
            SectionCode = sectionCode,
            SectionName = request.SectionName.Trim(),
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        var newId = await _sectionRepo.CreateAsync(entity);
        _logger.LogInformation("Section created: {Code} - {Name} (ID {Id})", entity.SectionCode, entity.SectionName, newId);

        var created = await _sectionRepo.GetByIdAsync(newId);
        return ApiResponse<SectionDto>.Created(MapToDto(created!));
    }

    public async Task<ApiResponse<SectionDto>> UpdateAsync(int sectionId, UpdateSectionRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ApiResponse<SectionDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage));

        var existing = await _sectionRepo.GetByIdAsync(sectionId);
        if (existing is null)
            return ApiResponse<SectionDto>.NotFound($"Section with ID {sectionId} was not found.");

        var sectionCode = request.SectionCode.Trim();
        if (await _sectionRepo.CodeExistsAsync(sectionCode, sectionId))
            return ApiResponse<SectionDto>.Fail($"Section code '{sectionCode}' is already in use.");

        existing.SectionCode = sectionCode;
        existing.SectionName = request.SectionName.Trim();
        existing.IsActive = request.IsActive;
        existing.UpdatedAt = DateTime.Now;

        await _sectionRepo.UpdateAsync(existing);
        _logger.LogInformation("Section updated: {Code} - {Name} (ID {Id})", existing.SectionCode, existing.SectionName, sectionId);

        var updated = await _sectionRepo.GetByIdAsync(sectionId);
        return ApiResponse<SectionDto>.Ok(MapToDto(updated!), "Updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(int sectionId)
    {
        var entity = await _sectionRepo.GetByIdAsync(sectionId);
        if (entity is null)
            return ApiResponse.NotFound($"Section with ID {sectionId} was not found.");

        await _sectionRepo.SoftDeleteAsync(sectionId);
        _logger.LogInformation("Section soft-deleted: ID {Id}", sectionId);

        return ApiResponse.Ok("Section deleted successfully.");
    }

    private static SectionDto MapToDto(Section entity) => new(
        entity.SectionId,
        entity.SectionCode,
        entity.SectionName,
        entity.IsActive,
        entity.CreatedAt,
        entity.UpdatedAt);

    private static SectionSummaryDto MapToSummaryDto(Section entity) => new(
        entity.SectionId,
        entity.SectionCode,
        entity.SectionName,
        entity.IsActive);

    private static SectionOptionDto MapToOptionDto(Section entity) => new(
        entity.SectionId,
        entity.SectionCode,
        entity.SectionName);
}
