using Project.Application.DTOs;
using Project.Domain.Entities;

namespace Project.Application.Interfaces;

public interface INotificationRepository
{
    Task CreateManyAsync(IEnumerable<NotificationItem> notifications);
    Task<(IEnumerable<NotificationItem> Items, int TotalCount)> GetInboxAsync(int userId, NotificationFilterParams filters);
    Task<NotificationItem?> GetByIdAsync(long notificationId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int userId, long notificationId, DateTime readAtUtc);
    Task<int> MarkAllAsReadAsync(int userId, DateTime readAtUtc);
    Task<IEnumerable<NotificationPreference>> GetPreferencesAsync(int userId);
    Task UpsertPreferenceAsync(NotificationPreference preference);
}
