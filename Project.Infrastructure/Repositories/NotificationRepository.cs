using Dapper;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Entities;
using Project.Infrastructure.Data;

namespace Project.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public NotificationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateManyAsync(IEnumerable<NotificationItem> notifications)
    {
        var items = notifications?.ToList() ?? [];
        if (items.Count == 0) return;

        const string sql = """
            INSERT INTO notifications (
                user_id,
                type,
                title,
                message,
                link_url,
                metadata_json,
                is_read,
                created_at,
                read_at
            )
            VALUES (
                @UserId,
                @Type,
                @Title,
                @Message,
                @LinkUrl,
                @MetadataJson,
                @IsRead,
                @CreatedAt,
                @ReadAt
            )
            """;

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, items);
    }

    public async Task<(IEnumerable<NotificationItem> Items, int TotalCount)> GetInboxAsync(int userId, NotificationFilterParams filters)
    {
        var where = new List<string> { "user_id = @UserId" };
        if (filters.IsRead.HasValue)
            where.Add("is_read = @IsRead");

        var whereClause = string.Join(" AND ", where);

        var countSql = $"SELECT COUNT(*) FROM notifications WHERE {whereClause}";
        var dataSql = $"""
            SELECT *
            FROM notifications
            WHERE {whereClause}
            ORDER BY created_at DESC, id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);
        parameters.Add("Offset", filters.Offset);
        parameters.Add("PageSize", filters.PageSize);
        if (filters.IsRead.HasValue)
            parameters.Add("IsRead", filters.IsRead.Value);

        using var connection = _connectionFactory.CreateConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        if (totalCount == 0) return ([], 0);

        var items = await connection.QueryAsync<NotificationItem>(dataSql, parameters);
        return (items, totalCount);
    }

    public async Task<NotificationItem?> GetByIdAsync(long notificationId)
    {
        const string sql = "SELECT * FROM notifications WHERE id = @NotificationId";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<NotificationItem>(sql, new { NotificationId = notificationId });
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        const string sql = "SELECT COUNT(*) FROM notifications WHERE user_id = @UserId AND is_read = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task<bool> MarkAsReadAsync(int userId, long notificationId, DateTime readAtUtc)
    {
        const string sql = """
            UPDATE notifications
            SET is_read = 1,
                read_at = @ReadAt
            WHERE id = @NotificationId AND user_id = @UserId
            """;
        using var connection = _connectionFactory.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            NotificationId = notificationId,
            ReadAt = readAtUtc
        });
        return affected > 0;
    }

    public async Task<int> MarkAllAsReadAsync(int userId, DateTime readAtUtc)
    {
        const string sql = """
            UPDATE notifications
            SET is_read = 1,
                read_at = @ReadAt
            WHERE user_id = @UserId AND is_read = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { UserId = userId, ReadAt = readAtUtc });
    }

    public async Task<IEnumerable<NotificationPreference>> GetPreferencesAsync(int userId)
    {
        const string sql = """
            SELECT
                user_id AS UserId,
                type AS Type,
                enabled AS Enabled,
                updated_at AS UpdatedAt
            FROM notification_preferences
            WHERE user_id = @UserId
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<NotificationPreference>(sql, new { UserId = userId });
    }

    public async Task UpsertPreferenceAsync(NotificationPreference preference)
    {
        const string sql = """
            MERGE notification_preferences AS target
            USING (SELECT @UserId AS user_id, @Type AS type) AS source
            ON target.user_id = source.user_id AND target.type = source.type
            WHEN MATCHED THEN
                UPDATE SET
                    enabled = @Enabled,
                    updated_at = @UpdatedAt
            WHEN NOT MATCHED THEN
                INSERT (user_id, type, enabled, updated_at)
                VALUES (@UserId, @Type, @Enabled, @UpdatedAt);
            """;

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            preference.UserId,
            preference.Type,
            preference.Enabled,
            preference.UpdatedAt
        });
    }
}
