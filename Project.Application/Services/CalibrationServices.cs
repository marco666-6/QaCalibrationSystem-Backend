using FluentValidation;
using Microsoft.Extensions.Logging;
using Project.Application.Common;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Entities;

namespace Project.Application.Services;

// ─── DefaultLocation Service ──────────────────────────────────────────────────

public sealed class DefaultLocationService : IDefaultLocationService
{
    private readonly IDefaultLocationRepository _repo;
    private readonly IValidator<UpsertDefaultLocationRequest> _validator;
    private readonly IExcelService _excel;

    public DefaultLocationService(IDefaultLocationRepository repo,
        IValidator<UpsertDefaultLocationRequest> validator, IExcelService excel)
    { _repo = repo; _validator = validator; _excel = excel; }

    public async Task<ApiResponse<IEnumerable<DefaultLocationDto>>> GetAllAsync(bool? isActive)
    {
        var items = await _repo.GetAllAsync(isActive);
        return ApiResponse<IEnumerable<DefaultLocationDto>>.Ok(items.Select(Map));
    }

    public async Task<ApiResponse<DefaultLocationDto>> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is null ? ApiResponse<DefaultLocationDto>.NotFound() : ApiResponse<DefaultLocationDto>.Ok(Map(item));
    }

    public async Task<ApiResponse<DefaultLocationDto>> UpsertAsync(int? id, UpsertDefaultLocationRequest request, string by)
    {
        var v = await _validator.ValidateAsync(request);
        if (!v.IsValid) return ApiResponse<DefaultLocationDto>.Fail("Validation failed.", v.Errors.Select(e => e.ErrorMessage));

        if (await _repo.ExistsAsync(request.DefaultLocationName, id))
            return ApiResponse<DefaultLocationDto>.Fail($"Location '{request.DefaultLocationName}' already exists.");

        var entity = new DefaultLocation
        {
            DefaultLocationId = id ?? 0,
            DefaultLocationName = request.DefaultLocationName.Trim(),
            IsActive = request.IsActive,
            CreatedBy = by,
            UpdatedBy = id.HasValue ? by : null
        };
        var newId = await _repo.UpsertAsync(entity);
        var result = await _repo.GetByIdAsync(newId);
        return ApiResponse<DefaultLocationDto>.Ok(Map(result!), id.HasValue ? "Updated." : "Created.");
    }

    public async Task<ApiResponse> DeleteAsync(int id)
    {
        var deleted = await _repo.DeleteAsync(id);
        return deleted ? ApiResponse.Ok("Deleted.") : ApiResponse.NotFound("Not found.");
    }

    public async Task<byte[]> ExportExcelAsync()
    {
        var items = await _repo.GetAllAsync();
        return _excel.ExportDefaultLocations(items.Select(Map));
    }

    public async Task<ApiResponse<int>> ImportExcelAsync(Stream stream, string by)
    {
        var rows = _excel.ImportDefaultLocations(stream);
        int count = 0;
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.DefaultLocationName)) continue;
            if (!await _repo.ExistsAsync(row.DefaultLocationName))
            {
                await _repo.UpsertAsync(new DefaultLocation
                {
                    DefaultLocationName = row.DefaultLocationName.Trim(),
                    IsActive = row.IsActive,
                    CreatedBy = by
                });
                count++;
            }
        }
        return ApiResponse<int>.Ok(count, $"{count} location(s) imported.");
    }

    private static DefaultLocationDto Map(DefaultLocation e) => new(
        e.DefaultLocationId, e.DefaultLocationName, e.IsActive, e.CreatedAt, e.UpdatedAt, e.CreatedBy);
}

// ─── SectionEmail Service ─────────────────────────────────────────────────────

public sealed class SectionEmailService : ISectionEmailService
{
    private readonly ISectionEmailRepository _repo;
    private readonly IValidator<UpsertSectionEmailRequest> _validator;
    private readonly IExcelService _excel;

    public SectionEmailService(ISectionEmailRepository repo,
        IValidator<UpsertSectionEmailRequest> validator, IExcelService excel)
    { _repo = repo; _validator = validator; _excel = excel; }

    public async Task<ApiResponse<IEnumerable<SectionEmailDto>>> GetAllAsync(bool? isActive)
    {
        var items = await _repo.GetAllAsync(isActive);
        return ApiResponse<IEnumerable<SectionEmailDto>>.Ok(items.Select(Map));
    }

    public async Task<ApiResponse<SectionEmailDto>> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is null ? ApiResponse<SectionEmailDto>.NotFound() : ApiResponse<SectionEmailDto>.Ok(Map(item));
    }

    public async Task<ApiResponse<SectionEmailDto>> UpsertAsync(int? id, UpsertSectionEmailRequest request, string by)
    {
        var v = await _validator.ValidateAsync(request);
        if (!v.IsValid) return ApiResponse<SectionEmailDto>.Fail("Validation failed.", v.Errors.Select(e => e.ErrorMessage));

        var entity = new SectionEmail
        {
            SectionEmailId = id ?? 0,
            SectionId = request.SectionId,
            SectionCode = request.SectionCode.Trim(),
            SectionName = request.SectionName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            IsActive = request.IsActive,
            CreatedBy = by,
            UpdatedBy = id.HasValue ? by : null
        };
        var newId = await _repo.UpsertAsync(entity);
        var result = await _repo.GetByIdAsync(newId);
        return ApiResponse<SectionEmailDto>.Ok(Map(result!), id.HasValue ? "Updated." : "Created.");
    }

    public async Task<ApiResponse> DeleteAsync(int id) =>
        await _repo.DeleteAsync(id) ? ApiResponse.Ok("Deleted.") : ApiResponse.NotFound();

    public async Task<byte[]> ExportExcelAsync()
    {
        var items = await _repo.GetAllAsync();
        return _excel.ExportSectionEmails(items.Select(Map));
    }

    public async Task<ApiResponse<int>> ImportExcelAsync(Stream stream, string by)
        => ApiResponse<int>.Ok(0, "Import not implemented for this entity.");

    private static SectionEmailDto Map(SectionEmail e) => new(
        e.SectionEmailId, e.SectionId, e.SectionCode, e.SectionName, e.Email, e.IsActive, e.CreatedAt, e.UpdatedAt);
}

