using FluentValidation;
using Project.Application.Common;

namespace Project.Application.DTOs;

public sealed record DefaultLocationDto(
    int DefaultLocationId,
    string DefaultLocationName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record DefaultLocationSummaryDto(
    int DefaultLocationId,
    string DefaultLocationName,
    bool IsActive
);

public sealed class DefaultLocationFilterParams : PaginationParams
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class DefaultLocationOptionFilterParams
{
    private const int MaxTop = 50;
    private int _top = 20;

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

public sealed record DefaultLocationOptionDto(
    int DefaultLocationId,
    string DefaultLocationName
);

public sealed record CreateDefaultLocationRequest(
    string DefaultLocationName
);

public sealed record UpdateDefaultLocationRequest(
    string DefaultLocationName,
    bool IsActive
);

public sealed class CreateDefaultLocationRequestValidator : AbstractValidator<CreateDefaultLocationRequest>
{
    public CreateDefaultLocationRequestValidator()
    {
        RuleFor(x => x.DefaultLocationName)
            .NotEmpty().WithMessage("Default location name is required.")
            .MaximumLength(200).WithMessage("Default location name cannot exceed 200 characters.");
    }
}

public sealed class UpdateDefaultLocationRequestValidator : AbstractValidator<UpdateDefaultLocationRequest>
{
    public UpdateDefaultLocationRequestValidator()
    {
        RuleFor(x => x.DefaultLocationName)
            .NotEmpty().WithMessage("Default location name is required.")
            .MaximumLength(200).WithMessage("Default location name cannot exceed 200 characters.");
    }
}
