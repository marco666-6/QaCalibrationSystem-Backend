using Dapper;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Entities;
using Project.Infrastructure.Data;

namespace Project.Infrastructure.Repositories;

// ─── DefaultLocation ─────────────────────────────────────────────────────────

public sealed class DefaultLocationRepository : BaseRepository<DefaultLocation>, IDefaultLocationRepository
{
    public DefaultLocationRepository(IDbConnectionFactory cf) : base(cf) { }

    public async Task<IEnumerable<DefaultLocation>> GetAllAsync(bool? isActive = null)
    {
        var where = isActive.HasValue ? "WHERE is_active = @IsActive" : "";
        var sql = $"SELECT * FROM default_locations {where} ORDER BY default_location_name";
        return await QueryAsync(sql, isActive.HasValue ? new { IsActive = isActive.Value } : null);
    }

    public async Task<DefaultLocation?> GetByIdAsync(int id)
        => await QuerySingleOrDefaultAsync(
            "SELECT * FROM default_locations WHERE default_location_id = @Id", new { Id = id });

    public async Task<int> UpsertAsync(DefaultLocation e)
    {
        if (e.DefaultLocationId == 0)
        {
            var sql = """
                INSERT INTO default_locations (default_location_name, is_active, created_at, created_by)
                OUTPUT INSERTED.default_location_id
                VALUES (@DefaultLocationName, @IsActive, GETDATE(), @CreatedBy)
                """;
            return await ExecuteScalarAsync<int>(sql, e);
        }
        else
        {
            var sql = """
                UPDATE default_locations
                SET default_location_name = @DefaultLocationName,
                    is_active = @IsActive,
                    updated_at = GETDATE(),
                    updated_by = @UpdatedBy
                WHERE default_location_id = @DefaultLocationId
                """;
            await ExecuteAsync(sql, e);
            return e.DefaultLocationId;
        }
    }

    public async Task<bool> DeleteAsync(int id)
        => await ExecuteAsync("DELETE FROM default_locations WHERE default_location_id = @Id", new { Id = id }) > 0;

    public async Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        var sql = """
            SELECT COUNT(1) FROM default_locations
            WHERE default_location_name = @Name
              AND (@ExcludeId IS NULL OR default_location_id <> @ExcludeId)
            """;
        using var conn = _connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, new { Name = name, ExcludeId = excludeId }) > 0;
    }
}

// ─── SectionEmail ─────────────────────────────────────────────────────────────

public sealed class SectionEmailRepository : BaseRepository<SectionEmail>, ISectionEmailRepository
{
    public SectionEmailRepository(IDbConnectionFactory cf) : base(cf) { }

    public async Task<IEnumerable<SectionEmail>> GetAllAsync(bool? isActive = null)
    {
        var where = isActive.HasValue ? "WHERE is_active = @IsActive" : "";
        return await QueryAsync($"SELECT * FROM section_emails {where} ORDER BY section_code",
            isActive.HasValue ? new { IsActive = isActive.Value } : null);
    }

    public async Task<SectionEmail?> GetByIdAsync(int id)
        => await QuerySingleOrDefaultAsync("SELECT * FROM section_emails WHERE section_email_id = @Id", new { Id = id });

    public async Task<int> UpsertAsync(SectionEmail e)
    {
        if (e.SectionEmailId == 0)
        {
            var sql = """
                INSERT INTO section_emails (section_id, section_code, section_name, email, is_active, created_at, created_by)
                OUTPUT INSERTED.section_email_id
                VALUES (@SectionId, @SectionCode, @SectionName, @Email, @IsActive, GETDATE(), @CreatedBy)
                """;
            return await ExecuteScalarAsync<int>(sql, e);
        }
        else
        {
            await ExecuteAsync("""
                UPDATE section_emails
                SET section_id = @SectionId, section_code = @SectionCode, section_name = @SectionName,
                    email = @Email, is_active = @IsActive, updated_at = GETDATE(), updated_by = @UpdatedBy
                WHERE section_email_id = @SectionEmailId
                """, e);
            return e.SectionEmailId;
        }
    }

    public async Task<bool> DeleteAsync(int id)
        => await ExecuteAsync("DELETE FROM section_emails WHERE section_email_id = @Id", new { Id = id }) > 0;
}

// ─── SectionPicEmail ──────────────────────────────────────────────────────────

public sealed class SectionPicEmailRepository : BaseRepository<SectionPicEmail>, ISectionPicEmailRepository
{
    public SectionPicEmailRepository(IDbConnectionFactory cf) : base(cf) { }

    public async Task<IEnumerable<SectionPicEmail>> GetAllAsync(bool? isActive = null)
    {
        var where = isActive.HasValue ? "WHERE is_active = @IsActive" : "";
        return await QueryAsync($"SELECT * FROM section_pic_emails {where} ORDER BY section_code",
            isActive.HasValue ? new { IsActive = isActive.Value } : null);
    }

    public async Task<SectionPicEmail?> GetByIdAsync(int id)
        => await QuerySingleOrDefaultAsync("SELECT * FROM section_pic_emails WHERE section_pic_email_id = @Id", new { Id = id });

    public async Task<int> UpsertAsync(SectionPicEmail e)
    {
        if (e.SectionPicEmailId == 0)
        {
            var sql = """
                INSERT INTO section_pic_emails (section_id, section_code, section_name, pic_name, email, is_active, created_at, created_by)
                OUTPUT INSERTED.section_pic_email_id
                VALUES (@SectionId, @SectionCode, @SectionName, @PicName, @Email, @IsActive, GETDATE(), @CreatedBy)
                """;
            return await ExecuteScalarAsync<int>(sql, e);
        }
        else
        {
            await ExecuteAsync("""
                UPDATE section_pic_emails
                SET section_id = @SectionId, section_code = @SectionCode, section_name = @SectionName,
                    pic_name = @PicName, email = @Email, is_active = @IsActive,
                    updated_at = GETDATE(), updated_by = @UpdatedBy
                WHERE section_pic_email_id = @SectionPicEmailId
                """, e);
            return e.SectionPicEmailId;
        }
    }

    public async Task<bool> DeleteAsync(int id)
        => await ExecuteAsync("DELETE FROM section_pic_emails WHERE section_pic_email_id = @Id", new { Id = id }) > 0;
}

// ─── CalibRole ────────────────────────────────────────────────────────────────

