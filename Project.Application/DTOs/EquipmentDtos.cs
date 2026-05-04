using FluentValidation;
using Project.Application.Common;
using Project.Domain.Enums;

namespace Project.Application.DTOs;

public sealed record EquipmentDto(
    int Id,
    string EquipmentName,
    string ControlNo,
    string? SerialNo,
    string? Brand,
    string? Model,
    string Location,
    int SectionId,
    string SectionCode,
    string SectionName,
    int PicId,
    string PicCode,
    string PicName,
    int CalibIntervalMonths,
    DateTime? LastCalibDate,
    int? LastCalibMonth,
    int? LastCalibYear,
    DateTime? NextCalibDate,
    int? NextCalibMonth,
    int? NextCalibYear,
    string CalibType,
    string EquipmentStatus,
    string? Remarks,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy
);

public sealed record EquipmentSummaryDto(
    int Id,
    string EquipmentName,
    string ControlNo,
    string? SerialNo,
    string? Brand,
    string? Model,
    string Location,
    int SectionId,
    string SectionCode,
    string SectionName,
    int PicId,
    string PicCode,
    string PicName,
    int CalibIntervalMonths,
    DateTime? LastCalibDate,
    DateTime? NextCalibDate,
    string CalibType,
    string EquipmentStatus,
    string? Remarks
);

public sealed class EquipmentFilterParams : PaginationParams
{
    public string? Search { get; set; }
    public int? SectionId { get; set; }
    public int? PicId { get; set; }
    public string? CalibType { get; set; }
    public string? EquipmentStatus { get; set; }
    public int? LastCalibYear { get; set; }
    public int? LastCalibMonth { get; set; }
    public int? NextCalibYear { get; set; }
    public int? NextCalibMonth { get; set; }
}

public sealed record CreateEquipmentRequest(
    string EquipmentName,
    string ControlNo,
    string? SerialNo,
    string? Brand,
    string? Model,
    string Location,
    int SectionId,
    int? PicId,
    string? PicCode,
    int CalibIntervalMonths,
    DateTime? LastCalibDate,
    string CalibType,
    string EquipmentStatus,
    string? Remarks
);

public sealed record UpdateEquipmentRequest(
    string EquipmentName,
    string ControlNo,
    string? SerialNo,
    string? Brand,
    string? Model,
    string Location,
    int SectionId,
    int? PicId,
    string? PicCode,
    int CalibIntervalMonths,
    DateTime? LastCalibDate,
    string CalibType,
    string EquipmentStatus,
    string? Remarks
);

public sealed record BulkDeleteEquipmentsRequest(
    IReadOnlyCollection<int> Ids
);

public sealed record BulkSectionChangeRequest(
    IReadOnlyCollection<int> Ids,
    int SectionId
);

public sealed record BulkPicChangeRequest(
    IReadOnlyCollection<int> Ids,
    int? PicId,
    string? PicCode
);

public sealed record BulkStatusChangeRequest(
    IReadOnlyCollection<int> Ids,
    string EquipmentStatus
);

public sealed record EquipmentImportRowErrorDto(
    int RowNumber,
    string? ControlNo,
    IReadOnlyCollection<string> Errors
);

public sealed record EquipmentImportResultDto(
    int TotalRows,
    int ImportedRows,
    int FailedRows,
    IReadOnlyCollection<EquipmentImportRowErrorDto> RowErrors
);

public sealed class CreateEquipmentRequestValidator : AbstractValidator<CreateEquipmentRequest>
{
    public CreateEquipmentRequestValidator()
    {
        Include(new EquipmentWriteRequestValidator<CreateEquipmentRequest>());
    }
}

public sealed class UpdateEquipmentRequestValidator : AbstractValidator<UpdateEquipmentRequest>
{
    public UpdateEquipmentRequestValidator()
    {
        Include(new EquipmentWriteRequestValidator<UpdateEquipmentRequest>());
    }
}

public sealed class BulkDeleteEquipmentsRequestValidator : AbstractValidator<BulkDeleteEquipmentsRequest>
{
    public BulkDeleteEquipmentsRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotNull().WithMessage("IDs are required.")
            .Must(ids => ids.Count > 0).WithMessage("At least one equipment ID is required.")
            .Must(ids => ids.Count <= 1000).WithMessage("A maximum of 1000 equipment IDs can be deleted at once.");

