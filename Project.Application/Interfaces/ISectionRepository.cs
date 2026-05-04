using Project.Application.DTOs;
using Project.Domain.Entities;

namespace Project.Application.Interfaces;

public interface ISectionRepository
{
    Task<(IEnumerable<Section> Items, int TotalCount)> GetAllAsync(SectionFilterParams filters);
    Task<IEnumerable<Section>> GetOptionsAsync(SectionOptionFilterParams filters);
    Task<Section?> GetByIdAsync(int sectionId);
    Task<bool> CodeExistsAsync(string sectionCode, int? excludeSectionId = null);
    Task<int> CreateAsync(Section entity);
    Task<bool> UpdateAsync(Section entity);
    Task<bool> SoftDeleteAsync(int sectionId);
}