public sealed class CalibRoleRepository : BaseRepository<CalibRole>, ICalibRoleRepository
{
    public CalibRoleRepository(IDbConnectionFactory cf) : base(cf) { }

    private const string SelectWithUser = """
        SELECT r.*, u.username, e.full_name
        FROM roles r
        JOIN users u ON r.user_id = u.user_id
        LEFT JOIN Shared.dbo.employees e ON e.employee_id = u.employee_id
        """;

    public async Task<IEnumerable<CalibRole>> GetAllAsync(bool? isActive = null)
    {
        var where = isActive.HasValue ? "WHERE r.is_active = @IsActive" : "";
        return await QueryAsync($"{SelectWithUser} {where} ORDER BY r.role, u.username",
            isActive.HasValue ? new { IsActive = isActive.Value } : null);
    }

    public async Task<CalibRole?> GetByIdAsync(int id)
        => await QuerySingleOrDefaultAsync($"{SelectWithUser} WHERE r.id = @Id", new { Id = id });

    public async Task<IEnumerable<CalibRole>> GetByUserIdAsync(int userId)
        => await QueryAsync($"{SelectWithUser} WHERE r.user_id = @UserId", new { UserId = userId });

    public async Task<IEnumerable<CalibRole>> GetByRoleAsync(string role)
        => await QueryAsync($"{SelectWithUser} WHERE r.role = @Role AND r.is_active = 1", new { Role = role });

    public async Task<int> CreateAsync(CalibRole e)
    {
        var sql = """
            INSERT INTO roles (user_id, role, is_active, created_at, created_by)
            OUTPUT INSERTED.id
            VALUES (@UserId, @Role, 1, GETDATE(), @CreatedBy)
            """;
        return await ExecuteScalarAsync<int>(sql, e);
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive, string updatedBy)
        => await ExecuteAsync("""
            UPDATE roles SET is_active = @IsActive, updated_at = GETDATE(), updated_by = @UpdatedBy
            WHERE id = @Id
            """, new { Id = id, IsActive = isActive, UpdatedBy = updatedBy }) > 0;

    public async Task<bool> DeleteAsync(int id)
        => await ExecuteAsync("DELETE FROM roles WHERE id = @Id", new { Id = id }) > 0;
}

// ─── External ─────────────────────────────────────────────────────────────────

public sealed class ExternalRepository : BaseRepository<External>, IExternalRepository
{
    public ExternalRepository(IDbConnectionFactory cf) : base(cf) { }

    public async Task<IEnumerable<External>> GetAllAsync(bool? isActive = null)
    {
        var where = isActive.HasValue ? "WHERE is_active = @IsActive" : "";
        return await QueryAsync($"SELECT * FROM externals {where} ORDER BY external_company",
            isActive.HasValue ? new { IsActive = isActive.Value } : null);
    }

    public async Task<External?> GetByIdAsync(int id)
        => await QuerySingleOrDefaultAsync("SELECT * FROM externals WHERE external_id = @Id", new { Id = id });

    public async Task<int> UpsertAsync(External e)
    {
        if (e.ExternalId == 0)
        {
            var sql = """
                INSERT INTO externals (external_company, external_email, external_phone, address, is_active, created_at, created_by)
                OUTPUT INSERTED.external_id
                VALUES (@ExternalCompany, @ExternalEmail, @ExternalPhone, @Address, @IsActive, GETDATE(), @CreatedBy)
                """;
            return await ExecuteScalarAsync<int>(sql, e);
        }
        else
        {
            await ExecuteAsync("""
                UPDATE externals
                SET external_company = @ExternalCompany, external_email = @ExternalEmail,
                    external_phone = @ExternalPhone, address = @Address, is_active = @IsActive,
                    updated_at = GETDATE(), updated_by = @UpdatedBy
                WHERE external_id = @ExternalId
                """, e);
            return e.ExternalId;
        }
    }

    public async Task<bool> DeleteAsync(int id)
        => await ExecuteAsync("DELETE FROM externals WHERE external_id = @Id", new { Id = id }) > 0;
}

// ─── Equipment ────────────────────────────────────────────────────────────────

public sealed class EquipmentRepository : BaseRepository<Equipment>, IEquipmentRepository
{
    public EquipmentRepository(IDbConnectionFactory cf) : base(cf) { }

