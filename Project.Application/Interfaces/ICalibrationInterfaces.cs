using Project.Application.Common;
using Project.Application.DTOs;
using Project.Domain.Entities;

namespace Project.Application.Interfaces;

// ─── Master Data Repositories ─────────────────────────────────────────────────

public interface IDefaultLocationRepository
{
    Task<IEnumerable<DefaultLocation>> GetAllAsync(bool? isActive = null);
    Task<DefaultLocation?> GetByIdAsync(int id);
    Task<int> UpsertAsync(DefaultLocation entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(string name, int? excludeId = null);
}

public interface ISectionEmailRepository
{
    Task<IEnumerable<SectionEmail>> GetAllAsync(bool? isActive = null);
    Task<SectionEmail?> GetByIdAsync(int id);
    Task<int> UpsertAsync(SectionEmail entity);
    Task<bool> DeleteAsync(int id);
}

public interface ISectionPicEmailRepository
{
    Task<IEnumerable<SectionPicEmail>> GetAllAsync(bool? isActive = null);
    Task<SectionPicEmail?> GetByIdAsync(int id);
    Task<int> UpsertAsync(SectionPicEmail entity);
    Task<bool> DeleteAsync(int id);
}

public interface ICalibRoleRepository
{
    Task<IEnumerable<CalibRole>> GetAllAsync(bool? isActive = null);
    Task<CalibRole?> GetByIdAsync(int id);
    Task<int> CreateAsync(CalibRole entity);
    Task<bool> SetActiveAsync(int id, bool isActive, string updatedBy);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CalibRole>> GetByUserIdAsync(int userId);
    Task<IEnumerable<CalibRole>> GetByRoleAsync(string role);
}

public interface IExternalRepository
{
    Task<IEnumerable<External>> GetAllAsync(bool? isActive = null);
    Task<External?> GetByIdAsync(int id);
    Task<int> UpsertAsync(External entity);
    Task<bool> DeleteAsync(int id);
}

public interface IEquipmentRepository
{
    Task<(IEnumerable<Equipment> Items, int TotalCount)> GetPagedAsync(EquipmentFilterParams filters);
    Task<Equipment?> GetByIdAsync(int id);
    Task<Equipment?> GetByControlNoAsync(string controlNo);
    Task<IEnumerable<Equipment>> GetDueForPlanAsync(int planMonth, int planYear);
    Task<int> UpsertAsync(Equipment entity, string by);
    Task<bool> ScrapAsync(int id, string? reason, string by);
    Task<bool> RestoreAsync(int id, string? reason, string by);
    Task<bool> HardDeleteAsync(int id, string by);
    Task<bool> BulkUpdateAsync(List<int> ids, string action, BulkEquipmentRequest request, string by);
    Task<bool> UpdateCalibResultDatesAsync(int equipmentId, DateOnly lastCalibDate, DateOnly nextCalibDate);
    Task<bool> SetOutOfServiceAsync(int equipmentId, string by);
    Task<IEnumerable<Equipment>> GetScrappedAsync();
    Task<IEnumerable<Equipment>> GetAllActiveAsync();
}

// ─── Calibration Plan Repository ──────────────────────────────────────────────

public interface ICalibPlanRepository
{
    Task<(IEnumerable<CalibPlan> Items, int TotalCount)> GetPagedAsync(CalibPlanFilterParams filters);
    Task<CalibPlan?> GetByIdAsync(int planId);
    Task<CalibPlan?> GetDetailAsync(int planId);
    Task<int> CreateAsync(CalibPlan plan);
    Task AddItemsAsync(int planId, IEnumerable<CalibPlanItem> items);
    Task AddTechniciansAsync(int planId, IEnumerable<CalibPlanTechnician> technicians);
    Task AddExternalsAsync(int planId, IEnumerable<CalibPlanExternal> externals);
    Task<bool> UpdateItemInclusionAsync(int planId, Dictionary<int, bool> equipmentInclusionMap);
    Task<bool> SubmitAsync(int planId, string by);
    Task<bool> ApproveAsync(int planId, int userId, string? remark, string by);
    Task<bool> CancelApprovalAsync(int planId, int userId, string? remark, string by);
    Task<bool> LockAsync(int planId, string? pdfPath, string by);
    Task<bool> UpdatePdfPathAsync(int planId, string pdfPath);
    Task<IEnumerable<CalibPlan>> GetExpiredFullyApprovedAsync();
}

// ─── Calibration Actual Repository ────────────────────────────────────────────

public interface ICalibActualRepository
{
    Task<(IEnumerable<CalibActual> Items, int TotalCount)> GetPagedAsync(CalibActualFilterParams filters);
    Task<CalibActual?> GetByIdAsync(int actualId);
    Task<CalibActual?> GetDetailAsync(int actualId);
    Task<CalibActual?> GetByPlanIdAsync(int planId);
    Task<bool> RecordItemResultAsync(int actualItemId, CalibActualItem updated, string by);
    Task<bool> SetStandardCalibrationAsync(int actualId, string equipmentName, string standardCalib, string by);
    Task<bool> ApproveAsync(int actualId, int userId, string? remark, string by);
    Task<bool> CancelApprovalAsync(int actualId, int userId, string? remark, string by);
    Task<bool> CloseAsync(int actualId, string? pdfPath, string closeReason, string by);
    Task<bool> UpdatePdfAfterApprovalAsync(int actualId, string pdfPath, string by);
    Task<IEnumerable<CalibActual>> GetOpenPastMonthsAsync();
}

// ─── OOS / Scrap Repositories ─────────────────────────────────────────────────

public interface IOosRepository
{
    Task<IEnumerable<OutOfServiceRecord>> GetAllAsync(bool? isResolved = null);
    Task<OutOfServiceRecord?> GetByIdAsync(int oosId);
    Task<int> CreateAsync(OutOfServiceRecord record);
    Task<bool> UpdateAsync(int oosId, UpdateOosRecordRequest request, string by);
}

// ─── Application Services ─────────────────────────────────────────────────────

public interface IDefaultLocationService
{
    Task<ApiResponse<IEnumerable<DefaultLocationDto>>> GetAllAsync(bool? isActive);
    Task<ApiResponse<DefaultLocationDto>> GetByIdAsync(int id);
    Task<ApiResponse<DefaultLocationDto>> UpsertAsync(int? id, UpsertDefaultLocationRequest request, string by);
    Task<ApiResponse> DeleteAsync(int id);
    Task<byte[]> ExportExcelAsync();
    Task<ApiResponse<int>> ImportExcelAsync(Stream stream, string by);
}

public interface ISectionEmailService
{
    Task<ApiResponse<IEnumerable<SectionEmailDto>>> GetAllAsync(bool? isActive);
    Task<ApiResponse<SectionEmailDto>> GetByIdAsync(int id);
    Task<ApiResponse<SectionEmailDto>> UpsertAsync(int? id, UpsertSectionEmailRequest request, string by);
    Task<ApiResponse> DeleteAsync(int id);
    Task<byte[]> ExportExcelAsync();
    Task<ApiResponse<int>> ImportExcelAsync(Stream stream, string by);
}

public interface ISectionPicEmailService
{
    Task<ApiResponse<IEnumerable<SectionPicEmailDto>>> GetAllAsync(bool? isActive);
    Task<ApiResponse<SectionPicEmailDto>> GetByIdAsync(int id);
    Task<ApiResponse<SectionPicEmailDto>> UpsertAsync(int? id, UpsertSectionPicEmailRequest request, string by);
    Task<ApiResponse> DeleteAsync(int id);
    Task<byte[]> ExportExcelAsync();
    Task<ApiResponse<int>> ImportExcelAsync(Stream stream, string by);
}

public interface ICalibRoleService
{
    Task<ApiResponse<IEnumerable<CalibRoleDto>>> GetAllAsync(bool? isActive);
    Task<ApiResponse<CalibRoleDto>> AssignAsync(AssignCalibRoleRequest request, string by);
    Task<ApiResponse> SetActiveAsync(int id, bool isActive, string by);
    Task<ApiResponse> DeleteAsync(int id);
    Task<byte[]> ExportExcelAsync();
    Task<ApiResponse<int>> ImportExcelAsync(Stream stream, string by);
}

public interface IExternalService
{
    Task<ApiResponse<IEnumerable<ExternalDto>>> GetAllAsync(bool? isActive);
    Task<ApiResponse<ExternalDto>> GetByIdAsync(int id);
    Task<ApiResponse<ExternalDto>> UpsertAsync(int? id, UpsertExternalRequest request, string by);
    Task<ApiResponse> DeleteAsync(int id);
    Task<byte[]> ExportExcelAsync();
    Task<ApiResponse<int>> ImportExcelAsync(Stream stream, string by);
}

public interface IEquipmentService
{
    Task<ApiResponse<PagedResult<EquipmentDto>>> GetPagedAsync(EquipmentFilterParams filters);
    Task<ApiResponse<EquipmentDto>> GetByIdAsync(int id);
    Task<ApiResponse<EquipmentDto>> UpsertAsync(int? id, UpsertEquipmentRequest request, string by);
    Task<ApiResponse> ScrapAsync(int id, string? reason, string by);
    Task<ApiResponse> RestoreAsync(int id, string? reason, string by);
    Task<ApiResponse> HardDeleteAsync(int id, string by);
    Task<ApiResponse> BulkUpdateAsync(BulkEquipmentRequest request, string by);
    Task<ApiResponse<IEnumerable<EquipmentDto>>> GetScrappedAsync();
    Task<byte[]> ExportExcelAsync(EquipmentFilterParams filters);
    Task<byte[]> ExportSchedulesExcelAsync();
    Task<ApiResponse<int>> ImportExcelAsync(Stream stream, string by);
    Task<byte[]> GenerateQrCodeAsync(int id);
    Task<byte[]> GetImportTemplateAsync();
}

public interface ICalibPlanService
{
    Task<ApiResponse<PagedResult<CalibPlanSummaryDto>>> GetPagedAsync(CalibPlanFilterParams filters);
    Task<ApiResponse<CalibPlanDetailDto>> GetByIdAsync(int planId);
    Task<ApiResponse<IEnumerable<CalibPlanItemDto>>> GetDueEquipmentsAsync(int month, int year);
    Task<ApiResponse<CalibPlanDetailDto>> CreateAsync(CreateCalibPlanRequest request, string by);
    Task<ApiResponse> SubmitAsync(int planId, string by);
    Task<ApiResponse> ApproveAsync(int planId, int userId, ApproveCalibPlanRequest request, string by);
    Task<ApiResponse> CancelApprovalAsync(int planId, int userId, CancelCalibPlanApprovalRequest request, string by);
    Task<ApiResponse<string>> LockAsync(int planId, string by);
    Task<ApiResponse<byte[]>> PreviewReportAsync(int planId);
    Task<ApiResponse<string>> GetReportUrlAsync(int planId);
}

public interface ICalibActualService
{
    Task<ApiResponse<PagedResult<CalibActualSummaryDto>>> GetPagedAsync(CalibActualFilterParams filters);
    Task<ApiResponse<CalibActualDetailDto>> GetByIdAsync(int actualId);
    Task<ApiResponse> RecordItemAsync(int actualId, int actualItemId, RecordActualItemRequest request, string by);
    Task<ApiResponse> SetStandardCalibrationAsync(int actualId, SetStandardCalibrationRequest request, string by);
    Task<ApiResponse> ApproveAsync(int actualId, int userId, ApproveCalibActualRequest request, string by);
    Task<ApiResponse> CancelApprovalAsync(int actualId, int userId, CancelCalibActualApprovalRequest request, string by);
    Task<ApiResponse<string>> CloseAsync(int actualId, CloseCalibActualRequest request, string by);
    Task<ApiResponse<byte[]>> PreviewReportAsync(int actualId);
    Task<ApiResponse<string>> GetReportUrlAsync(int actualId);
}

public interface IOosService
{
    Task<ApiResponse<IEnumerable<OosRecordDto>>> GetAllAsync(bool? isResolved);
    Task<ApiResponse<OosRecordDto>> GetByIdAsync(int oosId);
    Task<ApiResponse> UpdateAsync(int oosId, UpdateOosRecordRequest request, string by);
}

public interface IDashboardService
{
    Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync();
}

public interface IPdfService
{
    Task<byte[]> GenerateCalibPlanReportAsync(int planId);
    Task<byte[]> GenerateCalibActualReportAsync(int actualId);
}

public interface IEmailService
{
    Task SendApprovalNotificationAsync(string toEmail, string toName, string subject, string body);
    Task SendCalibrationReminderAsync(IEnumerable<string> toEmails, string equipmentName, string controlNo, DateOnly nextCalibDate);
}

public interface IQrCodeService
{
    byte[] GenerateQrCode(string content, int size = 300);
}

public interface IExcelService
{
    byte[] ExportDefaultLocations(IEnumerable<DefaultLocationDto> data);
    byte[] ExportSectionEmails(IEnumerable<SectionEmailDto> data);
    byte[] ExportSectionPicEmails(IEnumerable<SectionPicEmailDto> data);
    byte[] ExportCalibRoles(IEnumerable<CalibRoleDto> data);
    byte[] ExportExternals(IEnumerable<ExternalDto> data);
    byte[] ExportEquipments(IEnumerable<EquipmentDto> data);
    byte[] ExportEquipmentSchedules(IEnumerable<EquipmentDto> data);
    byte[] GetEquipmentImportTemplate();
    IEnumerable<EquipmentImportRowDto> ImportEquipments(Stream stream);
    IEnumerable<DefaultLocationDto> ImportDefaultLocations(Stream stream);
}