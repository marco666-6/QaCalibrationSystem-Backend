using FluentValidation;

namespace Project.Application.DTOs;

// ─── Default Locations ────────────────────────────────────────────────────────

public sealed record DefaultLocationDto(
    int DefaultLocationId,
    string DefaultLocationName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedBy
);

public sealed record UpsertDefaultLocationRequest(
    string DefaultLocationName,
    bool IsActive = true
);

public sealed class UpsertDefaultLocationValidator : AbstractValidator<UpsertDefaultLocationRequest>
{
    public UpsertDefaultLocationValidator()
    {
        RuleFor(x => x.DefaultLocationName)
            .NotEmpty().WithMessage("Location name is required.")
            .MaximumLength(200).WithMessage("Location name cannot exceed 200 characters.");
    }
}

// ─── Section Emails ───────────────────────────────────────────────────────────

public sealed record SectionEmailDto(
    int SectionEmailId,
    int? SectionId,
    string SectionCode,
    string SectionName,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record UpsertSectionEmailRequest(
    int? SectionId,
    string SectionCode,
    string SectionName,
    string Email,
    bool IsActive = true
);

public sealed class UpsertSectionEmailValidator : AbstractValidator<UpsertSectionEmailRequest>
{
    public UpsertSectionEmailValidator()
    {
        RuleFor(x => x.SectionCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SectionName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}

// ─── Section PIC Emails ───────────────────────────────────────────────────────

public sealed record SectionPicEmailDto(
    int SectionPicEmailId,
    int? SectionId,
    string SectionCode,
    string SectionName,
    string PicName,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record UpsertSectionPicEmailRequest(
    int? SectionId,
    string SectionCode,
    string SectionName,
    string PicName,
    string Email,
    bool IsActive = true
);

public sealed class UpsertSectionPicEmailValidator : AbstractValidator<UpsertSectionPicEmailRequest>
{
    public UpsertSectionPicEmailValidator()
    {
        RuleFor(x => x.SectionCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SectionName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PicName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}

// ─── Calib Roles ──────────────────────────────────────────────────────────────

public sealed record CalibRoleDto(
    int Id,
    int UserId,
    string Username,
    string? FullName,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);

public sealed record AssignCalibRoleRequest(
    int UserId,
    string Role
);

public sealed class AssignCalibRoleValidator : AbstractValidator<AssignCalibRoleRequest>
{
    private static readonly string[] ValidRoles = ["Preparer", "Checker", "Approver", "Technician"];
    public AssignCalibRoleValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => ValidRoles.Contains(r))
            .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}.");
    }
}

// ─── Externals ────────────────────────────────────────────────────────────────

public sealed record ExternalDto(
    int ExternalId,
    string ExternalCompany,
    string? ExternalEmail,
    string? ExternalPhone,
    string? Address,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record UpsertExternalRequest(
    string ExternalCompany,
    string? ExternalEmail,
    string? ExternalPhone,
    string? Address,
    bool IsActive = true
);

public sealed class UpsertExternalValidator : AbstractValidator<UpsertExternalRequest>
{
    public UpsertExternalValidator()
    {
        RuleFor(x => x.ExternalCompany).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ExternalEmail).EmailAddress().MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.ExternalEmail));
        RuleFor(x => x.ExternalPhone).MaximumLength(50);
        RuleFor(x => x.Address).MaximumLength(500);
    }
}

// ─── Equipments ───────────────────────────────────────────────────────────────

public sealed record EquipmentDto(
    int Id,
    string EquipmentName,
    string ControlNo,
    string? SerialNo,
    string? Brand,
    string? Model,
    string? Range,
    string? Location,
    int? SectionId,
    string SectionCode,
    string SectionName,
    int CalibIntervalMonths,
    DateOnly? LastCalibDate,
    DateOnly? NextCalibDate,
    string CalibType,
    string EquipmentStatus,
    string? Remarks,
    bool IsScrapped,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed class EquipmentFilterParams : Common.PaginationParams
{
    public string? Search { get; set; }
    public string? SectionCode { get; set; }
    public string? Status { get; set; }
    public string? CalibType { get; set; }
    public bool IsScrapped { get; set; } = false;
}

public sealed record UpsertEquipmentRequest(
    string EquipmentName,
    string ControlNo,
    string? SerialNo,
    string? Brand,
    string? Model,
    string? Range,
    string? Location,
    int? SectionId,
    string SectionCode,
    string SectionName,
    int CalibIntervalMonths,
    DateOnly? LastCalibDate,
    string CalibType,
    string EquipmentStatus = "Active",
    string? Remarks = null
);

public sealed class UpsertEquipmentValidator : AbstractValidator<UpsertEquipmentRequest>
{
    public UpsertEquipmentValidator()
    {
        RuleFor(x => x.EquipmentName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ControlNo).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SectionCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SectionName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CalibIntervalMonths).GreaterThan(0).LessThanOrEqualTo(120);
        RuleFor(x => x.CalibType).Must(t => t == "Internal" || t == "External")
            .WithMessage("CalibType must be 'Internal' or 'External'.");
        RuleFor(x => x.EquipmentStatus)
            .Must(s => new[] { "Active", "Out of Service", "Scrap" }.Contains(s));
    }
}

public sealed record BulkEquipmentRequest(
    List<int> Ids,
    string Action, // status | section | location | remarks | scrap | delete
    string? StatusValue,
    int? SectionId,
    string? SectionCode,
    string? SectionName,
    string? LocationValue,
    string? RemarksValue,
    string? ScrapReason
);

public sealed record EquipmentImportRowDto(
    string EquipmentName,
    string ControlNo,
    string? SerialNo,
    string? Brand,
    string? Model,
    string? Range,
    string? Location,
    string SectionCode,
    string SectionName,
    int CalibIntervalMonths,
    string? LastCalibDate,
    string CalibType
);

// ─── Calibration Plan ─────────────────────────────────────────────────────────

public sealed record CalibPlanSummaryDto(
    int PlanId,
    string PlanTitle,
    int PlanMonth,
    int PlanYear,
    string CalibType,
    string Status,
    bool IsLocked,
    string? PreparerUsername,
    string? CheckerUsername,
    string? ApproverUsername,
    DateTime? PreparerApprovedAt,
    DateTime? CheckerApprovedAt,
    DateTime? ApproverApprovedAt,
    DateTime? LockedAt,
    string? ReportPdfPath,
    int TotalItems,
    int IncludedItems,
    DateTime CreatedAt,
    string? CreatedBy
);

public sealed record CalibPlanDetailDto(
    int PlanId,
    string PlanTitle,
    int PlanMonth,
    int PlanYear,
    string CalibType,
    string Status,
    bool IsLocked,
    DateTime? LockedAt,
    string? ReportPdfPath,
    int? PreparerUserId,
    string? PreparerUsername,
    DateTime? PreparerApprovedAt,
    string? PreparerRemark,
    DateTime? PreparerCancelledAt,
    int? CheckerUserId,
    string? CheckerUsername,
    DateTime? CheckerApprovedAt,
    string? CheckerRemark,
    DateTime? CheckerCancelledAt,
    int? ApproverUserId,
    string? ApproverUsername,
    DateTime? ApproverApprovedAt,
    string? ApproverRemark,
    DateTime? ApproverCancelledAt,
    DateTime CreatedAt,
    string? CreatedBy,
    List<CalibPlanItemDto> Items,
    List<CalibTechnicianDto> Technicians,
    List<CalibExternalDto> Externals
);

public sealed record CalibPlanItemDto(
    int PlanItemId,
    int EquipmentId,
    string EquipmentName,
    string ControlNo,
    string? SerialNo,
    string? Brand,
    string? Model,
    string? Range,
    string? Location,
    string SectionCode,
    string SectionName,
    int CalibIntervalMonths,
    DateOnly? LastCalibDate,
    DateOnly? NextCalibDate,
    string CalibType,
    bool IsIncluded,
    string? Remarks
);

public sealed record CalibTechnicianDto(int UserId, string Username, string? FullName, bool IsPic);
public sealed record CalibExternalDto(int ExternalId, string ExternalCompany);

public sealed record CreateCalibPlanRequest(
    string PlanTitle,
    int PlanMonth,
    int PlanYear,
    string CalibType,
    int PreparerUserId,
    int CheckerUserId,
    int ApproverUserId,
    List<PlanItemInputDto> Items,
    List<int>? TechnicianUserIds,   // internal
    int? PicUserId,                  // internal
    List<int>? ExternalIds           // external
);

public sealed record PlanItemInputDto(
    int EquipmentId,
    string CalibType,   // can override per-item
    bool IsIncluded,
    string? Remarks
);

public sealed class CreateCalibPlanValidator : AbstractValidator<CreateCalibPlanRequest>
{
    public CreateCalibPlanValidator()
    {
        RuleFor(x => x.PlanTitle).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PlanMonth).InclusiveBetween(1, 12);
        RuleFor(x => x.PlanYear).InclusiveBetween(2000, 2100);
        RuleFor(x => x.CalibType).Must(t => t == "Internal" || t == "External");
        RuleFor(x => x.PreparerUserId).GreaterThan(0);
        RuleFor(x => x.CheckerUserId).GreaterThan(0);
        RuleFor(x => x.ApproverUserId).GreaterThan(0);
    }
}