        RuleForEach(x => x.Ids)
            .GreaterThan(0).WithMessage("Equipment ID must be greater than 0.");
    }
}

public sealed class BulkSectionChangeRequestValidator : AbstractValidator<BulkSectionChangeRequest>
{
    public BulkSectionChangeRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotNull().WithMessage("IDs are required.")
            .Must(ids => ids.Count > 0).WithMessage("At least one equipment ID is required.")
            .Must(ids => ids.Count <= 1000).WithMessage("A maximum of 1000 equipment IDs can be updated at once.");

        RuleForEach(x => x.Ids)
            .GreaterThan(0).WithMessage("Equipment ID must be greater than 0.");

        RuleFor(x => x.SectionId)
            .GreaterThan(0).WithMessage("Section ID must be greater than 0.");
    }
}

public sealed class BulkPicChangeRequestValidator : AbstractValidator<BulkPicChangeRequest>
{
    public BulkPicChangeRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotNull().WithMessage("IDs are required.")
            .Must(ids => ids.Count > 0).WithMessage("At least one equipment ID is required.")
            .Must(ids => ids.Count <= 1000).WithMessage("A maximum of 1000 equipment IDs can be updated at once.");

        RuleForEach(x => x.Ids)
            .GreaterThan(0).WithMessage("Equipment ID must be greater than 0.");

        RuleFor(x => x.PicId)
            .GreaterThan(0).When(x => x.PicId.HasValue)
            .WithMessage("PIC ID must be greater than 0.");

        RuleFor(x => x.PicCode)
            .MaximumLength(6).When(x => !string.IsNullOrWhiteSpace(x.PicCode))
            .WithMessage("PIC code cannot exceed 6 characters.")
            .Matches(@"^\d+$").When(x => !string.IsNullOrWhiteSpace(x.PicCode))
            .WithMessage("PIC code must be numeric.");

        RuleFor(x => x)
            .Must(x => x.PicId.HasValue || !string.IsNullOrWhiteSpace(x.PicCode))
            .WithMessage("Either PIC ID or PIC code is required.");
    }
}

public sealed class BulkStatusChangeRequestValidator : AbstractValidator<BulkStatusChangeRequest>
{
    public BulkStatusChangeRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotNull().WithMessage("IDs are required.")
            .Must(ids => ids.Count > 0).WithMessage("At least one equipment ID is required.")
            .Must(ids => ids.Count <= 1000).WithMessage("A maximum of 1000 equipment IDs can be updated at once.");

        RuleForEach(x => x.Ids)
            .GreaterThan(0).WithMessage("Equipment ID must be greater than 0.");

        RuleFor(x => x.EquipmentStatus)
            .NotEmpty().WithMessage("Equipment status is required.")
            .Must(value => EquipmentValueMappings.TryNormalizeEquipmentStatus(value, out _))
            .WithMessage($"Equipment status must be one of: {string.Join(", ", EquipmentStatus.All)}.");
    }
}

internal sealed class EquipmentWriteRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : class
{
    public EquipmentWriteRequestValidator()
    {
        RuleFor(x => GetString(x, nameof(CreateEquipmentRequest.EquipmentName)))
            .NotEmpty().WithMessage("Equipment name is required.")
            .MaximumLength(200).WithMessage("Equipment name cannot exceed 200 characters.");

        RuleFor(x => GetString(x, nameof(CreateEquipmentRequest.ControlNo)))
            .NotEmpty().WithMessage("Control number is required.")
            .MaximumLength(100).WithMessage("Control number cannot exceed 100 characters.");

        RuleFor(x => GetString(x, nameof(CreateEquipmentRequest.SerialNo)))
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(GetString(x, nameof(CreateEquipmentRequest.SerialNo))))
            .WithMessage("Serial number cannot exceed 100 characters.");

