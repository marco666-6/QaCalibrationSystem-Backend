using Dapper;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Entities;
using Project.Infrastructure.Data;

namespace Project.Infrastructure.Repositories;

public sealed class PositionRepository : BaseRepository<Position>, IPositionRepository
{
    public PositionRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    private const string BaseSelect = """
        SELECT
            position_id,
            position_code,
            position_name,
            is_active,
            created_at,
            updated_at
        FROM positions
        """;

    public async Task<(IEnumerable<Position> Items, int TotalCount)> GetAllAsync(PositionFilterParams filters)
    {
        var where = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filters.Code))
        {
            where.Add("position_code LIKE @Code");
            parameters.Add("Code", $"%{filters.Code.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            where.Add("position_name LIKE @Name");
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
            FROM positions
            {whereClause}
            """;

        var dataSql = $"""
            {BaseSelect}
            {whereClause}
            ORDER BY position_code
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        parameters.Add("Offset", filters.Offset);
        parameters.Add("PageSize", filters.PageSize);

        using var connection = _connectionFactory.CreateConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        if (totalCount == 0)
            return ([], 0);

        var items = await connection.QueryAsync<Position>(dataSql, parameters);
        return (items, totalCount);
    }

    public async Task<IEnumerable<Position>> GetOptionsAsync(PositionOptionFilterParams filters)
    {
        var where = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filters.Code))
        {
            where.Add("position_code LIKE @Code");
            parameters.Add("Code", $"%{filters.Code.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            where.Add("(position_code LIKE @Name OR position_name LIKE @Name)");
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
                position_id,
                position_code,
                position_name,
                is_active,
                created_at,
                updated_at
            FROM positions
            {whereClause}
            ORDER BY position_code
            """;

        parameters.Add("Top", filters.Top);
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Position>(sql, parameters);
    }

    public async Task<Position?> GetByIdAsync(int positionId)
    {
        var sql = $"{BaseSelect} WHERE position_id = @PositionId";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Position>(sql, new { PositionId = positionId });
    }

    public async Task<IEnumerable<Position>> GetByIdsAsync(IEnumerable<int> positionIds)
    {
        const string sql = """
            SELECT
                position_id,
                position_code,
                position_name,
                is_active,
                created_at,
                updated_at
            FROM positions
            WHERE position_id IN @PositionIds
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Position>(sql, new { PositionIds = positionIds.ToArray() });
    }

    public async Task<bool> CodeExistsAsync(string positionCode, int? excludePositionId = null)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM positions
            WHERE position_code = @PositionCode
              AND (@ExcludePositionId IS NULL OR position_id <> @ExcludePositionId)
            """;

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new
        {
            PositionCode = positionCode,
            ExcludePositionId = excludePositionId
        });

        return count > 0;
    }

    public async Task<IEnumerable<string>> GetExistingCodesAsync(IEnumerable<string> positionCodes)
    {
        const string sql = """
            SELECT position_code
            FROM positions
            WHERE position_code IN @PositionCodes
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<string>(sql, new { PositionCodes = positionCodes.ToArray() });
    }

    public async Task<int> CreateAsync(Position entity)
    {
        const string sql = """
            INSERT INTO positions (
                position_code,
                position_name,
                is_active,
                created_at
            ) VALUES (
                @PositionCode,
                @PositionName,
                @IsActive,
                @CreatedAt
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<IReadOnlyCollection<int>> CreateManyAsync(IEnumerable<Position> entities)
    {
        const string sql = """
            INSERT INTO positions (
                position_code,
                position_name,
                is_active,
                created_at
            ) VALUES (
                @PositionCode,
                @PositionName,
                @IsActive,
                @CreatedAt
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            var createdIds = new List<int>();
            foreach (var entity in entities)
            {
                var newId = await connection.ExecuteScalarAsync<int>(sql, entity, transaction);
                createdIds.Add(newId);
            }

            transaction.Commit();
            return createdIds;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Position entity)
    {
        const string sql = """
            UPDATE positions
            SET position_code = @PositionCode,
                position_name = @PositionName,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE position_id = @PositionId
            """;

        var affected = await ExecuteAsync(sql, entity);
        return affected > 0;
    }

    public async Task<bool> SoftDeleteAsync(int positionId)
    {
        const string sql = """
            UPDATE positions
            SET is_active = 0,
                updated_at = @UpdatedAt
            WHERE position_id = @PositionId
            """;

        var affected = await ExecuteAsync(sql, new
        {
            PositionId = positionId,
            UpdatedAt = DateTime.Now
        });

        return affected > 0;
    }

    public async Task<int> SoftDeleteManyAsync(IEnumerable<int> positionIds)
    {
        const string sql = """
            UPDATE positions
            SET is_active = 0,
                updated_at = @UpdatedAt
            WHERE position_id IN @PositionIds
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            PositionIds = positionIds.ToArray(),
            UpdatedAt = DateTime.Now
        });
    }
}
