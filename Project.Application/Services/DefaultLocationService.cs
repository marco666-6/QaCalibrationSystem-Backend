using FluentValidation;
using Microsoft.Extensions.Logging;
using Project.Application.Common;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Entities;

namespace Project.Application.Services;

public sealed class DefaultLocationService : IDefaultLocationService
{
    private readonly IDefaultLocationRepository _defaultLocationRepo;
    private readonly IValidator<CreateDefaultLocationRequest> _createValidator;
    private readonly IValidator<UpdateDefaultLocationRequest> _updateValidator;
    private readonly ILogger<DefaultLocationService> _logger;

    public DefaultLocationService(
        IDefaultLocationRepository defaultLocationRepo,
        IValidator<CreateDefaultLocationRequest> createValidator,
        IValidator<UpdateDefaultLocationRequest> updateValidator,
        ILogger<DefaultLocationService> logger)
    {
        _defaultLocationRepo = defaultLocationRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<DefaultLocationSummaryDto>>> GetAllAsync(DefaultLocationFilterParams filters)
    {
        var (items, totalCount) = await _defaultLocationRepo.GetAllAsync(filters);
        var result = PagedResult<DefaultLocationSummaryDto>.Create(
            items.Select(MapToSummaryDto),
            totalCount,
            filters);

        return ApiResponse<PagedResult<DefaultLocationSummaryDto>>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<DefaultLocationOptionDto>>> GetOptionsAsync(DefaultLocationOptionFilterParams filters)
    {
        var items = await _defaultLocationRepo.GetOptionsAsync(filters);
        return ApiResponse<IEnumerable<DefaultLocationOptionDto>>.Ok(items.Select(MapToOptionDto));
    }

    public async Task<ApiResponse<DefaultLocationDto>> GetByIdAsync(int defaultLocationId)
    {
        var entity = await _defaultLocationRepo.GetByIdAsync(defaultLocationId);
        if (entity is null)
            return ApiResponse<DefaultLocationDto>.NotFound($"Default location with ID {defaultLocationId} was not found.");

        return ApiResponse<DefaultLocationDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<DefaultLocationDto>> CreateAsync(CreateDefaultLocationRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ApiResponse<DefaultLocationDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage));

        var entity = new DefaultLocation
        {
            DefaultLocationName = request.DefaultLocationName.Trim(),
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        var newId = await _defaultLocationRepo.CreateAsync(entity);
        _logger.LogInformation("Default location created: {Name} (ID {Id})", entity.DefaultLocationName, newId);

        var created = await _defaultLocationRepo.GetByIdAsync(newId);
        return ApiResponse<DefaultLocationDto>.Created(MapToDto(created!));
    }

    public async Task<ApiResponse<DefaultLocationDto>> UpdateAsync(int defaultLocationId, UpdateDefaultLocationRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ApiResponse<DefaultLocationDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(e => e.ErrorMessage));

        var existing = await _defaultLocationRepo.GetByIdAsync(defaultLocationId);
        if (existing is null)
            return ApiResponse<DefaultLocationDto>.NotFound($"Default location with ID {defaultLocationId} was not found.");

        existing.DefaultLocationName = request.DefaultLocationName.Trim();
        existing.IsActive = request.IsActive;
        existing.UpdatedAt = DateTime.Now;

        await _defaultLocationRepo.UpdateAsync(existing);
        _logger.LogInformation("Default location updated: {Name} (ID {Id})", existing.DefaultLocationName, defaultLocationId);

        var updated = await _defaultLocationRepo.GetByIdAsync(defaultLocationId);
        return ApiResponse<DefaultLocationDto>.Ok(MapToDto(updated!), "Updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(int defaultLocationId)
    {
        var entity = await _defaultLocationRepo.GetByIdAsync(defaultLocationId);
        if (entity is null)
            return ApiResponse.NotFound($"Default location with ID {defaultLocationId} was not found.");

        await _defaultLocationRepo.SoftDeleteAsync(defaultLocationId);
        _logger.LogInformation("Default location soft-deleted: ID {Id}", defaultLocationId);

        return ApiResponse.Ok("Default location deleted successfully.");
    }

    private static DefaultLocationDto MapToDto(DefaultLocation entity) => new(
        entity.DefaultLocationId,
        entity.DefaultLocationName,
        entity.IsActive,
        entity.CreatedAt,
        entity.UpdatedAt);

    private static DefaultLocationSummaryDto MapToSummaryDto(DefaultLocation entity) => new(
        entity.DefaultLocationId,
        entity.DefaultLocationName,
        entity.IsActive);

    private static DefaultLocationOptionDto MapToOptionDto(DefaultLocation entity) => new(
        entity.DefaultLocationId,
        entity.DefaultLocationName);
}
