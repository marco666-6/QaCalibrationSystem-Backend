using Project.Application.DTOs;
using Project.Domain.Entities;

namespace Project.Application.Interfaces;

public interface ISectionRepository
{
    Task<(IEnumerable<Section> Items, int TotalCount)> GetAllAsync(SectionFilterParams filters);
    Task<IEnumerable<Section>> GetOptionsAsync(SectionOptionFilterParams filters);
    Task<Section?> GetByIdAsync(int sectionId);
    Task<IEnumerable<Section>> GetByIdsAsync(IEnumerable<int> sectionIds);
    Task<bool> CodeExistsAsync(string sectionCode, int? excludeSectionId = null);
    Task<IEnumerable<string>> GetExistingCodesAsync(IEnumerable<string> sectionCodes);
    Task<int> CreateAsync(Section entity);
    Task<IReadOnlyCollection<int>> CreateManyAsync(IEnumerable<Section> entities);
    Task<bool> UpdateAsync(Section entity);
    Task<bool> SoftDeleteAsync(int sectionId);
    Task<int> SoftDeleteManyAsync(IEnumerable<int> sectionIds);
}
