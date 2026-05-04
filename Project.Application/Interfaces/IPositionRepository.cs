using Project.Application.DTOs;
using Project.Domain.Entities;

namespace Project.Application.Interfaces;

public interface IPositionRepository
{
    Task<(IEnumerable<Position> Items, int TotalCount)> GetAllAsync(PositionFilterParams filters);
    Task<IEnumerable<Position>> GetOptionsAsync(PositionOptionFilterParams filters);
    Task<Position?> GetByIdAsync(int positionId);
    Task<bool> CodeExistsAsync(string positionCode, int? excludePositionId = null);
    Task<int> CreateAsync(Position entity);
    Task<bool> UpdateAsync(Position entity);
    Task<bool> SoftDeleteAsync(int positionId);
}
