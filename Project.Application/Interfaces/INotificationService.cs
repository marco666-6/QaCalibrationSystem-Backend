using Project.Application.Common;
using Project.Application.DTOs;

namespace Project.Application.Interfaces;

public interface INotificationService
{
    Task<ApiResponse<PagedResult<NotificationDto>>> GetInboxAsync(int currentUserId, NotificationFilterParams filters);
    Task<ApiResponse<NotificationUnreadCountDto>> GetUnreadCountAsync(int currentUserId);
    Task<ApiResponse> MarkAsReadAsync(int currentUserId, long notificationId);
    Task<ApiResponse> MarkAllAsReadAsync(int currentUserId);
    Task<ApiResponse<IEnumerable<NotificationPreferenceDto>>> GetPreferencesAsync(int currentUserId);
    Task<ApiResponse<IEnumerable<NotificationPreferenceDto>>> UpsertPreferenceAsync(int currentUserId, string type, UpdateNotificationPreferenceRequest request);
    Task CreateAsync(CreateNotificationRequest request);
}