public sealed record ApproveCalibPlanRequest(string? Remark);
public sealed record CancelCalibPlanApprovalRequest(string? Remark);

// ─── Calibration Actual ───────────────────────────────────────────────────────

public sealed record CalibActualSummaryDto(
    int ActualId,
    int PlanId,
    int PlanMonth,
    int PlanYear,
    string CalibType,
    string Status,
    bool IsClosed,
    DateTime? ClosedAt,
    string? CloseReason,
    bool ReportHasWatermark,
    string? ReportPdfPath,
    string? PreparerUsername,
    string? CheckerUsername,
    string? ApproverUsername,
    DateTime? PreparerApprovedAt,
    DateTime? CheckerApprovedAt,
    DateTime? ApproverApprovedAt,
    int TotalItems,
    int RecordedItems,
    int OkCount,
    int NgCount,
    DateTime CreatedAt
);

public sealed record CalibActualDetailDto(
    int ActualId,
    int PlanId,
    int PlanMonth,
    int PlanYear,
    string CalibType,
    string Status,
    bool IsClosed,
    DateTime? ClosedAt,
    string? CloseReason,
    bool ReportHasWatermark,
    string? ReportPdfPath,
    int? PreparerUserId,
    string? PreparerUsername,
    DateTime? PreparerApprovedAt,
    string? PreparerRemark,
    DateTime? PreparerCancelledAt,
    int? CheckerUserId,
    string? CheckerUsername,
    DateTime? CheckerApprovedAt,
    string? CheckerRemark,
    DateTime? CheckerCancelledAt,
    int? ApproverUserId,
    string? ApproverUsername,
    DateTime? ApproverApprovedAt,
    string? ApproverRemark,
    DateTime? ApproverCancelledAt,
    DateTime CreatedAt,
    List<CalibActualItemDto> Items,
    List<CalibTechnicianDto> Technicians,
    List<CalibExternalDto> Externals
);