    public async Task<(IEnumerable<Equipment> Items, int TotalCount)> GetPagedAsync(EquipmentFilterParams filters)
    {
        var where = new List<string> { "e.is_scrapped = @IsScrapped" };
        var p = new DynamicParameters();
        p.Add("IsScrapped", filters.IsScrapped);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            where.Add("(e.equipment_name LIKE @S OR e.control_no LIKE @S OR e.serial_no LIKE @S)");
            p.Add("S", $"%{filters.Search.Trim()}%");
        }
        if (!string.IsNullOrWhiteSpace(filters.SectionCode))
        {
            where.Add("e.section_code = @SectionCode");
            p.Add("SectionCode", filters.SectionCode);
        }
        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            where.Add("e.equipment_status = @Status");
            p.Add("Status", filters.Status);
        }
        if (!string.IsNullOrWhiteSpace(filters.CalibType))
        {
            where.Add("e.calib_type = @CalibType");
            p.Add("CalibType", filters.CalibType);
        }

        var whereStr = "WHERE " + string.Join(" AND ", where);

        var countSql = $"SELECT COUNT(*) FROM equipments e {whereStr}";
        var dataSql = $"""
            SELECT * FROM equipments e {whereStr}
            ORDER BY e.equipment_name, e.control_no
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        p.Add("Offset", filters.Offset);
        p.Add("PageSize", filters.PageSize);

        using var conn = _connectionFactory.CreateConnection();
        var total = await conn.ExecuteScalarAsync<int>(countSql, p);
        if (total == 0) return ([], 0);
        var items = await conn.QueryAsync<Equipment>(dataSql, p);
        return (items, total);
    }

    public async Task<Equipment?> GetByIdAsync(int id)
        => await QuerySingleOrDefaultAsync("SELECT * FROM equipments WHERE id = @Id", new { Id = id });

    public async Task<Equipment?> GetByControlNoAsync(string controlNo)
        => await QuerySingleOrDefaultAsync("SELECT * FROM equipments WHERE control_no = @ControlNo", new { ControlNo = controlNo });

    public async Task<IEnumerable<Equipment>> GetDueForPlanAsync(int planMonth, int planYear)
    {
        var sql = """
            SELECT * FROM equipments
            WHERE is_scrapped = 0 AND equipment_status = 'Active'
              AND (last_calib_date IS NULL
                   OR next_calib_year < @Year
                   OR (next_calib_year = @Year AND next_calib_month <= @Month))
            ORDER BY next_calib_date ASC, equipment_name ASC
            """;
        return await QueryAsync(sql, new { Month = planMonth, Year = planYear });
    }

    public async Task<int> UpsertAsync(Equipment e, string by)
    {
        if (e.Id == 0)
        {
            var sql = """
                INSERT INTO equipments (equipment_name, control_no, serial_no, brand, model, range,
                    location, section_id, section_code, section_name, calib_interval_months,
                    last_calib_date, calib_type, equipment_status, remarks, created_at, created_by)
                OUTPUT INSERTED.id
                VALUES (@EquipmentName, @ControlNo, @SerialNo, @Brand, @Model, @Range,
                    @Location, @SectionId, @SectionCode, @SectionName, @CalibIntervalMonths,
                    @LastCalibDate, @CalibType, @EquipmentStatus, @Remarks, GETDATE(), @By)
                """;
            using var conn = _connectionFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                e.EquipmentName,
                e.ControlNo,
                e.SerialNo,
                e.Brand,
                e.Model,
                e.Range,
                e.Location,
                e.SectionId,
                e.SectionCode,
                e.SectionName,
                e.CalibIntervalMonths,
                e.LastCalibDate,
                e.CalibType,
                e.EquipmentStatus,
                e.Remarks,
                By = by
            });
        }
        else
        {
            await ExecuteAsync("""
                UPDATE equipments SET
                    equipment_name = @EquipmentName, control_no = @ControlNo, serial_no = @SerialNo,
                    brand = @Brand, model = @Model, range = @Range, location = @Location,
                    section_id = @SectionId, section_code = @SectionCode, section_name = @SectionName,
                    calib_interval_months = @CalibIntervalMonths, last_calib_date = @LastCalibDate,
                    calib_type = @CalibType, equipment_status = @EquipmentStatus, remarks = @Remarks,
                    updated_at = GETDATE(), updated_by = @By
                WHERE id = @Id AND is_scrapped = 0
                """, new
            {
                e.EquipmentName,
                e.ControlNo,
                e.SerialNo,
                e.Brand,
                e.Model,
                e.Range,
                e.Location,
                e.SectionId,
                e.SectionCode,
                e.SectionName,
                e.CalibIntervalMonths,
                e.LastCalibDate,
                e.CalibType,
                e.EquipmentStatus,
                e.Remarks,
                By = by,
                e.Id
            });
            return e.Id;
        }
    }

    public async Task<bool> ScrapAsync(int id, string? reason, string by)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE equipments
            SET equipment_status = 'Scrap', is_scrapped = 1,
                scrapped_at = GETDATE(), scrapped_by = @By, updated_at = GETDATE()
            WHERE id = @Id AND is_scrapped = 0;
            INSERT INTO scrap_records (equipment_id, action, reason, actioned_at, actioned_by)
            VALUES (@Id, 'Scrap', @Reason, GETDATE(), @By);
            """, new { Id = id, Reason = reason, By = by });
        return true;
    }

    public async Task<bool> RestoreAsync(int id, string? reason, string by)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE equipments
            SET equipment_status = 'Active', is_scrapped = 0, scrapped_at = NULL, scrapped_by = NULL, updated_at = GETDATE()
            WHERE id = @Id AND is_scrapped = 1;
            INSERT INTO scrap_records (equipment_id, action, reason, actioned_at, actioned_by)
            VALUES (@Id, 'Restore', @Reason, GETDATE(), @By);
            """, new { Id = id, Reason = reason, By = by });
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, string by)
    {
        using var conn = _connectionFactory.CreateConnection();
        var isScrapped = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM equipments WHERE id = @Id AND is_scrapped = 1", new { Id = id });
        if (isScrapped == 0) throw new InvalidOperationException("Equipment must be in Scrap bin before permanent deletion.");

        await conn.ExecuteAsync("""
            INSERT INTO scrap_records (equipment_id, action, actioned_at, actioned_by)
            VALUES (@Id, 'Delete', GETDATE(), @By);
            DELETE FROM equipments WHERE id = @Id;
            """, new { Id = id, By = by });
        return true;
    }

    public async Task<bool> BulkUpdateAsync(List<int> ids, string action, BulkEquipmentRequest req, string by)
    {
        var idList = string.Join(",", ids);
        using var conn = _connectionFactory.CreateConnection();

        switch (action)
        {
            case "status":
                await conn.ExecuteAsync($"""
                    UPDATE e SET e.equipment_status = @StatusValue, e.updated_at = GETDATE(), e.updated_by = @By
                    FROM equipments e WHERE e.id IN ({idList}) AND e.is_scrapped = 0
                    """, new { req.StatusValue, By = by });
                break;
            case "section":
                await conn.ExecuteAsync($"""
                    UPDATE e SET e.section_id = @SectionId, e.section_code = @SectionCode, e.section_name = @SectionName,
                        e.updated_at = GETDATE(), e.updated_by = @By
                    FROM equipments e WHERE e.id IN ({idList}) AND e.is_scrapped = 0
                    """, new { req.SectionId, req.SectionCode, req.SectionName, By = by });
                break;
            case "location":
                await conn.ExecuteAsync($"""
                    UPDATE e SET e.location = @LocationValue, e.updated_at = GETDATE(), e.updated_by = @By
                    FROM equipments e WHERE e.id IN ({idList}) AND e.is_scrapped = 0
                    """, new { req.LocationValue, By = by });
                break;
            case "remarks":
                await conn.ExecuteAsync($"""
                    UPDATE e SET e.remarks = @RemarksValue, e.updated_at = GETDATE(), e.updated_by = @By
                    FROM equipments e WHERE e.id IN ({idList}) AND e.is_scrapped = 0
                    """, new { req.RemarksValue, By = by });
                break;
            case "scrap":
                await conn.ExecuteAsync($"""
                    UPDATE e SET e.equipment_status = 'Scrap', e.is_scrapped = 1,
                        e.scrapped_at = GETDATE(), e.scrapped_by = @By, e.updated_at = GETDATE()
                    FROM equipments e WHERE e.id IN ({idList}) AND e.is_scrapped = 0;
                    INSERT INTO scrap_records (equipment_id, action, reason, actioned_at, actioned_by)
                    SELECT id, 'Scrap', @ScrapReason, GETDATE(), @By FROM equipments WHERE id IN ({idList});
                    """, new { By = by, req.ScrapReason });
                break;
        }
        return true;
    }

    public async Task<bool> UpdateCalibResultDatesAsync(int equipmentId, DateOnly lastCalibDate, DateOnly nextCalibDate)
    {
        await ExecuteAsync("""
            UPDATE equipments SET last_calib_date = @LastCalibDate, updated_at = GETDATE()
            WHERE id = @EquipmentId
            """, new { EquipmentId = equipmentId, LastCalibDate = lastCalibDate });
        return true;
    }

    public async Task<bool> SetOutOfServiceAsync(int equipmentId, string by)
    {
        await ExecuteAsync("""
            UPDATE equipments SET equipment_status = 'Out of Service', updated_at = GETDATE(), updated_by = @By
            WHERE id = @Id
            """, new { Id = equipmentId, By = by });
        return true;
    }

    public async Task<IEnumerable<Equipment>> GetScrappedAsync()
        => await QueryAsync("SELECT * FROM equipments WHERE is_scrapped = 1 ORDER BY scrapped_at DESC");

    public async Task<IEnumerable<Equipment>> GetAllActiveAsync()
        => await QueryAsync("SELECT * FROM equipments WHERE is_scrapped = 0 AND equipment_status = 'Active' ORDER BY equipment_name");
}

// ─── Calib Plan ───────────────────────────────────────────────────────────────

public sealed class CalibPlanRepository : BaseRepository<CalibPlan>, ICalibPlanRepository
{
    public CalibPlanRepository(IDbConnectionFactory cf) : base(cf) { }

    private const string SummarySql = """
        SELECT cp.*, up.username AS preparer_username, uc.username AS checker_username, ua.username AS approver_username,
               COUNT(pi.plan_item_id) AS total_items,
               SUM(CASE WHEN pi.is_included = 1 THEN 1 ELSE 0 END) AS included_items
        FROM calib_plans cp
        LEFT JOIN calib_plan_items pi ON cp.plan_id = pi.plan_id
        LEFT JOIN users up ON cp.preparer_user_id = up.user_id
        LEFT JOIN users uc ON cp.checker_user_id = uc.user_id
        LEFT JOIN users ua ON cp.approver_user_id = ua.user_id
        """;

    public async Task<(IEnumerable<CalibPlan> Items, int TotalCount)> GetPagedAsync(CalibPlanFilterParams filters)
    {
        var where = new List<string>();
        var p = new DynamicParameters();

        if (filters.Year.HasValue) { where.Add("cp.plan_year = @Year"); p.Add("Year", filters.Year); }
        if (filters.Month.HasValue) { where.Add("cp.plan_month = @Month"); p.Add("Month", filters.Month); }
        if (!string.IsNullOrWhiteSpace(filters.Status)) { where.Add("cp.status = @Status"); p.Add("Status", filters.Status); }

        var whereStr = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var groupBy = "GROUP BY cp.plan_id, cp.plan_title, cp.plan_month, cp.plan_year, cp.calib_type, cp.status, cp.is_locked, cp.preparer_user_id, up.username, cp.checker_user_id, uc.username, cp.approver_user_id, ua.username, cp.preparer_approved_at, cp.checker_approved_at, cp.approver_approved_at, cp.locked_at, cp.report_pdf_path, cp.created_at, cp.created_by";

        var countSql = $"SELECT COUNT(DISTINCT cp.plan_id) FROM calib_plans cp {whereStr}";
        p.Add("Offset", filters.Offset); p.Add("PageSize", filters.PageSize);
        var dataSql = $"""
            {SummarySql} {whereStr} {groupBy}
            ORDER BY cp.plan_year DESC, cp.plan_month DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        using var conn = _connectionFactory.CreateConnection();
        var total = await conn.ExecuteScalarAsync<int>(countSql, p);
        if (total == 0) return ([], 0);
        var items = await conn.QueryAsync<CalibPlan>(dataSql, p);
        return (items, total);
    }

    public async Task<CalibPlan?> GetByIdAsync(int planId)
    {
        var sql = $"{SummarySql} WHERE cp.plan_id = @PlanId GROUP BY cp.plan_id, cp.plan_title, cp.plan_month, cp.plan_year, cp.calib_type, cp.status, cp.is_locked, cp.preparer_user_id, up.username, cp.checker_user_id, uc.username, cp.approver_user_id, ua.username, cp.preparer_approved_at, cp.checker_approved_at, cp.approver_approved_at, cp.locked_at, cp.report_pdf_path, cp.created_at, cp.created_by, cp.preparer_remark, cp.checker_remark, cp.approver_remark, cp.preparer_cancelled_at, cp.checker_cancelled_at, cp.approver_cancelled_at, cp.updated_at, cp.updated_by";
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<CalibPlan>(sql, new { PlanId = planId });
    }

    public async Task<CalibPlan?> GetDetailAsync(int planId)
    {
        using var conn = _connectionFactory.CreateConnection();
        var plan = await conn.QuerySingleOrDefaultAsync<CalibPlan>(
            $"{SummarySql} WHERE cp.plan_id = @PlanId GROUP BY cp.plan_id, cp.plan_title, cp.plan_month, cp.plan_year, cp.calib_type, cp.status, cp.is_locked, cp.preparer_user_id, up.username, cp.checker_user_id, uc.username, cp.approver_user_id, ua.username, cp.preparer_approved_at, cp.checker_approved_at, cp.approver_approved_at, cp.locked_at, cp.report_pdf_path, cp.created_at, cp.created_by, cp.preparer_remark, cp.checker_remark, cp.approver_remark, cp.preparer_cancelled_at, cp.checker_cancelled_at, cp.approver_cancelled_at, cp.updated_at, cp.updated_by",
            new { PlanId = planId });

        if (plan is null) return null;

        plan.Items = (await conn.QueryAsync<CalibPlanItem>(
            "SELECT * FROM calib_plan_items WHERE plan_id = @PlanId ORDER BY equipment_name",
            new { PlanId = planId })).ToList();

        plan.Technicians = (await conn.QueryAsync<CalibPlanTechnician>("""
            SELECT t.*, u.username, e.full_name
            FROM calib_plan_technicians t
            JOIN users u ON t.user_id = u.user_id
            LEFT JOIN Shared.dbo.employees e ON e.employee_id = u.employee_id
            WHERE t.plan_id = @PlanId
            """, new { PlanId = planId })).ToList();

        plan.Externals = (await conn.QueryAsync<CalibPlanExternal>(
            "SELECT * FROM calib_plan_externals WHERE plan_id = @PlanId",
            new { PlanId = planId })).ToList();

        return plan;
    }

    public async Task<int> CreateAsync(CalibPlan plan)
    {
        var sql = """
            INSERT INTO calib_plans (plan_title, plan_month, plan_year, calib_type, status,
                preparer_user_id, checker_user_id, approver_user_id, created_at, created_by)
            OUTPUT INSERTED.plan_id
            VALUES (@PlanTitle, @PlanMonth, @PlanYear, @CalibType, 'Draft',
                @PreparerUserId, @CheckerUserId, @ApproverUserId, GETDATE(), @CreatedBy)
            """;
        using var conn = _connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, plan);
    }

    public async Task AddItemsAsync(int planId, IEnumerable<CalibPlanItem> items)
    {
        using var conn = _connectionFactory.CreateConnection();
        foreach (var item in items)
        {
            await conn.ExecuteAsync("""
                INSERT INTO calib_plan_items (plan_id, equipment_id, equipment_name, control_no, serial_no,
                    brand, model, range, location, section_code, section_name, calib_interval_months,
                    last_calib_date, next_calib_date, calib_type, is_included, remarks, created_at)
                VALUES (@PlanId, @EquipmentId, @EquipmentName, @ControlNo, @SerialNo,
                    @Brand, @Model, @Range, @Location, @SectionCode, @SectionName, @CalibIntervalMonths,
                    @LastCalibDate, @NextCalibDate, @CalibType, @IsIncluded, @Remarks, GETDATE())
                """, new
            {
                PlanId = planId,
                item.EquipmentId,
                item.EquipmentName,
                item.ControlNo,
                item.SerialNo,
                item.Brand,
                item.Model,
                item.Range,
                item.Location,
                item.SectionCode,
                item.SectionName,
                item.CalibIntervalMonths,
                item.LastCalibDate,
                item.NextCalibDate,
                item.CalibType,
                item.IsIncluded,
                item.Remarks
            });
        }
    }

    public async Task AddTechniciansAsync(int planId, IEnumerable<CalibPlanTechnician> technicians)
    {
        using var conn = _connectionFactory.CreateConnection();
        foreach (var t in technicians)
            await conn.ExecuteAsync("""
                INSERT INTO calib_plan_technicians (plan_id, user_id, is_pic, created_at)
                VALUES (@PlanId, @UserId, @IsPic, GETDATE())
                """, new { PlanId = planId, t.UserId, t.IsPic });
    }

    public async Task AddExternalsAsync(int planId, IEnumerable<CalibPlanExternal> externals)
    {
        using var conn = _connectionFactory.CreateConnection();
        foreach (var ext in externals)
            await conn.ExecuteAsync("""
                INSERT INTO calib_plan_externals (plan_id, external_id, external_company, created_at)
                VALUES (@PlanId, @ExternalId, @ExternalCompany, GETDATE())
                """, new { PlanId = planId, ext.ExternalId, ext.ExternalCompany });
    }

    public async Task<bool> UpdateItemInclusionAsync(int planId, Dictionary<int, bool> map)
    {
        using var conn = _connectionFactory.CreateConnection();
        foreach (var (equipId, included) in map)
            await conn.ExecuteAsync("""
                UPDATE calib_plan_items SET is_included = @IsIncluded, updated_at = GETDATE()
                WHERE plan_id = @PlanId AND equipment_id = @EquipId
                """, new { PlanId = planId, EquipId = equipId, IsIncluded = included });
        return true;
    }

    public async Task<bool> SubmitAsync(int planId, string by)
        => await ExecuteAsync("""
            UPDATE calib_plans SET status = 'Submitted', updated_at = GETDATE(), updated_by = @By
            WHERE plan_id = @PlanId AND status = 'Draft'
            """, new { PlanId = planId, By = by }) > 0;

    public async Task<bool> ApproveAsync(int planId, int userId, string? remark, string by)
    {
        var plan = await GetByIdAsync(planId);
        if (plan is null) return false;

        string? sql = null;
        if (plan.Status == "Submitted" && userId == plan.PreparerUserId)
            sql = "UPDATE calib_plans SET status = 'Preparer Approved', preparer_approved_at = GETDATE(), preparer_remark = @Remark, updated_at = GETDATE(), updated_by = @By WHERE plan_id = @PlanId";
        else if (plan.Status == "Preparer Approved" && userId == plan.CheckerUserId)
            sql = "UPDATE calib_plans SET status = 'Checker Approved', checker_approved_at = GETDATE(), checker_remark = @Remark, updated_at = GETDATE(), updated_by = @By WHERE plan_id = @PlanId";
        else if (plan.Status == "Checker Approved" && userId == plan.ApproverUserId)
            sql = "UPDATE calib_plans SET status = 'Fully Approved', approver_approved_at = GETDATE(), approver_remark = @Remark, updated_at = GETDATE(), updated_by = @By WHERE plan_id = @PlanId";
        else
            throw new InvalidOperationException("Approval step not valid for this user or current plan status.");

        return await ExecuteAsync(sql, new { PlanId = planId, Remark = remark, By = by }) > 0;
    }

    public async Task<bool> CancelApprovalAsync(int planId, int userId, string? remark, string by)
    {
        var plan = await GetByIdAsync(planId);
        if (plan is null) return false;

        string? sql = null;
        if (plan.Status == "Fully Approved" && userId == plan.ApproverUserId)
        {
            if (plan.ApproverApprovedAt.HasValue && DateTime.Now.Subtract(plan.ApproverApprovedAt.Value).TotalHours > 24)
                throw new InvalidOperationException("Approver 1-day cancellation window has expired.");
            sql = "UPDATE calib_plans SET status = 'Checker Approved', approver_approved_at = NULL, approver_cancelled_at = GETDATE(), approver_remark = @Remark, updated_at = GETDATE(), updated_by = @By WHERE plan_id = @PlanId";
        }
        else if (plan.Status == "Checker Approved" && userId == plan.CheckerUserId)
            sql = "UPDATE calib_plans SET status = 'Preparer Approved', checker_approved_at = NULL, checker_cancelled_at = GETDATE(), checker_remark = @Remark, updated_at = GETDATE(), updated_by = @By WHERE plan_id = @PlanId";
        else if (plan.Status == "Preparer Approved" && userId == plan.PreparerUserId)
            sql = "UPDATE calib_plans SET status = 'Submitted', preparer_approved_at = NULL, preparer_cancelled_at = GETDATE(), preparer_remark = @Remark, updated_at = GETDATE(), updated_by = @By WHERE plan_id = @PlanId";
        else
            throw new InvalidOperationException("Cancellation not valid for this user or current plan status.");

        return await ExecuteAsync(sql, new { PlanId = planId, Remark = remark, By = by }) > 0;
    }

    public async Task<bool> LockAsync(int planId, string? pdfPath, string by)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            // Lock plan
            await conn.ExecuteAsync("""
                UPDATE calib_plans
                SET status = 'Locked', is_locked = 1, locked_at = GETDATE(),
                    report_pdf_path = @PdfPath, updated_at = GETDATE(), updated_by = @By
                WHERE plan_id = @PlanId AND status = 'Fully Approved'
                """, new { PlanId = planId, PdfPath = pdfPath, By = by }, tx);

            // Check if actual already exists
            var hasActual = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM calib_actuals WHERE plan_id = @PlanId", new { PlanId = planId }, tx);

            if (hasActual == 0)
            {
                var plan = await conn.QuerySingleAsync<CalibPlan>(
                    "SELECT * FROM calib_plans WHERE plan_id = @PlanId", new { PlanId = planId }, tx);

                // Create actual
                var actualId = await conn.ExecuteScalarAsync<int>("""
                    INSERT INTO calib_actuals (plan_id, plan_month, plan_year, calib_type,
                        preparer_user_id, checker_user_id, approver_user_id, created_at, created_by)
                    OUTPUT INSERTED.actual_id
                    VALUES (@PlanId, @PlanMonth, @PlanYear, @CalibType,
                        @PreparerUserId, @CheckerUserId, @ApproverUserId, GETDATE(), @By)
                    """, new
                {
                    PlanId = planId,
                    plan.PlanMonth,
                    plan.PlanYear,
                    plan.CalibType,
                    plan.PreparerUserId,
                    plan.CheckerUserId,
                    plan.ApproverUserId,
                    By = by
                }, tx);

                // Copy plan items -> actual items
                await conn.ExecuteAsync("""
                    INSERT INTO calib_actual_items (actual_id, plan_item_id, equipment_id, equipment_name,
                        control_no, serial_no, brand, model, range, location, section_code, section_name, calib_type, created_at)
                    SELECT @ActualId, pi.plan_item_id, pi.equipment_id, pi.equipment_name,
                        pi.control_no, pi.serial_no, pi.brand, pi.model, pi.range, pi.location,
                        pi.section_code, pi.section_name, pi.calib_type, GETDATE()
                    FROM calib_plan_items pi
                    WHERE pi.plan_id = @PlanId AND pi.is_included = 1
                    """, new { ActualId = actualId, PlanId = planId }, tx);

                // Copy technicians
                await conn.ExecuteAsync("""
                    INSERT INTO calib_actual_technicians (actual_id, user_id, is_pic, created_at)
                    SELECT @ActualId, user_id, is_pic, GETDATE()
                    FROM calib_plan_technicians WHERE plan_id = @PlanId
                    """, new { ActualId = actualId, PlanId = planId }, tx);

                // Copy externals
                await conn.ExecuteAsync("""
                    INSERT INTO calib_actual_externals (actual_id, external_id, external_company, created_at)
                    SELECT @ActualId, external_id, external_company, GETDATE()
                    FROM calib_plan_externals WHERE plan_id = @PlanId
                    """, new { ActualId = actualId, PlanId = planId }, tx);
            }

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdatePdfPathAsync(int planId, string pdfPath)
        => await ExecuteAsync("UPDATE calib_plans SET report_pdf_path = @PdfPath WHERE plan_id = @PlanId",
            new { PlanId = planId, PdfPath = pdfPath }) > 0;

    public async Task<IEnumerable<CalibPlan>> GetExpiredFullyApprovedAsync()
        => await QueryAsync("""
            SELECT * FROM calib_plans
            WHERE status = 'Fully Approved' AND is_locked = 0
              AND DATEDIFF(HOUR, approver_approved_at, GETDATE()) > 24
            """);
}