// ─── SectionPicEmail Service ──────────────────────────────────────────────────

public sealed class SectionPicEmailService : ISectionPicEmailService
{
    private readonly ISectionPicEmailRepository _repo;
    private readonly IValidator<UpsertSectionPicEmailRequest> _validator;
    private readonly IExcelService _excel;

    public SectionPicEmailService(ISectionPicEmailRepository repo,
        IValidator<UpsertSectionPicEmailRequest> validator, IExcelService excel)
    { _repo = repo; _validator = validator; _excel = excel; }

    public async Task<ApiResponse<IEnumerable<SectionPicEmailDto>>> GetAllAsync(bool? isActive)
    {
        var items = await _repo.GetAllAsync(isActive);
        return ApiResponse<IEnumerable<SectionPicEmailDto>>.Ok(items.Select(Map));
    }

    public async Task<ApiResponse<SectionPicEmailDto>> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is null ? ApiResponse<SectionPicEmailDto>.NotFound() : ApiResponse<SectionPicEmailDto>.Ok(Map(item));
    }

    public async Task<ApiResponse<SectionPicEmailDto>> UpsertAsync(int? id, UpsertSectionPicEmailRequest request, string by)
    {
        var v = await _validator.ValidateAsync(request);
        if (!v.IsValid) return ApiResponse<SectionPicEmailDto>.Fail("Validation failed.", v.Errors.Select(e => e.ErrorMessage));

        var entity = new SectionPicEmail
        {
            SectionPicEmailId = id ?? 0,
            SectionId = request.SectionId,
            SectionCode = request.SectionCode.Trim(),
            SectionName = request.SectionName.Trim(),
            PicName = request.PicName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            IsActive = request.IsActive,
            CreatedBy = by,
            UpdatedBy = id.HasValue ? by : null
        };
        var newId = await _repo.UpsertAsync(entity);
        var result = await _repo.GetByIdAsync(newId);
        return ApiResponse<SectionPicEmailDto>.Ok(Map(result!), id.HasValue ? "Updated." : "Created.");
    }

    public async Task<ApiResponse> DeleteAsync(int id) =>
        await _repo.DeleteAsync(id) ? ApiResponse.Ok("Deleted.") : ApiResponse.NotFound();

    public async Task<byte[]> ExportExcelAsync()
    {
        var items = await _repo.GetAllAsync();
        return _excel.ExportSectionPicEmails(items.Select(Map));
    }

    public async Task<ApiResponse<int>> ImportExcelAsync(Stream stream, string by)
        => ApiResponse<int>.Ok(0, "Import not implemented.");

    private static SectionPicEmailDto Map(SectionPicEmail e) => new(
        e.SectionPicEmailId, e.SectionId, e.SectionCode, e.SectionName, e.PicName, e.Email, e.IsActive, e.CreatedAt, e.UpdatedAt);
}

// ─── CalibRole Service ────────────────────────────────────────────────────────

public sealed class CalibRoleService : ICalibRoleService
{
    private readonly ICalibRoleRepository _repo;
    private readonly IValidator<AssignCalibRoleRequest> _validator;
    private readonly IExcelService _excel;

    public CalibRoleService(ICalibRoleRepository repo,
        IValidator<AssignCalibRoleRequest> validator, IExcelService excel)
    { _repo = repo; _validator = validator; _excel = excel; }

    public async Task<ApiResponse<IEnumerable<CalibRoleDto>>> GetAllAsync(bool? isActive)
    {
        var items = await _repo.GetAllAsync(isActive);
        return ApiResponse<IEnumerable<CalibRoleDto>>.Ok(items.Select(Map));
    }

    public async Task<ApiResponse<CalibRoleDto>> AssignAsync(AssignCalibRoleRequest request, string by)
    {
        var v = await _validator.ValidateAsync(request);
        if (!v.IsValid) return ApiResponse<CalibRoleDto>.Fail("Validation failed.", v.Errors.Select(e => e.ErrorMessage));

        var existing = await _repo.GetByUserIdAsync(request.UserId);
        if (existing.Any(r => r.Role == request.Role && r.IsActive))
            return ApiResponse<CalibRoleDto>.Fail($"User already has role '{request.Role}'.");

        var entity = new CalibRole { UserId = request.UserId, Role = request.Role, CreatedBy = by };
        var newId = await _repo.CreateAsync(entity);
        var result = await _repo.GetByIdAsync(newId);
        return ApiResponse<CalibRoleDto>.Ok(Map(result!), "Role assigned.");
    }

    public async Task<ApiResponse> SetActiveAsync(int id, bool isActive, string by) =>
        await _repo.SetActiveAsync(id, isActive, by) ? ApiResponse.Ok("Updated.") : ApiResponse.NotFound();

    public async Task<ApiResponse> DeleteAsync(int id) =>
        await _repo.DeleteAsync(id) ? ApiResponse.Ok("Deleted.") : ApiResponse.NotFound();

    public async Task<byte[]> ExportExcelAsync()
    {
        var items = await _repo.GetAllAsync();
        return _excel.ExportCalibRoles(items.Select(Map));
    }

