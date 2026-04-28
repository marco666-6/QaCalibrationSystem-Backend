/*
 ============================================================================
 SEED SCRIPT FOR QA_CALIB DATABASE
 ============================================================================
 Prerequisite:
 1. `shared.sql` has been executed successfully.
 2. `newschema.sql` has been executed successfully.
*/

USE qa_calib;
GO

SET NOCOUNT ON;
GO

-- =========================================================================
-- 1. SECTIONS (both main and backup)
-- =========================================================================
INSERT INTO dbo.sections (section_code, section_name) VALUES
('000', 'JAPANESE'),
('110', 'GENERAL AFFAIR'),
('120', 'HUMAN RESOURCES'),
('130', 'TRAINING CENTER'),
('140', 'SUBSIDIARY'),
('200', 'INFORMATION SYSTEM'),
('310', 'MATERIAL CONTROL'),
('330', 'LOGISTIC CONTROL'),
('350', 'PRODUCTION CONTROL'),
('410', 'CUTTING & CRIMPING'),
('420', 'QUALITY CONTROL'),
('430', 'PROCESS ENGINEERING'),
('450', 'ASSEMBLY'),
('460', 'TRAINING ASSY & CC'),
('510', 'PRODUCTION ENGINEERING'),
('520', 'MAINTENANCE'),
('530', 'DOCUMENT CONTROL'),
('550', 'QUALITY ASSURANCE'),
('600', 'FINANCE & ACCOUNTING'),
('610', 'FINANCE'),
('620', 'ACCOUNTING'),
('630', 'PURCHASING'),
('700', 'SAFETY & MTA BUILDING'),
('800', 'PURCHASING'),
('910', 'ELECTRICAL APPLIANCES W/H'),
('930', 'DESIGN');

INSERT INTO dbo.sections_bkp (section_code, section_name)
SELECT section_code, section_name FROM dbo.sections;
GO

-- =========================================================================
-- 2. POSITIONS (both main and backup)
-- =========================================================================
INSERT INTO dbo.positions (position_code, position_name) VALUES
('010', 'PRESIDENT DIRECTOR'),
('020', 'DIRECTOR'),
('025', 'SEN. GENERAL MANAGER'),
('030', 'GENERAL MANAGER'),
('040', 'DEPUTY GENERAL MANAGER'),
('050', 'ASST. GENERAL MANAGER'),
('055', 'SENIOR MANAGER'),
('060', 'MANAGER'),
('070', 'DEPUTY MANAGER'),
('080', 'ASST. MANAGER'),
('081', 'EXECUTIVE SECRETARY'),
('082', 'INTERPRETER II'),
('090', 'SENIOR SUPERVISOR'),
('091', 'ENGINEER'),
('092', 'SEN. SYSTEM ENGINEER'),
('093', 'SENIOR OFFICER'),
('094', 'INTERPRETER I'),
('100', 'SUPERVISOR'),
('101', 'ASST. ENGINEER'),
('102', 'SYSTEM ENGINEER'),
('103', 'OFFICER'),
('104', 'INTERPRETER'),
('110', 'FOREMAN'),
('111', 'ASST. SYSTEM ENGINEER'),
('112', 'ASST.OFFICER'),
('120', 'SENIOR LEADER'),
('121', 'SENIOR TECHNICIAN'),
('123', 'SENIOR CLERK'),
('130', 'LEADER'),
('131', 'TECHNICIAN'),
('132', 'CLERK'),
('140', 'SUB LEADER'),
('141', 'JUNIOR TECHINICIAN'),
('142', 'JUNIOR CLERK'),
('150', 'OPERATOR'),
('151', 'TECHNICAL OPERATOR'),
('152', 'ADM. OPERATOR'),
('153', 'OPERATOR DC'),
('160', 'SECURITY'),
('170', 'DRIVER'),
('180', 'OFFICE BOY / GIRL'),
('190', 'SENIOR NURSE'),
('191', 'NURSE'),
('192', 'JUNIOR NURSE'),
('200', 'TRAINEE');

