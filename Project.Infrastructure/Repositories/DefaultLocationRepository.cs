using Dapper;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Entities;
using Project.Infrastructure.Data;

namespace Project.Infrastructure.Repositories;

public sealed class DefaultLocationRepository : BaseRepository<DefaultLocation>, IDefaultLocationRepository
{
    public DefaultLocationRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    private const string BaseSelect = """
        SELECT
            def_location_id,
            def_location_name,
            is_active,
            created_at,
            updated_at
        FROM def_locations
        """;

    public async Task<(IEnumerable<DefaultLocation> Items, int TotalCount)> GetAllAsync(DefaultLocationFilterParams filters)
    {
        var where = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            where.Add("def_location_name LIKE @Name");
            parameters.Add("Name", $"%{filters.Name.Trim()}%");
        }

        if (filters.IsActive.HasValue)
        {
            where.Add("is_active = @IsActive");
            parameters.Add("IsActive", filters.IsActive.Value);
        }

        var whereClause = where.Count == 0
            ? string.Empty
            : $" WHERE {string.Join(" AND ", where)}";

        var countSql = $"""
            SELECT COUNT(*)
            FROM def_locations
            {whereClause}
            """;

        var dataSql = $"""
            {BaseSelect}
            {whereClause}
            ORDER BY def_location_name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        parameters.Add("Offset", filters.Offset);
        parameters.Add("PageSize", filters.PageSize);

        using var connection = _connectionFactory.CreateConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        if (totalCount == 0)
            return ([], 0);

        var items = await connection.QueryAsync<DefaultLocation>(dataSql, parameters);
        return (items, totalCount);
    }

    public async Task<IEnumerable<DefaultLocation>> GetOptionsAsync(DefaultLocationOptionFilterParams filters)
    {
        var where = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            where.Add("def_location_name LIKE @Name");
            parameters.Add("Name", $"%{filters.Name.Trim()}%");
        }

        if (filters.ActiveOnly)
        {
            where.Add("is_active = 1");
        }

        var whereClause = where.Count == 0
            ? string.Empty
            : $" WHERE {string.Join(" AND ", where)}";

        var sql = $"""
            SELECT TOP (@Top)
                def_location_id,
                def_location_name,
                is_active,
                created_at,
                updated_at
            FROM def_locations
            {whereClause}
            ORDER BY def_location_name
            """;

        parameters.Add("Top", filters.Top);
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<DefaultLocation>(sql, parameters);
    }

    public async Task<DefaultLocation?> GetByIdAsync(int defaultLocationId)
    {
        var sql = $"{BaseSelect} WHERE def_location_id = @DefaultLocationId";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<DefaultLocation>(sql, new { DefaultLocationId = defaultLocationId });
    }

    public async Task<int> CreateAsync(DefaultLocation entity)
    {
        const string sql = """
            INSERT INTO def_locations (
                def_location_name,
                is_active,
                created_at
            ) VALUES (
                @DefaultLocationName,
                @IsActive,
                @CreatedAt
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<bool> UpdateAsync(DefaultLocation entity)
    {
        const string sql = """
            UPDATE def_locations
            SET def_location_name = @DefaultLocationName,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE def_location_id = @DefaultLocationId
            """;

        var affected = await ExecuteAsync(sql, entity);
        return affected > 0;
    }

    public async Task<bool> SoftDeleteAsync(int defaultLocationId)
    {
        const string sql = """
            UPDATE def_locations
            SET is_active = 0,
                updated_at = @UpdatedAt
            WHERE def_location_id = @DefaultLocationId
            """;

        var affected = await ExecuteAsync(sql, new
        {
            DefaultLocationId = defaultLocationId,
            UpdatedAt = DateTime.Now
        });

        return affected > 0;
    }
}