// ─── Calib Actual ─────────────────────────────────────────────────────────────

public sealed class CalibActualRepository : BaseRepository<CalibActual>, ICalibActualRepository
{
    public CalibActualRepository(IDbConnectionFactory cf) : base(cf) { }

    private const string SummarySql = """
        SELECT ca.*, up.username AS preparer_username, uc.username AS checker_username, ua.username AS approver_username,
               COUNT(ai.actual_item_id) AS total_items,
               SUM(CASE WHEN ai.calib_result IS NOT NULL THEN 1 ELSE 0 END) AS recorded_items,
               SUM(CASE WHEN ai.calib_result = 'OK' THEN 1 ELSE 0 END) AS ok_count,
               SUM(CASE WHEN ai.calib_result = 'NG' THEN 1 ELSE 0 END) AS ng_count
        FROM calib_actuals ca
        LEFT JOIN calib_actual_items ai ON ca.actual_id = ai.actual_id
        LEFT JOIN users up ON ca.preparer_user_id = up.user_id
        LEFT JOIN users uc ON ca.checker_user_id = uc.user_id
        LEFT JOIN users ua ON ca.approver_user_id = ua.user_id
        """;

    private const string GroupBySql = """
        GROUP BY ca.actual_id, ca.plan_id, ca.plan_month, ca.plan_year, ca.calib_type, ca.status,
            ca.is_closed, ca.closed_at, ca.close_reason, ca.report_has_watermark, ca.report_pdf_path,
            ca.preparer_user_id, up.username, ca.checker_user_id, uc.username, ca.approver_user_id, ua.username,
            ca.preparer_approved_at, ca.checker_approved_at, ca.approver_approved_at,
            ca.preparer_remark, ca.checker_remark, ca.approver_remark,
            ca.preparer_cancelled_at, ca.checker_cancelled_at, ca.approver_cancelled_at,
            ca.created_at, ca.created_by, ca.updated_at, ca.updated_by,
            ca.closed_by, ca.preparer_approved_at, ca.checker_approved_at, ca.approver_approved_at
        """;

