using Project.Application.Common;

namespace Project.Application.DTOs;

public sealed record NotificationDto(
    long Id,
    int UserId,
    string Type,
    string Title,
    string Message,
    string? LinkUrl,
    string? MetadataJson,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt
);

public sealed class NotificationFilterParams : PaginationParams
{
    public bool? IsRead { get; set; }
}

public sealed record NotificationUnreadCountDto(
    int UnreadCount
);

public sealed record CreateNotificationRequest(
    IReadOnlyList<int> UserIds,
    string Type,
    string Title,
    string Message,
    string? LinkUrl,
    string? MetadataJson
);

public sealed record NotificationPreferenceDto(
    string Type,
    bool Enabled
);

public sealed record UpdateNotificationPreferenceRequest(
    bool Enabled
);