INSERT INTO dbo.positions_bkp (position_code, position_name)
SELECT position_code, position_name FROM dbo.positions;
GO

-- =========================================================================
-- 3. LOCATIONS
-- =========================================================================
INSERT INTO dbo.locations (location_name) VALUES
('SBI Plant 1 - Floor 1'),
('SBI Plant 1 - Floor 2'),
('SBI Plant 1 - Floor 3'),
('SBI Plant 2 - Floor 1'),
('SBI Plant 2 - Floor 2'),
('SBI Plant 2 - Floor 3'),
('SBI Plant 3 - Floor 1'),
('SBI Plant 3 - Floor 2'),
('SBI Plant 3 - Floor 3'),
('With External or Vendor');

DECLARE @sectionAbbrev TABLE (section_code NVARCHAR(50), abbrev NVARCHAR(50));

INSERT INTO @sectionAbbrev (section_code, abbrev) VALUES
('000', 'JPN'), ('110', 'GA'), ('120', 'HR'),
('130', 'TC'), ('140', 'SUB'), ('200', 'IS'),
('310', 'MC'), ('330', 'LC'), ('350', 'PC'),
('410', 'C&C'), ('420', 'QC'), ('430', 'PE'),
('450', 'ASSY'), ('460', 'TA&CC'), ('510', 'ProdEng'),
('520', 'MAINT'), ('530', 'DC'), ('550', 'QA'),
('600', 'F&A'), ('610', 'FIN'), ('620', 'ACC'),
('630', 'PUR'), ('700', 'S&MB'), ('800', 'PUR'),
('910', 'EAWH'), ('930', 'DSGN');

INSERT INTO dbo.locations (location_name)
SELECT CONCAT('In Section ', abbrev, ' Room')
FROM dbo.sections s
INNER JOIN @sectionAbbrev a ON s.section_code = a.section_code;
GO

-- =========================================================================
-- 4. USERS
--    Assumes Shared.dbo.employees has been seeded by `shared.sql`.
-- =========================================================================
INSERT INTO dbo.users (employee_id, username, password_hash, email, role, is_active, must_change_password)
SELECT
    e.employee_id,
    e.employee_code,
    CONVERT(NVARCHAR(500), HASHBYTES('SHA2_256', 'P@ssw0rd'), 2),
    CONCAT(e.employee_code, '@example.com'),
    'Employee',
    1,
    1
FROM Shared.dbo.employees e
WHERE e.employee_code IN ('220021', '222299', '223549', '240127');
GO

-- =========================================================================
-- 5. CALIBRATION APPROVERS
-- =========================================================================
INSERT INTO dbo.qa_calib_approvers (employee_id, step_no, is_active, created_by)
SELECT
    e.employee_id,
    CASE e.employee_code
        WHEN '220021' THEN '1'
        WHEN '222299' THEN '2'
        WHEN '223549' THEN '3'
    END,
    1,
    '220021'
FROM Shared.dbo.employees e
WHERE e.employee_code IN ('220021', '222299', '223549');
GO


-- =========================================================================
-- 6. CALIBRATION EQUIPMENTS (migrated from legacy qa_calib_reg_eq export)
-- =========================================================================
IF OBJECT_ID('tempdb..#legacy_qa_calib_reg_eq', 'U') IS NOT NULL DROP TABLE #legacy_qa_calib_reg_eq;