        RuleFor(x => GetString(x, nameof(CreateEquipmentRequest.Brand)))
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(GetString(x, nameof(CreateEquipmentRequest.Brand))))
            .WithMessage("Brand cannot exceed 100 characters.");

        RuleFor(x => GetString(x, nameof(CreateEquipmentRequest.Model)))
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(GetString(x, nameof(CreateEquipmentRequest.Model))))
            .WithMessage("Model cannot exceed 100 characters.");

        RuleFor(x => GetString(x, nameof(CreateEquipmentRequest.Location)))
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters.");

        RuleFor(x => GetInt(x, nameof(CreateEquipmentRequest.SectionId)))
            .GreaterThan(0).WithMessage("Section ID must be greater than 0.");

        RuleFor(x => GetNullableInt(x, nameof(CreateEquipmentRequest.PicId)))
            .GreaterThan(0).When(x => GetNullableInt(x, nameof(CreateEquipmentRequest.PicId)).HasValue)
            .WithMessage("PIC ID must be greater than 0.");

        RuleFor(x => GetString(x, nameof(CreateEquipmentRequest.PicCode)))
            .MaximumLength(6).When(x => !string.IsNullOrWhiteSpace(GetString(x, nameof(CreateEquipmentRequest.PicCode))))
            .WithMessage("PIC code cannot exceed 6 characters.")
            .Matches(@"^\d+$").When(x => !string.IsNullOrWhiteSpace(GetString(x, nameof(CreateEquipmentRequest.PicCode))))
            .WithMessage("PIC code must be numeric.");

        RuleFor(x => x)
            .Must(x => GetNullableInt(x, nameof(CreateEquipmentRequest.PicId)).HasValue
                || !string.IsNullOrWhiteSpace(GetString(x, nameof(CreateEquipmentRequest.PicCode))))
            .WithMessage("Either PIC ID or PIC code is required.");

        RuleFor(x => GetInt(x, nameof(CreateEquipmentRequest.CalibIntervalMonths)))
            .GreaterThan(0).WithMessage("Calibration interval months must be greater than 0.");

        RuleFor(x => GetString(x, nameof(CreateEquipmentRequest.CalibType)))
            .NotEmpty().WithMessage("Calibration type is required.")
            .Must(value => EquipmentValueMappings.TryNormalizeCalibType(value, out _))
            .WithMessage($"Calibration type must be one of: {string.Join(", ", CalibType.All)}.");

        RuleFor(x => GetString(x, nameof(CreateEquipmentRequest.EquipmentStatus)))
            .NotEmpty().WithMessage("Equipment status is required.")
            .Must(value => EquipmentValueMappings.TryNormalizeEquipmentStatus(value, out _))
            .WithMessage($"Equipment status must be one of: {string.Join(", ", EquipmentStatus.All)}.");
    }

    private static string? GetString(TRequest instance, string propertyName) =>
        typeof(TRequest).GetProperty(propertyName)?.GetValue(instance) as string;

    private static int GetInt(TRequest instance, string propertyName) =>
        (int?)typeof(TRequest).GetProperty(propertyName)?.GetValue(instance) ?? 0;

    private static int? GetNullableInt(TRequest instance, string propertyName) =>
        (int?)typeof(TRequest).GetProperty(propertyName)?.GetValue(instance);
}

public static class EquipmentValueMappings
{
    public static bool TryNormalizeCalibType(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var cleaned = value.Trim().ToUpperInvariant();
        normalized = cleaned switch
        {
            "I" => CalibType.Internal,
            "INTERNAL" => CalibType.Internal,
            "E" => CalibType.External,
            "EXTERNAL" => CalibType.External,
            _ => string.Empty
        };

        return normalized.Length > 0;
    }

    public static bool TryNormalizeEquipmentStatus(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var cleaned = value.Trim().ToUpperInvariant();
        normalized = cleaned switch
        {
            "A" => EquipmentStatus.Active,
            "ACTIVE" => EquipmentStatus.Active,
            "O" => EquipmentStatus.OutForService,
            "OUT FOR SERVICE" => EquipmentStatus.OutForService,
            "OUT-FOR-SERVICE" => EquipmentStatus.OutForService,
            "OUT_OF_SERVICE" => EquipmentStatus.OutForService,
            "S" => EquipmentStatus.Scrapped,
            "SCRAPPED" => EquipmentStatus.Scrapped,
            _ => string.Empty
        };

        return normalized.Length > 0;
    }
}