    public async Task<ApiResponse<int>> ImportExcelAsync(Stream stream, string by)
        => ApiResponse<int>.Ok(0, "Import not implemented.");

    private static CalibRoleDto Map(CalibRole r) => new(
        r.Id, r.UserId, r.Username ?? "", r.FullName, r.Role, r.IsActive, r.CreatedAt);
}

// ─── External Service ─────────────────────────────────────────────────────────

public sealed class ExternalService : IExternalService
{
    private readonly IExternalRepository _repo;
    private readonly IValidator<UpsertExternalRequest> _validator;
    private readonly IExcelService _excel;

    public ExternalService(IExternalRepository repo,
        IValidator<UpsertExternalRequest> validator, IExcelService excel)
    { _repo = repo; _validator = validator; _excel = excel; }

    public async Task<ApiResponse<IEnumerable<ExternalDto>>> GetAllAsync(bool? isActive)
    {
        var items = await _repo.GetAllAsync(isActive);
        return ApiResponse<IEnumerable<ExternalDto>>.Ok(items.Select(Map));
    }

    public async Task<ApiResponse<ExternalDto>> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is null ? ApiResponse<ExternalDto>.NotFound() : ApiResponse<ExternalDto>.Ok(Map(item));
    }

    public async Task<ApiResponse<ExternalDto>> UpsertAsync(int? id, UpsertExternalRequest request, string by)
    {
        var v = await _validator.ValidateAsync(request);
        if (!v.IsValid) return ApiResponse<ExternalDto>.Fail("Validation failed.", v.Errors.Select(e => e.ErrorMessage));

        var entity = new External
        {
            ExternalId = id ?? 0,
            ExternalCompany = request.ExternalCompany.Trim(),
            ExternalEmail = request.ExternalEmail?.Trim().ToLowerInvariant(),
            ExternalPhone = request.ExternalPhone?.Trim(),
            Address = request.Address?.Trim(),
            IsActive = request.IsActive,
            CreatedBy = by,
            UpdatedBy = id.HasValue ? by : null
        };
        var newId = await _repo.UpsertAsync(entity);
        var result = await _repo.GetByIdAsync(newId);
        return ApiResponse<ExternalDto>.Ok(Map(result!), id.HasValue ? "Updated." : "Created.");
    }

    public async Task<ApiResponse> DeleteAsync(int id) =>
        await _repo.DeleteAsync(id) ? ApiResponse.Ok("Deleted.") : ApiResponse.NotFound();

    public async Task<byte[]> ExportExcelAsync()
    {
        var items = await _repo.GetAllAsync();
        return _excel.ExportExternals(items.Select(Map));
    }

    public async Task<ApiResponse<int>> ImportExcelAsync(Stream stream, string by)
        => ApiResponse<int>.Ok(0, "Import not implemented.");

    private static ExternalDto Map(External e) => new(
        e.ExternalId, e.ExternalCompany, e.ExternalEmail, e.ExternalPhone, e.Address, e.IsActive, e.CreatedAt, e.UpdatedAt);
}

// ─── Equipment Service ────────────────────────────────────────────────────────

