using Project.Application.Common;
using Project.Application.DTOs;

namespace Project.Application.Interfaces;

public interface IDefaultLocationService
{
    Task<ApiResponse<PagedResult<DefaultLocationSummaryDto>>> GetAllAsync(DefaultLocationFilterParams filters);
    Task<ApiResponse<IEnumerable<DefaultLocationOptionDto>>> GetOptionsAsync(DefaultLocationOptionFilterParams filters);
    Task<ApiResponse<DefaultLocationDto>> GetByIdAsync(int defaultLocationId);
    Task<ApiResponse<DefaultLocationDto>> CreateAsync(CreateDefaultLocationRequest request);
    Task<ApiResponse<DefaultLocationDto>> UpdateAsync(int defaultLocationId, UpdateDefaultLocationRequest request);
    Task<ApiResponse> DeleteAsync(int defaultLocationId);
}
