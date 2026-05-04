using Dapper;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Entities;
using Project.Infrastructure.Data;

namespace Project.Infrastructure.Repositories;

public sealed class SectionRepository : BaseRepository<Section>, ISectionRepository
{
    public SectionRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    private const string BaseSelect = """
        SELECT
            section_id,
            section_code,
            section_name,
            is_active,
            created_at,
            updated_at
        FROM sections
        """;

    public async Task<(IEnumerable<Section> Items, int TotalCount)> GetAllAsync(SectionFilterParams filters)
    {
        var where = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filters.Code))
        {
            where.Add("section_code LIKE @Code");
            parameters.Add("Code", $"%{filters.Code.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            where.Add("section_name LIKE @Name");
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
            FROM sections
            {whereClause}
            """;

        var dataSql = $"""
            {BaseSelect}
            {whereClause}
            ORDER BY section_code
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        parameters.Add("Offset", filters.Offset);
        parameters.Add("PageSize", filters.PageSize);

        using var connection = _connectionFactory.CreateConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        if (totalCount == 0)
            return ([], 0);

        var items = await connection.QueryAsync<Section>(dataSql, parameters);
        return (items, totalCount);
    }

    public async Task<IEnumerable<Section>> GetOptionsAsync(SectionOptionFilterParams filters)
    {
        var where = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filters.Code))
        {
            where.Add("section_code LIKE @Code");
            parameters.Add("Code", $"%{filters.Code.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            where.Add("(section_code LIKE @Name OR section_name LIKE @Name)");
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
                section_id,
                section_code,
                section_name,
                is_active,
                created_at,
                updated_at
            FROM sections
            {whereClause}
            ORDER BY section_code
            """;

        parameters.Add("Top", filters.Top);
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Section>(sql, parameters);
    }

    public async Task<Section?> GetByIdAsync(int sectionId)
    {
        var sql = $"{BaseSelect} WHERE section_id = @SectionId";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Section>(sql, new { SectionId = sectionId });
    }

    public async Task<IEnumerable<Section>> GetByIdsAsync(IEnumerable<int> sectionIds)
    {
        const string sql = """
            SELECT
                section_id,
                section_code,
                section_name,
                is_active,
                created_at,
                updated_at
            FROM sections
            WHERE section_id IN @SectionIds
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Section>(sql, new { SectionIds = sectionIds.ToArray() });
    }

    public async Task<bool> CodeExistsAsync(string sectionCode, int? excludeSectionId = null)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM sections
            WHERE section_code = @SectionCode
              AND (@ExcludeSectionId IS NULL OR section_id <> @ExcludeSectionId)
            """;

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new
        {
            SectionCode = sectionCode,
            ExcludeSectionId = excludeSectionId
        });

        return count > 0;
    }

    public async Task<IEnumerable<string>> GetExistingCodesAsync(IEnumerable<string> sectionCodes)
    {
        const string sql = """
            SELECT section_code
            FROM sections
            WHERE section_code IN @SectionCodes
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<string>(sql, new { SectionCodes = sectionCodes.ToArray() });
    }

    public async Task<int> CreateAsync(Section entity)
    {
        const string sql = """
            INSERT INTO sections (
                section_code,
                section_name,
                is_active,
                created_at
            ) VALUES (
                @SectionCode,
                @SectionName,
                @IsActive,
                @CreatedAt
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<IReadOnlyCollection<int>> CreateManyAsync(IEnumerable<Section> entities)
    {
        const string sql = """
            INSERT INTO sections (
                section_code,
                section_name,
                is_active,
                created_at
            ) VALUES (
                @SectionCode,
                @SectionName,
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

    public async Task<bool> UpdateAsync(Section entity)
    {
        const string sql = """
            UPDATE sections
            SET section_code = @SectionCode,
                section_name = @SectionName,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE section_id = @SectionId
            """;

        var affected = await ExecuteAsync(sql, entity);
        return affected > 0;
    }

    public async Task<bool> SoftDeleteAsync(int sectionId)
    {
        const string sql = """
            UPDATE sections
            SET is_active = 0,
                updated_at = @UpdatedAt
            WHERE section_id = @SectionId
            """;

        var affected = await ExecuteAsync(sql, new
        {
            SectionId = sectionId,
            UpdatedAt = DateTime.Now
        });

        return affected > 0;
    }

    public async Task<int> SoftDeleteManyAsync(IEnumerable<int> sectionIds)
    {
        const string sql = """
            UPDATE sections
            SET is_active = 0,
                updated_at = @UpdatedAt
            WHERE section_id IN @SectionIds
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            SectionIds = sectionIds.ToArray(),
            UpdatedAt = DateTime.Now
        });
    }
}
