namespace Project.Domain.Entities;

// ─── Master Tables ────────────────────────────────────────────────────────────

public class DefaultLocation
{
    public int DefaultLocationId { get; set; }
    public string DefaultLocationName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class SectionEmail
{
    public int SectionEmailId { get; set; }
    public int? SectionId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class SectionPicEmail
{
    public int SectionPicEmailId { get; set; }
    public int? SectionId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string PicName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class CalibRole
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty; // Preparer, Checker, Approver, Technician
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    // navigation
    public string? Username { get; set; }
    public string? FullName { get; set; }
}

public class External
{
    public int ExternalId { get; set; }
    public string ExternalCompany { get; set; } = string.Empty;
    public string? ExternalEmail { get; set; }
    public string? ExternalPhone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class Equipment
{
    public int Id { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string ControlNo { get; set; } = string.Empty;
    public string? SerialNo { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Range { get; set; }
    public string? Location { get; set; }
    public int? SectionId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public int CalibIntervalMonths { get; set; } = 12;
    public DateOnly? LastCalibDate { get; set; }
    public DateOnly? NextCalibDate { get; set; }
    public int? NextCalibMonth { get; set; }
    public int? NextCalibYear { get; set; }
    public string CalibType { get; set; } = "Internal"; // Internal or External
    public string EquipmentStatus { get; set; } = "Active"; // Active, Out of Service, Scrap
    public string? Remarks { get; set; }
    public bool IsScrapped { get; set; }
    public DateTime? ScrappedAt { get; set; }
    public string? ScrappedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

// ─── Calibration Plan ─────────────────────────────────────────────────────────

public class CalibPlan
{
    public int PlanId { get; set; }
    public string PlanTitle { get; set; } = string.Empty;
    public int PlanMonth { get; set; }
    public int PlanYear { get; set; }
    public string CalibType { get; set; } = "Internal";
    public string Status { get; set; } = "Draft";
    public bool IsLocked { get; set; }
    public DateTime? LockedAt { get; set; }
    public string? ReportPdfPath { get; set; }

    public int? PreparerUserId { get; set; }
    public int? CheckerUserId { get; set; }
    public int? ApproverUserId { get; set; }

    public DateTime? PreparerApprovedAt { get; set; }
    public string? PreparerRemark { get; set; }
    public DateTime? CheckerApprovedAt { get; set; }
    public string? CheckerRemark { get; set; }
    public DateTime? ApproverApprovedAt { get; set; }
    public string? ApproverRemark { get; set; }

    public DateTime? PreparerCancelledAt { get; set; }
    public DateTime? CheckerCancelledAt { get; set; }
    public DateTime? ApproverCancelledAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // nav
    public string? PreparerUsername { get; set; }
    public string? CheckerUsername { get; set; }
    public string? ApproverUsername { get; set; }
    public int TotalItems { get; set; }
    public int IncludedItems { get; set; }

    public List<CalibPlanItem> Items { get; set; } = [];
    public List<CalibPlanTechnician> Technicians { get; set; } = [];
    public List<CalibPlanExternal> Externals { get; set; } = [];
}

public class CalibPlanItem
{
    public int PlanItemId { get; set; }
    public int PlanId { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string ControlNo { get; set; } = string.Empty;
    public string? SerialNo { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Range { get; set; }
    public string? Location { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public int CalibIntervalMonths { get; set; }
    public DateOnly? LastCalibDate { get; set; }
    public DateOnly? NextCalibDate { get; set; }
    public string CalibType { get; set; } = "Internal";
    public bool IsIncluded { get; set; } = true;
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CalibPlanTechnician
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public int UserId { get; set; }
    public bool IsPic { get; set; }
    public DateTime CreatedAt { get; set; }
    // nav
    public string? Username { get; set; }
    public string? FullName { get; set; }
}

public class CalibPlanExternal
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public int ExternalId { get; set; }
    public string ExternalCompany { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ─── Calibration Actual ───────────────────────────────────────────────────────

public class CalibActual
{
    public int ActualId { get; set; }
    public int PlanId { get; set; }
    public int PlanMonth { get; set; }
    public int PlanYear { get; set; }
    public string CalibType { get; set; } = "Internal";
    public string Status { get; set; } = "In Progress";
    public bool IsClosed { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosedBy { get; set; }
    public string? CloseReason { get; set; }
    public string? ReportPdfPath { get; set; }
    public bool ReportHasWatermark { get; set; }

    public int? PreparerUserId { get; set; }
    public int? CheckerUserId { get; set; }
    public int? ApproverUserId { get; set; }

    public DateTime? PreparerApprovedAt { get; set; }
    public string? PreparerRemark { get; set; }
    public DateTime? CheckerApprovedAt { get; set; }
    public string? CheckerRemark { get; set; }
    public DateTime? ApproverApprovedAt { get; set; }
    public string? ApproverRemark { get; set; }

    public DateTime? PreparerCancelledAt { get; set; }
    public DateTime? CheckerCancelledAt { get; set; }
    public DateTime? ApproverCancelledAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // nav
    public string? PreparerUsername { get; set; }
    public string? CheckerUsername { get; set; }
    public string? ApproverUsername { get; set; }
    public int TotalItems { get; set; }
    public int RecordedItems { get; set; }
    public int OkCount { get; set; }
    public int NgCount { get; set; }

    public List<CalibActualItem> Items { get; set; } = [];
    public List<CalibActualTechnician> Technicians { get; set; } = [];
    public List<CalibActualExternal> Externals { get; set; } = [];
}

public class CalibActualItem
{
    public int ActualItemId { get; set; }
    public int ActualId { get; set; }
    public int PlanItemId { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string ControlNo { get; set; } = string.Empty;
    public string? SerialNo { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Range { get; set; }
    public string? Location { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string CalibType { get; set; } = "Internal";
    public string? StandardCalibration { get; set; }
    public string? CalibResult { get; set; }   // OK, NG, null
    public string? NgAction { get; set; }       // Repair, Replacement, None
    public DateOnly? CalibDate { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? RecordedBy { get; set; }
    public DateTime? RecordedAt { get; set; }
}

public class CalibActualTechnician
{
    public int Id { get; set; }
    public int ActualId { get; set; }
    public int UserId { get; set; }
    public bool IsPic { get; set; }
    public DateTime CreatedAt { get; set; }
    // nav
    public string? Username { get; set; }
    public string? FullName { get; set; }
}

public class CalibActualExternal
{
    public int Id { get; set; }
    public int ActualId { get; set; }
    public int ExternalId { get; set; }
    public string ExternalCompany { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ─── Follow-up / Maintenance ──────────────────────────────────────────────────

public class OutOfServiceRecord
{
    public int OosId { get; set; }
    public int EquipmentId { get; set; }
    public int? ActualItemId { get; set; }
    public string NgAction { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateOnly? ExpectedReturnDate { get; set; }
    public string? RepairDetails { get; set; }
    public string? ResolutionNote { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public bool IsResolved { get; set; }
    public string? ResolvedStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    // nav
    public string? EquipmentName { get; set; }
    public string? ControlNo { get; set; }
    public string? SectionName { get; set; }
}

public class ScrapRecord
{
    public int ScrapRecordId { get; set; }
    public int EquipmentId { get; set; }
    public string Action { get; set; } = string.Empty; // Scrap, Restore, Delete
    public string? Reason { get; set; }
    public DateTime ActionedAt { get; set; }
    public string? ActionedBy { get; set; }
}