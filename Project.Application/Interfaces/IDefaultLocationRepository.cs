using Project.Application.DTOs;
using Project.Domain.Entities;

namespace Project.Application.Interfaces;

public interface IDefaultLocationRepository
{
    Task<(IEnumerable<DefaultLocation> Items, int TotalCount)> GetAllAsync(DefaultLocationFilterParams filters);
    Task<IEnumerable<DefaultLocation>> GetOptionsAsync(DefaultLocationOptionFilterParams filters);
    Task<DefaultLocation?> GetByIdAsync(int defaultLocationId);
    Task<int> CreateAsync(DefaultLocation entity);
    Task<bool> UpdateAsync(DefaultLocation entity);
    Task<bool> SoftDeleteAsync(int defaultLocationId);
}