CREATE TABLE #legacy_qa_calib_reg_eq (
    id               INT NOT NULL,
    ctrl_no          NVARCHAR(50) NOT NULL,
    eq_nm            NVARCHAR(200) NOT NULL,
    serial_no        NVARCHAR(100) NULL,
    brand_model      NVARCHAR(200) NULL,
    location         NVARCHAR(100) NOT NULL,
    section_cd       NVARCHAR(10) NOT NULL,
    eq_pic_nik       NVARCHAR(100) NULL,
    eq_pic_nm        NVARCHAR(100) NOT NULL,
    last_calib_dt    DATE NOT NULL,
    calib_interval   INT NOT NULL,
    next_calib_dt    DATE NULL,
    calib_type       CHAR(1) NOT NULL,
    eq_status        CHAR(1) NOT NULL,
    remarks          NVARCHAR(MAX) NULL,
    ent_by           NVARCHAR(20) NOT NULL,
    ent_dt           DATETIME NULL,
    upd_by           NVARCHAR(20) NULL,
    upd_dt           DATETIME NULL
);
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (1, N'DC 06', N'Digital Caliper', N'A22078419', N'Mitutoyo / CD-15AXR', N'Dismantle', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-12-12' AS Date), 12, CAST(N'2026-12-12' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-20T13:32:00.017' AS DateTime), N'212325', CAST(N'2026-02-21T16:15:30.757' AS DateTime))
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (2, N'CB 01', N'Standard Weights ', N'313082', N'Mitutoyo', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2026-03-17' AS Date), 12, CAST(N'2027-03-17' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T09:48:11.083' AS DateTime), N'212325', CAST(N'2026-03-26T11:12:51.307' AS DateTime))
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (3, N'CB 02', N'Gauge Block Set', N'1904767', N'Mitutoyo', N'Maintenance CC', N'520', N'212325', N'Rismawati', CAST(N'2025-05-26' AS Date), 12, CAST(N'2026-05-26' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T13:59:06.377' AS DateTime), N'212325', CAST(N'2026-02-25T15:50:08.453' AS DateTime))
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (5, N'SR 02', N'Standart Stainless Steel Ruler', N'SN13064', N'Shinwa', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-06-26' AS Date), 12, CAST(N'2026-06-26' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T14:05:56.200' AS DateTime), NULL, NULL)
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (6, N'SR 01', N'Standart Stainless Steel Ruler', N'SN13048', N'Shinwa', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-06-26' AS Date), 12, CAST(N'2026-06-26' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T14:11:49.163' AS DateTime), NULL, NULL)
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (7, N'SW 01', N'Standart Stainless Weights', N'-', N'-', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-08-21' AS Date), 12, CAST(N'2026-08-21' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T14:22:12.340' AS DateTime), N'212325', CAST(N'2026-02-21T15:36:41.093' AS DateTime))
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (8, N'SW 02', N'Standart Stainless Weights', N'-', N'-', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-12-09' AS Date), 12, CAST(N'2026-12-09' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T14:24:30.117' AS DateTime), NULL, NULL)
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (9, N'M5', N'Standard Weights ', N'BC-1034', N'1 kg', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-08-21' AS Date), 12, CAST(N'2026-08-21' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T14:30:34.440' AS DateTime), N'212325', CAST(N'2026-02-21T15:20:20.920' AS DateTime))
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (10, N'M6', N'Standard Weights ', N'-', N'2 kg', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-05-15' AS Date), 12, CAST(N'2026-05-15' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T14:58:07.413' AS DateTime), N'212325', CAST(N'2026-02-21T15:17:58.227' AS DateTime))
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (11, N'M7', N'Standard Weights ', N'-', N'2 kg', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-08-10' AS Date), 10, CAST(N'2026-06-10' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T14:59:50.180' AS DateTime), N'212325', CAST(N'2026-02-21T15:18:50.800' AS DateTime))
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (12, N'M8', N'Standard Weights ', N'BC-1035', N'5 kg', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-08-10' AS Date), 12, CAST(N'2026-08-10' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T15:02:44.817' AS DateTime), N'212325', CAST(N'2026-02-21T15:19:59.267' AS DateTime))
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (13, N'M9', N'Standard Weights ', N'-', N'10 kg', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-05-15' AS Date), 12, CAST(N'2026-05-15' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T15:04:38.450' AS DateTime), N'212325', CAST(N'2026-03-26T11:10:01.967' AS DateTime))
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (14, N'M10', N'Standard Weights ', N'-', N'10 kg', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-08-10' AS Date), 10, CAST(N'2026-06-10' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T15:14:52.663' AS DateTime), N'212325', CAST(N'2026-02-21T15:18:26.003' AS DateTime))
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (15, N'M11', N'Standard Weights ', N'-', N'20 kg', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-08-10' AS Date), 12, CAST(N'2026-08-10' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T15:16:31.270' AS DateTime), N'212325', CAST(N'2026-02-21T15:19:16.250' AS DateTime))
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (16, N'M12', N'Standard Weights ', N'-', N'10 kg', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-08-10' AS Date), 12, CAST(N'2026-08-10' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T15:22:48.063' AS DateTime), NULL, NULL)
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (17, N'M13', N'Standard Weights ', N'', N'20 KG', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-05-15' AS Date), 12, CAST(N'2026-05-15' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T15:25:59.700' AS DateTime), NULL, NULL)
INSERT [#legacy_qa_calib_reg_eq] ([id], [ctrl_no], [eq_nm], [serial_no], [brand_model], [location], [section_cd], [eq_pic_nik], [eq_pic_nm], [last_calib_dt], [calib_interval], [next_calib_dt], [calib_type], [eq_status], [remarks], [ent_by], [ent_dt], [upd_by], [upd_dt]) VALUES (18, N'M14', N'Standard Weights ', N'', N'20 kg', N'Calibration room', N'550', N'212325', N'TRIS TIWANTO', CAST(N'2025-05-15' AS Date), 12, CAST(N'2026-05-15' AS Date), N'E', N'A', N'', N'212325', CAST(N'2026-02-21T15:28:13.960' AS DateTime), NULL, NULL)

-- Plus 2,000+ more inserts (cropped for easier maintenance)

DECLARE @LegacyEquipmentCount INT;
DECLARE @InsertedEquipmentCount INT;
DECLARE @FallbackEmployeeId INT;
DECLARE @FallbackEmployeeCode NVARCHAR(6);
DECLARE @FallbackEmployeeName NVARCHAR(200);
DECLARE @FallbackSectionId INT;

SELECT @LegacyEquipmentCount = COUNT(*)
FROM #legacy_qa_calib_reg_eq;

SELECT
    @FallbackEmployeeId = employee_id,
    @FallbackEmployeeCode = employee_code,
    @FallbackEmployeeName = full_name
FROM Shared.dbo.employees
WHERE employee_code = '220021';

IF @FallbackEmployeeId IS NULL
BEGIN
    THROW 50001, 'Fallback employee 220021 was not found in Shared.dbo.employees.', 1;
END;

SELECT @FallbackSectionId = section_id
FROM dbo.sections
WHERE section_code = '550';

IF @FallbackSectionId IS NULL
BEGIN
    THROW 50002, 'Fallback section 550 was not found in dbo.sections.', 1;
END;

IF EXISTS (
    SELECT 1
    FROM #legacy_qa_calib_reg_eq
    GROUP BY LTRIM(RTRIM(ctrl_no))
    HAVING COUNT(*) > 1
)
BEGIN
    THROW 50003, 'Duplicate control numbers found in the legacy equipment export.', 1;
END;

;WITH legacy_resolved AS (
    SELECT
        src.id,
        equipment_name = LTRIM(RTRIM(src.eq_nm)),
        control_no = LTRIM(RTRIM(src.ctrl_no)),
        serial_no = NULLIF(NULLIF(LTRIM(RTRIM(src.serial_no)), ''), '-'),
        brand = CASE
            WHEN NULLIF(LTRIM(RTRIM(src.brand_model)), '') IS NULL THEN NULL
            WHEN LTRIM(RTRIM(src.brand_model)) = '-' THEN NULL
            WHEN CHARINDEX('/', src.brand_model) > 0 THEN NULLIF(LTRIM(RTRIM(LEFT(src.brand_model, CHARINDEX('/', src.brand_model) - 1))), '')
            ELSE NULLIF(LTRIM(RTRIM(src.brand_model)), '')
        END,
        model = CASE
            WHEN NULLIF(LTRIM(RTRIM(src.brand_model)), '') IS NULL THEN NULL
            WHEN LTRIM(RTRIM(src.brand_model)) = '-' THEN NULL
            WHEN CHARINDEX('/', src.brand_model) > 0 THEN NULLIF(LTRIM(RTRIM(SUBSTRING(src.brand_model, CHARINDEX('/', src.brand_model) + 1, LEN(src.brand_model)))), '')
            ELSE NULL
        END,
        location = COALESCE(NULLIF(LTRIM(RTRIM(src.location)), ''), 'Legacy location not provided'),
        section_id = COALESCE(sec.section_id, @FallbackSectionId),
        pic_id = COALESCE(pic.employee_id, creator.employee_id, @FallbackEmployeeId),
        pic_code = COALESCE(NULLIF(LTRIM(RTRIM(src.eq_pic_nik)), ''), pic.employee_code, creator.employee_code, @FallbackEmployeeCode),
        pic_full_name = COALESCE(NULLIF(LTRIM(RTRIM(src.eq_pic_nm)), ''), pic.full_name, creator.full_name, @FallbackEmployeeName),
        calib_interval_months = CASE WHEN src.calib_interval > 0 THEN src.calib_interval ELSE 12 END,
        last_calib_date = COALESCE(src.last_calib_dt, CAST(GETDATE() AS DATE)),
        calib_type = CASE WHEN src.calib_type IN ('I', 'E') THEN src.calib_type ELSE 'I' END,
        equipment_status = CASE WHEN src.eq_status IN ('A', 'O', 'S') THEN src.eq_status ELSE 'A' END,
        remarks = NULLIF(LTRIM(RTRIM(src.remarks)), ''),
        created_at = COALESCE(CAST(src.ent_dt AS DATETIME2), GETDATE()),
        updated_at = CAST(src.upd_dt AS DATETIME2),
        created_by = LEFT(COALESCE(NULLIF(LTRIM(RTRIM(src.ent_by)), ''), @FallbackEmployeeCode), 6),
        updated_by = LEFT(NULLIF(LTRIM(RTRIM(src.upd_by)), ''), 6)
    FROM #legacy_qa_calib_reg_eq src
    LEFT JOIN dbo.sections sec
        ON sec.section_code = LTRIM(RTRIM(src.section_cd))
    LEFT JOIN Shared.dbo.employees pic
        ON pic.employee_code = LEFT(LTRIM(RTRIM(src.eq_pic_nik)), 6)
    LEFT JOIN Shared.dbo.employees creator
        ON creator.employee_code = LEFT(LTRIM(RTRIM(src.ent_by)), 6)
)
INSERT INTO dbo.qa_calib_equipments (
    equipment_name,
    control_no,
    serial_no,
    brand,
    model,
    location,
    section_id,
    pic_id,
    pic_code,
    pic_full_name,
    calib_interval_months,
    last_calib_date,
    calib_type,
    equipment_status,
    remarks,
    created_at,
    updated_at,
    created_by,
    updated_by
)
SELECT
    equipment_name,
    control_no,
    serial_no,
    brand,
    model,
    location,
    section_id,
    pic_id,
    pic_code,
    pic_full_name,
    calib_interval_months,
    last_calib_date,
    calib_type,
    equipment_status,
    remarks,
    created_at,
    updated_at,
    created_by,
    updated_by
FROM legacy_resolved
ORDER BY id;

SET @InsertedEquipmentCount = @@ROWCOUNT;

IF @InsertedEquipmentCount <> @LegacyEquipmentCount
BEGIN
    THROW 50004, 'Legacy equipment migration count mismatch.', 1;
END;

PRINT CONCAT('Legacy equipment migration completed: ', @InsertedEquipmentCount, ' rows inserted into dbo.qa_calib_equipments.');

DROP TABLE #legacy_qa_calib_reg_eq;
GO

PRINT 'Database seeding completed successfully.';