    public async Task<(IEnumerable<CalibActual> Items, int TotalCount)> GetPagedAsync(CalibActualFilterParams filters)
    {
        var where = new List<string>();
        var p = new DynamicParameters();

        if (filters.Year.HasValue) { where.Add("ca.plan_year = @Year"); p.Add("Year", filters.Year); }
        if (filters.Month.HasValue) { where.Add("ca.plan_month = @Month"); p.Add("Month", filters.Month); }
        if (!string.IsNullOrWhiteSpace(filters.Status)) { where.Add("ca.status = @Status"); p.Add("Status", filters.Status); }

        var whereStr = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        p.Add("Offset", filters.Offset); p.Add("PageSize", filters.PageSize);

        var countSql = $"SELECT COUNT(DISTINCT ca.actual_id) FROM calib_actuals ca {whereStr}";
        var dataSql = $"{SummarySql} {whereStr} {GroupBySql} ORDER BY ca.plan_year DESC, ca.plan_month DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var conn = _connectionFactory.CreateConnection();
        var total = await conn.ExecuteScalarAsync<int>(countSql, p);
        if (total == 0) return ([], 0);
        var items = await conn.QueryAsync<CalibActual>(dataSql, p);
        return (items, total);
    }

    public async Task<CalibActual?> GetByIdAsync(int actualId)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<CalibActual>(
            $"{SummarySql} WHERE ca.actual_id = @ActualId {GroupBySql}",
            new { ActualId = actualId });
    }

    public async Task<CalibActual?> GetDetailAsync(int actualId)
    {
        using var conn = _connectionFactory.CreateConnection();
        var actual = await conn.QuerySingleOrDefaultAsync<CalibActual>(
            $"{SummarySql} WHERE ca.actual_id = @ActualId {GroupBySql}",
            new { ActualId = actualId });

        if (actual is null) return null;

        actual.Items = (await conn.QueryAsync<CalibActualItem>(
            "SELECT * FROM calib_actual_items WHERE actual_id = @ActualId ORDER BY equipment_name",
            new { ActualId = actualId })).ToList();

        actual.Technicians = (await conn.QueryAsync<CalibActualTechnician>("""
            SELECT t.*, u.username, e.full_name
            FROM calib_actual_technicians t
            JOIN users u ON t.user_id = u.user_id
            LEFT JOIN Shared.dbo.employees e ON e.employee_id = u.employee_id
            WHERE t.actual_id = @ActualId
            """, new { ActualId = actualId })).ToList();

        actual.Externals = (await conn.QueryAsync<CalibActualExternal>(
            "SELECT * FROM calib_actual_externals WHERE actual_id = @ActualId",
            new { ActualId = actualId })).ToList();

        return actual;
    }

    public async Task<CalibActual?> GetByPlanIdAsync(int planId)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<CalibActual>(
            $"{SummarySql} WHERE ca.plan_id = @PlanId {GroupBySql}",
            new { PlanId = planId });
    }

    public async Task<bool> RecordItemResultAsync(int actualItemId, CalibActualItem updated, string by)
    {
        await ExecuteAsync("""
            UPDATE calib_actual_items
            SET standard_calibration = @StandardCalibration,
                calib_result = @CalibResult,
                ng_action = @NgAction,
                calib_date = @CalibDate,
                remarks = @Remarks,
                recorded_by = @By,
                recorded_at = GETDATE(),
                updated_at = GETDATE()
            WHERE actual_item_id = @ActualItemId
            """, new
        {
            ActualItemId = actualItemId,
            updated.StandardCalibration,
            updated.CalibResult,
            updated.NgAction,
            updated.CalibDate,
            updated.Remarks,
            By = by
        });
        return true;
    }

    public async Task<bool> SetStandardCalibrationAsync(int actualId, string equipmentName, string standardCalib, string by)
    {
        await ExecuteAsync("""
            UPDATE calib_actual_items
            SET standard_calibration = @StandardCalib, updated_at = GETDATE()
            WHERE actual_id = @ActualId AND equipment_name = @EquipmentName
            """, new { ActualId = actualId, EquipmentName = equipmentName, StandardCalib = standardCalib });
        return true;
    }

    public async Task<bool> ApproveAsync(int actualId, int userId, string? remark, string by)
    {
        var actual = await GetByIdAsync(actualId);
        if (actual is null) return false;

        string? sql = null;
        if (actual.Status == "In Progress" && userId == actual.PreparerUserId)
            sql = "UPDATE calib_actuals SET status = 'Preparer Approved', preparer_approved_at = GETDATE(), preparer_remark = @Remark, updated_at = GETDATE(), updated_by = @By WHERE actual_id = @ActualId";
        else if (actual.Status == "Preparer Approved" && userId == actual.CheckerUserId)
            sql = "UPDATE calib_actuals SET status = 'Checker Approved', checker_approved_at = GETDATE(), checker_remark = @Remark, updated_at = GETDATE(), updated_by = @By WHERE actual_id = @ActualId";
        else if (actual.Status == "Checker Approved" && userId == actual.ApproverUserId)
            sql = "UPDATE calib_actuals SET status = 'Fully Approved', approver_approved_at = GETDATE(), approver_remark = @Remark, updated_at = GETDATE(), updated_by = @By WHERE actual_id = @ActualId";
        else
            throw new InvalidOperationException("Approval step not valid for this user or current actual status.");

        return await ExecuteAsync(sql, new { ActualId = actualId, Remark = remark, By = by }) > 0;
    }

    public async Task<bool> CancelApprovalAsync(int actualId, int userId, string? remark, string by)
    {
        var actual = await GetByIdAsync(actualId);
        if (actual is null) return false;

        string? sql = null;
        if (actual.Status == "Fully Approved" && userId == actual.ApproverUserId)
        {
            if (actual.ApproverApprovedAt.HasValue && DateTime.Now.Subtract(actual.ApproverApprovedAt.Value).TotalHours > 24)
                throw new InvalidOperationException("Approver 1-day cancellation window has expired.");
            sql = "UPDATE calib_actuals SET status = 'Checker Approved', approver_approved_at = NULL, approver_cancelled_at = GETDATE(), approver_remark = @Remark, updated_at = GETDATE(), updated_by = @By WHERE actual_id = @ActualId";
        }
        else if (actual.Status == "Checker Approved" && userId == actual.CheckerUserId)
            sql = "UPDATE calib_actuals SET status = 'Preparer Approved', checker_approved_at = NULL, checker_cancelled_at = GETDATE(), checker_remark = @Remark, updated_at = GETDATE(), updated_by = @By WHERE actual_id = @ActualId";
        else if (actual.Status == "Preparer Approved" && userId == actual.PreparerUserId)
            sql = "UPDATE calib_actuals SET status = 'In Progress', preparer_approved_at = NULL, preparer_cancelled_at = GETDATE(), preparer_remark = @Remark, updated_at = GETDATE(), updated_by = @By WHERE actual_id = @ActualId";
        else
            throw new InvalidOperationException("Cancellation not valid for this user or current actual status.");

        return await ExecuteAsync(sql, new { ActualId = actualId, Remark = remark, By = by }) > 0;
    }

    public async Task<bool> CloseAsync(int actualId, string? pdfPath, string closeReason, string by)
    {
        var actual = await GetByIdAsync(actualId);
        if (actual is null || actual.IsClosed) return false;

        var isApproved = actual.Status == "Fully Approved";

        await ExecuteAsync("""
            UPDATE calib_actuals
            SET is_closed = 1, status = 'Closed', closed_at = GETDATE(), closed_by = @By,
                close_reason = @CloseReason, report_pdf_path = @PdfPath,
                report_has_watermark = @HasWatermark, updated_at = GETDATE(), updated_by = @By
            WHERE actual_id = @ActualId AND is_closed = 0
            """, new
        {
            ActualId = actualId,
            By = by,
            CloseReason = closeReason,
            PdfPath = pdfPath,
            HasWatermark = !isApproved
        });
        return true;
    }

    public async Task<bool> UpdatePdfAfterApprovalAsync(int actualId, string pdfPath, string by)
        => await ExecuteAsync("""
            UPDATE calib_actuals
            SET report_pdf_path = @PdfPath, report_has_watermark = 0, updated_at = GETDATE(), updated_by = @By
            WHERE actual_id = @ActualId AND status = 'Fully Approved' AND is_closed = 1
            """, new { ActualId = actualId, PdfPath = pdfPath, By = by }) > 0;

    public async Task<IEnumerable<CalibActual>> GetOpenPastMonthsAsync()
    {
        var sql = """
            SELECT * FROM calib_actuals
            WHERE is_closed = 0
              AND (plan_year < YEAR(GETDATE()) OR (plan_year = YEAR(GETDATE()) AND plan_month < MONTH(GETDATE())))
            """;
        return await QueryAsync(sql);
    }
}

