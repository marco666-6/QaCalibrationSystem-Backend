using Dapper;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Entities;
using Project.Infrastructure.Data;

namespace Project.Infrastructure.Repositories;

public sealed class EquipmentRepository : BaseRepository<Equipment>, IEquipmentRepository
{
    public EquipmentRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory) { }

    private const string BaseSelect = """
        SELECT
            e.id,
            e.equipment_name,
            e.control_no,
            e.serial_no,
            e.brand,
            e.model,
            e.location,
            e.section_id,
            s.section_code,
            s.section_name,
            e.pic_id,
            e.pic_code,
            e.pic_name,
            e.calib_interval_months,
            e.last_calib_date,
            e.calib_type,
            e.equipment_status,
            e.remarks,
            e.created_at,
            e.updated_at,
            e.created_by,
            e.updated_by
        FROM equipments e
        LEFT JOIN sections s ON s.section_id = e.section_id
        """;

    public async Task<(IEnumerable<Equipment> Items, int TotalCount)> GetAllAsync(EquipmentFilterParams filters)
    {
        var parameters = new DynamicParameters();
        var whereClause = BuildWhereClause(filters, parameters);

        var countSql = $"""
            SELECT COUNT(*)
            FROM equipments e
            LEFT JOIN sections s ON s.section_id = e.section_id
            {whereClause}
            """;

        var dataSql = $"""
            {BaseSelect}
            {whereClause}
            ORDER BY e.control_no
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        parameters.Add("Offset", filters.Offset);
        parameters.Add("PageSize", filters.PageSize);

        using var connection = _connectionFactory.CreateConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        if (totalCount == 0)
            return ([], 0);

        var items = await connection.QueryAsync<Equipment>(dataSql, parameters);
        return (items, totalCount);
    }

    public async Task<IEnumerable<Equipment>> GetAllForExportAsync(EquipmentFilterParams filters)
    {
        var parameters = new DynamicParameters();
        var whereClause = BuildWhereClause(filters, parameters);
        var sql = $"""
            {BaseSelect}
            {whereClause}
            ORDER BY e.control_no
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Equipment>(sql, parameters);
    }

    public async Task<Equipment?> GetByIdAsync(int equipmentId)
    {
        var sql = $"{BaseSelect} WHERE e.id = @EquipmentId";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Equipment>(sql, new { EquipmentId = equipmentId });
    }

    public async Task<IEnumerable<Equipment>> GetByIdsAsync(IEnumerable<int> equipmentIds)
    {
        var sql = $"""
            {BaseSelect}
            WHERE e.id IN @EquipmentIds
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Equipment>(sql, new { EquipmentIds = equipmentIds.ToArray() });
    }

    public async Task<bool> ControlNoExistsAsync(string controlNo, int? excludeEquipmentId = null)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM equipments
            WHERE control_no = @ControlNo
              AND (@ExcludeEquipmentId IS NULL OR id <> @ExcludeEquipmentId)
            """;

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new
        {
            ControlNo = controlNo,
            ExcludeEquipmentId = excludeEquipmentId
        });

        return count > 0;
    }

    public async Task<IEnumerable<string>> GetExistingControlNumbersAsync(IEnumerable<string> controlNos)
    {
        const string sql = """
            SELECT control_no
            FROM equipments
            WHERE control_no IN @ControlNos
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<string>(sql, new { ControlNos = controlNos.ToArray() });
    }

    public async Task<Section?> GetSectionByIdAsync(int sectionId)
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
            WHERE section_id = @SectionId
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Section>(sql, new { SectionId = sectionId });
    }

    public async Task<IEnumerable<Section>> GetSectionsByCodesAsync(IEnumerable<string> sectionCodes)
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
            WHERE section_code IN @SectionCodes
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Section>(sql, new { SectionCodes = sectionCodes.ToArray() });
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
    {
        const string sql = """
            SELECT
                employee_id,
                employee_code,
                full_name,
                email,
                date_of_birth,
                gender
            FROM Shared.dbo.employees
            WHERE employee_id = @EmployeeId
              AND is_active = 1
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Employee>(sql, new { EmployeeId = employeeId });
    }

    public async Task<Employee?> GetEmployeeByCodeAsync(string employeeCode)
    {
        const string sql = """
            SELECT
                employee_id,
                employee_code,
                full_name,
                email,
                date_of_birth,
                gender
            FROM Shared.dbo.employees
            WHERE employee_code = @EmployeeCode
              AND is_active = 1
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Employee>(sql, new { EmployeeCode = employeeCode });
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByCodesAsync(IEnumerable<string> employeeCodes)
    {
        const string sql = """
            SELECT
                employee_id,
                employee_code,
                full_name,
                email,
                date_of_birth,
                gender
            FROM Shared.dbo.employees
            WHERE employee_code IN @EmployeeCodes
              AND is_active = 1
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Employee>(sql, new { EmployeeCodes = employeeCodes.ToArray() });
    }

    public async Task<int> CreateAsync(Equipment entity)
    {
        const string sql = """
            INSERT INTO equipments (
                equipment_name,
                control_no,
                serial_no,
                brand,
                model,
                location,
                section_id,
                pic_id,
                pic_code,
                pic_name,
                calib_interval_months,
                last_calib_date,
                calib_type,
                equipment_status,
                remarks,
                created_at,
                updated_at,
                created_by,
                updated_by
            ) VALUES (
                @EquipmentName,
                @ControlNo,
                @SerialNo,
                @Brand,
                @Model,
                @Location,
                @SectionId,
                @PicId,
                @PicCode,
                @PicName,
                @CalibIntervalMonths,
                @LastCalibDate,
                @CalibType,
                @EquipmentStatus,
                @Remarks,
                @CreatedAt,
                @UpdatedAt,
                @CreatedBy,
                @UpdatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<IReadOnlyCollection<int>> CreateManyAsync(IEnumerable<Equipment> entities)
    {
        const string sql = """
            INSERT INTO equipments (
                equipment_name,
                control_no,
                serial_no,
                brand,
                model,
                location,
                section_id,
                pic_id,
                pic_code,
                pic_name,
                calib_interval_months,
                last_calib_date,
                calib_type,
                equipment_status,
                remarks,
                created_at,
                updated_at,
                created_by,
                updated_by
            ) VALUES (
                @EquipmentName,
                @ControlNo,
                @SerialNo,
                @Brand,
                @Model,
                @Location,
                @SectionId,
                @PicId,
                @PicCode,
                @PicName,
                @CalibIntervalMonths,
                @LastCalibDate,
                @CalibType,
                @EquipmentStatus,
                @Remarks,
                @CreatedAt,
                @UpdatedAt,
                @CreatedBy,
                @UpdatedBy
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

    public async Task<bool> UpdateAsync(Equipment entity)
    {
        const string sql = """
            UPDATE equipments
            SET equipment_name = @EquipmentName,
                control_no = @ControlNo,
                serial_no = @SerialNo,
                brand = @Brand,
                model = @Model,
                location = @Location,
                section_id = @SectionId,
                pic_id = @PicId,
                pic_code = @PicCode,
                pic_name = @PicName,
                calib_interval_months = @CalibIntervalMonths,
                last_calib_date = @LastCalibDate,
                calib_type = @CalibType,
                equipment_status = @EquipmentStatus,
                remarks = @Remarks,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id
            """;

        var affected = await ExecuteAsync(sql, entity);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int equipmentId)
    {
        const string sql = "DELETE FROM equipments WHERE id = @EquipmentId";
        var affected = await ExecuteAsync(sql, new { EquipmentId = equipmentId });
        return affected > 0;
    }

    public async Task<int> DeleteManyAsync(IEnumerable<int> equipmentIds)
    {
        const string sql = "DELETE FROM equipments WHERE id IN @EquipmentIds";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { EquipmentIds = equipmentIds.ToArray() });
    }

    public async Task<int> UpdateSectionManyAsync(IEnumerable<int> equipmentIds, int sectionId, string updatedBy, DateTime updatedAt)
    {
        const string sql = """
            UPDATE equipments
            SET section_id = @SectionId,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id IN @EquipmentIds
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            EquipmentIds = equipmentIds.ToArray(),
            SectionId = sectionId,
            UpdatedBy = updatedBy,
            UpdatedAt = updatedAt
        });
    }

    public async Task<int> UpdatePicManyAsync(IEnumerable<int> equipmentIds, int picId, string picCode, string picName, string updatedBy, DateTime updatedAt)
    {
        const string sql = """
            UPDATE equipments
            SET pic_id = @PicId,
                pic_code = @PicCode,
                pic_name = @PicName,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id IN @EquipmentIds
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            EquipmentIds = equipmentIds.ToArray(),
            PicId = picId,
            PicCode = picCode,
            PicName = picName,
            UpdatedBy = updatedBy,
            UpdatedAt = updatedAt
        });
    }

    public async Task<int> UpdateStatusManyAsync(IEnumerable<int> equipmentIds, string equipmentStatus, string updatedBy, DateTime updatedAt)
    {
        const string sql = """
            UPDATE equipments
            SET equipment_status = @EquipmentStatus,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id IN @EquipmentIds
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new
        {
            EquipmentIds = equipmentIds.ToArray(),
            EquipmentStatus = equipmentStatus,
            UpdatedBy = updatedBy,
            UpdatedAt = updatedAt
        });
    }

    private static string BuildWhereClause(EquipmentFilterParams filters, DynamicParameters parameters)
    {
        var where = new List<string>();

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            where.Add("""
                (
                    e.equipment_name LIKE @Search
                    OR e.control_no LIKE @Search
                    OR ISNULL(e.serial_no, '') LIKE @Search
                    OR ISNULL(e.brand, '') LIKE @Search
                    OR ISNULL(e.model, '') LIKE @Search
                    OR e.location LIKE @Search
                    OR e.pic_code LIKE @Search
                    OR e.pic_name LIKE @Search
                    OR ISNULL(s.section_code, '') LIKE @Search
                    OR ISNULL(s.section_name, '') LIKE @Search
                )
                """);
            parameters.Add("Search", $"%{filters.Search.Trim()}%");
        }

        if (filters.SectionId.HasValue)
        {
            where.Add("e.section_id = @SectionId");
            parameters.Add("SectionId", filters.SectionId.Value);
        }

        if (filters.PicId.HasValue)
        {
            where.Add("e.pic_id = @PicId");
            parameters.Add("PicId", filters.PicId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.CalibType))
        {
            where.Add("e.calib_type = @CalibType");
            parameters.Add("CalibType", filters.CalibType.Trim().ToUpperInvariant());
        }

        if (!string.IsNullOrWhiteSpace(filters.EquipmentStatus))
        {
            where.Add("e.equipment_status = @EquipmentStatus");
            parameters.Add("EquipmentStatus", filters.EquipmentStatus.Trim().ToUpperInvariant());
        }

        if (filters.LastCalibYear.HasValue)
        {
            where.Add("e.last_calib_year = @LastCalibYear");
            parameters.Add("LastCalibYear", filters.LastCalibYear.Value);
        }

        if (filters.LastCalibMonth.HasValue)
        {
            where.Add("e.last_calib_month = @LastCalibMonth");
            parameters.Add("LastCalibMonth", filters.LastCalibMonth.Value);
        }

        if (filters.NextCalibYear.HasValue)
        {
            where.Add("e.next_calib_year = @NextCalibYear");
            parameters.Add("NextCalibYear", filters.NextCalibYear.Value);
        }

        if (filters.NextCalibMonth.HasValue)
        {
            where.Add("e.next_calib_month = @NextCalibMonth");
            parameters.Add("NextCalibMonth", filters.NextCalibMonth.Value);
        }

        return where.Count == 0
            ? string.Empty
            : $" WHERE {string.Join(" AND ", where)}";
    }
}
