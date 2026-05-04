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
    private readonly IValidator<UpdatePositionRequest> _updateValidator;
    private readonly ILogger<PositionService> _logger;

    public PositionService(
        IPositionRepository positionRepo,
        IValidator<CreatePositionRequest> createValidator,
        IValidator<UpdatePositionRequest> updateValidator,
        ILogger<PositionService> logger)
    {
        _positionRepo = positionRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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
