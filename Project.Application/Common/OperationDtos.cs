namespace Project.Application.Common;

public sealed record BulkDeleteResultDto(
    int RequestedCount,
    int DeletedCount,
    IReadOnlyCollection<int> NotFoundIds);

public sealed record BulkUpdateResultDto(
    int RequestedCount,
    int UpdatedCount,
    IReadOnlyCollection<int> NotFoundIds);

public sealed record FileContentDto(
    string FileName,
    string ContentType,
    byte[] Content);