public sealed class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository _repo;
    private readonly IValidator<UpsertEquipmentRequest> _validator;
    private readonly IExcelService _excel;
    private readonly IQrCodeService _qr;

    public EquipmentService(IEquipmentRepository repo,
        IValidator<UpsertEquipmentRequest> validator, IExcelService excel, IQrCodeService qr)
    { _repo = repo; _validator = validator; _excel = excel; _qr = qr; }

    public async Task<ApiResponse<PagedResult<EquipmentDto>>> GetPagedAsync(EquipmentFilterParams filters)
    {
        var (items, total) = await _repo.GetPagedAsync(filters);
        return ApiResponse<PagedResult<EquipmentDto>>.Ok(
            PagedResult<EquipmentDto>.Create(items.Select(Map), total, filters));
    }

    public async Task<ApiResponse<EquipmentDto>> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is null ? ApiResponse<EquipmentDto>.NotFound() : ApiResponse<EquipmentDto>.Ok(Map(item));
    }

    public async Task<ApiResponse<EquipmentDto>> UpsertAsync(int? id, UpsertEquipmentRequest request, string by)
    {
        var v = await _validator.ValidateAsync(request);
        if (!v.IsValid) return ApiResponse<EquipmentDto>.Fail("Validation failed.", v.Errors.Select(e => e.ErrorMessage));

        // Check control_no uniqueness
        var existing = await _repo.GetByControlNoAsync(request.ControlNo);
        if (existing is not null && existing.Id != (id ?? 0))
            return ApiResponse<EquipmentDto>.Fail($"Control No '{request.ControlNo}' already exists.");

        var entity = new Equipment
        {
            Id = id ?? 0,
            EquipmentName = request.EquipmentName.Trim(),
            ControlNo = request.ControlNo.Trim(),
            SerialNo = request.SerialNo?.Trim(),
            Brand = request.Brand?.Trim(),
            Model = request.Model?.Trim(),
            Range = request.Range?.Trim(),
            Location = request.Location?.Trim(),
            SectionId = request.SectionId,
            SectionCode = request.SectionCode.Trim(),
            SectionName = request.SectionName.Trim(),
            CalibIntervalMonths = request.CalibIntervalMonths,
            LastCalibDate = request.LastCalibDate,
            CalibType = request.CalibType,
            EquipmentStatus = request.EquipmentStatus,
            Remarks = request.Remarks?.Trim()
        };

        var newId = await _repo.UpsertAsync(entity, by);
        var result = await _repo.GetByIdAsync(newId);
        return ApiResponse<EquipmentDto>.Ok(Map(result!), id.HasValue ? "Updated." : "Created.");
    }

    public async Task<ApiResponse> ScrapAsync(int id, string? reason, string by)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item is null) return ApiResponse.NotFound();
        await _repo.ScrapAsync(id, reason, by);
        return ApiResponse.Ok("Equipment moved to scrap bin.");
    }

    public async Task<ApiResponse> RestoreAsync(int id, string? reason, string by)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item is null || !item.IsScrapped) return ApiResponse.NotFound("Equipment not in scrap bin.");
        await _repo.RestoreAsync(id, reason, by);
        return ApiResponse.Ok("Equipment restored.");
    }

    public async Task<ApiResponse> HardDeleteAsync(int id, string by)
    {
        try
        {
            await _repo.HardDeleteAsync(id, by);
            return ApiResponse.Ok("Equipment permanently deleted.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse> BulkUpdateAsync(BulkEquipmentRequest request, string by)
    {
        if (request.Ids is null || request.Ids.Count == 0)
            return ApiResponse.Fail("No equipment IDs provided.");
        await _repo.BulkUpdateAsync(request.Ids, request.Action, request, by);
        return ApiResponse.Ok("Bulk update applied.");
    }

    public async Task<ApiResponse<IEnumerable<EquipmentDto>>> GetScrappedAsync()
    {
        var items = await _repo.GetScrappedAsync();
        return ApiResponse<IEnumerable<EquipmentDto>>.Ok(items.Select(Map));
    }

    public async Task<byte[]> ExportExcelAsync(EquipmentFilterParams filters)
    {
        filters.PageSize = 10000; filters.Page = 1;
        var (items, _) = await _repo.GetPagedAsync(filters);
        return _excel.ExportEquipments(items.Select(Map));
    }

    public async Task<byte[]> ExportSchedulesExcelAsync()
    {
        var items = await _repo.GetAllActiveAsync();
        return _excel.ExportEquipmentSchedules(items.Select(Map));
    }

    public async Task<ApiResponse<int>> ImportExcelAsync(Stream stream, string by)
    {
        var rows = _excel.ImportEquipments(stream);
        int count = 0;
        foreach (var row in rows)
        {
            var existing = await _repo.GetByControlNoAsync(row.ControlNo);
            DateOnly? lastCalib = null;
            if (!string.IsNullOrWhiteSpace(row.LastCalibDate) && DateOnly.TryParse(row.LastCalibDate, out var d))
                lastCalib = d;

            var entity = new Equipment
            {
                Id = existing?.Id ?? 0,
                EquipmentName = row.EquipmentName,
                ControlNo = row.ControlNo,
                SerialNo = row.SerialNo,
                Brand = row.Brand,
                Model = row.Model,
                Range = row.Range,
                Location = row.Location,
                SectionCode = row.SectionCode,
                SectionName = row.SectionName,
                CalibIntervalMonths = row.CalibIntervalMonths,
                LastCalibDate = lastCalib,
                CalibType = row.CalibType,
                EquipmentStatus = "Active"
            };
            await _repo.UpsertAsync(entity, by);
            count++;
        }
        return ApiResponse<int>.Ok(count, $"{count} equipment(s) imported.");
    }

    public async Task<byte[]> GenerateQrCodeAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item is null) throw new KeyNotFoundException($"Equipment {id} not found.");
        var content = $"CalibEquip|{item.Id}|{item.ControlNo}|{item.EquipmentName}";
        return _qr.GenerateQrCode(content);
    }

    public Task<byte[]> GetImportTemplateAsync()
        => Task.FromResult(_excel.GetEquipmentImportTemplate());

    private static EquipmentDto Map(Equipment e) => new(
        e.Id, e.EquipmentName, e.ControlNo, e.SerialNo, e.Brand, e.Model, e.Range,
        e.Location, e.SectionId, e.SectionCode, e.SectionName, e.CalibIntervalMonths,
        e.LastCalibDate, e.NextCalibDate, e.CalibType, e.EquipmentStatus, e.Remarks,
        e.IsScrapped, e.CreatedAt, e.UpdatedAt);
}

// ─── CalibPlan Service ────────────────────────────────────────────────────────

public sealed class CalibPlanService : ICalibPlanService
{
    private readonly ICalibPlanRepository _planRepo;
    private readonly IEquipmentRepository _equipRepo;
    private readonly IExternalRepository _externalRepo;
    private readonly IOosRepository _oosRepo;
    private readonly IPdfService _pdf;
    private readonly IValidator<CreateCalibPlanRequest> _validator;
    private readonly ILogger<CalibPlanService> _logger;

