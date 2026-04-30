using Dapper;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Entities;
using Project.Infrastructure.Data;

namespace Project.Infrastructure.Repositories;

/// <inheritdoc cref="IUserRepository"/>
public sealed class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    private const string BaseSelect = """
        SELECT
            u.user_id,
            u.employee_id,
            u.username,
            u.password_hash,
            u.email,
            u.role,
            u.is_active,
            u.last_login,
            u.failed_login_attempts,
            u.lockout_until,
            u.must_change_password,
            u.refresh_token,
            u.refresh_token_expires_at,
            u.created_at,
            u.updated_at,
            e.employee_id   AS EmpId,
            e.full_name    AS FullName,
            e.employee_code AS EmployeeCode
        FROM users u
        LEFT JOIN Shared.dbo.employees e ON e.employee_id = u.employee_id
        """;

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var sql = $"{BaseSelect} WHERE u.username = @Username";
        var results = await QueryWithEmployeeAsync(sql, new { Username = username });
        return results.SingleOrDefault();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var sql = $"{BaseSelect} WHERE u.email = @Email";
        var results = await QueryWithEmployeeAsync(sql, new { Email = email.ToLowerInvariant() });
        return results.SingleOrDefault();
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        const string sql = """
            UPDATE users
            SET last_login = @Now
            WHERE user_id = @UserId
            """;

        await ExecuteAsync(sql, new { UserId = userId, Now = DateTime.UtcNow });
    }

    public async Task IncrementFailedLoginAttemptsAsync(int userId)
    {
        const string sql = """
            UPDATE users
            SET failed_login_attempts = failed_login_attempts + 1
            WHERE user_id = @UserId
            """;

        await ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task ResetFailedLoginAttemptsAsync(int userId)
    {
        const string sql = """
            UPDATE users
            SET failed_login_attempts = 0,
                lockout_until = NULL
            WHERE user_id = @UserId
            """;

        await ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task LockAccountAsync(int userId, DateTime lockoutUntil)
    {
        const string sql = """
            UPDATE users
            SET failed_login_attempts = 0,
                lockout_until = @LockoutUntil
            WHERE user_id = @UserId
            """;

        await ExecuteAsync(sql, new { UserId = userId, LockoutUntil = lockoutUntil });
    }

    public async Task StoreRefreshTokenAsync(int userId, string refreshToken, DateTime expiresAt)
    {
        const string sql = """
            UPDATE users
            SET refresh_token = @RefreshToken,
                refresh_token_expires_at = @ExpiresAt
            WHERE user_id = @UserId
            """;

        await ExecuteAsync(sql, new { UserId = userId, RefreshToken = refreshToken, ExpiresAt = expiresAt });
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        var sql = $"{BaseSelect} WHERE u.refresh_token = @RefreshToken AND u.refresh_token_expires_at > @Now";
        var results = await QueryWithEmployeeAsync(sql, new { RefreshToken = refreshToken, Now = DateTime.UtcNow });
        return results.SingleOrDefault();
    }

    public async Task<EmployeeByCodeDto?> GetEmployeeByCodeAsync(string code)
    {
        const string sql = """
            SELECT TOP 1
                e.name AS FullName,
                e.nik AS Code
            FROM Shared.dbo.View_emp_mst e
            WHERE e.nik = @Code;
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<EmployeeByCodeDto>(sql, new { Code = code });
    }

    public async Task<int?> UpsertEmployeeIdFromNikAsync(string employeeCode, string email)
    {
        const string upsertEmployeeSql = """
            MERGE INTO Shared.dbo.employees AS target
            USING (
                SELECT
                    nik as employee_code,
                    TRIM(name) as full_name,
                    CONVERT(DATE,dateofbirth) as date_of_birth,
                    sex as gender,
                    section as section_cd,
                    position_c as position_cd,
                    1 as is_active
                FROM Shared.dbo.View_emp_mst
                WHERE nik = @EmployeeCode
            ) AS source
            ON target.employee_code = source.employee_code
            WHEN NOT MATCHED THEN
                INSERT (
                    employee_code, full_name,
                    date_of_birth, gender, section_cd, position_cd,email,
                    is_active, created_at
                )
                VALUES (
                    source.employee_code, source.full_name,
                    source.date_of_birth, source.gender, source.section_cd, source.position_cd,@email,
                    source.is_active, @CreatedAt
                );

            SELECT employee_id
            FROM Shared.dbo.employees
            WHERE employee_code = @EmployeeCode;
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int?>(
            upsertEmployeeSql,
            new { EmployeeCode = employeeCode, CreatedAt = DateTime.UtcNow, Email = email });
    }

    public async Task CreatePasswordResetTokenAsync(PasswordResetToken token)
    {
        const string sql = """
            INSERT INTO password_reset_tokens (
                user_id,
                token,
                expires_at,
                created_at,
                consumed_at
            ) VALUES (
                @UserId,
                @Token,
                @ExpiresAt,
                @CreatedAt,
                @ConsumedAt
            );
            """;

        await ExecuteAsync(sql, new
        {
            token.UserId,
            token.Token,
            token.ExpiresAt,
            token.CreatedAt,
            token.ConsumedAt
        });
    }

    public async Task<PasswordResetToken?> GetValidPasswordResetTokenAsync(string token)
    {
        const string sql = """
            SELECT TOP 1
                id,
                user_id,
                token,
                expires_at,
                created_at,
                consumed_at
            FROM password_reset_tokens
            WHERE token = @Token
              AND consumed_at IS NULL
              AND expires_at > @Now
            ORDER BY id DESC
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<PasswordResetToken>(sql, new
        {
            Token = token,
            Now = DateTime.UtcNow
        });
    }

    public async Task InvalidatePasswordResetTokensAsync(int userId)
    {
        const string sql = """
            UPDATE password_reset_tokens
            SET consumed_at = @Now
            WHERE user_id = @UserId
              AND consumed_at IS NULL
            """;

        await ExecuteAsync(sql, new
        {
            UserId = userId,
            Now = DateTime.UtcNow
        });
    }

    public async Task ConsumePasswordResetTokenAsync(long id, DateTime consumedAt)
    {
        const string sql = """
            UPDATE password_reset_tokens
            SET consumed_at = @ConsumedAt
            WHERE id = @Id
            """;

        await ExecuteAsync(sql, new
        {
            Id = id,
            ConsumedAt = consumedAt
        });
    }



    public async Task<(IEnumerable<User> Items, int TotalCount)> GetAllAsync(UserFilterParams filters)
    {
        var where = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            where.Add("(u.username LIKE @Name OR e.full_name LIKE @Name)");
            parameters.Add("Name", $"%{filters.Name.Trim()}%");
        }

        var whereClause = where.Count == 0
            ? string.Empty
            : $" WHERE {string.Join(" AND ", where)}";

        var countSql = $"""
            SELECT COUNT(*)
            FROM users u
            LEFT JOIN Shared.dbo.employees e ON e.employee_id = u.employee_id
            {whereClause}
            """;

        var dataSql = $"""
            {BaseSelect}
            {whereClause}
            ORDER BY u.username
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        parameters.Add("Offset", filters.Offset);
        parameters.Add("PageSize", filters.PageSize);

        using var connection = _connectionFactory.CreateConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        if (totalCount == 0)
            return ([], 0);

        var items = await QueryWithEmployeeAsync(dataSql, parameters);
        return (items, totalCount);
    }

    public async Task<IEnumerable<User>> GetOptionsAsync(UserOptionFilterParams filters)
    {
        var where = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            where.Add("(u.username LIKE @Name OR e.full_name LIKE @Name)");
            parameters.Add("Name", $"%{filters.Name.Trim()}%");
        }

        var whereClause = where.Count == 0
            ? string.Empty
            : $" WHERE {string.Join(" AND ", where)}";

        var sql = $"""
            SELECT TOP (@Top)
                u.user_id,
                u.employee_id,
                u.username,
                u.password_hash,
                u.email,
                u.role,
                u.is_active,
                u.last_login,
                u.failed_login_attempts,
                u.lockout_until,
                u.must_change_password,
                u.refresh_token,
                u.refresh_token_expires_at,
                u.created_at,
                u.updated_at,
                e.employee_id AS EmpId,
                e.full_name AS FullName
            FROM users u
            LEFT JOIN Shared.dbo.employees e ON e.employee_id = u.employee_id
            {whereClause}
            ORDER BY u.username
            """;

        parameters.Add("Top", filters.Top);
        return await QueryWithEmployeeAsync(sql, parameters);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        var sql = $"{BaseSelect} WHERE u.user_id = @UserId";
        var results = await QueryWithEmployeeAsync(sql, new { UserId = userId });
        return results.SingleOrDefault();
    }

    public async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
    {
        const string sql = """
            SELECT COUNT(1) FROM users
            WHERE username = @Username
              AND (@ExcludeUserId IS NULL OR user_id <> @ExcludeUserId)
            """;

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql,
            new { Username = username, ExcludeUserId = excludeUserId });

        return count > 0;
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        const string sql = """
            SELECT COUNT(1) FROM users
            WHERE email = @Email
              AND (@ExcludeUserId IS NULL OR user_id <> @ExcludeUserId)
            """;

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql,
            new { Email = email.ToLowerInvariant(), ExcludeUserId = excludeUserId });

        return count > 0;
    }


    public async Task<int> CreateAsync(User user)
    {
        const string insertUserSql = """
            INSERT INTO users (
                employee_id, username, password_hash, email, role,
                is_active, must_change_password, created_at
            ) VALUES (
                @EmployeeId, @Username, @PasswordHash, @Email, @Role,
                @IsActive, @MustChangePassword, @CreatedAt
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        int? employeeId = null;

        if (!string.IsNullOrWhiteSpace(user.EmployeeCode))
        {
            employeeId = await UpsertEmployeeIdFromNikAsync(user.EmployeeCode, user.Email);
            if (employeeId is null or 0)
                throw new InvalidOperationException(
                    $"Employee with code '{user.EmployeeCode}' was not found in View_emp_mst.");
        }
        else if (user.EmployeeId.HasValue)
        {
            employeeId = user.EmployeeId;
        }

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            insertUserSql,
            new
            {
                EmployeeId = employeeId,
                user.Username,
                user.PasswordHash,
                user.Email,
                user.Role,
                user.IsActive,
                user.MustChangePassword,
                user.CreatedAt
            });
    }

    public async Task<bool> UpdateAsync(User user)
    {
        const string sql = """
            UPDATE users SET
                employee_id         = @EmployeeId,
                username            = @Username,
                email               = @Email,
                role                = @Role,
                is_active           = @IsActive,
                must_change_password = @MustChangePassword,
                updated_at          = @UpdatedAt
            WHERE user_id = @UserId
            """;

        var affected = await ExecuteAsync(sql, new
        {
            user.EmployeeId,
            user.Username,
            user.Email,
            user.Role,
            user.IsActive,
            user.MustChangePassword,
            user.UpdatedAt,
            user.UserId
        });

        return affected > 0;
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        const string sql = """
            UPDATE users SET
                password_hash        = @PasswordHash,
                must_change_password = 0,
                updated_at           = @UpdatedAt
            WHERE user_id = @UserId
            """;

        var affected = await ExecuteAsync(sql, new
        {
            UserId = userId,
            PasswordHash = newPasswordHash,
            UpdatedAt = DateTime.UtcNow
        });

        return affected > 0;
    }

    public async Task<bool> SoftDeleteAsync(int userId)
    {
        const string sql = """
            UPDATE users
            SET is_active = 0,
                updated_at = @UpdatedAt
            WHERE user_id = @UserId
            """;

        var affected = await ExecuteAsync(sql, new
        {
            UserId = userId,
            UpdatedAt = DateTime.UtcNow
        });

        return affected > 0;
    }



    private async Task<IEnumerable<User>> QueryWithEmployeeAsync(
        string sql, object? param = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<User, Employee?, User>(
            sql,
            (user, employee) =>
            {
                user.Employee = employee;
                return user;
            },
            param,
            splitOn: "EmpId");
    }
}
