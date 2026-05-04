using FluentValidation;
using Project.Application.Common;

namespace Project.Application.DTOs;

public sealed record SectionDto(
    int SectionId,
    string SectionCode,
    string SectionName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record SectionSummaryDto(
    int SectionId,
    string SectionCode,
    string SectionName,
    bool IsActive
);

public sealed class SectionFilterParams : PaginationParams
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class SectionOptionFilterParams
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

public sealed record SectionOptionDto(
    int SectionId,
    string SectionCode,
    string SectionName
);

public sealed record CreateSectionRequest(
    string SectionCode,
    string SectionName
);

public sealed record CreateSectionsRequest(
    IReadOnlyCollection<CreateSectionRequest> Items
);

public sealed record UpdateSectionRequest(
    string SectionCode,
    string SectionName,
    bool IsActive
);

public sealed record BulkDeleteSectionsRequest(
    IReadOnlyCollection<int> Ids
);

public sealed class CreateSectionRequestValidator : AbstractValidator<CreateSectionRequest>
{
    public CreateSectionRequestValidator()
    {
        RuleFor(x => x.SectionCode)
            .NotEmpty().WithMessage("Section code is required.")
            .MaximumLength(6).WithMessage("Section code cannot exceed 6 characters.");

        RuleFor(x => x.SectionName)
            .NotEmpty().WithMessage("Section name is required.")
            .MaximumLength(100).WithMessage("Section name cannot exceed 100 characters.");
    }
}

public sealed class UpdateSectionRequestValidator : AbstractValidator<UpdateSectionRequest>
{
    public UpdateSectionRequestValidator()
    {
        RuleFor(x => x.SectionCode)
            .NotEmpty().WithMessage("Section code is required.")
            .MaximumLength(6).WithMessage("Section code cannot exceed 6 characters.");

        RuleFor(x => x.SectionName)
            .NotEmpty().WithMessage("Section name is required.")
            .MaximumLength(100).WithMessage("Section name cannot exceed 100 characters.");
    }
}

public sealed class CreateSectionsRequestValidator : AbstractValidator<CreateSectionsRequest>
{
    public CreateSectionsRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items are required.")
            .Must(items => items.Count > 0).WithMessage("At least one section is required.")
            .Must(items => items.Count <= 100).WithMessage("A maximum of 100 sections can be submitted at once.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateSectionRequestValidator());
    }
}

public sealed class BulkDeleteSectionsRequestValidator : AbstractValidator<BulkDeleteSectionsRequest>
{
    public BulkDeleteSectionsRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotNull().WithMessage("IDs are required.")
            .Must(ids => ids.Count > 0).WithMessage("At least one section ID is required.")
            .Must(ids => ids.Count <= 500).WithMessage("A maximum of 500 section IDs can be deleted at once.");

        RuleForEach(x => x.Ids)
            .GreaterThan(0).WithMessage("Section ID must be greater than 0.");
    }
}