    public CalibPlanService(ICalibPlanRepository planRepo, IEquipmentRepository equipRepo,
        IExternalRepository externalRepo, IOosRepository oosRepo,
        IPdfService pdf, IValidator<CreateCalibPlanRequest> validator,
        ILogger<CalibPlanService> logger)
    {
        _planRepo = planRepo; _equipRepo = equipRepo;
        _externalRepo = externalRepo; _oosRepo = oosRepo;
        _pdf = pdf; _validator = validator; _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<CalibPlanSummaryDto>>> GetPagedAsync(CalibPlanFilterParams filters)
    {
        var (items, total) = await _planRepo.GetPagedAsync(filters);
        return ApiResponse<PagedResult<CalibPlanSummaryDto>>.Ok(
            PagedResult<CalibPlanSummaryDto>.Create(items.Select(MapSummary), total, filters));
    }

    public async Task<ApiResponse<CalibPlanDetailDto>> GetByIdAsync(int planId)
    {
        var plan = await _planRepo.GetDetailAsync(planId);
        return plan is null ? ApiResponse<CalibPlanDetailDto>.NotFound() : ApiResponse<CalibPlanDetailDto>.Ok(MapDetail(plan));
    }

    public async Task<ApiResponse<IEnumerable<CalibPlanItemDto>>> GetDueEquipmentsAsync(int month, int year)
    {
        var items = await _equipRepo.GetDueForPlanAsync(month, year);
        return ApiResponse<IEnumerable<CalibPlanItemDto>>.Ok(items.Select(MapEquipToItem));
    }

    public async Task<ApiResponse<CalibPlanDetailDto>> CreateAsync(CreateCalibPlanRequest request, string by)
    {
        var v = await _validator.ValidateAsync(request);
        if (!v.IsValid) return ApiResponse<CalibPlanDetailDto>.Fail("Validation failed.", v.Errors.Select(e => e.ErrorMessage));

        // Validate internal plan: 2-5 technicians, 1 PIC
        if (request.CalibType == "Internal")
        {
            if (request.TechnicianUserIds is null || request.TechnicianUserIds.Count < 2 || request.TechnicianUserIds.Count > 5)
                return ApiResponse<CalibPlanDetailDto>.Fail("Internal plans require 2-5 technicians.");
            if (!request.PicUserId.HasValue || !request.TechnicianUserIds.Contains(request.PicUserId.Value))
                return ApiResponse<CalibPlanDetailDto>.Fail("PIC must be one of the selected technicians.");
        }
        else if (request.CalibType == "External")
        {
            if (request.ExternalIds is null || request.ExternalIds.Count < 1 || request.ExternalIds.Count > 5)
                return ApiResponse<CalibPlanDetailDto>.Fail("External plans require 1-5 external companies.");
        }

        var plan = new CalibPlan
        {
            PlanTitle = request.PlanTitle,
            PlanMonth = request.PlanMonth,
            PlanYear = request.PlanYear,
            CalibType = request.CalibType,
            PreparerUserId = request.PreparerUserId,
            CheckerUserId = request.CheckerUserId,
            ApproverUserId = request.ApproverUserId,
            CreatedBy = by
        };

        var planId = await _planRepo.CreateAsync(plan);

        // Add items
        var planItems = new List<CalibPlanItem>();
        foreach (var item in request.Items)
        {
            var equip = await _equipRepo.GetByIdAsync(item.EquipmentId);
            if (equip is null) continue;
            planItems.Add(new CalibPlanItem
            {
                EquipmentId = equip.Id,
                EquipmentName = equip.EquipmentName,
                ControlNo = equip.ControlNo,
                SerialNo = equip.SerialNo,
                Brand = equip.Brand,
                Model = equip.Model,
                Range = equip.Range,
                Location = equip.Location,
                SectionCode = equip.SectionCode,
                SectionName = equip.SectionName,
                CalibIntervalMonths = equip.CalibIntervalMonths,
                LastCalibDate = equip.LastCalibDate,
                NextCalibDate = equip.NextCalibDate,
                CalibType = item.CalibType,
                IsIncluded = item.IsIncluded,
                Remarks = item.Remarks
            });
        }
        await _planRepo.AddItemsAsync(planId, planItems);

        // Add technicians (internal)
        if (request.CalibType == "Internal" && request.TechnicianUserIds is not null)
            await _planRepo.AddTechniciansAsync(planId, request.TechnicianUserIds.Select(uid =>
                new CalibPlanTechnician { UserId = uid, IsPic = uid == request.PicUserId }));

        // Add externals
        if (request.CalibType == "External" && request.ExternalIds is not null)
        {
            var extList = new List<CalibPlanExternal>();
            foreach (var eid in request.ExternalIds)
            {
                var ext = await _externalRepo.GetByIdAsync(eid);
                if (ext is not null)
                    extList.Add(new CalibPlanExternal { ExternalId = eid, ExternalCompany = ext.ExternalCompany });
            }
            await _planRepo.AddExternalsAsync(planId, extList);
        }

        var result = await _planRepo.GetDetailAsync(planId);
        return ApiResponse<CalibPlanDetailDto>.Ok(MapDetail(result!), "Calibration plan created.");
    }

    public async Task<ApiResponse> SubmitAsync(int planId, string by)
    {
        var plan = await _planRepo.GetByIdAsync(planId);
        if (plan is null) return ApiResponse.NotFound("Plan not found.");
        if (plan.Status != "Draft") return ApiResponse.Fail("Plan must be in Draft status to submit.");

        await _planRepo.SubmitAsync(planId, by);
        return ApiResponse.Ok("Plan submitted for approval.");
    }

    public async Task<ApiResponse> ApproveAsync(int planId, int userId, ApproveCalibPlanRequest request, string by)
    {
        try
        {
            var ok = await _planRepo.ApproveAsync(planId, userId, request.Remark, by);
            return ok ? ApiResponse.Ok("Approval recorded.") : ApiResponse.Fail("Could not process approval.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse> CancelApprovalAsync(int planId, int userId, CancelCalibPlanApprovalRequest request, string by)
    {
        try
        {
            var ok = await _planRepo.CancelApprovalAsync(planId, userId, request.Remark, by);
            return ok ? ApiResponse.Ok("Approval cancelled.") : ApiResponse.Fail("Could not cancel approval.");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<string>> LockAsync(int planId, string by)
    {
        var plan = await _planRepo.GetDetailAsync(planId);
        if (plan is null) return ApiResponse<string>.NotFound("Plan not found.");
        if (plan.Status != "Fully Approved") return ApiResponse<string>.Fail("Plan must be Fully Approved to lock.");

        // Generate PDF
        var pdfBytes = await _pdf.GenerateCalibPlanReportAsync(planId);
        var pdfFileName = $"plan_{planId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        var pdfDir = Path.Combine(AppContext.BaseDirectory, "uploads", "reports", "plans");
        Directory.CreateDirectory(pdfDir);
        var pdfPath = Path.Combine(pdfDir, pdfFileName);
        await File.WriteAllBytesAsync(pdfPath, pdfBytes);

        var relativePath = $"/uploads/reports/plans/{pdfFileName}";
        await _planRepo.LockAsync(planId, relativePath, by);

        return ApiResponse<string>.Ok(relativePath, "Plan locked. Actual calibration created.");
    }

    public async Task<ApiResponse<byte[]>> PreviewReportAsync(int planId)
    {
        var plan = await _planRepo.GetByIdAsync(planId);
        if (plan is null) return ApiResponse<byte[]>.NotFound();
        var pdf = await _pdf.GenerateCalibPlanReportAsync(planId);
        return ApiResponse<byte[]>.Ok(pdf);
    }

    public async Task<ApiResponse<string>> GetReportUrlAsync(int planId)
    {
        var plan = await _planRepo.GetByIdAsync(planId);
        if (plan is null) return ApiResponse<string>.NotFound();
        if (string.IsNullOrEmpty(plan.ReportPdfPath)) return ApiResponse<string>.Fail("Report not yet generated.");
        return ApiResponse<string>.Ok(plan.ReportPdfPath);
    }

    // ─── Mappers ─────────────────────────────────────────────────────────────

    private static CalibPlanSummaryDto MapSummary(CalibPlan p) => new(
        p.PlanId, p.PlanTitle, p.PlanMonth, p.PlanYear, p.CalibType, p.Status, p.IsLocked,
        p.PreparerUsername, p.CheckerUsername, p.ApproverUsername,
        p.PreparerApprovedAt, p.CheckerApprovedAt, p.ApproverApprovedAt,
        p.LockedAt, p.ReportPdfPath, p.TotalItems, p.IncludedItems, p.CreatedAt, p.CreatedBy);

    private static CalibPlanDetailDto MapDetail(CalibPlan p) => new(
        p.PlanId, p.PlanTitle, p.PlanMonth, p.PlanYear, p.CalibType, p.Status, p.IsLocked, p.LockedAt,
        p.ReportPdfPath,
        p.PreparerUserId, p.PreparerUsername, p.PreparerApprovedAt, p.PreparerRemark, p.PreparerCancelledAt,
        p.CheckerUserId, p.CheckerUsername, p.CheckerApprovedAt, p.CheckerRemark, p.CheckerCancelledAt,
        p.ApproverUserId, p.ApproverUsername, p.ApproverApprovedAt, p.ApproverRemark, p.ApproverCancelledAt,
        p.CreatedAt, p.CreatedBy,
        p.Items.Select(MapItem).ToList(),
        p.Technicians.Select(t => new CalibTechnicianDto(t.UserId, t.Username ?? "", t.FullName, t.IsPic)).ToList(),
        p.Externals.Select(e => new CalibExternalDto(e.ExternalId, e.ExternalCompany)).ToList());

    private static CalibPlanItemDto MapItem(CalibPlanItem i) => new(
        i.PlanItemId, i.EquipmentId, i.EquipmentName, i.ControlNo, i.SerialNo, i.Brand, i.Model,
        i.Range, i.Location, i.SectionCode, i.SectionName, i.CalibIntervalMonths,
        i.LastCalibDate, i.NextCalibDate, i.CalibType, i.IsIncluded, i.Remarks);

    private static CalibPlanItemDto MapEquipToItem(Equipment e) => new(
        0, e.Id, e.EquipmentName, e.ControlNo, e.SerialNo, e.Brand, e.Model, e.Range,
        e.Location, e.SectionCode, e.SectionName, e.CalibIntervalMonths,
        e.LastCalibDate, e.NextCalibDate, e.CalibType, true, e.Remarks);
}

// ─── CalibActual Service ──────────────────────────────────────────────────────

public sealed class CalibActualService : ICalibActualService
{
    private readonly ICalibActualRepository _actualRepo;
    private readonly IEquipmentRepository _equipRepo;
    private readonly IOosRepository _oosRepo;
    private readonly IPdfService _pdf;
    private readonly ILogger<CalibActualService> _logger;

    public CalibActualService(ICalibActualRepository actualRepo, IEquipmentRepository equipRepo,
        IOosRepository oosRepo, IPdfService pdf, ILogger<CalibActualService> logger)
    { _actualRepo = actualRepo; _equipRepo = equipRepo; _oosRepo = oosRepo; _pdf = pdf; _logger = logger; }

    public async Task<ApiResponse<PagedResult<CalibActualSummaryDto>>> GetPagedAsync(CalibActualFilterParams filters)
    {
        var (items, total) = await _actualRepo.GetPagedAsync(filters);
        return ApiResponse<PagedResult<CalibActualSummaryDto>>.Ok(
            PagedResult<CalibActualSummaryDto>.Create(items.Select(MapSummary), total, filters));
    }

    public async Task<ApiResponse<CalibActualDetailDto>> GetByIdAsync(int actualId)
    {
        var actual = await _actualRepo.GetDetailAsync(actualId);
        return actual is null ? ApiResponse<CalibActualDetailDto>.NotFound() : ApiResponse<CalibActualDetailDto>.Ok(MapDetail(actual));
    }

    public async Task<ApiResponse> RecordItemAsync(int actualId, int actualItemId, RecordActualItemRequest request, string by)
    {
        var actual = await _actualRepo.GetByIdAsync(actualId);
        if (actual is null) return ApiResponse.NotFound("Actual not found.");
        if (actual.IsClosed) return ApiResponse.Fail("Actual is closed and cannot be edited.");

        var updated = new CalibActualItem
        {
            StandardCalibration = request.StandardCalibration,
            CalibResult = request.CalibResult,
            NgAction = request.NgAction,
            CalibDate = request.CalibDate,
            Remarks = request.Remarks
        };

        await _actualRepo.RecordItemResultAsync(actualItemId, updated, by);

        // Update equipment last calib date when result is recorded
        var detail = await _actualRepo.GetDetailAsync(actualId);
        var item = detail?.Items.FirstOrDefault(i => i.ActualItemId == actualItemId);
        if (item is not null && request.CalibResult is not null && request.CalibDate.HasValue)
        {
            var equip = await _equipRepo.GetByIdAsync(item.EquipmentId);
            if (equip is not null)
            {
                var nextDate = request.CalibDate.Value.AddMonths(equip.CalibIntervalMonths);
                await _equipRepo.UpdateCalibResultDatesAsync(equip.Id, request.CalibDate.Value, nextDate);

                // Handle NG
                if (request.CalibResult == "NG")
                {
                    await _equipRepo.SetOutOfServiceAsync(equip.Id, by);
                    await _oosRepo.CreateAsync(new OutOfServiceRecord
                    {
                        EquipmentId = equip.Id,
                        ActualItemId = actualItemId,
                        NgAction = request.NgAction ?? "None",
                        CreatedBy = by
                    });
                }
            }
        }
        else if (request.CalibResult is null)
        {
            // Clearing result - optionally restore equipment to Active if it was set OOS by this item
        }

        return ApiResponse.Ok("Result recorded.");
    }

    public async Task<ApiResponse> SetStandardCalibrationAsync(int actualId, SetStandardCalibrationRequest request, string by)
    {
        await _actualRepo.SetStandardCalibrationAsync(actualId, request.EquipmentName, request.StandardCalibration, by);
        return ApiResponse.Ok("Standard calibration set for equipment group.");
    }

    public async Task<ApiResponse> ApproveAsync(int actualId, int userId, ApproveCalibActualRequest request, string by)
    {
        try
        {
            var ok = await _actualRepo.ApproveAsync(actualId, userId, request.Remark, by);

            // If now Fully Approved and already closed with watermark, regenerate PDF
            var actual = await _actualRepo.GetByIdAsync(actualId);
            if (actual?.Status == "Fully Approved" && actual.IsClosed && actual.ReportHasWatermark)
            {
                var pdfBytes = await _pdf.GenerateCalibActualReportAsync(actualId);
                var pdfFileName = $"actual_{actualId}_approved_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var pdfDir = Path.Combine(AppContext.BaseDirectory, "uploads", "reports", "actuals");
                Directory.CreateDirectory(pdfDir);
                await File.WriteAllBytesAsync(Path.Combine(pdfDir, pdfFileName), pdfBytes);
                var relativePath = $"/uploads/reports/actuals/{pdfFileName}";
                await _actualRepo.UpdatePdfAfterApprovalAsync(actualId, relativePath, by);
            }

            return ok ? ApiResponse.Ok("Approval recorded.") : ApiResponse.Fail("Could not process approval.");
        }
        catch (InvalidOperationException ex) { return ApiResponse.Fail(ex.Message); }
    }

    public async Task<ApiResponse> CancelApprovalAsync(int actualId, int userId, CancelCalibActualApprovalRequest request, string by)
    {
        try
        {
            var ok = await _actualRepo.CancelApprovalAsync(actualId, userId, request.Remark, by);
            return ok ? ApiResponse.Ok("Approval cancelled.") : ApiResponse.Fail("Could not cancel approval.");
        }
        catch (InvalidOperationException ex) { return ApiResponse.Fail(ex.Message); }
    }

    public async Task<ApiResponse<string>> CloseAsync(int actualId, CloseCalibActualRequest request, string by)
    {
        var actual = await _actualRepo.GetDetailAsync(actualId);
        if (actual is null) return ApiResponse<string>.NotFound();
        if (actual.IsClosed) return ApiResponse<string>.Fail("Actual is already closed.");

        var pdfBytes = await _pdf.GenerateCalibActualReportAsync(actualId);
        var pdfFileName = $"actual_{actualId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        var pdfDir = Path.Combine(AppContext.BaseDirectory, "uploads", "reports", "actuals");
        Directory.CreateDirectory(pdfDir);
        var pdfPath = Path.Combine(pdfDir, pdfFileName);
        await File.WriteAllBytesAsync(pdfPath, pdfBytes);
        var relativePath = $"/uploads/reports/actuals/{pdfFileName}";

        await _actualRepo.CloseAsync(actualId, relativePath, request.CloseReason, by);
        return ApiResponse<string>.Ok(relativePath, "Actual closed.");
    }

    public async Task<ApiResponse<byte[]>> PreviewReportAsync(int actualId)
    {
        var actual = await _actualRepo.GetByIdAsync(actualId);
        if (actual is null) return ApiResponse<byte[]>.NotFound();
        var pdf = await _pdf.GenerateCalibActualReportAsync(actualId);
        return ApiResponse<byte[]>.Ok(pdf);
    }

    public async Task<ApiResponse<string>> GetReportUrlAsync(int actualId)
    {
        var actual = await _actualRepo.GetByIdAsync(actualId);
        if (actual is null) return ApiResponse<string>.NotFound();
        if (string.IsNullOrEmpty(actual.ReportPdfPath)) return ApiResponse<string>.Fail("Report not yet generated.");
        return ApiResponse<string>.Ok(actual.ReportPdfPath);
    }

    // ─── Mappers ─────────────────────────────────────────────────────────────

    private static CalibActualSummaryDto MapSummary(CalibActual a) => new(
        a.ActualId, a.PlanId, a.PlanMonth, a.PlanYear, a.CalibType, a.Status,
        a.IsClosed, a.ClosedAt, a.CloseReason, a.ReportHasWatermark, a.ReportPdfPath,
        a.PreparerUsername, a.CheckerUsername, a.ApproverUsername,
        a.PreparerApprovedAt, a.CheckerApprovedAt, a.ApproverApprovedAt,
        a.TotalItems, a.RecordedItems, a.OkCount, a.NgCount, a.CreatedAt);

    private static CalibActualDetailDto MapDetail(CalibActual a) => new(
        a.ActualId, a.PlanId, a.PlanMonth, a.PlanYear, a.CalibType, a.Status,
        a.IsClosed, a.ClosedAt, a.CloseReason, a.ReportHasWatermark, a.ReportPdfPath,
        a.PreparerUserId, a.PreparerUsername, a.PreparerApprovedAt, a.PreparerRemark, a.PreparerCancelledAt,
        a.CheckerUserId, a.CheckerUsername, a.CheckerApprovedAt, a.CheckerRemark, a.CheckerCancelledAt,
        a.ApproverUserId, a.ApproverUsername, a.ApproverApprovedAt, a.ApproverRemark, a.ApproverCancelledAt,
        a.CreatedAt,
        a.Items.Select(i => new CalibActualItemDto(
            i.ActualItemId, i.EquipmentId, i.EquipmentName, i.ControlNo, i.SerialNo, i.Brand,
            i.Model, i.Range, i.Location, i.SectionCode, i.SectionName, i.CalibType,
            i.StandardCalibration, i.CalibResult, i.NgAction, i.CalibDate, i.Remarks,
            i.RecordedBy, i.RecordedAt)).ToList(),
        a.Technicians.Select(t => new CalibTechnicianDto(t.UserId, t.Username ?? "", t.FullName, t.IsPic)).ToList(),
        a.Externals.Select(e => new CalibExternalDto(e.ExternalId, e.ExternalCompany)).ToList());
}

// ─── OOS Service ──────────────────────────────────────────────────────────────

public sealed class OosService : IOosService
{
    private readonly IOosRepository _repo;
    public OosService(IOosRepository repo) => _repo = repo;

    public async Task<ApiResponse<IEnumerable<OosRecordDto>>> GetAllAsync(bool? isResolved)
    {
        var items = await _repo.GetAllAsync(isResolved);
        return ApiResponse<IEnumerable<OosRecordDto>>.Ok(items.Select(Map));
    }

    public async Task<ApiResponse<OosRecordDto>> GetByIdAsync(int oosId)
    {
        var item = await _repo.GetByIdAsync(oosId);
        return item is null ? ApiResponse<OosRecordDto>.NotFound() : ApiResponse<OosRecordDto>.Ok(Map(item));
    }

    public async Task<ApiResponse> UpdateAsync(int oosId, UpdateOosRecordRequest request, string by)
    {
        var ok = await _repo.UpdateAsync(oosId, request, by);
        return ok ? ApiResponse.Ok("OOS record updated.") : ApiResponse.NotFound();
    }

    private static OosRecordDto Map(OutOfServiceRecord r) => new(
        r.OosId, r.EquipmentId, r.EquipmentName ?? "", r.ControlNo ?? "", r.SectionName,
        r.NgAction, r.AssignedTo, r.ExpectedReturnDate, r.RepairDetails,
        r.ResolutionNote, r.IsResolved, r.ResolvedAt, r.CreatedAt);
}

// ─── Dashboard Service ────────────────────────────────────────────────────────

public sealed class DashboardService : IDashboardService
{
    private readonly IEquipmentRepository _equipRepo;
    private readonly ICalibPlanRepository _planRepo;
    private readonly ICalibActualRepository _actualRepo;
    private readonly IOosRepository _oosRepo;

    public DashboardService(IEquipmentRepository equipRepo, ICalibPlanRepository planRepo,
        ICalibActualRepository actualRepo, IOosRepository oosRepo)
    { _equipRepo = equipRepo; _planRepo = planRepo; _actualRepo = actualRepo; _oosRepo = oosRepo; }

    public async Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync()
    {
        var allEquip = (await _equipRepo.GetPagedAsync(new EquipmentFilterParams { PageSize = 100000, IsScrapped = false })).Items.ToList();
        var scrapped = (await _equipRepo.GetScrappedAsync()).Count();
        var openPlans = (await _planRepo.GetPagedAsync(new CalibPlanFilterParams { PageSize = 100000 })).Items.Where(p => p.Status != "Locked").Count();
        var openActuals = (await _actualRepo.GetPagedAsync(new CalibActualFilterParams { PageSize = 100000 })).Items.Where(a => !a.IsClosed).Count();
        var oosItems = await _oosRepo.GetAllAsync(false);

        var now = DateTime.Now;
        var dueThisMonth = allEquip.Count(e => e.NextCalibMonth == now.Month && e.NextCalibYear == now.Year);
        var overdue = allEquip.Count(e => e.NextCalibYear < now.Year || (e.NextCalibYear == now.Year && e.NextCalibMonth < now.Month));
        var neverCalib = allEquip.Count(e => e.LastCalibDate is null);

        return ApiResponse<DashboardSummaryDto>.Ok(new DashboardSummaryDto(
            allEquip.Count,
            allEquip.Count(e => e.EquipmentStatus == "Active"),
            allEquip.Count(e => e.EquipmentStatus == "Out of Service"),
            scrapped,
            dueThisMonth, overdue, neverCalib,
            openPlans, openActuals,
            oosItems.Count()
        ));
    }
}