using Project.Application.DTOs;
using Project.Domain.Entities;

namespace Project.Application.Interfaces;

public interface IPositionRepository
{
    Task<(IEnumerable<Position> Items, int TotalCount)> GetAllAsync(PositionFilterParams filters);
    Task<IEnumerable<Position>> GetOptionsAsync(PositionOptionFilterParams filters);
    Task<Position?> GetByIdAsync(int positionId);
    Task<IEnumerable<Position>> GetByIdsAsync(IEnumerable<int> positionIds);
    Task<bool> CodeExistsAsync(string positionCode, int? excludePositionId = null);
    Task<IEnumerable<string>> GetExistingCodesAsync(IEnumerable<string> positionCodes);
    Task<int> CreateAsync(Position entity);
    Task<IReadOnlyCollection<int>> CreateManyAsync(IEnumerable<Position> entities);
    Task<bool> UpdateAsync(Position entity);
    Task<bool> SoftDeleteAsync(int positionId);
    Task<int> SoftDeleteManyAsync(IEnumerable<int> positionIds);
}
