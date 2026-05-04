using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Project.Application.Common;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Entities;

namespace Project.Application.Services;

public sealed class EquipmentService : IEquipmentService
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private static readonly string[] ImportHeaders =
    [
        "Equipment Name",
        "Control No",
        "Serial No",
        "Brand",
        "Model",
        "Location",
        "Section Code",
        "PIC Code",
        "Calib Interval Months",
        "Last Calib Date",
        "Calib Type",
        "Equipment Status",
        "Remarks"
    ];

    private readonly IEquipmentRepository _equipmentRepo;
    private readonly IUserRepository _userRepo;
    private readonly IValidator<CreateEquipmentRequest> _createValidator;
    private readonly IValidator<UpdateEquipmentRequest> _updateValidator;
    private readonly IValidator<BulkDeleteEquipmentsRequest> _bulkDeleteValidator;
    private readonly IValidator<BulkSectionChangeRequest> _bulkSectionChangeValidator;
    private readonly IValidator<BulkPicChangeRequest> _bulkPicChangeValidator;
    private readonly IValidator<BulkStatusChangeRequest> _bulkStatusChangeValidator;
    private readonly ILogger<EquipmentService> _logger;

    public EquipmentService(
        IEquipmentRepository equipmentRepo,
        IUserRepository userRepo,
        IValidator<CreateEquipmentRequest> createValidator,
        IValidator<UpdateEquipmentRequest> updateValidator,
        IValidator<BulkDeleteEquipmentsRequest> bulkDeleteValidator,
        IValidator<BulkSectionChangeRequest> bulkSectionChangeValidator,
        IValidator<BulkPicChangeRequest> bulkPicChangeValidator,
        IValidator<BulkStatusChangeRequest> bulkStatusChangeValidator,
        ILogger<EquipmentService> logger)
    {
        _equipmentRepo = equipmentRepo;
        _userRepo = userRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _bulkDeleteValidator = bulkDeleteValidator;
        _bulkSectionChangeValidator = bulkSectionChangeValidator;
        _bulkPicChangeValidator = bulkPicChangeValidator;
        _bulkStatusChangeValidator = bulkStatusChangeValidator;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<EquipmentSummaryDto>>> GetAllAsync(EquipmentFilterParams filters)
    {
        NormalizeFilters(filters);

        var (items, totalCount) = await _equipmentRepo.GetAllAsync(filters);
        var result = PagedResult<EquipmentSummaryDto>.Create(items.Select(MapToSummaryDto), totalCount, filters);

        return ApiResponse<PagedResult<EquipmentSummaryDto>>.Ok(result);
    }

    public async Task<ApiResponse<EquipmentDto>> GetByIdAsync(int equipmentId)
    {
        var entity = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (entity is null)
            return ApiResponse<EquipmentDto>.NotFound($"Equipment with ID {equipmentId} was not found.");

        return ApiResponse<EquipmentDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<EquipmentDto>> CreateAsync(CreateEquipmentRequest request, string? actorUsername, string? actorEmployeeCode)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<EquipmentDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));
        }

        var normalized = NormalizeCreateRequest(request);
        var actorCode = await ResolveActorEmployeeCodeAsync(actorUsername, actorEmployeeCode);
        if (string.IsNullOrWhiteSpace(actorCode))
            return ApiResponse<EquipmentDto>.Fail("Authenticated user is not linked to an employee code.");

        if (await _equipmentRepo.ControlNoExistsAsync(normalized.ControlNo))
            return ApiResponse<EquipmentDto>.Fail($"Control number '{normalized.ControlNo}' is already in use.");

        var section = await _equipmentRepo.GetSectionByIdAsync(normalized.SectionId);
        if (section is null || !section.IsActive)
            return ApiResponse<EquipmentDto>.Fail($"Section with ID {normalized.SectionId} was not found or is inactive.");

        var employee = await ResolveEmployeeAsync(normalized.PicId, normalized.PicCode);
        if (employee is null)
            return ApiResponse<EquipmentDto>.Fail("PIC was not found or is inactive.");

        var entity = new Equipment
        {
            EquipmentName = normalized.EquipmentName,
            ControlNo = normalized.ControlNo,
            SerialNo = NormalizeOptional(normalized.SerialNo),
            Brand = NormalizeOptional(normalized.Brand),
            Model = NormalizeOptional(normalized.Model),
            Location = normalized.Location,
            SectionId = section.SectionId,
            SectionCode = section.SectionCode,
            SectionName = section.SectionName,
            PicId = employee.EmployeeId,
            PicCode = employee.EmployeeCode,
            PicName = employee.FullName,
            CalibIntervalMonths = normalized.CalibIntervalMonths,
            LastCalibDate = normalized.LastCalibDate,
            CalibType = normalized.CalibType,
            EquipmentStatus = normalized.EquipmentStatus,
            Remarks = NormalizeOptional(normalized.Remarks),
            CreatedAt = DateTime.Now,
            CreatedBy = actorCode
        };

        var newId = await _equipmentRepo.CreateAsync(entity);
        _logger.LogInformation("Equipment created: {ControlNo} (ID {Id})", entity.ControlNo, newId);

        var created = await _equipmentRepo.GetByIdAsync(newId);
        return ApiResponse<EquipmentDto>.Created(MapToDto(created!));
    }

    public async Task<ApiResponse<EquipmentDto>> UpdateAsync(int equipmentId, UpdateEquipmentRequest request, string? actorUsername, string? actorEmployeeCode)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<EquipmentDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));
        }

        var existing = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (existing is null)
            return ApiResponse<EquipmentDto>.NotFound($"Equipment with ID {equipmentId} was not found.");

        var normalized = NormalizeUpdateRequest(request);
        var actorCode = await ResolveActorEmployeeCodeAsync(actorUsername, actorEmployeeCode);
        if (string.IsNullOrWhiteSpace(actorCode))
            return ApiResponse<EquipmentDto>.Fail("Authenticated user is not linked to an employee code.");

        if (await _equipmentRepo.ControlNoExistsAsync(normalized.ControlNo, equipmentId))
            return ApiResponse<EquipmentDto>.Fail($"Control number '{normalized.ControlNo}' is already in use.");

        var section = await _equipmentRepo.GetSectionByIdAsync(normalized.SectionId);
        if (section is null || !section.IsActive)
            return ApiResponse<EquipmentDto>.Fail($"Section with ID {normalized.SectionId} was not found or is inactive.");

        var employee = await ResolveEmployeeAsync(normalized.PicId, normalized.PicCode);
        if (employee is null)
            return ApiResponse<EquipmentDto>.Fail("PIC was not found or is inactive.");

        existing.EquipmentName = normalized.EquipmentName;
        existing.ControlNo = normalized.ControlNo;
        existing.SerialNo = NormalizeOptional(normalized.SerialNo);
        existing.Brand = NormalizeOptional(normalized.Brand);
        existing.Model = NormalizeOptional(normalized.Model);
        existing.Location = normalized.Location;
        existing.SectionId = section.SectionId;
        existing.SectionCode = section.SectionCode;
        existing.SectionName = section.SectionName;
        existing.PicId = employee.EmployeeId;
        existing.PicCode = employee.EmployeeCode;
        existing.PicName = employee.FullName;
        existing.CalibIntervalMonths = normalized.CalibIntervalMonths;
        existing.LastCalibDate = normalized.LastCalibDate;
        existing.CalibType = normalized.CalibType;
        existing.EquipmentStatus = normalized.EquipmentStatus;
        existing.Remarks = NormalizeOptional(normalized.Remarks);
        existing.UpdatedAt = DateTime.Now;
        existing.UpdatedBy = actorCode;

        await _equipmentRepo.UpdateAsync(existing);
        _logger.LogInformation("Equipment updated: {ControlNo} (ID {Id})", existing.ControlNo, equipmentId);

        var updated = await _equipmentRepo.GetByIdAsync(equipmentId);
        return ApiResponse<EquipmentDto>.Ok(MapToDto(updated!), "Updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(int equipmentId)
    {
        var entity = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (entity is null)
            return ApiResponse.NotFound($"Equipment with ID {equipmentId} was not found.");

        await _equipmentRepo.DeleteAsync(equipmentId);
        _logger.LogInformation("Equipment deleted: ID {Id}", equipmentId);

        return ApiResponse.Ok("Equipment deleted successfully.");
    }

    public async Task<ApiResponse<BulkDeleteResultDto>> DeleteManyAsync(BulkDeleteEquipmentsRequest request)
    {
        var validation = await _bulkDeleteValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<BulkDeleteResultDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));
        }

        var ids = request.Ids.Distinct().ToArray();
        var existingIds = (await _equipmentRepo.GetByIdsAsync(ids))
            .Select(item => item.Id)
            .Distinct()
            .ToArray();

        var notFoundIds = ids.Except(existingIds).OrderBy(id => id).ToArray();
        var deletedCount = existingIds.Length == 0
            ? 0
            : await _equipmentRepo.DeleteManyAsync(existingIds);

        var result = new BulkDeleteResultDto(ids.Length, deletedCount, notFoundIds);
        if (deletedCount == 0)
            return ApiResponse<BulkDeleteResultDto>.NotFound("No matching equipments were found.");

        _logger.LogInformation("Deleted {Count} equipments in bulk.", deletedCount);
        return ApiResponse<BulkDeleteResultDto>.Ok(
            result,
            notFoundIds.Length == 0
                ? "Equipments deleted successfully."
                : "Equipments deleted with some IDs not found.");
    }

    public async Task<ApiResponse<BulkUpdateResultDto>> BulkChangeSectionAsync(BulkSectionChangeRequest request, string? actorUsername, string? actorEmployeeCode)
    {
        var validation = await _bulkSectionChangeValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<BulkUpdateResultDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));
        }

        var actorCode = await ResolveActorEmployeeCodeAsync(actorUsername, actorEmployeeCode);
        if (string.IsNullOrWhiteSpace(actorCode))
            return ApiResponse<BulkUpdateResultDto>.Fail("Authenticated user is not linked to an employee code.");

        var section = await _equipmentRepo.GetSectionByIdAsync(request.SectionId);
        if (section is null || !section.IsActive)
            return ApiResponse<BulkUpdateResultDto>.Fail($"Section with ID {request.SectionId} was not found or is inactive.");

        var ids = request.Ids.Distinct().ToArray();
        var existingIds = (await _equipmentRepo.GetByIdsAsync(ids))
            .Select(item => item.Id)
            .Distinct()
            .ToArray();

        var notFoundIds = ids.Except(existingIds).OrderBy(id => id).ToArray();
        var updatedCount = existingIds.Length == 0
            ? 0
            : await _equipmentRepo.UpdateSectionManyAsync(existingIds, section.SectionId, actorCode, DateTime.Now);

        var result = new BulkUpdateResultDto(ids.Length, updatedCount, notFoundIds);
        if (updatedCount == 0)
            return ApiResponse<BulkUpdateResultDto>.NotFound("No matching equipments were found.");

        _logger.LogInformation("Bulk changed section for {Count} equipments to section ID {SectionId}.", updatedCount, section.SectionId);
        return ApiResponse<BulkUpdateResultDto>.Ok(
            result,
            notFoundIds.Length == 0
                ? "Equipment sections updated successfully."
                : "Equipment sections updated with some IDs not found.");
    }

    public async Task<ApiResponse<BulkUpdateResultDto>> BulkChangePicAsync(BulkPicChangeRequest request, string? actorUsername, string? actorEmployeeCode)
    {
        var validation = await _bulkPicChangeValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<BulkUpdateResultDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));
        }

        var actorCode = await ResolveActorEmployeeCodeAsync(actorUsername, actorEmployeeCode);
        if (string.IsNullOrWhiteSpace(actorCode))
            return ApiResponse<BulkUpdateResultDto>.Fail("Authenticated user is not linked to an employee code.");

        var employee = await ResolveEmployeeAsync(request.PicId, request.PicCode);
        if (employee is null)
            return ApiResponse<BulkUpdateResultDto>.Fail("PIC was not found or is inactive.");

        var ids = request.Ids.Distinct().ToArray();
        var existingIds = (await _equipmentRepo.GetByIdsAsync(ids))
            .Select(item => item.Id)
            .Distinct()
            .ToArray();

        var notFoundIds = ids.Except(existingIds).OrderBy(id => id).ToArray();
        var updatedCount = existingIds.Length == 0
            ? 0
            : await _equipmentRepo.UpdatePicManyAsync(existingIds, employee.EmployeeId, employee.EmployeeCode, employee.FullName, actorCode, DateTime.Now);

        var result = new BulkUpdateResultDto(ids.Length, updatedCount, notFoundIds);
        if (updatedCount == 0)
            return ApiResponse<BulkUpdateResultDto>.NotFound("No matching equipments were found.");

        _logger.LogInformation("Bulk changed PIC for {Count} equipments to employee {EmployeeCode}.", updatedCount, employee.EmployeeCode);
        return ApiResponse<BulkUpdateResultDto>.Ok(
            result,
            notFoundIds.Length == 0
                ? "Equipment PIC updated successfully."
                : "Equipment PIC updated with some IDs not found.");
    }

    public async Task<ApiResponse<BulkUpdateResultDto>> BulkChangeStatusAsync(BulkStatusChangeRequest request, string? actorUsername, string? actorEmployeeCode)
    {
        var validation = await _bulkStatusChangeValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ApiResponse<BulkUpdateResultDto>.Fail(
                "Validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));
        }

        var actorCode = await ResolveActorEmployeeCodeAsync(actorUsername, actorEmployeeCode);
        if (string.IsNullOrWhiteSpace(actorCode))
            return ApiResponse<BulkUpdateResultDto>.Fail("Authenticated user is not linked to an employee code.");

        EquipmentValueMappings.TryNormalizeEquipmentStatus(request.EquipmentStatus, out var normalizedStatus);

        var ids = request.Ids.Distinct().ToArray();
        var existingIds = (await _equipmentRepo.GetByIdsAsync(ids))
            .Select(item => item.Id)
            .Distinct()
            .ToArray();

        var notFoundIds = ids.Except(existingIds).OrderBy(id => id).ToArray();
        var updatedCount = existingIds.Length == 0
            ? 0
            : await _equipmentRepo.UpdateStatusManyAsync(existingIds, normalizedStatus, actorCode, DateTime.Now);

        var result = new BulkUpdateResultDto(ids.Length, updatedCount, notFoundIds);
        if (updatedCount == 0)
            return ApiResponse<BulkUpdateResultDto>.NotFound("No matching equipments were found.");

        _logger.LogInformation("Bulk changed status for {Count} equipments to {Status}.", updatedCount, normalizedStatus);
        return ApiResponse<BulkUpdateResultDto>.Ok(
            result,
            notFoundIds.Length == 0
                ? "Equipment status updated successfully."
                : "Equipment status updated with some IDs not found.");
    }

    public async Task<ApiResponse<EquipmentImportResultDto>> ImportAsync(Stream stream, string fileName, string? actorUsername, string? actorEmployeeCode)
    {
        if (stream is null || !stream.CanRead)
            return ApiResponse<EquipmentImportResultDto>.Fail("A readable Excel file is required.");

        if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<EquipmentImportResultDto>.Fail("Only .xlsx files are supported.");

        var actorCode = await ResolveActorEmployeeCodeAsync(actorUsername, actorEmployeeCode);
        if (string.IsNullOrWhiteSpace(actorCode))
            return ApiResponse<EquipmentImportResultDto>.Fail("Authenticated user is not linked to an employee code.");

        ConfigureExcelLicense();

        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet?.Dimension is null)
            return ApiResponse<EquipmentImportResultDto>.Fail("The uploaded Excel file is empty.");

        var headerErrors = ValidateImportHeaders(worksheet);
        if (headerErrors.Count > 0)
            return ApiResponse<EquipmentImportResultDto>.Fail("Import template headers are invalid.", headerErrors);

        var rows = ReadImportRows(worksheet);
        if (rows.Count == 0)
        {
            var emptyResult = new EquipmentImportResultDto(0, 0, 0, []);
            return ApiResponse<EquipmentImportResultDto>.Ok(emptyResult, "No data rows were found in the import file.");
        }

        var sectionsByCode = (await _equipmentRepo.GetSectionsByCodesAsync(rows
                .Select(row => row.SectionCode)
                .OfType<string>()
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)))
            .ToDictionary(item => item.SectionCode, StringComparer.OrdinalIgnoreCase);

        var employeesByCode = (await _equipmentRepo.GetEmployeesByCodesAsync(rows
                .Select(row => row.PicCode)
                .OfType<string>()
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)))
            .ToDictionary(item => item.EmployeeCode, StringComparer.OrdinalIgnoreCase);

        var existingControlNos = new HashSet<string>(
            await _equipmentRepo.GetExistingControlNumbersAsync(rows
                .Select(row => row.ControlNo)
                .OfType<string>()
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)),
            StringComparer.OrdinalIgnoreCase);

        var seenControlNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var validEntities = new List<Equipment>();
        var rowErrors = new List<EquipmentImportRowErrorDto>();
        var now = DateTime.Now;

        foreach (var row in rows)
        {
            var errors = ValidateImportRow(row, sectionsByCode, employeesByCode, existingControlNos, seenControlNos);
            if (errors.Count > 0)
            {
                rowErrors.Add(new EquipmentImportRowErrorDto(row.RowNumber, row.ControlNo, errors));
                continue;
            }

            EquipmentValueMappings.TryNormalizeCalibType(row.CalibType, out var calibType);
            EquipmentValueMappings.TryNormalizeEquipmentStatus(row.EquipmentStatus, out var equipmentStatus);

            var section = sectionsByCode[row.SectionCode!];
            var employee = employeesByCode[row.PicCode!];

            validEntities.Add(new Equipment
            {
                EquipmentName = row.EquipmentName!,
                ControlNo = row.ControlNo!,
                SerialNo = NormalizeOptional(row.SerialNo),
                Brand = NormalizeOptional(row.Brand),
                Model = NormalizeOptional(row.Model),
                Location = row.Location!,
                SectionId = section.SectionId,
                SectionCode = section.SectionCode,
                SectionName = section.SectionName,
                PicId = employee.EmployeeId,
                PicCode = employee.EmployeeCode,
                PicName = employee.FullName,
                CalibIntervalMonths = row.CalibIntervalMonths!.Value,
                LastCalibDate = row.LastCalibDate,
                CalibType = calibType,
                EquipmentStatus = equipmentStatus,
                Remarks = NormalizeOptional(row.Remarks),
                CreatedAt = now,
                CreatedBy = actorCode
            });
        }

        if (validEntities.Count > 0)
        {
            await _equipmentRepo.CreateManyAsync(validEntities);
        }

        var result = new EquipmentImportResultDto(
            rows.Count,
            validEntities.Count,
            rowErrors.Count,
            rowErrors);

        var message = rowErrors.Count == 0
            ? "Equipment import completed successfully."
            : "Equipment import completed with row-level errors.";

        _logger.LogInformation("Equipment import completed. Imported: {Imported}, Failed: {Failed}.", validEntities.Count, rowErrors.Count);
        return ApiResponse<EquipmentImportResultDto>.Ok(result, message);
    }

    public async Task<FileContentDto> GetImportTemplateAsync()
    {
        ConfigureExcelLicense();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Equipments");

        for (var index = 0; index < ImportHeaders.Length; index++)
        {
            worksheet.Cells[1, index + 1].Value = ImportHeaders[index];
        }

        using (var headerRange = worksheet.Cells[1, 1, 1, ImportHeaders.Length])
        {
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);
        }

        worksheet.View.FreezePanes(2, 1);
        worksheet.Cells.AutoFitColumns();

        var instructionSheet = package.Workbook.Worksheets.Add("Instructions");
        instructionSheet.Cells[1, 1].Value = "Column";
        instructionSheet.Cells[1, 2].Value = "Notes";
        instructionSheet.Cells[2, 1].Value = "Section Code";
        instructionSheet.Cells[2, 2].Value = "Use an active code from the sections master.";
        instructionSheet.Cells[3, 1].Value = "PIC Code";
        instructionSheet.Cells[3, 2].Value = "Use an active employee code from Shared.dbo.employees.";
        instructionSheet.Cells[4, 1].Value = "Calib Type";
        instructionSheet.Cells[4, 2].Value = "Allowed values: I or E.";
        instructionSheet.Cells[5, 1].Value = "Equipment Status";
        instructionSheet.Cells[5, 2].Value = "Allowed values: A, O, or S.";
        instructionSheet.Cells[6, 1].Value = "Last Calib Date";
        instructionSheet.Cells[6, 2].Value = "Leave blank or write 'No Record' if there is no calibration record yet.";
        instructionSheet.Cells.AutoFitColumns();

        var fileName = $"equipment-import-template-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return new FileContentDto(fileName, ExcelContentType, await package.GetAsByteArrayAsync());
    }

    public async Task<FileContentDto> ExportAsync(EquipmentFilterParams filters)
    {
        NormalizeFilters(filters);
        var items = (await _equipmentRepo.GetAllForExportAsync(filters))
            .OrderBy(item => item.ControlNo, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ConfigureExcelLicense();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Equipments");

        var headers = new[]
        {
            "Equipment Name",
            "Control No",
            "Serial No",
            "Brand",
            "Model",
            "Location",
            "Section Code",
            "Section Name",
            "PIC Code",
            "PIC Name",
            "Calib Interval Months",
            "Last Calib Date",
            "Next Calib Date",
            "Calib Type",
            "Equipment Status",
            "Remarks",
            "Created At",
            "Updated At",
            "Created By",
            "Updated By"
        };

        for (var index = 0; index < headers.Length; index++)
        {
            worksheet.Cells[1, index + 1].Value = headers[index];
        }

        using (var headerRange = worksheet.Cells[1, 1, 1, headers.Length])
        {
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);
        }

        var rowIndex = 2;
        foreach (var item in items)
        {
            worksheet.Cells[rowIndex, 1].Value = item.EquipmentName;
            worksheet.Cells[rowIndex, 2].Value = item.ControlNo;
            worksheet.Cells[rowIndex, 3].Value = item.SerialNo;
            worksheet.Cells[rowIndex, 4].Value = item.Brand;
            worksheet.Cells[rowIndex, 5].Value = item.Model;
            worksheet.Cells[rowIndex, 6].Value = item.Location;
            worksheet.Cells[rowIndex, 7].Value = item.SectionCode;
            worksheet.Cells[rowIndex, 8].Value = item.SectionName;
            worksheet.Cells[rowIndex, 9].Value = item.PicCode;
            worksheet.Cells[rowIndex, 10].Value = item.PicName;
            worksheet.Cells[rowIndex, 11].Value = item.CalibIntervalMonths;
            worksheet.Cells[rowIndex, 12].Value = item.LastCalibDate;
            worksheet.Cells[rowIndex, 13].Value = item.NextCalibDate;
            worksheet.Cells[rowIndex, 14].Value = item.CalibType;
            worksheet.Cells[rowIndex, 15].Value = item.EquipmentStatus;
            worksheet.Cells[rowIndex, 16].Value = item.Remarks;
            worksheet.Cells[rowIndex, 17].Value = item.CreatedAt;
            worksheet.Cells[rowIndex, 18].Value = item.UpdatedAt;
            worksheet.Cells[rowIndex, 19].Value = item.CreatedBy;
            worksheet.Cells[rowIndex, 20].Value = item.UpdatedBy;
            rowIndex++;
        }

        if (items.Count > 0)
        {
            worksheet.Cells[2, 12, rowIndex - 1, 13].Style.Numberformat.Format = "yyyy-mm-dd";
            worksheet.Cells[2, 17, rowIndex - 1, 18].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
        }

        worksheet.View.FreezePanes(2, 1);
        worksheet.Cells.AutoFitColumns();

        var fileName = $"equipments-export-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return new FileContentDto(fileName, ExcelContentType, await package.GetAsByteArrayAsync());
    }

    private static void NormalizeFilters(EquipmentFilterParams filters)
    {
        if (EquipmentValueMappings.TryNormalizeCalibType(filters.CalibType, out var normalizedCalibType))
            filters.CalibType = normalizedCalibType;

        if (EquipmentValueMappings.TryNormalizeEquipmentStatus(filters.EquipmentStatus, out var normalizedStatus))
            filters.EquipmentStatus = normalizedStatus;
    }

    private static CreateEquipmentRequest NormalizeCreateRequest(CreateEquipmentRequest request)
    {
        EquipmentValueMappings.TryNormalizeCalibType(request.CalibType, out var calibType);
        EquipmentValueMappings.TryNormalizeEquipmentStatus(request.EquipmentStatus, out var equipmentStatus);

        return request with
        {
            EquipmentName = request.EquipmentName.Trim(),
            ControlNo = request.ControlNo.Trim(),
            SerialNo = NormalizeOptional(request.SerialNo),
            Brand = NormalizeOptional(request.Brand),
            Model = NormalizeOptional(request.Model),
            Location = request.Location.Trim(),
            PicCode = NormalizeOptional(request.PicCode),
            CalibType = calibType,
            EquipmentStatus = equipmentStatus,
            Remarks = NormalizeOptional(request.Remarks)
        };
    }

    private static UpdateEquipmentRequest NormalizeUpdateRequest(UpdateEquipmentRequest request)
    {
        EquipmentValueMappings.TryNormalizeCalibType(request.CalibType, out var calibType);
        EquipmentValueMappings.TryNormalizeEquipmentStatus(request.EquipmentStatus, out var equipmentStatus);

        return request with
        {
            EquipmentName = request.EquipmentName.Trim(),
            ControlNo = request.ControlNo.Trim(),
            SerialNo = NormalizeOptional(request.SerialNo),
            Brand = NormalizeOptional(request.Brand),
            Model = NormalizeOptional(request.Model),
            Location = request.Location.Trim(),
            PicCode = NormalizeOptional(request.PicCode),
            CalibType = calibType,
            EquipmentStatus = equipmentStatus,
            Remarks = NormalizeOptional(request.Remarks)
        };
    }

    private async Task<string?> ResolveActorEmployeeCodeAsync(string? actorUsername, string? actorEmployeeCode)
    {
        if (!string.IsNullOrWhiteSpace(actorEmployeeCode))
            return actorEmployeeCode.Trim();

        if (string.IsNullOrWhiteSpace(actorUsername))
            return null;

        var user = await _userRepo.GetByUsernameAsync(actorUsername.Trim());
        return string.IsNullOrWhiteSpace(user?.Employee?.EmployeeCode)
            ? null
            : user.Employee.EmployeeCode;
    }

    private async Task<Employee?> ResolveEmployeeAsync(int? picId, string? picCode)
    {
        if (picId.HasValue)
        {
            var employeeById = await _equipmentRepo.GetEmployeeByIdAsync(picId.Value);
            if (employeeById is not null)
                return employeeById;
        }

        if (!string.IsNullOrWhiteSpace(picCode))
            return await _equipmentRepo.GetEmployeeByCodeAsync(picCode.Trim());

        return null;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static EquipmentDto MapToDto(Equipment entity) => new(
        entity.Id,
        entity.EquipmentName,
        entity.ControlNo,
        entity.SerialNo,
        entity.Brand,
        entity.Model,
        entity.Location,
        entity.SectionId,
        entity.SectionCode,
        entity.SectionName,
        entity.PicId,
        entity.PicCode,
        entity.PicName,
        entity.CalibIntervalMonths,
        entity.LastCalibDate,
        entity.LastCalibMonth,
        entity.LastCalibYear,
        entity.NextCalibDate,
        entity.NextCalibMonth,
        entity.NextCalibYear,
        entity.CalibType,
        entity.EquipmentStatus,
        entity.Remarks,
        entity.CreatedAt,
        entity.UpdatedAt,
        entity.CreatedBy,
        entity.UpdatedBy);

    private static EquipmentSummaryDto MapToSummaryDto(Equipment entity) => new(
        entity.Id,
        entity.EquipmentName,
        entity.ControlNo,
        entity.SerialNo,
        entity.Brand,
        entity.Model,
        entity.Location,
        entity.SectionId,
        entity.SectionCode,
        entity.SectionName,
        entity.PicId,
        entity.PicCode,
        entity.PicName,
        entity.CalibIntervalMonths,
        entity.LastCalibDate,
        entity.NextCalibDate,
        entity.CalibType,
        entity.EquipmentStatus,
        entity.Remarks);

    private static void ConfigureExcelLicense()
    {
        ExcelPackage.LicenseContext = LicenseContext.Commercial;
    }

    private static IReadOnlyCollection<string> ValidateImportHeaders(ExcelWorksheet worksheet)
    {
        var errors = new List<string>();
        for (var index = 0; index < ImportHeaders.Length; index++)
        {
            var actual = worksheet.Cells[1, index + 1].Text?.Trim();
            if (!string.Equals(actual, ImportHeaders[index], StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Column {index + 1} header must be '{ImportHeaders[index]}'.");
            }
        }

        return errors;
    }

    private static List<EquipmentImportRawRow> ReadImportRows(ExcelWorksheet worksheet)
    {
        var rows = new List<EquipmentImportRawRow>();
        var lastRow = worksheet.Dimension?.End.Row ?? 1;

        for (var rowIndex = 2; rowIndex <= lastRow; rowIndex++)
        {
            var values = Enumerable.Range(1, ImportHeaders.Length)
                .Select(column => worksheet.Cells[rowIndex, column].Text?.Trim())
                .ToArray();

            if (values.All(string.IsNullOrWhiteSpace))
                continue;

            var parsedLastCalibDate = ParseNullableDate(worksheet.Cells[rowIndex, 10].Value, values[9]);

            rows.Add(new EquipmentImportRawRow(
                rowIndex,
                values[0],
                values[1],
                values[2],
                values[3],
                values[4],
                values[5],
                values[6],
                values[7],
                ParseNullableInt(values[8]),
                parsedLastCalibDate.Value,
                parsedLastCalibDate.IsValid,
                values[9],
                values[10],
                values[11],
                values[12]));
        }

        return rows;
    }

    private static List<string> ValidateImportRow(
        EquipmentImportRawRow row,
        IReadOnlyDictionary<string, Section> sectionsByCode,
        IReadOnlyDictionary<string, Employee> employeesByCode,
        HashSet<string> existingControlNos,
        HashSet<string> seenControlNos)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(row.EquipmentName))
            errors.Add("Equipment Name is required.");
        else if (row.EquipmentName.Length > 200)
            errors.Add("Equipment Name cannot exceed 200 characters.");

        if (string.IsNullOrWhiteSpace(row.ControlNo))
        {
            errors.Add("Control No is required.");
        }
        else
        {
            if (row.ControlNo.Length > 100)
                errors.Add("Control No cannot exceed 100 characters.");

            if (existingControlNos.Contains(row.ControlNo))
                errors.Add($"Control No '{row.ControlNo}' already exists.");

            if (!seenControlNos.Add(row.ControlNo))
                errors.Add($"Control No '{row.ControlNo}' appears more than once in the import file.");
        }

        if (!string.IsNullOrWhiteSpace(row.SerialNo) && row.SerialNo.Length > 100)
            errors.Add("Serial No cannot exceed 100 characters.");

        if (!string.IsNullOrWhiteSpace(row.Brand) && row.Brand.Length > 100)
            errors.Add("Brand cannot exceed 100 characters.");

        if (!string.IsNullOrWhiteSpace(row.Model) && row.Model.Length > 100)
            errors.Add("Model cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(row.Location))
            errors.Add("Location is required.");
        else if (row.Location.Length > 200)
            errors.Add("Location cannot exceed 200 characters.");

        if (string.IsNullOrWhiteSpace(row.SectionCode))
        {
            errors.Add("Section Code is required.");
        }
        else if (!sectionsByCode.TryGetValue(row.SectionCode, out var section) || !section.IsActive)
        {
            errors.Add($"Section Code '{row.SectionCode}' was not found or is inactive.");
        }

        if (string.IsNullOrWhiteSpace(row.PicCode))
        {
            errors.Add("PIC Code is required.");
        }
        else if (!employeesByCode.ContainsKey(row.PicCode))
        {
            errors.Add($"PIC Code '{row.PicCode}' was not found or is inactive.");
        }

        if (!row.CalibIntervalMonths.HasValue)
            errors.Add("Calib Interval Months must be a valid integer.");
        else if (row.CalibIntervalMonths.Value <= 0)
            errors.Add("Calib Interval Months must be greater than 0.");

        if (!row.IsLastCalibDateValid)
            errors.Add("Last Calib Date must be a valid date or 'No Record'.");

        if (!EquipmentValueMappings.TryNormalizeCalibType(row.CalibType, out _))
            errors.Add("Calib Type must be I or E.");

        if (!EquipmentValueMappings.TryNormalizeEquipmentStatus(row.EquipmentStatus, out _))
            errors.Add("Equipment Status must be A, O, or S.");

        return errors;
    }

    private static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    private static ParsedDateValue ParseNullableDate(object? rawValue, string? displayValue)
    {
        if (string.IsNullOrWhiteSpace(displayValue) ||
            string.Equals(displayValue.Trim(), "No Record", StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedDateValue(null, true);
        }

        if (rawValue is DateTime dateTime)
            return new ParsedDateValue(dateTime.Date, true);

        if (DateTime.TryParse(displayValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return new ParsedDateValue(parsed.Date, true);

        if (DateTime.TryParse(displayValue, out parsed))
            return new ParsedDateValue(parsed.Date, true);

        return new ParsedDateValue(null, false);
    }

    private sealed record EquipmentImportRawRow(
        int RowNumber,
        string? EquipmentName,
        string? ControlNo,
        string? SerialNo,
        string? Brand,
        string? Model,
        string? Location,
        string? SectionCode,
        string? PicCode,
        int? CalibIntervalMonths,
        DateTime? LastCalibDate,
        bool IsLastCalibDateValid,
        string? LastCalibDateRaw,
        string? CalibType,
        string? EquipmentStatus,
        string? Remarks);

    private sealed record ParsedDateValue(
        DateTime? Value,
        bool IsValid);
}
