using Project.Application.DTOs;
using Project.Domain.Entities;

namespace Project.Application.Interfaces;

public interface IEquipmentRepository
{
    Task<(IEnumerable<Equipment> Items, int TotalCount)> GetAllAsync(EquipmentFilterParams filters);
    Task<IEnumerable<Equipment>> GetAllForExportAsync(EquipmentFilterParams filters);
    Task<Equipment?> GetByIdAsync(int equipmentId);
    Task<IEnumerable<Equipment>> GetByIdsAsync(IEnumerable<int> equipmentIds);
    Task<bool> ControlNoExistsAsync(string controlNo, int? excludeEquipmentId = null);
    Task<IEnumerable<string>> GetExistingControlNumbersAsync(IEnumerable<string> controlNos);
    Task<Section?> GetSectionByIdAsync(int sectionId);
    Task<IEnumerable<Section>> GetSectionsByCodesAsync(IEnumerable<string> sectionCodes);
    Task<Employee?> GetEmployeeByIdAsync(int employeeId);
    Task<Employee?> GetEmployeeByCodeAsync(string employeeCode);
    Task<IEnumerable<Employee>> GetEmployeesByCodesAsync(IEnumerable<string> employeeCodes);
    Task<int> CreateAsync(Equipment entity);
    Task<IReadOnlyCollection<int>> CreateManyAsync(IEnumerable<Equipment> entities);
    Task<bool> UpdateAsync(Equipment entity);
    Task<bool> DeleteAsync(int equipmentId);
    Task<int> DeleteManyAsync(IEnumerable<int> equipmentIds);
    Task<int> UpdateSectionManyAsync(IEnumerable<int> equipmentIds, int sectionId, string updatedBy, DateTime updatedAt);
    Task<int> UpdatePicManyAsync(IEnumerable<int> equipmentIds, int picId, string picCode, string picName, string updatedBy, DateTime updatedAt);
    Task<int> UpdateStatusManyAsync(IEnumerable<int> equipmentIds, string equipmentStatus, string updatedBy, DateTime updatedAt);
}
