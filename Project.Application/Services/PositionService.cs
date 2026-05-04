using FluentValidation;
using Microsoft.Extensions.Logging;
using Project.Application.Common;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Entities;

namespace Project.Application.Services;

public sealed class PositionService : IPositionService
{
    private readonly IPositionRepository _positionRepo;
    private readonly IValidator<CreatePositionRequest> _createValidator;
    private readonly IValidator<CreatePositionsRequest> _createManyValidator;
    private readonly IValidator<UpdatePositionRequest> _updateValidator;
    private readonly IValidator<BulkDeletePositionsRequest> _bulkDeleteValidator;
    private readonly ILogger<PositionService> _logger;

    public PositionService(
        IPositionRepository positionRepo,
        IValidator<CreatePositionRequest> createValidator,
        IValidator<CreatePositionsRequest> createManyValidator,
        IValidator<UpdatePositionRequest> updateValidator,
        IValidator<BulkDeletePositionsRequest> bulkDeleteValidator,
        ILogger<PositionService> logger)
    {
        _positionRepo = positionRepo;
        _createValidator = createValidator;
        _createManyValidator = createManyValidator;
        _updateValidator = updateValidator;
        _bulkDeleteValidator = bulkDeleteValidator;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<PositionSummaryDto>>> GetAllAsync(PositionFilterParams filters)
    {
        var (items, totalCount) = await _positionRepo.GetAllAsync(filters);
        var result = PagedResult<PositionSummaryDto>.Create(
            items.Select(MapToSummaryDto),
            totalCount,
            filters);

        return ApiResponse<PagedResult<PositionSummaryDto>>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<PositionOptionDto>>> GetOptionsAsync(PositionOptionFilterParams filters)
    {
        var items = await _positionRepo.GetOptionsAsync(filters);
        return ApiResponse<IEnumerable<PositionOptionDto>>.Ok(items.Select(MapToOptionDto));
    }

    public async Task<ApiResponse<PositionDto>> GetByIdAsync(int positionId)
    {
        var entity = await _positionRepo.GetByIdAsync(positionId);
        if (entity is null)
            return ApiResponse<PositionDto>.NotFound($"Position with ID {positionId} was not found.");

        return ApiResponse<PositionDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<PositionDto>> CreateAsync(CreatePositionRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ApiResponse<PositionDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage));

        var positionCode = request.PositionCode.Trim();
        if (await _positionRepo.CodeExistsAsync(positionCode))
            return ApiResponse<PositionDto>.Fail($"Position code '{positionCode}' is already in use.");

        var entity = new Position
        {
            PositionCode = positionCode,
            PositionName = request.PositionName.Trim(),
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        var newId = await _positionRepo.CreateAsync(entity);
        _logger.LogInformation("Position created: {Code} - {Name} (ID {Id})", entity.PositionCode, entity.PositionName, newId);

        var created = await _positionRepo.GetByIdAsync(newId);
        return ApiResponse<PositionDto>.Created(MapToDto(created!));
    }

    public async Task<ApiResponse<IReadOnlyCollection<PositionDto>>> CreateManyAsync(CreatePositionsRequest request)
    {
        var validation = await _createManyValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<IReadOnlyCollection<PositionDto>>.Fail(
                "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage));
        }

        var normalizedItems = request.Items
            .Select(item => new CreatePositionRequest(
                item.PositionCode.Trim(),
                item.PositionName.Trim()))
            .ToList();

        var duplicateCodes = normalizedItems
            .GroupBy(item => item.PositionCode, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (duplicateCodes.Length > 0)
        {
            return ApiResponse<IReadOnlyCollection<PositionDto>>.Fail(
                "Validation failed.",
                duplicateCodes.Select(code => $"Position code '{code}' appears more than once in the request."));
        }

        var existingCodes = (await _positionRepo.GetExistingCodesAsync(normalizedItems.Select(item => item.PositionCode)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (existingCodes.Length > 0)
        {
            return ApiResponse<IReadOnlyCollection<PositionDto>>.Fail(
                "One or more position codes are already in use.",
                existingCodes.Select(code => $"Position code '{code}' is already in use."));
        }

        var now = DateTime.Now;
        var entities = normalizedItems.Select(item => new Position
        {
            PositionCode = item.PositionCode,
            PositionName = item.PositionName,
            IsActive = true,
            CreatedAt = now
        }).ToList();

        var createdIds = await _positionRepo.CreateManyAsync(entities);
        var createdItems = (await _positionRepo.GetByIdsAsync(createdIds))
            .OrderBy(item => item.PositionCode, StringComparer.OrdinalIgnoreCase)
            .Select(MapToDto)
            .ToArray();

        _logger.LogInformation("Created {Count} positions in a single request.", createdItems.Length);
        return ApiResponse<IReadOnlyCollection<PositionDto>>.Created(createdItems);
    }

    public async Task<ApiResponse<PositionDto>> UpdateAsync(int positionId, UpdatePositionRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ApiResponse<PositionDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage));

        var existing = await _positionRepo.GetByIdAsync(positionId);
        if (existing is null)
            return ApiResponse<PositionDto>.NotFound($"Position with ID {positionId} was not found.");

        var positionCode = request.PositionCode.Trim();
        if (await _positionRepo.CodeExistsAsync(positionCode, positionId))
            return ApiResponse<PositionDto>.Fail($"Position code '{positionCode}' is already in use.");

        existing.PositionCode = positionCode;
        existing.PositionName = request.PositionName.Trim();
        existing.IsActive = request.IsActive;
        existing.UpdatedAt = DateTime.Now;

        await _positionRepo.UpdateAsync(existing);
        _logger.LogInformation("Position updated: {Code} - {Name} (ID {Id})", existing.PositionCode, existing.PositionName, positionId);

        var updated = await _positionRepo.GetByIdAsync(positionId);
        return ApiResponse<PositionDto>.Ok(MapToDto(updated!), "Updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(int positionId)
    {
        var entity = await _positionRepo.GetByIdAsync(positionId);
        if (entity is null)
            return ApiResponse.NotFound($"Position with ID {positionId} was not found.");

        await _positionRepo.SoftDeleteAsync(positionId);
        _logger.LogInformation("Position soft-deleted: ID {Id}", positionId);

        return ApiResponse.Ok("Position deleted successfully.");
    }

    public async Task<ApiResponse<BulkDeleteResultDto>> DeleteManyAsync(BulkDeletePositionsRequest request)
    {
        var validation = await _bulkDeleteValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<BulkDeleteResultDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage));
        }

        var ids = request.Ids.Distinct().ToArray();
        var existingIds = (await _positionRepo.GetByIdsAsync(ids))
            .Select(item => item.PositionId)
            .Distinct()
            .ToArray();

        var notFoundIds = ids.Except(existingIds).OrderBy(id => id).ToArray();
        var deletedCount = existingIds.Length == 0
            ? 0
            : await _positionRepo.SoftDeleteManyAsync(existingIds);

        var result = new BulkDeleteResultDto(ids.Length, deletedCount, notFoundIds);
        if (deletedCount == 0)
        {
            return ApiResponse<BulkDeleteResultDto>.NotFound("No matching positions were found.");
        }

        _logger.LogInformation("Soft-deleted {Count} positions in bulk.", deletedCount);
        return ApiResponse<BulkDeleteResultDto>.Ok(
            result,
            notFoundIds.Length == 0
                ? "Positions deleted successfully."
                : "Positions deleted with some IDs not found.");
    }

    private static PositionDto MapToDto(Position entity) => new(
        entity.PositionId,
        entity.PositionCode,
        entity.PositionName,
        entity.IsActive,
        entity.CreatedAt,
        entity.UpdatedAt);

    private static PositionSummaryDto MapToSummaryDto(Position entity) => new(
        entity.PositionId,
        entity.PositionCode,
        entity.PositionName,
        entity.IsActive);

    private static PositionOptionDto MapToOptionDto(Position entity) => new(
        entity.PositionId,
        entity.PositionCode,
        entity.PositionName);
}
