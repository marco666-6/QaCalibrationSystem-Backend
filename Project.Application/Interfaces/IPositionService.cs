using Project.Application.Common;
using Project.Application.DTOs;

namespace Project.Application.Interfaces;

public interface IPositionService
{
    Task<ApiResponse<PagedResult<PositionSummaryDto>>> GetAllAsync(PositionFilterParams filters);
    Task<ApiResponse<IEnumerable<PositionOptionDto>>> GetOptionsAsync(PositionOptionFilterParams filters);
    Task<ApiResponse<PositionDto>> GetByIdAsync(int positionId);
    Task<ApiResponse<PositionDto>> CreateAsync(CreatePositionRequest request);
    Task<ApiResponse<PositionDto>> UpdateAsync(int positionId, UpdatePositionRequest request);
    Task<ApiResponse> DeleteAsync(int positionId);
}
