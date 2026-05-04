using FluentValidation;
using Project.Application.Common;

namespace Project.Application.DTOs;

public sealed record PositionDto(
    int PositionId,
    string PositionCode,
    string PositionName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record PositionSummaryDto(
    int PositionId,
    string PositionCode,
    string PositionName,
    bool IsActive
);

public sealed class PositionFilterParams : PaginationParams
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class PositionOptionFilterParams
{
    private const int MaxTop = 50;
    private int _top = 20;

    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool ActiveOnly { get; set; } = true;

    public int Top
    {
        get => _top;
        set => _top = value > MaxTop ? MaxTop
                     : value < 1 ? 1
                     : value;
    }
}

public sealed record PositionOptionDto(
    int PositionId,
    string PositionCode,
    string PositionName
);

public sealed record CreatePositionRequest(
    string PositionCode,
    string PositionName
);

public sealed record CreatePositionsRequest(
    IReadOnlyCollection<CreatePositionRequest> Items
);

public sealed record UpdatePositionRequest(
    string PositionCode,
    string PositionName,
    bool IsActive
);

public sealed record BulkDeletePositionsRequest(
    IReadOnlyCollection<int> Ids
);

public sealed class CreatePositionRequestValidator : AbstractValidator<CreatePositionRequest>
{
    public CreatePositionRequestValidator()
    {
        RuleFor(x => x.PositionCode)
            .NotEmpty().WithMessage("Position code is required.")
            .MaximumLength(6).WithMessage("Position code cannot exceed 6 characters.");

        RuleFor(x => x.PositionName)
            .NotEmpty().WithMessage("Position name is required.")
            .MaximumLength(100).WithMessage("Position name cannot exceed 100 characters.");
    }
}

public sealed class UpdatePositionRequestValidator : AbstractValidator<UpdatePositionRequest>
{
    public UpdatePositionRequestValidator()
    {
        RuleFor(x => x.PositionCode)
            .NotEmpty().WithMessage("Position code is required.")
            .MaximumLength(6).WithMessage("Position code cannot exceed 6 characters.");

        RuleFor(x => x.PositionName)
            .NotEmpty().WithMessage("Position name is required.")
            .MaximumLength(100).WithMessage("Position name cannot exceed 100 characters.");
    }
}

public sealed class CreatePositionsRequestValidator : AbstractValidator<CreatePositionsRequest>
{
    public CreatePositionsRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items are required.")
            .Must(items => items.Count > 0).WithMessage("At least one position is required.")
            .Must(items => items.Count <= 100).WithMessage("A maximum of 100 positions can be submitted at once.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreatePositionRequestValidator());
    }
}

public sealed class BulkDeletePositionsRequestValidator : AbstractValidator<BulkDeletePositionsRequest>
{
    public BulkDeletePositionsRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotNull().WithMessage("IDs are required.")
            .Must(ids => ids.Count > 0).WithMessage("At least one position ID is required.")
            .Must(ids => ids.Count <= 500).WithMessage("A maximum of 500 position IDs can be deleted at once.");

        RuleForEach(x => x.Ids)
            .GreaterThan(0).WithMessage("Position ID must be greater than 0.");
    }
}
