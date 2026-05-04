using Project.Application.Common;
using Project.Application.DTOs;

namespace Project.Application.Interfaces;

public interface IEquipmentService
{
    Task<ApiResponse<PagedResult<EquipmentSummaryDto>>> GetAllAsync(EquipmentFilterParams filters);
    Task<ApiResponse<EquipmentDto>> GetByIdAsync(int equipmentId);
    Task<ApiResponse<EquipmentDto>> CreateAsync(CreateEquipmentRequest request, string? actorUsername, string? actorEmployeeCode);
    Task<ApiResponse<EquipmentDto>> UpdateAsync(int equipmentId, UpdateEquipmentRequest request, string? actorUsername, string? actorEmployeeCode);
    Task<ApiResponse> DeleteAsync(int equipmentId);
    Task<ApiResponse<BulkDeleteResultDto>> DeleteManyAsync(BulkDeleteEquipmentsRequest request);
    Task<ApiResponse<BulkUpdateResultDto>> BulkChangeSectionAsync(BulkSectionChangeRequest request, string? actorUsername, string? actorEmployeeCode);
    Task<ApiResponse<BulkUpdateResultDto>> BulkChangePicAsync(BulkPicChangeRequest request, string? actorUsername, string? actorEmployeeCode);
    Task<ApiResponse<BulkUpdateResultDto>> BulkChangeStatusAsync(BulkStatusChangeRequest request, string? actorUsername, string? actorEmployeeCode);
    Task<ApiResponse<EquipmentImportResultDto>> ImportAsync(Stream stream, string fileName, string? actorUsername, string? actorEmployeeCode);
    Task<FileContentDto> GetImportTemplateAsync();
    Task<FileContentDto> ExportAsync(EquipmentFilterParams filters);
}
