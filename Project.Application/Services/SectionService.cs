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
    private readonly IValidator<CreateSectionsRequest> _createManyValidator;
    private readonly IValidator<UpdateSectionRequest> _updateValidator;
    private readonly IValidator<BulkDeleteSectionsRequest> _bulkDeleteValidator;
    private readonly ILogger<SectionService> _logger;

    public SectionService(
        ISectionRepository sectionRepo,
        IValidator<CreateSectionRequest> createValidator,
        IValidator<CreateSectionsRequest> createManyValidator,
        IValidator<UpdateSectionRequest> updateValidator,
        IValidator<BulkDeleteSectionsRequest> bulkDeleteValidator,
        ILogger<SectionService> logger)
    {
        _sectionRepo = sectionRepo;
        _createValidator = createValidator;
        _createManyValidator = createManyValidator;
        _updateValidator = updateValidator;
        _bulkDeleteValidator = bulkDeleteValidator;
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

    public async Task<ApiResponse<IReadOnlyCollection<SectionDto>>> CreateManyAsync(CreateSectionsRequest request)
    {
        var validation = await _createManyValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<IReadOnlyCollection<SectionDto>>.Fail(
                "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage));
        }

        var normalizedItems = request.Items
            .Select(item => new CreateSectionRequest(
                item.SectionCode.Trim(),
                item.SectionName.Trim()))
            .ToList();

        var duplicateCodes = normalizedItems
            .GroupBy(item => item.SectionCode, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (duplicateCodes.Length > 0)
        {
            return ApiResponse<IReadOnlyCollection<SectionDto>>.Fail(
                "Validation failed.",
                duplicateCodes.Select(code => $"Section code '{code}' appears more than once in the request."));
        }

        var existingCodes = (await _sectionRepo.GetExistingCodesAsync(normalizedItems.Select(item => item.SectionCode)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (existingCodes.Length > 0)
        {
            return ApiResponse<IReadOnlyCollection<SectionDto>>.Fail(
                "One or more section codes are already in use.",
                existingCodes.Select(code => $"Section code '{code}' is already in use."));
        }

        var now = DateTime.Now;
        var entities = normalizedItems.Select(item => new Section
        {
            SectionCode = item.SectionCode,
            SectionName = item.SectionName,
            IsActive = true,
            CreatedAt = now
        }).ToList();

        var createdIds = await _sectionRepo.CreateManyAsync(entities);
        var createdItems = (await _sectionRepo.GetByIdsAsync(createdIds))
            .OrderBy(item => item.SectionCode, StringComparer.OrdinalIgnoreCase)
            .Select(MapToDto)
            .ToArray();

        _logger.LogInformation("Created {Count} sections in a single request.", createdItems.Length);
        return ApiResponse<IReadOnlyCollection<SectionDto>>.Created(createdItems);
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

    public async Task<ApiResponse<BulkDeleteResultDto>> DeleteManyAsync(BulkDeleteSectionsRequest request)
    {
        var validation = await _bulkDeleteValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<BulkDeleteResultDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage));
        }

        var ids = request.Ids.Distinct().ToArray();
        var existingIds = (await _sectionRepo.GetByIdsAsync(ids))
            .Select(item => item.SectionId)
            .Distinct()
            .ToArray();

        var notFoundIds = ids.Except(existingIds).OrderBy(id => id).ToArray();
        var deletedCount = existingIds.Length == 0
            ? 0
            : await _sectionRepo.SoftDeleteManyAsync(existingIds);

        var result = new BulkDeleteResultDto(ids.Length, deletedCount, notFoundIds);
        if (deletedCount == 0)
        {
            return ApiResponse<BulkDeleteResultDto>.NotFound("No matching sections were found.");
        }

        _logger.LogInformation("Soft-deleted {Count} sections in bulk.", deletedCount);
        return ApiResponse<BulkDeleteResultDto>.Ok(
            result,
            notFoundIds.Length == 0
                ? "Sections deleted successfully."
                : "Sections deleted with some IDs not found.");
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
