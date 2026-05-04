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

public sealed record UpdateSectionRequest(
    string SectionCode,
    string SectionName,
    bool IsActive
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