// ─── OOS ──────────────────────────────────────────────────────────────────────

public sealed class OosRepository : BaseRepository<OutOfServiceRecord>, IOosRepository
{
    public OosRepository(IDbConnectionFactory cf) : base(cf) { }

    public async Task<IEnumerable<OutOfServiceRecord>> GetAllAsync(bool? isResolved = null)
    {
        var where = isResolved.HasValue ? "WHERE o.is_resolved = @IsResolved" : "";
        var sql = $"""
            SELECT o.*, e.equipment_name, e.control_no, e.section_name
            FROM out_of_service_records o
            JOIN equipments e ON o.equipment_id = e.id
            {where}
            ORDER BY o.created_at DESC
            """;
        return await QueryAsync(sql, isResolved.HasValue ? new { IsResolved = isResolved.Value } : null);
    }

    public async Task<OutOfServiceRecord?> GetByIdAsync(int oosId)
    {
        var sql = """
            SELECT o.*, e.equipment_name, e.control_no, e.section_name
            FROM out_of_service_records o
            JOIN equipments e ON o.equipment_id = e.id
            WHERE o.oos_id = @OosId
            """;
        return await QuerySingleOrDefaultAsync(sql, new { OosId = oosId });
    }

    public async Task<int> CreateAsync(OutOfServiceRecord r)
    {
        var sql = """
            INSERT INTO out_of_service_records (equipment_id, actual_item_id, ng_action, assigned_to,
                expected_return_date, repair_details, created_at, created_by)
            OUTPUT INSERTED.oos_id
            VALUES (@EquipmentId, @ActualItemId, @NgAction, @AssignedTo,
                @ExpectedReturnDate, @RepairDetails, GETDATE(), @CreatedBy)
            """;
        return await ExecuteScalarAsync<int>(sql, r);
    }