public sealed record CalibActualItemDto(
    int ActualItemId,
    int EquipmentId,
    string EquipmentName,
    string ControlNo,
    string? SerialNo,
    string? Brand,
    string? Model,
    string? Range,
    string? Location,
    string SectionCode,
    string SectionName,
    string CalibType,
    string? StandardCalibration,
    string? CalibResult,
    string? NgAction,
    DateOnly? CalibDate,
    string? Remarks,
    string? RecordedBy,
    DateTime? RecordedAt
);

public sealed record RecordActualItemRequest(
    string? StandardCalibration,
    string? CalibResult,   // OK, NG, or null to clear
    string? NgAction,
    DateOnly? CalibDate,
    string? Remarks
);

public sealed class RecordActualItemValidator : AbstractValidator<RecordActualItemRequest>
{
    public RecordActualItemValidator()
    {
        RuleFor(x => x.CalibResult)
            .Must(r => r == null || r == "OK" || r == "NG")
            .WithMessage("CalibResult must be 'OK', 'NG', or null.");
        RuleFor(x => x.NgAction)
            .Must(a => a == null || new[] { "Repair", "Replacement", "None" }.Contains(a))
            .WithMessage("NgAction must be 'Repair', 'Replacement', 'None', or null.");
        RuleFor(x => x.NgAction).NotEmpty()
            .When(x => x.CalibResult == "NG")
            .WithMessage("NgAction is required when result is NG.");
    }
}

public sealed record SetStandardCalibrationRequest(string EquipmentName, string StandardCalibration);

public sealed record ApproveCalibActualRequest(string? Remark);
public sealed record CancelCalibActualApprovalRequest(string? Remark);
public sealed record CloseCalibActualRequest(string CloseReason = "Manual");

// ─── Out of Service ───────────────────────────────────────────────────────────

public sealed record OosRecordDto(
    int OosId,
    int EquipmentId,
    string EquipmentName,
    string ControlNo,
    string? SectionName,
    string NgAction,
    string? AssignedTo,
    DateOnly? ExpectedReturnDate,
    string? RepairDetails,
    string? ResolutionNote,
    bool IsResolved,
    DateTime? ResolvedAt,
    DateTime CreatedAt
);

public sealed record UpdateOosRecordRequest(
    string? AssignedTo,
    DateOnly? ExpectedReturnDate,
    string? RepairDetails,
    string? ResolutionNote,
    bool MarkResolved = false
);

// ─── Scrap Bin ────────────────────────────────────────────────────────────────

public sealed record ScrapEquipmentRequest(string? Reason);
public sealed record RestoreEquipmentRequest(string? Reason);

// ─── Dashboard / Summary ──────────────────────────────────────────────────────

public sealed record DashboardSummaryDto(
    int TotalEquipment,
    int ActiveEquipment,
    int OutOfServiceEquipment,
    int ScrappedEquipment,
    int DueThisMonth,
    int Overdue,
    int NeverCalibrated,
    int OpenPlans,
    int OpenActuals,
    int UnresolvedOos
);

// ─── Filter params ────────────────────────────────────────────────────────────

public sealed class CalibPlanFilterParams : Common.PaginationParams
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public string? Status { get; set; }
}

public sealed class CalibActualFilterParams : Common.PaginationParams
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public string? Status { get; set; }
}