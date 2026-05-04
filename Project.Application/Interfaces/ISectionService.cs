using Project.Application.Common;
using Project.Application.DTOs;

namespace Project.Application.Interfaces;

public interface ISectionService
{
    Task<ApiResponse<PagedResult<SectionSummaryDto>>> GetAllAsync(SectionFilterParams filters);
    Task<ApiResponse<IEnumerable<SectionOptionDto>>> GetOptionsAsync(SectionOptionFilterParams filters);
    Task<ApiResponse<SectionDto>> GetByIdAsync(int sectionId);
    Task<ApiResponse<SectionDto>> CreateAsync(CreateSectionRequest request);
    Task<ApiResponse<IReadOnlyCollection<SectionDto>>> CreateManyAsync(CreateSectionsRequest request);
    Task<ApiResponse<SectionDto>> UpdateAsync(int sectionId, UpdateSectionRequest request);
    Task<ApiResponse> DeleteAsync(int sectionId);
    Task<ApiResponse<BulkDeleteResultDto>> DeleteManyAsync(BulkDeleteSectionsRequest request);
}