    public async Task<bool> UpdateAsync(int oosId, UpdateOosRecordRequest req, string by)
    {
        var record = await GetByIdAsync(oosId);
        if (record is null) return false;

        await ExecuteAsync("""
            UPDATE out_of_service_records
            SET assigned_to = ISNULL(@AssignedTo, assigned_to),
                expected_return_date = ISNULL(@ExpectedReturnDate, expected_return_date),
                repair_details = ISNULL(@RepairDetails, repair_details),
                resolution_note = ISNULL(@ResolutionNote, resolution_note),
                is_resolved = @MarkResolved,
                resolved_at = CASE WHEN @MarkResolved = 1 THEN GETDATE() ELSE NULL END,
                resolved_by = CASE WHEN @MarkResolved = 1 THEN @By ELSE NULL END,
                resolved_status = CASE WHEN @MarkResolved = 1 THEN 'Active' ELSE NULL END,
                updated_at = GETDATE(), updated_by = @By
            WHERE oos_id = @OosId
            """, new
        {
            OosId = oosId,
            req.AssignedTo,
            req.ExpectedReturnDate,
            req.RepairDetails,
            req.ResolutionNote,
            MarkResolved = req.MarkResolved,
            By = by
        });

        if (req.MarkResolved)
            await ExecuteAsync("""
                UPDATE equipments SET equipment_status = 'Active', updated_at = GETDATE(), updated_by = @By
                WHERE id = @EquipmentId AND equipment_status = 'Out of Service'
                """, new { record.EquipmentId, By = by });

        return true;
    }
}