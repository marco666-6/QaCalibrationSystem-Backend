-- ============================================================
-- Calibreq Complete Database Script
-- Includes: Shared DB + Calibreq DB (Tables, Indexes,
--           Stored Procedures, Seed Data)
-- ============================================================

-- ============================================================
-- [1] SHARED DATABASE
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'Shared')
BEGIN
    CREATE DATABASE Shared;
END;
GO
USE [Shared];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================
-- Shared: departments
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'departments' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[departments](
        [department_id]   INT IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
        [department_code] NVARCHAR(50)  NOT NULL,
        [department_name] NVARCHAR(200) NOT NULL,
        [is_active]       BIT          NOT NULL DEFAULT 1,
        [created_at]      DATETIME2    NOT NULL DEFAULT GETDATE(),
        [updated_at]      DATETIME2    NULL,
        PRIMARY KEY CLUSTERED ([department_id] ASC)
    );
    ALTER TABLE [dbo].[departments] ADD UNIQUE NONCLUSTERED ([department_code] ASC);
END;
GO

-- ============================
-- Shared: employees
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'employees' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[employees](
        [employee_id]       INT IDENTITY(1,1) NOT NULL,
        [employee_code]     NVARCHAR(6)   NOT NULL,
        [full_name]         NVARCHAR(200) NOT NULL,
        [email]             NVARCHAR(200) NULL,
        [date_of_birth]     DATE          NULL,
        [gender]            NVARCHAR(20)  NULL,
        [section_cd]        CHAR(3)       NOT NULL,
        [position_cd]       CHAR(3)       NOT NULL,
        [manager_id]        INT           NULL,
        [profile_photo_url] NVARCHAR(500) NULL,
        [is_active]         BIT           NOT NULL DEFAULT 1,
        [created_at]        DATETIME2     NOT NULL DEFAULT GETDATE(),
        [updated_at]        DATETIME2     NULL,
        CONSTRAINT [PK_employees] PRIMARY KEY CLUSTERED ([employee_id] ASC),
        CONSTRAINT [UQ_employees_code]  UNIQUE ([employee_code]),
        CONSTRAINT [UQ_employees_email] UNIQUE ([email]),
        CONSTRAINT [FK_employees_manager] FOREIGN KEY ([manager_id]) REFERENCES [dbo].[employees]([employee_id])
    );
END;
GO

-- ============================
-- Shared: test / View_emp_mst
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'test' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[test](
        [nik]        CHAR(6)  NOT NULL,
        [Name]       CHAR(50) NOT NULL,
        [dateofbirth] DATETIME NOT NULL,
        [sex]        CHAR(1)  NULL,
        [section]    CHAR(3)  NOT NULL,
        [position_c] CHAR(3)  NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.views WHERE name = 'View_emp_mst' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    EXEC('CREATE VIEW dbo.View_emp_mst AS
          SELECT nik, Name, dateofbirth, sex, section, position_c FROM dbo.test;');
END;
GO

-- ============================
-- Shared: Seed departments
-- ============================
SET IDENTITY_INSERT [dbo].[departments] ON;
GO
MERGE [dbo].[departments] AS target
USING (VALUES
    (1,  N'200', N'INFORMATION SYSTEMS',    1),
    (2,  N'110', N'GENERAL AFFAIR',         1),
    (3,  N'120', N'HUMAN RESOURCES',        1),
    (4,  N'310', N'MATERIAL CONTROL',       1),
    (5,  N'350', N'PRODUCTION CONTROL',     1),
    (6,  N'330', N'LOGISTIC CONTROL',       1),
    (7,  N'410', N'CUTTING & CRIMPING',     1),
    (8,  N'420', N'QUALITY CONTROL',        1),
    (9,  N'450', N'ASSEMBLY',               1),
    (10, N'510', N'PRODUCTION ENGINEERING', 1),
    (11, N'910', N'ELECTRICAL APPLIANCES W/H', 1),
    (12, N'430', N'PROCESS ENGINEERING',    1),
    (13, N'610', N'FINANCE',                1),
    (14, N'620', N'ACCOUNTING',             1),
    (15, N'550', N'QUALITY ASSURANCE',      1),
    (16, N'520', N'MAINTENANCE',            1),
    (17, N'630', N'PURCHASING',             1),
    (18, N'930', N'DESIGN',                 1),
    (19, N'130', N'TRAINING CENTER',        1),
    (20, N'600', N'FINANCE & ACCOUNTING',   1),
    (21, N'700', N'SAFETY & MTA BUILDING',  1),
    (22, N'800', N'PURCHASING',             1),
    (23, N'460', N'TRAINING ASSY & CC',     1),
    (24, N'000', N'JAPANESE',               1),
    (25, N'140', N'SUBSIDIARY',             1),
    (26, N'530', N'DOCUMENT CONTROL',       1)
) AS source (department_id, department_code, department_name, is_active)
ON target.department_id = source.department_id
WHEN NOT MATCHED THEN
    INSERT (department_id, department_code, department_name, is_active, created_at)
    VALUES (source.department_id, source.department_code, source.department_name, source.is_active, GETDATE());
GO
SET IDENTITY_INSERT [dbo].[departments] OFF;
GO

-- ============================
-- Shared: Seed employees
-- ============================
SET IDENTITY_INSERT [dbo].[employees] ON;
GO
MERGE [dbo].[employees] AS target
USING (VALUES
    (1,  N'220021', N'AHMADUN',                  N'sattuo-ahmadun@sws.com',       '1995-05-06', N'Male',   N'200', N'103', NULL),
    (20, N'222299', N'PUSPA KARTIKANING WIKONO',  N'puspa-kartikaning@sws.com',    '1998-03-25', N'Female', N'200', N'123', NULL),
    (21, N'223549', N'ADINDA SELFIANI',            N'adindaselfiani@sbi.sws.co.jp', '2001-11-27', N'Female', N'200', N'123', NULL),
    (22, N'213553', N'RISMA SARI DEWI',            N'risma-saridewi@sws.com',       '1993-05-07', N'Female', N'350', N'103', NULL),
    (23, N'240127', N'MIFTAHUL APRILIANA',         N'miftahul-apriliana@sws.com',   '1999-04-15', N'Female', N'530', N'130', NULL),
    (24, N'223725', N'YORA KURNIA ILAHI NF',       N'yora-kurnia@sws.com',          '2000-08-09', N'Female', N'530', N'132', NULL),
    (25, N'260016', N'YULIA PIPKA ZILIWU',         N'yulia-pipka@sws.com',          '2005-09-15', N'Female', N'200', N'130', NULL)
) AS source (employee_id, employee_code, full_name, email, date_of_birth, gender, section_cd, position_cd, manager_id)
ON target.employee_id = source.employee_id
WHEN NOT MATCHED THEN
    INSERT (employee_id, employee_code, full_name, email, date_of_birth, gender, section_cd, position_cd, manager_id, is_active, created_at)
    VALUES (source.employee_id, source.employee_code, source.full_name, source.email, source.date_of_birth, source.gender, source.section_cd, source.position_cd, source.manager_id, 1, GETDATE());
GO
SET IDENTITY_INSERT [dbo].[employees] OFF;
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'195007', N'SAMYONO                                           ', CAST(N'1974-10-14T00:00:00.000' AS DateTime), N'M', N'520', N'101')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'195072', N'MUHAMAD SYAFARUDIN                                ', CAST(N'1974-03-12T00:00:00.000' AS DateTime), N'M', N'450', N'110')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196032', N'AGUS SUGIHARTO                                    ', CAST(N'1974-10-14T00:00:00.000' AS DateTime), N'M', N'450', N'130')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196034', N'EDY YUANA                                         ', CAST(N'1974-08-25T00:00:00.000' AS DateTime), N'M', N'410', N'140')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196037', N'IMAM TRISNO YUWONO                                ', CAST(N'1974-07-24T00:00:00.000' AS DateTime), N'M', N'450', N'100')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196040', N'M U R S I T O                                     ', CAST(N'1976-05-18T00:00:00.000' AS DateTime), N'M', N'410', N'100')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196041', N'P O N I M A N                                     ', CAST(N'1974-05-09T00:00:00.000' AS DateTime), N'M', N'450', N'130')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196052', N'DWI ERNAWATI                                      ', CAST(N'1974-03-23T00:00:00.000' AS DateTime), N'F', N'450', N'150')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196082', N'S U J A T N O                                     ', CAST(N'1973-06-23T00:00:00.000' AS DateTime), N'M', N'520', N'050')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196104', N'SYAMSUL ANWAR                                     ', CAST(N'1973-05-26T00:00:00.000' AS DateTime), N'M', N'420', N'150')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196118', N'RIAMA HOTMAIDA                                    ', CAST(N'1976-03-20T00:00:00.000' AS DateTime), N'F', N'450', N'150')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196124', N'TUTIK ROHMAWATI                                   ', CAST(N'1975-06-27T00:00:00.000' AS DateTime), N'F', N'450', N'150')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196181', N'ANDRI NOVA                                        ', CAST(N'1972-11-13T00:00:00.000' AS DateTime), N'M', N'310', N'150')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196186', N'JENNI TOVRIN LUBIS                                ', CAST(N'1974-10-10T00:00:00.000' AS DateTime), N'M', N'130', N'090')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196203', N'ROSINTAN FRANSISCA S                              ', CAST(N'1972-02-06T00:00:00.000' AS DateTime), N'F', N'450', N'150')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196209', N'ISWANDI                                           ', CAST(N'1975-08-20T00:00:00.000' AS DateTime), N'M', N'410', N'140')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196219', N'LASPITA TINDAON                                   ', CAST(N'1978-10-09T00:00:00.000' AS DateTime), N'F', N'410', N'150')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196226', N'ASMUIN                                            ', CAST(N'1978-02-10T00:00:00.000' AS DateTime), N'M', N'450', N'060')
GO
INSERT [dbo].[test] ([nik], [Name], [dateofbirth], [sex], [section], [position_c]) VALUES (N'196229', N'D A N D E L                                       ', CAST(N'1973-12-17T00:00:00.000' AS DateTime), N'M', N'350', N'100')
-- AND 2000+ more view_emp_mst data

-- ============================================================
-- [2] CALIBREQ DATABASE
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'Calibreq')
BEGIN
    CREATE DATABASE Calibreq;
END;
GO
USE [Calibreq];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================================
-- MASTER TABLES
-- ============================================================

-- ============================
-- users
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'users')
CREATE TABLE dbo.users (
    user_id                     INT IDENTITY PRIMARY KEY,
    employee_id                 INT NULL,           -- ref: Shared.dbo.employees.employee_id (no FK: cross-db)
    username                    NVARCHAR(100) NOT NULL UNIQUE,
    password_hash               NVARCHAR(500) NOT NULL,
    email                       NVARCHAR(200) NOT NULL,
    role                        NVARCHAR(20)  NOT NULL DEFAULT 'User',  -- 'Admin' or 'User'
    is_active                   BIT NOT NULL DEFAULT 1,
    failed_login_attempts       INT NOT NULL DEFAULT 0,
    must_change_password        BIT NOT NULL DEFAULT 1,
    last_login                  DATETIME2 NULL,
    lockout_until               DATETIME2 NULL,
    refresh_token               NVARCHAR(MAX) NULL,
    refresh_token_expires_at    DATETIME2 NULL,
    created_at                  DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at                  DATETIME2 NULL,

    CONSTRAINT CK_users_role CHECK (role IN ('Admin', 'User'))
);
GO

-- ============================
-- password_reset_tokens
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'password_reset_tokens')
BEGIN
    CREATE TABLE dbo.password_reset_tokens (
        id          BIGINT IDENTITY PRIMARY KEY,
        user_id     INT NOT NULL,
        token       NVARCHAR(200) NOT NULL UNIQUE,
        expires_at  DATETIME2 NOT NULL,
        created_at  DATETIME2 NOT NULL DEFAULT GETDATE(),
        consumed_at DATETIME2 NULL,
        CONSTRAINT FK_prt_users FOREIGN KEY (user_id) REFERENCES dbo.users(user_id) ON DELETE CASCADE
    );
    CREATE INDEX IX_prt_user_status ON dbo.password_reset_tokens(user_id, consumed_at, expires_at);
END;
GO

-- ============================
-- default_locations
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'default_locations')
CREATE TABLE dbo.default_locations (
    default_location_id     INT IDENTITY PRIMARY KEY,
    default_location_name   NVARCHAR(200) NOT NULL,
    is_active               BIT NOT NULL DEFAULT 1,
    created_at              DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at              DATETIME2 NULL,
    created_by              NVARCHAR(6) NULL,
    updated_by              NVARCHAR(6) NULL
);
GO

-- ============================
-- section_emails  (Admin only)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'section_emails')
CREATE TABLE dbo.section_emails (
    section_email_id    INT IDENTITY PRIMARY KEY,
    section_id          INT NULL,               -- ref: Shared.dbo.departments.department_id (no FK: cross-db)
    section_code        NVARCHAR(50)  NOT NULL,
    section_name        NVARCHAR(200) NOT NULL,
    email               NVARCHAR(200) NOT NULL,
    is_active           BIT NOT NULL DEFAULT 1,
    created_at          DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2 NULL,
    created_by          NVARCHAR(6) NULL,
    updated_by          NVARCHAR(6) NULL
);
GO

-- ============================
-- section_pic_emails  (Admin only)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'section_pic_emails')
CREATE TABLE dbo.section_pic_emails (
    section_pic_email_id    INT IDENTITY PRIMARY KEY,
    section_id              INT NULL,
    section_code            NVARCHAR(50)  NOT NULL,
    section_name            NVARCHAR(200) NOT NULL,
    pic_name                NVARCHAR(200) NOT NULL,
    email                   NVARCHAR(200) NOT NULL,
    is_active               BIT NOT NULL DEFAULT 1,
    created_at              DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at              DATETIME2 NULL,
    created_by              NVARCHAR(6) NULL,
    updated_by              NVARCHAR(6) NULL
);
GO

-- ============================
-- roles  (calib-roles; Admin only)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'roles')
CREATE TABLE dbo.roles (
    id          INT IDENTITY PRIMARY KEY,
    user_id     INT NOT NULL,
    role        NVARCHAR(10) NOT NULL,  -- 'Preparer','Checker','Approver','Technician'
    is_active   BIT NOT NULL DEFAULT 1,
    created_at  DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at  DATETIME2 NULL,
    created_by  NVARCHAR(6) NULL,
    updated_by  NVARCHAR(6) NULL,

    CONSTRAINT CK_roles_role CHECK (role IN ('Preparer', 'Checker', 'Approver', 'Technician')),
    CONSTRAINT FK_roles_users FOREIGN KEY (user_id) REFERENCES dbo.users(user_id) ON DELETE CASCADE
);
GO

-- ============================
-- externals  (Admin only)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'externals')
CREATE TABLE dbo.externals (
    external_id         INT IDENTITY PRIMARY KEY,
    external_company    NVARCHAR(200) NOT NULL,
    external_email      NVARCHAR(200) NULL,
    external_phone      NVARCHAR(50)  NULL,
    address             NVARCHAR(500) NULL,
    is_active           BIT NOT NULL DEFAULT 1,
    created_at          DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2 NULL,
    created_by          NVARCHAR(6) NULL,
    updated_by          NVARCHAR(6) NULL
);
GO

-- ============================
-- equipments
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'equipments')
CREATE TABLE dbo.equipments (
    id                      INT IDENTITY PRIMARY KEY,
    equipment_name          NVARCHAR(200) NOT NULL,
    control_no              NVARCHAR(100) NOT NULL UNIQUE,
    serial_no               NVARCHAR(100) NULL,
    brand                   NVARCHAR(100) NULL,
    model                   NVARCHAR(100) NULL,
    range                   NVARCHAR(100) NULL,
    location                NVARCHAR(200) NULL,
    section_id              INT NULL,
    section_code            NVARCHAR(50)  NOT NULL,
    section_name            NVARCHAR(200) NOT NULL,
    calib_interval_months   INT NOT NULL DEFAULT 12,
    last_calib_date         DATE NULL,
    last_calib_month        AS MONTH(last_calib_date) PERSISTED,
    last_calib_year         AS YEAR(last_calib_date)  PERSISTED,
    next_calib_date         AS DATEADD(MONTH, calib_interval_months, last_calib_date) PERSISTED,
    next_calib_month        AS MONTH(DATEADD(MONTH, calib_interval_months, last_calib_date)) PERSISTED,
    next_calib_year         AS YEAR(DATEADD(MONTH,  calib_interval_months, last_calib_date)) PERSISTED,
    calib_type              NVARCHAR(8)   NOT NULL DEFAULT 'Internal',
    equipment_status        NVARCHAR(14)  NOT NULL DEFAULT 'Active',
    remarks                 NVARCHAR(MAX) NULL,
    is_scrapped             BIT NOT NULL DEFAULT 0,  -- soft-delete / scrap bin flag
    scrapped_at             DATETIME2 NULL,
    scrapped_by             NVARCHAR(6)   NULL,
    created_at              DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at              DATETIME2 NULL,
    created_by              NVARCHAR(6) NULL,
    updated_by              NVARCHAR(6) NULL,

    CONSTRAINT CK_equipments_calib_type     CHECK (calib_type      IN ('Internal', 'External')),
    CONSTRAINT CK_equipments_status         CHECK (equipment_status IN ('Active', 'Out of Service', 'Scrap'))
);
CREATE INDEX IX_equipments_next_calib ON dbo.equipments(next_calib_year, next_calib_month) WHERE is_scrapped = 0;
CREATE INDEX IX_equipments_status     ON dbo.equipments(equipment_status)                  WHERE is_scrapped = 0;
GO

-- ============================================================
-- CALIBRATION PLAN TABLES
-- ============================================================

-- ============================
-- calib_plans
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'calib_plans')
CREATE TABLE dbo.calib_plans (
    plan_id             INT IDENTITY PRIMARY KEY,
    plan_title          NVARCHAR(300) NOT NULL,
    plan_month          INT  NOT NULL,   -- 1-12
    plan_year           INT  NOT NULL,
    calib_type          NVARCHAR(8)  NOT NULL DEFAULT 'Internal',  -- plan-level default; overridable per item
    status              NVARCHAR(20) NOT NULL DEFAULT 'Draft',
    -- 'Draft','Submitted','Preparer Approved','Checker Approved','Fully Approved','Locked'
    is_locked           BIT NOT NULL DEFAULT 0,
    locked_at           DATETIME2 NULL,
    report_pdf_path     NVARCHAR(500) NULL,   -- permanent PDF generated on lock
    -- approval role assignments (user_ids)
    preparer_user_id    INT NULL,
    checker_user_id     INT NULL,
    approver_user_id    INT NULL,
    -- approval timestamps & remarks
    preparer_approved_at    DATETIME2 NULL,
    preparer_remark         NVARCHAR(MAX) NULL,
    checker_approved_at     DATETIME2 NULL,
    checker_remark          NVARCHAR(MAX) NULL,
    approver_approved_at    DATETIME2 NULL,
    approver_remark         NVARCHAR(MAX) NULL,
    -- cancellation
    preparer_cancelled_at   DATETIME2 NULL,
    checker_cancelled_at    DATETIME2 NULL,
    approver_cancelled_at   DATETIME2 NULL,
    created_at          DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2 NULL,
    created_by          NVARCHAR(6) NULL,
    updated_by          NVARCHAR(6) NULL,

    CONSTRAINT CK_calib_plans_status    CHECK (status IN ('Draft','Submitted','Preparer Approved','Checker Approved','Fully Approved','Locked')),
    CONSTRAINT CK_calib_plans_calib_type CHECK (calib_type IN ('Internal','External')),
    CONSTRAINT CK_calib_plans_month     CHECK (plan_month BETWEEN 1 AND 12),
    CONSTRAINT FK_calib_plans_preparer  FOREIGN KEY (preparer_user_id) REFERENCES dbo.users(user_id),
    CONSTRAINT FK_calib_plans_checker   FOREIGN KEY (checker_user_id)  REFERENCES dbo.users(user_id),
    CONSTRAINT FK_calib_plans_approver  FOREIGN KEY (approver_user_id) REFERENCES dbo.users(user_id)
);
CREATE UNIQUE INDEX UX_calib_plans_period ON dbo.calib_plans(plan_month, plan_year) WHERE status NOT IN ('Draft');
CREATE INDEX IX_calib_plans_status ON dbo.calib_plans(status, plan_year, plan_month);
GO

-- ============================
-- calib_plan_items  (one row per equipment in a plan)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'calib_plan_items')
CREATE TABLE dbo.calib_plan_items (
    plan_item_id        INT IDENTITY PRIMARY KEY,
    plan_id             INT NOT NULL,
    equipment_id        INT NOT NULL,
    -- denormalised snapshot at plan creation time
    equipment_name      NVARCHAR(200) NOT NULL,
    control_no          NVARCHAR(100) NOT NULL,
    serial_no           NVARCHAR(100) NULL,
    brand               NVARCHAR(100) NULL,
    model               NVARCHAR(100) NULL,
    range               NVARCHAR(100) NULL,
    location            NVARCHAR(200) NULL,
    section_code        NVARCHAR(50)  NOT NULL,
    section_name        NVARCHAR(200) NOT NULL,
    calib_interval_months INT NOT NULL,
    last_calib_date     DATE NULL,
    next_calib_date     DATE NULL,
    -- per-item overrides
    calib_type          NVARCHAR(8)  NOT NULL,  -- 'Internal' or 'External'
    is_included         BIT NOT NULL DEFAULT 1,  -- user can uncheck to exclude from plan
    remarks             NVARCHAR(MAX) NULL,
    created_at          DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2 NULL,

    CONSTRAINT FK_cpi_plan      FOREIGN KEY (plan_id)      REFERENCES dbo.calib_plans(plan_id) ON DELETE CASCADE,
    CONSTRAINT FK_cpi_equipment FOREIGN KEY (equipment_id) REFERENCES dbo.equipments(id),
    CONSTRAINT UX_cpi_plan_equip UNIQUE (plan_id, equipment_id)
);
GO

-- ============================
-- calib_plan_technicians  (internal plans; 2-5 technicians)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'calib_plan_technicians')
CREATE TABLE dbo.calib_plan_technicians (
    id              INT IDENTITY PRIMARY KEY,
    plan_id         INT NOT NULL,
    user_id         INT NOT NULL,
    is_pic          BIT NOT NULL DEFAULT 0,  -- only one per plan should be 1
    created_at      DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_cpt_plan FOREIGN KEY (plan_id)  REFERENCES dbo.calib_plans(plan_id) ON DELETE CASCADE,
    CONSTRAINT FK_cpt_user FOREIGN KEY (user_id)  REFERENCES dbo.users(user_id),
    CONSTRAINT UX_cpt_plan_user UNIQUE (plan_id, user_id)
);
GO

-- ============================
-- calib_plan_externals  (external plans; 1-5 companies)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'calib_plan_externals')
CREATE TABLE dbo.calib_plan_externals (
    id              INT IDENTITY PRIMARY KEY,
    plan_id         INT NOT NULL,
    external_id     INT NOT NULL,
    -- denorm snapshot
    external_company NVARCHAR(200) NOT NULL,
    created_at      DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_cpe_plan     FOREIGN KEY (plan_id)     REFERENCES dbo.calib_plans(plan_id) ON DELETE CASCADE,
    CONSTRAINT FK_cpe_external FOREIGN KEY (external_id) REFERENCES dbo.externals(external_id),
    CONSTRAINT UX_cpe_plan_ext UNIQUE (plan_id, external_id)
);
GO

-- ============================================================
-- CALIBRATION ACTUAL TABLES
-- ============================================================

-- ============================
-- calib_actuals
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'calib_actuals')
CREATE TABLE dbo.calib_actuals (
    actual_id           INT IDENTITY PRIMARY KEY,
    plan_id             INT NOT NULL UNIQUE,  -- 1-to-1 with plan
    plan_month          INT NOT NULL,
    plan_year           INT NOT NULL,
    calib_type          NVARCHAR(8)  NOT NULL,
    status              NVARCHAR(20) NOT NULL DEFAULT 'In Progress',
    -- 'In Progress','Preparer Approved','Checker Approved','Fully Approved','Closed'
    is_closed           BIT NOT NULL DEFAULT 0,
    closed_at           DATETIME2 NULL,
    closed_by           NVARCHAR(6) NULL,
    close_reason        NVARCHAR(20) NULL,  -- 'Manual' or 'Auto'
    report_pdf_path     NVARCHAR(500) NULL,   -- permanent PDF; NULL until closed+approved
    report_has_watermark BIT NOT NULL DEFAULT 0,  -- TRUE if auto-closed before approval
    -- approval role assignments (inherited from plan; stored for independence)
    preparer_user_id    INT NULL,
    checker_user_id     INT NULL,
    approver_user_id    INT NULL,
    preparer_approved_at    DATETIME2 NULL,
    preparer_remark         NVARCHAR(MAX) NULL,
    checker_approved_at     DATETIME2 NULL,
    checker_remark          NVARCHAR(MAX) NULL,
    approver_approved_at    DATETIME2 NULL,
    approver_remark         NVARCHAR(MAX) NULL,
    preparer_cancelled_at   DATETIME2 NULL,
    checker_cancelled_at    DATETIME2 NULL,
    approver_cancelled_at   DATETIME2 NULL,
    created_at          DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2 NULL,
    created_by          NVARCHAR(6) NULL,
    updated_by          NVARCHAR(6) NULL,

    CONSTRAINT CK_calib_actuals_status     CHECK (status IN ('In Progress','Preparer Approved','Checker Approved','Fully Approved','Closed')),
    CONSTRAINT CK_calib_actuals_calib_type CHECK (calib_type IN ('Internal','External')),
    CONSTRAINT CK_calib_actuals_month      CHECK (plan_month BETWEEN 1 AND 12),
    CONSTRAINT CK_calib_actuals_close_reason CHECK (close_reason IN ('Manual','Auto')),
    CONSTRAINT FK_ca_plan      FOREIGN KEY (plan_id)          REFERENCES dbo.calib_plans(plan_id),
    CONSTRAINT FK_ca_preparer  FOREIGN KEY (preparer_user_id) REFERENCES dbo.users(user_id),
    CONSTRAINT FK_ca_checker   FOREIGN KEY (checker_user_id)  REFERENCES dbo.users(user_id),
    CONSTRAINT FK_ca_approver  FOREIGN KEY (approver_user_id) REFERENCES dbo.users(user_id)
);
CREATE INDEX IX_calib_actuals_status ON dbo.calib_actuals(status, plan_year, plan_month);
GO

-- ============================
-- calib_actual_items  (one row per equipment in an actual)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'calib_actual_items')
CREATE TABLE dbo.calib_actual_items (
    actual_item_id          INT IDENTITY PRIMARY KEY,
    actual_id               INT NOT NULL,
    plan_item_id            INT NOT NULL,
    equipment_id            INT NOT NULL,
    -- denorm snapshot from plan item
    equipment_name          NVARCHAR(200) NOT NULL,
    control_no              NVARCHAR(100) NOT NULL,
    serial_no               NVARCHAR(100) NULL,
    brand                   NVARCHAR(100) NULL,
    model                   NVARCHAR(100) NULL,
    range                   NVARCHAR(100) NULL,
    location                NVARCHAR(200) NULL,
    section_code            NVARCHAR(50)  NOT NULL,
    section_name            NVARCHAR(200) NOT NULL,
    calib_type              NVARCHAR(8)   NOT NULL,
    -- standard calibration (per equipment_name group)
    standard_calibration    NVARCHAR(MAX) NULL,
    -- result recording
    calib_result            NVARCHAR(2)   NULL,   -- 'OK' or 'NG' or NULL (not yet recorded)
    ng_action               NVARCHAR(20)  NULL,   -- 'Repair','Replacement','None' (only when NG)
    calib_date              DATE NULL,            -- date calibration was actually performed
    remarks                 NVARCHAR(MAX) NULL,
    created_at              DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at              DATETIME2 NULL,
    recorded_by             NVARCHAR(6) NULL,
    recorded_at             DATETIME2 NULL,

    CONSTRAINT CK_cai_result    CHECK (calib_result IN ('OK','NG') OR calib_result IS NULL),
    CONSTRAINT CK_cai_ng_action CHECK (ng_action    IN ('Repair','Replacement','None') OR ng_action IS NULL),
    CONSTRAINT FK_cai_actual    FOREIGN KEY (actual_id)    REFERENCES dbo.calib_actuals(actual_id) ON DELETE CASCADE,
    CONSTRAINT FK_cai_plan_item FOREIGN KEY (plan_item_id) REFERENCES dbo.calib_plan_items(plan_item_id),
    CONSTRAINT FK_cai_equipment FOREIGN KEY (equipment_id) REFERENCES dbo.equipments(id),
    CONSTRAINT UX_cai_actual_equip UNIQUE (actual_id, equipment_id)
);
GO

-- ============================
-- calib_actual_technicians  (internal actuals)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'calib_actual_technicians')
CREATE TABLE dbo.calib_actual_technicians (
    id          INT IDENTITY PRIMARY KEY,
    actual_id   INT NOT NULL,
    user_id     INT NOT NULL,
    is_pic      BIT NOT NULL DEFAULT 0,
    created_at  DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_cat_actual FOREIGN KEY (actual_id) REFERENCES dbo.calib_actuals(actual_id) ON DELETE CASCADE,
    CONSTRAINT FK_cat_user   FOREIGN KEY (user_id)   REFERENCES dbo.users(user_id),
    CONSTRAINT UX_cat_actual_user UNIQUE (actual_id, user_id)
);
GO

-- ============================
-- calib_actual_externals  (external actuals)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'calib_actual_externals')
CREATE TABLE dbo.calib_actual_externals (
    id              INT IDENTITY PRIMARY KEY,
    actual_id       INT NOT NULL,
    external_id     INT NOT NULL,
    external_company NVARCHAR(200) NOT NULL,
    created_at      DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_cae_actual   FOREIGN KEY (actual_id)   REFERENCES dbo.calib_actuals(actual_id) ON DELETE CASCADE,
    CONSTRAINT FK_cae_external FOREIGN KEY (external_id) REFERENCES dbo.externals(external_id),
    CONSTRAINT UX_cae_actual_ext UNIQUE (actual_id, external_id)
);
GO

-- ============================================================
-- FOLLOW-UP / MAINTENANCE TABLES
-- ============================================================

-- ============================
-- out_of_service_records  (NG equipment tracking)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'out_of_service_records')
CREATE TABLE dbo.out_of_service_records (
    oos_id              INT IDENTITY PRIMARY KEY,
    equipment_id        INT NOT NULL,
    actual_item_id      INT NULL,   -- link to the NG result that triggered this
    ng_action           NVARCHAR(20) NOT NULL,  -- 'Repair','Replacement','None'
    -- tracking
    assigned_to         NVARCHAR(6)   NULL,     -- employee_code responsible
    expected_return_date DATE NULL,
    repair_details      NVARCHAR(MAX) NULL,
    resolution_note     NVARCHAR(MAX) NULL,
    resolved_at         DATETIME2 NULL,
    resolved_by         NVARCHAR(6)   NULL,
    -- final state
    is_resolved         BIT NOT NULL DEFAULT 0,
    resolved_status     NVARCHAR(14) NULL,  -- equipment_status restored to: 'Active'
    created_at          DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2 NULL,
    created_by          NVARCHAR(6) NULL,
    updated_by          NVARCHAR(6) NULL,

    CONSTRAINT FK_oos_equipment   FOREIGN KEY (equipment_id)   REFERENCES dbo.equipments(id),
    CONSTRAINT FK_oos_actual_item FOREIGN KEY (actual_item_id) REFERENCES dbo.calib_actual_items(actual_item_id),
    CONSTRAINT CK_oos_ng_action   CHECK (ng_action IN ('Repair','Replacement','None'))
);
CREATE INDEX IX_oos_equipment   ON dbo.out_of_service_records(equipment_id, is_resolved);
GO

-- ============================
-- scrap_records  (audit trail when equipment is scrapped/restored)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'scrap_records')
CREATE TABLE dbo.scrap_records (
    scrap_record_id     INT IDENTITY PRIMARY KEY,
    equipment_id        INT NOT NULL,
    action              NVARCHAR(10) NOT NULL,  -- 'Scrap','Restore','Delete'
    reason              NVARCHAR(MAX) NULL,
    actioned_at         DATETIME2 NOT NULL DEFAULT GETDATE(),
    actioned_by         NVARCHAR(6) NULL,

    CONSTRAINT FK_sr_equipment FOREIGN KEY (equipment_id) REFERENCES dbo.equipments(id),
    CONSTRAINT CK_sr_action    CHECK (action IN ('Scrap','Restore','Delete'))
);
GO

-- ============================
-- audit_logs  (generic audit trail for sensitive operations)
-- ============================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'audit_logs')
CREATE TABLE dbo.audit_logs (
    log_id          BIGINT IDENTITY PRIMARY KEY,
    table_name      NVARCHAR(100) NOT NULL,
    record_id       NVARCHAR(50)  NOT NULL,  -- PK of affected row (as string)
    action          NVARCHAR(10)  NOT NULL,  -- 'INSERT','UPDATE','DELETE'
    old_values      NVARCHAR(MAX) NULL,      -- JSON snapshot before
    new_values      NVARCHAR(MAX) NULL,      -- JSON snapshot after
    performed_by    NVARCHAR(6)   NULL,
    performed_at    DATETIME2     NOT NULL DEFAULT GETDATE(),
    ip_address      NVARCHAR(45)  NULL
);
CREATE INDEX IX_audit_table_record ON dbo.audit_logs(table_name, record_id);
GO


-- ============================================================
-- STORED PROCEDURES
-- ============================================================

-- ============================================================
-- MASTER DATA: DEFAULT LOCATIONS
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_upsert_default_location
    @default_location_id    INT = NULL,
    @default_location_name  NVARCHAR(200),
    @is_active              BIT = 1,
    @by                     NVARCHAR(6) = NULL,
    @new_id                 INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @default_location_id IS NULL OR @default_location_id = 0
    BEGIN
        INSERT INTO dbo.default_locations (default_location_name, is_active, created_by)
        VALUES (@default_location_name, @is_active, @by);
        SET @new_id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.default_locations
        SET default_location_name = @default_location_name,
            is_active = @is_active,
            updated_at = GETDATE(),
            updated_by = @by
        WHERE default_location_id = @default_location_id;
        SET @new_id = @default_location_id;
    END
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_delete_default_location
    @default_location_id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.default_locations WHERE default_location_id = @default_location_id;
END;
GO

-- ============================================================
-- MASTER DATA: EQUIPMENTS
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_upsert_equipment
    @id                     INT = NULL,
    @equipment_name         NVARCHAR(200),
    @control_no             NVARCHAR(100),
    @serial_no              NVARCHAR(100) = NULL,
    @brand                  NVARCHAR(100) = NULL,
    @model                  NVARCHAR(100) = NULL,
    @range                  NVARCHAR(100) = NULL,
    @location               NVARCHAR(200) = NULL,
    @section_id             INT           = NULL,
    @section_code           NVARCHAR(50),
    @section_name           NVARCHAR(200),
    @calib_interval_months  INT,
    @last_calib_date        DATE          = NULL,
    @calib_type             NVARCHAR(8)   = 'Internal',
    @equipment_status       NVARCHAR(14)  = 'Active',
    @remarks                NVARCHAR(MAX) = NULL,
    @by                     NVARCHAR(6)   = NULL,
    @new_id                 INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @id IS NULL OR @id = 0
    BEGIN
        INSERT INTO dbo.equipments (equipment_name, control_no, serial_no, brand, model, range,
            location, section_id, section_code, section_name, calib_interval_months,
            last_calib_date, calib_type, equipment_status, remarks, created_by)
        VALUES (@equipment_name, @control_no, @serial_no, @brand, @model, @range,
            @location, @section_id, @section_code, @section_name, @calib_interval_months,
            @last_calib_date, @calib_type, @equipment_status, @remarks, @by);
        SET @new_id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.equipments
        SET equipment_name = @equipment_name, control_no = @control_no, serial_no = @serial_no,
            brand = @brand, model = @model, range = @range, location = @location,
            section_id = @section_id, section_code = @section_code, section_name = @section_name,
            calib_interval_months = @calib_interval_months, last_calib_date = @last_calib_date,
            calib_type = @calib_type, equipment_status = @equipment_status, remarks = @remarks,
            updated_at = GETDATE(), updated_by = @by
        WHERE id = @id AND is_scrapped = 0;
        SET @new_id = @id;
    END
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_scrap_equipment
    @id         INT,
    @reason     NVARCHAR(MAX) = NULL,
    @by         NVARCHAR(6)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.equipments
    SET equipment_status = 'Scrap', is_scrapped = 1, scrapped_at = GETDATE(), scrapped_by = @by, updated_at = GETDATE()
    WHERE id = @id AND is_scrapped = 0;

    INSERT INTO dbo.scrap_records (equipment_id, action, reason, actioned_by)
    VALUES (@id, 'Scrap', @reason, @by);
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_restore_equipment
    @id     INT,
    @reason NVARCHAR(MAX) = NULL,
    @by     NVARCHAR(6)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.equipments
    SET equipment_status = 'Active', is_scrapped = 0, scrapped_at = NULL, scrapped_by = NULL, updated_at = GETDATE()
    WHERE id = @id AND is_scrapped = 1;

    INSERT INTO dbo.scrap_records (equipment_id, action, reason, actioned_by)
    VALUES (@id, 'Restore', @reason, @by);
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_hard_delete_equipment
    @id     INT,
    @by     NVARCHAR(6) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- Only allow hard-delete of scrapped equipment
    IF NOT EXISTS (SELECT 1 FROM dbo.equipments WHERE id = @id AND is_scrapped = 1)
    BEGIN
        RAISERROR('Equipment must be in Scrap bin before permanent deletion.', 16, 1);
        RETURN;
    END;
    INSERT INTO dbo.scrap_records (equipment_id, action, actioned_by)
    VALUES (@id, 'Delete', @by);
    DELETE FROM dbo.equipments WHERE id = @id;
END;
GO

-- Bulk operations helper
CREATE OR ALTER PROCEDURE dbo.sp_bulk_update_equipments
    @ids            NVARCHAR(MAX),  -- comma-separated equipment ids
    @action         NVARCHAR(30),
    -- 'status'|'section'|'location'|'remarks'|'scrap'|'delete'
    @status_value   NVARCHAR(14)  = NULL,
    @section_id     INT           = NULL,
    @section_code   NVARCHAR(50)  = NULL,
    @section_name   NVARCHAR(200) = NULL,
    @location_value NVARCHAR(200) = NULL,
    @remarks_value  NVARCHAR(MAX) = NULL,
    @scrap_reason   NVARCHAR(MAX) = NULL,
    @by             NVARCHAR(6)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- Parse ids into temp table
    SELECT CAST(value AS INT) AS id
    INTO #ids
    FROM STRING_SPLIT(@ids, ',')
    WHERE LTRIM(RTRIM(value)) <> '';

    IF @action = 'status'
        UPDATE e SET e.equipment_status = @status_value, e.updated_at = GETDATE(), e.updated_by = @by
        FROM dbo.equipments e JOIN #ids t ON e.id = t.id WHERE e.is_scrapped = 0;
    ELSE IF @action = 'section'
        UPDATE e SET e.section_id = @section_id, e.section_code = @section_code, e.section_name = @section_name,
            e.updated_at = GETDATE(), e.updated_by = @by
        FROM dbo.equipments e JOIN #ids t ON e.id = t.id WHERE e.is_scrapped = 0;
    ELSE IF @action = 'location'
        UPDATE e SET e.location = @location_value, e.updated_at = GETDATE(), e.updated_by = @by
        FROM dbo.equipments e JOIN #ids t ON e.id = t.id WHERE e.is_scrapped = 0;
    ELSE IF @action = 'remarks'
        UPDATE e SET e.remarks = @remarks_value, e.updated_at = GETDATE(), e.updated_by = @by
        FROM dbo.equipments e JOIN #ids t ON e.id = t.id WHERE e.is_scrapped = 0;
    ELSE IF @action = 'scrap'
    BEGIN
        UPDATE e SET e.equipment_status = 'Scrap', e.is_scrapped = 1,
            e.scrapped_at = GETDATE(), e.scrapped_by = @by, e.updated_at = GETDATE()
        FROM dbo.equipments e JOIN #ids t ON e.id = t.id WHERE e.is_scrapped = 0;
        INSERT INTO dbo.scrap_records (equipment_id, action, reason, actioned_by)
        SELECT t.id, 'Scrap', @scrap_reason, @by FROM #ids t;
    END

    DROP TABLE #ids;
END;
GO

-- ============================================================
-- CALIBRATION PLAN STORED PROCEDURES
-- ============================================================

-- sp_create_calib_plan
CREATE OR ALTER PROCEDURE dbo.sp_create_calib_plan
    @plan_title         NVARCHAR(300),
    @plan_month         INT,
    @plan_year          INT,
    @calib_type         NVARCHAR(8)  = 'Internal',
    @preparer_user_id   INT,
    @checker_user_id    INT,
    @approver_user_id   INT,
    @by                 NVARCHAR(6)  = NULL,
    @plan_id            INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.calib_plans (
        plan_title, plan_month, plan_year, calib_type, status,
        preparer_user_id, checker_user_id, approver_user_id,
        created_by
    )
    VALUES (
        @plan_title, @plan_month, @plan_year, @calib_type, 'Draft',
        @preparer_user_id, @checker_user_id, @approver_user_id,
        @by
    );
    SET @plan_id = SCOPE_IDENTITY();
END;
GO

-- sp_get_due_equipments_for_plan  (returns equipments due for a given month-year)
CREATE OR ALTER PROCEDURE dbo.sp_get_due_equipments_for_plan
    @plan_month INT,
    @plan_year  INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Due = next_calib_year < @plan_year
    --    OR (next_calib_year = @plan_year AND next_calib_month <= @plan_month)
    --    OR last_calib_date IS NULL (never calibrated)
    SELECT
        id, equipment_name, control_no, serial_no, brand, model, range,
        location, section_id, section_code, section_name,
        calib_interval_months, last_calib_date, next_calib_date,
        calib_type, equipment_status, remarks
    FROM dbo.equipments
    WHERE is_scrapped = 0
      AND equipment_status = 'Active'
      AND (
          last_calib_date IS NULL
          OR next_calib_year < @plan_year
          OR (next_calib_year = @plan_year AND next_calib_month <= @plan_month)
      )
    ORDER BY next_calib_date ASC, equipment_name ASC;
END;
GO

-- sp_add_plan_items  (batch-add equipment rows to a draft plan)
CREATE OR ALTER PROCEDURE dbo.sp_add_plan_items
    @plan_id        INT,
    @equipment_ids  NVARCHAR(MAX),  -- comma-separated
    @calib_type     NVARCHAR(8) = NULL  -- NULL = use equipment's own calib_type
AS
BEGIN
    SET NOCOUNT ON;
    -- Verify plan is still Draft
    IF NOT EXISTS (SELECT 1 FROM dbo.calib_plans WHERE plan_id = @plan_id AND status = 'Draft')
    BEGIN
        RAISERROR('Plan must be in Draft status to add items.', 16, 1); RETURN;
    END;

    SELECT CAST(value AS INT) AS id INTO #eids FROM STRING_SPLIT(@equipment_ids, ',') WHERE LTRIM(RTRIM(value)) <> '';

    INSERT INTO dbo.calib_plan_items (
        plan_id, equipment_id, equipment_name, control_no, serial_no, brand, model, range,
        location, section_code, section_name, calib_interval_months, last_calib_date, next_calib_date, calib_type
    )
    SELECT
        @plan_id, e.id, e.equipment_name, e.control_no, e.serial_no, e.brand, e.model, e.range,
        e.location, e.section_code, e.section_name, e.calib_interval_months,
        e.last_calib_date, e.next_calib_date,
        ISNULL(@calib_type, e.calib_type)
    FROM dbo.equipments e
    JOIN #eids t ON e.id = t.id
    WHERE NOT EXISTS (SELECT 1 FROM dbo.calib_plan_items WHERE plan_id = @plan_id AND equipment_id = e.id);

    DROP TABLE #eids;
END;
GO

-- sp_submit_calib_plan  (Draft -> Submitted)
CREATE OR ALTER PROCEDURE dbo.sp_submit_calib_plan
    @plan_id INT,
    @by      NVARCHAR(6) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.calib_plans
    SET status = 'Submitted', updated_at = GETDATE(), updated_by = @by
    WHERE plan_id = @plan_id AND status = 'Draft';
    IF @@ROWCOUNT = 0 RAISERROR('Plan must be in Draft status to submit.', 16, 1);
END;
GO

-- sp_approve_calib_plan  (sequential: Preparer -> Checker -> Approver)
CREATE OR ALTER PROCEDURE dbo.sp_approve_calib_plan
    @plan_id    INT,
    @user_id    INT,
    @remark     NVARCHAR(MAX) = NULL,
    @by         NVARCHAR(6)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @current_status NVARCHAR(20);
    DECLARE @preparer_id INT, @checker_id INT, @approver_id INT;

    SELECT @current_status = status,
           @preparer_id = preparer_user_id,
           @checker_id  = checker_user_id,
           @approver_id = approver_user_id
    FROM dbo.calib_plans WHERE plan_id = @plan_id;

    IF @current_status = 'Submitted' AND @user_id = @preparer_id
        UPDATE dbo.calib_plans SET status = 'Preparer Approved',
            preparer_approved_at = GETDATE(), preparer_remark = @remark,
            updated_at = GETDATE(), updated_by = @by
        WHERE plan_id = @plan_id;

    ELSE IF @current_status = 'Preparer Approved' AND @user_id = @checker_id
        UPDATE dbo.calib_plans SET status = 'Checker Approved',
            checker_approved_at = GETDATE(), checker_remark = @remark,
            updated_at = GETDATE(), updated_by = @by
        WHERE plan_id = @plan_id;

    ELSE IF @current_status = 'Checker Approved' AND @user_id = @approver_id
        UPDATE dbo.calib_plans SET status = 'Fully Approved',
            approver_approved_at = GETDATE(), approver_remark = @remark,
            updated_at = GETDATE(), updated_by = @by
        WHERE plan_id = @plan_id;

    ELSE
        RAISERROR('Approval step not valid for this user or current plan status.', 16, 1);
END;
GO

-- sp_cancel_plan_approval
CREATE OR ALTER PROCEDURE dbo.sp_cancel_plan_approval
    @plan_id    INT,
    @user_id    INT,
    @remark     NVARCHAR(MAX) = NULL,
    @by         NVARCHAR(6)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @current_status NVARCHAR(20);
    DECLARE @approver_approved_at DATETIME2;
    DECLARE @preparer_id INT, @checker_id INT, @approver_id INT;

    SELECT @current_status = status,
           @approver_approved_at = approver_approved_at,
           @preparer_id = preparer_user_id,
           @checker_id  = checker_user_id,
           @approver_id = approver_user_id
    FROM dbo.calib_plans WHERE plan_id = @plan_id;

    -- Approver can cancel within 1 day
    IF @current_status = 'Fully Approved' AND @user_id = @approver_id
    BEGIN
        IF DATEDIFF(HOUR, @approver_approved_at, GETDATE()) <= 24
            UPDATE dbo.calib_plans SET status = 'Checker Approved',
                approver_approved_at = NULL, approver_cancelled_at = GETDATE(), approver_remark = @remark,
                updated_at = GETDATE(), updated_by = @by
            WHERE plan_id = @plan_id;
        ELSE
            RAISERROR('Approver 1-day cancellation window has expired. Plan is now permanently locked.', 16, 1);
    END
    ELSE IF @current_status = 'Checker Approved' AND @user_id = @checker_id
        UPDATE dbo.calib_plans SET status = 'Preparer Approved',
            checker_approved_at = NULL, checker_cancelled_at = GETDATE(), checker_remark = @remark,
            updated_at = GETDATE(), updated_by = @by
        WHERE plan_id = @plan_id;
    ELSE IF @current_status = 'Preparer Approved' AND @user_id = @preparer_id
        UPDATE dbo.calib_plans SET status = 'Submitted',
            preparer_approved_at = NULL, preparer_cancelled_at = GETDATE(), preparer_remark = @remark,
            updated_at = GETDATE(), updated_by = @by
        WHERE plan_id = @plan_id;
    ELSE
        RAISERROR('Cancellation not valid for this user or current plan status.', 16, 1);
END;
GO

-- sp_lock_calib_plan  (Fully Approved -> Locked; also auto-creates the Actual)
CREATE OR ALTER PROCEDURE dbo.sp_lock_calib_plan
    @plan_id        INT,
    @report_pdf_path NVARCHAR(500) = NULL,
    @by             NVARCHAR(6)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Validate
        IF NOT EXISTS (SELECT 1 FROM dbo.calib_plans WHERE plan_id = @plan_id AND status = 'Fully Approved')
        BEGIN
            RAISERROR('Plan must be Fully Approved to lock.', 16, 1);
        END;

        -- Lock plan
        UPDATE dbo.calib_plans
        SET status = 'Locked', is_locked = 1, locked_at = GETDATE(),
            report_pdf_path = @report_pdf_path, updated_at = GETDATE(), updated_by = @by
        WHERE plan_id = @plan_id;

        -- Create calib_actual if not already exists
        IF NOT EXISTS (SELECT 1 FROM dbo.calib_actuals WHERE plan_id = @plan_id)
        BEGIN
            DECLARE @plan_month INT, @plan_year INT, @calib_type NVARCHAR(8);
            DECLARE @preparer_id INT, @checker_id INT, @approver_id INT;

            SELECT @plan_month = plan_month, @plan_year = plan_year, @calib_type = calib_type,
                   @preparer_id = preparer_user_id, @checker_id = checker_user_id, @approver_id = approver_user_id
            FROM dbo.calib_plans WHERE plan_id = @plan_id;

            DECLARE @actual_id INT;
            INSERT INTO dbo.calib_actuals (
                plan_id, plan_month, plan_year, calib_type,
                preparer_user_id, checker_user_id, approver_user_id, created_by
            )
            VALUES (
                @plan_id, @plan_month, @plan_year, @calib_type,
                @preparer_id, @checker_id, @approver_id, @by
            );
            SET @actual_id = SCOPE_IDENTITY();

            -- Copy plan items -> actual items
            INSERT INTO dbo.calib_actual_items (
                actual_id, plan_item_id, equipment_id, equipment_name, control_no, serial_no,
                brand, model, range, location, section_code, section_name, calib_type
            )
            SELECT
                @actual_id, pi.plan_item_id, pi.equipment_id, pi.equipment_name, pi.control_no,
                pi.serial_no, pi.brand, pi.model, pi.range, pi.location,
                pi.section_code, pi.section_name, pi.calib_type
            FROM dbo.calib_plan_items pi
            WHERE pi.plan_id = @plan_id AND pi.is_included = 1;

            -- Copy technicians
            INSERT INTO dbo.calib_actual_technicians (actual_id, user_id, is_pic)
            SELECT @actual_id, user_id, is_pic FROM dbo.calib_plan_technicians WHERE plan_id = @plan_id;

            -- Copy externals
            INSERT INTO dbo.calib_actual_externals (actual_id, external_id, external_company)
            SELECT @actual_id, external_id, external_company FROM dbo.calib_plan_externals WHERE plan_id = @plan_id;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

-- sp_auto_lock_expired_plans  (call via SQL Agent job daily)
CREATE OR ALTER PROCEDURE dbo.sp_auto_lock_expired_plans
AS
BEGIN
    SET NOCOUNT ON;
    -- Lock plans where approver approved > 24 hours ago and not yet locked
    DECLARE @plan_ids TABLE (plan_id INT);

    INSERT INTO @plan_ids
    SELECT plan_id FROM dbo.calib_plans
    WHERE status = 'Fully Approved'
      AND is_locked = 0
      AND DATEDIFF(HOUR, approver_approved_at, GETDATE()) > 24;

    DECLARE @pid INT;
    DECLARE cur CURSOR FOR SELECT plan_id FROM @plan_ids;
    OPEN cur;
    FETCH NEXT FROM cur INTO @pid;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.sp_lock_calib_plan @plan_id = @pid, @by = 'SYSTEM';
        FETCH NEXT FROM cur INTO @pid;
    END;
    CLOSE cur; DEALLOCATE cur;
END;
GO

-- ============================================================
-- CALIBRATION ACTUAL STORED PROCEDURES
-- ============================================================

-- sp_record_calib_result
CREATE OR ALTER PROCEDURE dbo.sp_record_calib_result
    @actual_item_id         INT,
    @calib_result           NVARCHAR(2),    -- 'OK' or 'NG' or NULL (clear)
    @ng_action              NVARCHAR(20)   = NULL,
    @calib_date             DATE           = NULL,
    @standard_calibration   NVARCHAR(MAX)  = NULL,
    @remarks                NVARCHAR(MAX)  = NULL,
    @by                     NVARCHAR(6)    = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @equipment_id INT, @actual_id INT;
        DECLARE @actual_closed BIT, @calib_interval INT;

        SELECT @equipment_id = ai.equipment_id, @actual_id = ai.actual_id
        FROM dbo.calib_actual_items ai WHERE ai.actual_item_id = @actual_item_id;

        SELECT @actual_closed = is_closed FROM dbo.calib_actuals WHERE actual_id = @actual_id;
        IF @actual_closed = 1 RAISERROR('Cannot modify a closed actual.', 16, 1);

        SELECT @calib_interval = calib_interval_months FROM dbo.equipments WHERE id = @equipment_id;

        -- Update actual item
        UPDATE dbo.calib_actual_items
        SET calib_result = @calib_result,
            ng_action = CASE WHEN @calib_result = 'NG' THEN @ng_action ELSE NULL END,
            calib_date = ISNULL(@calib_date, CAST(GETDATE() AS DATE)),
            standard_calibration = ISNULL(@standard_calibration, standard_calibration),
            remarks = @remarks,
            recorded_by = @by,
            recorded_at = GETDATE(),
            updated_at = GETDATE()
        WHERE actual_item_id = @actual_item_id;

        -- Update equipment last_calib_date and status
        IF @calib_result IS NOT NULL
        BEGIN
            UPDATE dbo.equipments
            SET last_calib_date = ISNULL(@calib_date, CAST(GETDATE() AS DATE)),
                equipment_status = CASE WHEN @calib_result = 'NG' THEN 'Out of Service' ELSE 'Active' END,
                updated_at = GETDATE(), updated_by = @by
            WHERE id = @equipment_id;

            -- If NG, create OOS record
            IF @calib_result = 'NG'
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM dbo.out_of_service_records WHERE actual_item_id = @actual_item_id AND is_resolved = 0)
                    INSERT INTO dbo.out_of_service_records (equipment_id, actual_item_id, ng_action, created_by)
                    VALUES (@equipment_id, @actual_item_id, ISNULL(@ng_action, 'None'), @by);
            END
            -- If previously NG and now OK (result change), close any open OOS
            ELSE IF @calib_result = 'OK'
            BEGIN
                UPDATE dbo.out_of_service_records
                SET is_resolved = 1, resolved_at = GETDATE(), resolved_by = @by,
                    resolution_note = 'Result changed to OK on re-record.'
                WHERE actual_item_id = @actual_item_id AND is_resolved = 0;

                UPDATE dbo.equipments SET equipment_status = 'Active', updated_at = GETDATE()
                WHERE id = @equipment_id;
            END;
        END
        ELSE -- clearing the result
        BEGIN
            -- Reset equipment dates to previous state (set to NULL if we don't have prior data)
            UPDATE dbo.equipments
            SET last_calib_date = NULL, updated_at = GETDATE(), updated_by = @by
            WHERE id = @equipment_id;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

-- sp_approve_calib_actual
CREATE OR ALTER PROCEDURE dbo.sp_approve_calib_actual
    @actual_id  INT,
    @user_id    INT,
    @remark     NVARCHAR(MAX) = NULL,
    @by         NVARCHAR(6)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @current_status NVARCHAR(20);
    DECLARE @preparer_id INT, @checker_id INT, @approver_id INT;

    SELECT @current_status = status,
           @preparer_id = preparer_user_id,
           @checker_id  = checker_user_id,
           @approver_id = approver_user_id
    FROM dbo.calib_actuals WHERE actual_id = @actual_id;

    IF @current_status = 'In Progress' AND @user_id = @preparer_id
        UPDATE dbo.calib_actuals SET status = 'Preparer Approved',
            preparer_approved_at = GETDATE(), preparer_remark = @remark,
            updated_at = GETDATE(), updated_by = @by
        WHERE actual_id = @actual_id;
    ELSE IF @current_status = 'Preparer Approved' AND @user_id = @checker_id
        UPDATE dbo.calib_actuals SET status = 'Checker Approved',
            checker_approved_at = GETDATE(), checker_remark = @remark,
            updated_at = GETDATE(), updated_by = @by
        WHERE actual_id = @actual_id;
    ELSE IF @current_status = 'Checker Approved' AND @user_id = @approver_id
        UPDATE dbo.calib_actuals SET status = 'Fully Approved',
            approver_approved_at = GETDATE(), approver_remark = @remark,
            updated_at = GETDATE(), updated_by = @by
        WHERE actual_id = @actual_id;
    ELSE
        RAISERROR('Approval step not valid for this user or current actual status.', 16, 1);
END;
GO

-- sp_cancel_actual_approval
CREATE OR ALTER PROCEDURE dbo.sp_cancel_actual_approval
    @actual_id  INT,
    @user_id    INT,
    @remark     NVARCHAR(MAX) = NULL,
    @by         NVARCHAR(6)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @current_status NVARCHAR(20);
    DECLARE @approver_approved_at DATETIME2;
    DECLARE @preparer_id INT, @checker_id INT, @approver_id INT;
    DECLARE @is_closed BIT;

    SELECT @current_status = status, @is_closed = is_closed,
           @approver_approved_at = approver_approved_at,
           @preparer_id = preparer_user_id, @checker_id = checker_user_id, @approver_id = approver_user_id
    FROM dbo.calib_actuals WHERE actual_id = @actual_id;

    IF @is_closed = 1 AND @current_status NOT IN ('Fully Approved')
        RAISERROR('Cannot cancel approval on a closed actual that is not Fully Approved.', 16, 1);

    IF @current_status = 'Fully Approved' AND @user_id = @approver_id
    BEGIN
        IF DATEDIFF(HOUR, @approver_approved_at, GETDATE()) <= 24
            UPDATE dbo.calib_actuals SET status = 'Checker Approved',
                approver_approved_at = NULL, approver_cancelled_at = GETDATE(), approver_remark = @remark,
                updated_at = GETDATE(), updated_by = @by
            WHERE actual_id = @actual_id;
        ELSE
            RAISERROR('Approver 1-day cancellation window has expired.', 16, 1);
    END
    ELSE IF @current_status = 'Checker Approved' AND @user_id = @checker_id
        UPDATE dbo.calib_actuals SET status = 'Preparer Approved',
            checker_approved_at = NULL, checker_cancelled_at = GETDATE(), checker_remark = @remark,
            updated_at = GETDATE(), updated_by = @by
        WHERE actual_id = @actual_id;
    ELSE IF @current_status = 'Preparer Approved' AND @user_id = @preparer_id
        UPDATE dbo.calib_actuals SET status = 'In Progress',
            preparer_approved_at = NULL, preparer_cancelled_at = GETDATE(), preparer_remark = @remark,
            updated_at = GETDATE(), updated_by = @by
        WHERE actual_id = @actual_id;
    ELSE
        RAISERROR('Cancellation not valid for this user or current actual status.', 16, 1);
END;
GO

-- sp_close_calib_actual  (manual close)
CREATE OR ALTER PROCEDURE dbo.sp_close_calib_actual
    @actual_id          INT,
    @report_pdf_path    NVARCHAR(500) = NULL,
    @close_reason       NVARCHAR(20)  = 'Manual',  -- 'Manual' or 'Auto'
    @by                 NVARCHAR(6)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @is_approved BIT;
    SELECT @is_approved = CASE WHEN status = 'Fully Approved' THEN 1 ELSE 0 END
    FROM dbo.calib_actuals WHERE actual_id = @actual_id;

    UPDATE dbo.calib_actuals
    SET is_closed = 1,
        status = 'Closed',
        closed_at = GETDATE(),
        closed_by = @by,
        close_reason = @close_reason,
        report_pdf_path = @report_pdf_path,
        report_has_watermark = CASE WHEN @is_approved = 0 THEN 1 ELSE 0 END,
        updated_at = GETDATE(), updated_by = @by
    WHERE actual_id = @actual_id AND is_closed = 0;

    IF @@ROWCOUNT = 0 RAISERROR('Actual is already closed or does not exist.', 16, 1);
END;
GO

-- sp_auto_close_actuals  (call via SQL Agent job at month-end)
CREATE OR ALTER PROCEDURE dbo.sp_auto_close_actuals
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @this_month INT = MONTH(GETDATE());
    DECLARE @this_year  INT = YEAR(GETDATE());

    DECLARE @to_close TABLE (actual_id INT);
    INSERT INTO @to_close
    SELECT actual_id FROM dbo.calib_actuals
    WHERE is_closed = 0
      AND (plan_year < @this_year OR (plan_year = @this_year AND plan_month < @this_month));

    DECLARE @aid INT;
    DECLARE cur CURSOR FOR SELECT actual_id FROM @to_close;
    OPEN cur;
    FETCH NEXT FROM cur INTO @aid;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.sp_close_calib_actual @actual_id = @aid, @close_reason = 'Auto', @by = 'SYSTEM';
        FETCH NEXT FROM cur INTO @aid;
    END;
    CLOSE cur; DEALLOCATE cur;
END;
GO

-- sp_update_actual_report_after_approval  (replace PDF without watermark after post-close approval)
CREATE OR ALTER PROCEDURE dbo.sp_update_actual_report_after_approval
    @actual_id          INT,
    @report_pdf_path    NVARCHAR(500),
    @by                 NVARCHAR(6) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.calib_actuals
    SET report_pdf_path = @report_pdf_path,
        report_has_watermark = 0,
        updated_at = GETDATE(), updated_by = @by
    WHERE actual_id = @actual_id AND status = 'Fully Approved' AND is_closed = 1;
END;
GO

-- ============================================================
-- OUT OF SERVICE MANAGEMENT
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_update_oos_record
    @oos_id                 INT,
    @assigned_to            NVARCHAR(6)   = NULL,
    @expected_return_date   DATE          = NULL,
    @repair_details         NVARCHAR(MAX) = NULL,
    @resolution_note        NVARCHAR(MAX) = NULL,
    @mark_resolved          BIT           = 0,
    @by                     NVARCHAR(6)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @equipment_id INT;
    SELECT @equipment_id = equipment_id FROM dbo.out_of_service_records WHERE oos_id = @oos_id;

    UPDATE dbo.out_of_service_records
    SET assigned_to = ISNULL(@assigned_to, assigned_to),
        expected_return_date = ISNULL(@expected_return_date, expected_return_date),
        repair_details = ISNULL(@repair_details, repair_details),
        resolution_note = ISNULL(@resolution_note, resolution_note),
        is_resolved = @mark_resolved,
        resolved_at = CASE WHEN @mark_resolved = 1 THEN GETDATE() ELSE NULL END,
        resolved_by = CASE WHEN @mark_resolved = 1 THEN @by ELSE NULL END,
        resolved_status = CASE WHEN @mark_resolved = 1 THEN 'Active' ELSE NULL END,
        updated_at = GETDATE(), updated_by = @by
    WHERE oos_id = @oos_id;

    -- Restore equipment status when resolved
    IF @mark_resolved = 1
        UPDATE dbo.equipments SET equipment_status = 'Active', updated_at = GETDATE(), updated_by = @by
        WHERE id = @equipment_id AND equipment_status = 'Out of Service';
END;
GO

-- ============================================================
-- UTILITY / REPORTING VIEWS
-- ============================================================

GO
CREATE OR ALTER VIEW dbo.vw_equipments_due_summary AS
SELECT
    e.id, e.equipment_name, e.control_no, e.section_code, e.section_name,
    e.calib_interval_months, e.last_calib_date, e.next_calib_date,
    e.next_calib_month, e.next_calib_year,
    e.calib_type, e.equipment_status,
    CASE
        WHEN e.last_calib_date IS NULL THEN 'Never Calibrated'
        WHEN e.next_calib_year < YEAR(GETDATE())
            OR (e.next_calib_year = YEAR(GETDATE()) AND e.next_calib_month < MONTH(GETDATE()))
        THEN 'Overdue'
        WHEN e.next_calib_year = YEAR(GETDATE()) AND e.next_calib_month = MONTH(GETDATE())
        THEN 'Due This Month'
        ELSE 'Upcoming'
    END AS due_status
FROM dbo.equipments e
WHERE e.is_scrapped = 0;
GO

CREATE OR ALTER VIEW dbo.vw_calib_plan_summary AS
SELECT
    cp.plan_id, cp.plan_title, cp.plan_month, cp.plan_year, cp.calib_type, cp.status, cp.is_locked,
    cp.preparer_user_id, up.username AS preparer_username,
    cp.checker_user_id,  uc.username AS checker_username,
    cp.approver_user_id, ua.username AS approver_username,
    cp.preparer_approved_at, cp.checker_approved_at, cp.approver_approved_at,
    cp.locked_at, cp.report_pdf_path,
    COUNT(pi.plan_item_id) AS total_items,
    SUM(CASE WHEN pi.is_included = 1 THEN 1 ELSE 0 END) AS included_items,
    cp.created_at, cp.created_by
FROM dbo.calib_plans cp
LEFT JOIN dbo.calib_plan_items pi ON cp.plan_id = pi.plan_id
LEFT JOIN dbo.users up ON cp.preparer_user_id = up.user_id
LEFT JOIN dbo.users uc ON cp.checker_user_id  = uc.user_id
LEFT JOIN dbo.users ua ON cp.approver_user_id = ua.user_id
GROUP BY cp.plan_id, cp.plan_title, cp.plan_month, cp.plan_year, cp.calib_type, cp.status, cp.is_locked,
    cp.preparer_user_id, up.username, cp.checker_user_id, uc.username, cp.approver_user_id, ua.username,
    cp.preparer_approved_at, cp.checker_approved_at, cp.approver_approved_at,
    cp.locked_at, cp.report_pdf_path, cp.created_at, cp.created_by;
GO

CREATE OR ALTER VIEW dbo.vw_calib_actual_summary AS
SELECT
    ca.actual_id, ca.plan_id, ca.plan_month, ca.plan_year, ca.calib_type, ca.status,
    ca.is_closed, ca.closed_at, ca.close_reason, ca.report_has_watermark, ca.report_pdf_path,
    ca.preparer_user_id, up.username AS preparer_username,
    ca.checker_user_id,  uc.username AS checker_username,
    ca.approver_user_id, ua.username AS approver_username,
    ca.preparer_approved_at, ca.checker_approved_at, ca.approver_approved_at,
    COUNT(ai.actual_item_id)  AS total_items,
    SUM(CASE WHEN ai.calib_result IS NOT NULL THEN 1 ELSE 0 END) AS recorded_items,
    SUM(CASE WHEN ai.calib_result = 'OK' THEN 1 ELSE 0 END) AS ok_count,
    SUM(CASE WHEN ai.calib_result = 'NG' THEN 1 ELSE 0 END) AS ng_count,
    ca.created_at, ca.created_by
FROM dbo.calib_actuals ca
LEFT JOIN dbo.calib_actual_items ai ON ca.actual_id = ai.actual_id
LEFT JOIN dbo.users up ON ca.preparer_user_id = up.user_id
LEFT JOIN dbo.users uc ON ca.checker_user_id  = uc.user_id
LEFT JOIN dbo.users ua ON ca.approver_user_id = ua.user_id
GROUP BY ca.actual_id, ca.plan_id, ca.plan_month, ca.plan_year, ca.calib_type, ca.status,
    ca.is_closed, ca.closed_at, ca.close_reason, ca.report_has_watermark, ca.report_pdf_path,
    ca.preparer_user_id, up.username, ca.checker_user_id, uc.username, ca.approver_user_id, ua.username,
    ca.preparer_approved_at, ca.checker_approved_at, ca.approver_approved_at, ca.created_at, ca.created_by;
GO


-- ============================================================
-- SEED DATA
-- ============================================================

-- ============================
-- Seed: users
-- ============================
MERGE dbo.users AS target
USING (VALUES
    (1,  N'220021', N'$2a$11$examplehashedpassword', N'sattuo-ahmadun@sws.com',      N'Admin'),
    (20, N'222299', N'$2a$11$examplehashedpassword', N'puspa-kartikaning@sws.com',   N'User'),
    (21, N'223549', N'$2a$11$examplehashedpassword', N'adindaselfiani@sbi.sws.co.jp',N'User'),
    (22, N'213553', N'$2a$11$examplehashedpassword', N'risma-saridewi@sws.com',      N'User'),
    (23, N'240127', N'$2a$11$examplehashedpassword', N'miftahul-apriliana@sws.com',  N'User'),
    (24, N'223725', N'$2a$11$examplehashedpassword', N'yora-kurnia@sws.com',         N'User'),
    (25, N'260016', N'$2a$11$examplehashedpassword', N'yulia-pipka@sws.com',         N'User')
) AS source (employee_id, username, password_hash, email, role)
ON target.username = source.username
WHEN NOT MATCHED THEN
    INSERT (employee_id, username, password_hash, email, role, must_change_password)
    VALUES (source.employee_id, source.username, source.password_hash, source.email, source.role, 1);
GO

-- ============================
-- Seed: roles (calib-roles)
-- ============================
MERGE dbo.roles AS target
USING (
    SELECT u.user_id, r.role
    FROM dbo.users u
    JOIN (VALUES
        (N'220021', N'Approver'),   (N'220021', N'Checker'),
        (N'220021', N'Preparer'),   (N'220021', N'Technician'),
        (N'222299', N'Checker'),    (N'222299', N'Technician'),
        (N'223549', N'Preparer'),   (N'223549', N'Technician'),
        (N'260016', N'Approver')
    ) r(username, role) ON u.username = r.username
) AS source (user_id, role)
ON target.user_id = source.user_id AND target.role = source.role
WHEN NOT MATCHED THEN
    INSERT (user_id, role, created_by) VALUES (source.user_id, source.role, N'220021');
GO

-- ============================
-- Seed: externals
-- ============================
MERGE dbo.externals AS target
USING (VALUES
    (N'PT Global Calibration Services', N'contact@globalcalibration.co.id',  N'+62-778-123456', N'Batamindo Industrial Park, Batam'),
    (N'PT Mitra Kalibrasi Nasional',    N'admin@mitrakalibrasi.co.id',        N'+62-21-5557788', N'Jakarta Industrial Estate, Jakarta'),
    (N'PT Precision Instruments',       N'service@precision-inst.co.id',      N'+62-31-888999',  N'Surabaya Industrial Area, Surabaya'),
    (N'SGS Indonesia',                  N'indonesia.lab@sgs.com',             N'+62-21-29780600',N'Cilandak Commercial Estate, Jakarta'),
    (N'Sucofindo',                      N'calibration@sucofindo.co.id',       N'+62-21-5265526', N'Jl. Raya Pasar Minggu, Jakarta')
) AS source (external_company, external_email, external_phone, address)
ON target.external_company = source.external_company
WHEN NOT MATCHED THEN
    INSERT (external_company, external_email, external_phone, address, created_by)
    VALUES (source.external_company, source.external_email, source.external_phone, source.address, N'220021');
GO

-- ============================
-- Seed: default_locations
-- ============================
MERGE dbo.default_locations AS target
USING (VALUES
    (N'QA Lab Room A'),
    (N'QA Lab Room B'),
    (N'Production Floor 1'),
    (N'Production Floor 2'),
    (N'Cutting & Crimping Area'),
    (N'Assembly Line 1'),
    (N'Assembly Line 2'),
    (N'Maintenance Workshop'),
    (N'Warehouse'),
    (N'Document Control Room')
) AS source (default_location_name)
ON target.default_location_name = source.default_location_name
WHEN NOT MATCHED THEN
    INSERT (default_location_name, created_by) VALUES (source.default_location_name, N'220021');
GO

-- ============================
-- Seed: section_emails
-- ============================
MERGE dbo.section_emails AS target
USING (VALUES
    (N'550', N'QUALITY ASSURANCE',      N'qa-section@sws.com'),
    (N'420', N'QUALITY CONTROL',        N'qc-section@sws.com'),
    (N'410', N'CUTTING & CRIMPING',     N'cc-section@sws.com'),
    (N'520', N'MAINTENANCE',            N'maintenance-section@sws.com'),
    (N'450', N'ASSEMBLY',               N'assembly-section@sws.com')
) AS source (section_code, section_name, email)
ON target.section_code = source.section_code AND target.email = source.email
WHEN NOT MATCHED THEN
    INSERT (section_code, section_name, email, created_by)
    VALUES (source.section_code, source.section_name, source.email, N'220021');
GO

-- ============================
-- Seed: section_pic_emails
-- ============================
MERGE dbo.section_pic_emails AS target
USING (VALUES
    (N'550', N'QUALITY ASSURANCE', N'AHMADUN',               N'sattuo-ahmadun@sws.com'),
    (N'420', N'QUALITY CONTROL',   N'RISMA SARI DEWI',        N'risma-saridewi@sws.com'),
    (N'520', N'MAINTENANCE',       N'PUSPA KARTIKANING WIKONO', N'puspa-kartikaning@sws.com')
) AS source (section_code, section_name, pic_name, email)
ON target.section_code = source.section_code AND target.email = source.email
WHEN NOT MATCHED THEN
    INSERT (section_code, section_name, pic_name, email, created_by)
    VALUES (source.section_code, source.section_name, source.pic_name, source.email, N'220021');
GO

-- ============================
-- Seed: equipments (15 sample records)
-- ============================
MERGE dbo.equipments AS target
USING (VALUES
    (N'Digital Caliper',        N'QA-CAL-001', N'SN-00001', N'Mitutoyo', N'CD-6"CSX',  N'0-150mm / 0.01mm',  N'QA Lab Room A', 8, N'420', N'QUALITY CONTROL',    12, '2024-05-01', N'Internal'),
    (N'Digital Caliper',        N'QA-CAL-002', N'SN-00002', N'Mitutoyo', N'CD-6"CSX',  N'0-150mm / 0.01mm',  N'QA Lab Room A', 8, N'420', N'QUALITY CONTROL',    12, '2024-06-01', N'Internal'),
    (N'Digital Caliper',        N'CC-CAL-001', N'SN-00003', N'Mitutoyo', N'CD-6"CSX',  N'0-150mm / 0.01mm',  N'Cutting & Crimping Area', 7, N'410', N'CUTTING & CRIMPING', 12, '2024-07-01', N'Internal'),
    (N'Micrometer',             N'QA-MIC-001', N'SN-00010', N'Mitutoyo', N'MDC-25MX',  N'0-25mm / 0.001mm',  N'QA Lab Room B', 8, N'420', N'QUALITY CONTROL',    12, '2024-05-15', N'Internal'),
    (N'Micrometer',             N'QA-MIC-002', N'SN-00011', N'Mitutoyo', N'MDC-25MX',  N'0-25mm / 0.001mm',  N'QA Lab Room B', 8, N'420', N'QUALITY CONTROL',    12, '2024-08-01', N'Internal'),
    (N'Torque Wrench',          N'MN-TW-001',  N'SN-00020', N'Snap-on',  N'QJDP170A',  N'20-170 Nm',         N'Maintenance Workshop', 16, N'520', N'MAINTENANCE',   6,  '2024-11-01', N'External'),
    (N'Torque Wrench',          N'MN-TW-002',  N'SN-00021', N'Snap-on',  N'QJDP170A',  N'20-170 Nm',         N'Maintenance Workshop', 16, N'520', N'MAINTENANCE',   6,  '2024-12-01', N'External'),
    (N'Multimeter',             N'MN-MMT-001', N'SN-00030', N'Fluke',    N'87V',        N'AC/DC 1000V',       N'Maintenance Workshop', 16, N'520', N'MAINTENANCE',   12, '2024-04-01', N'External'),
    (N'Pressure Gauge',         N'MN-PG-001',  N'SN-00040', N'WIKA',     N'232.50',    N'0-10 bar',          N'Production Floor 1',   9,  N'450', N'ASSEMBLY',      6,  '2024-10-01', N'Internal'),
    (N'Pressure Gauge',         N'MN-PG-002',  N'SN-00041', N'WIKA',     N'232.50',    N'0-10 bar',          N'Production Floor 2',   9,  N'450', N'ASSEMBLY',      6,  '2024-11-15', N'Internal'),
    (N'Thermometer',            N'QA-TH-001',  N'SN-00050', N'Fluke',    N'52-2',      N'-200 to 1090 C',    N'QA Lab Room A',        8,  N'420', N'QUALITY CONTROL', 12, '2024-05-01', N'Internal'),
    (N'Height Gauge',           N'QA-HG-001',  N'SN-00060', N'Mitutoyo', N'192-605',   N'0-600mm / 0.01mm',  N'QA Lab Room A',        8,  N'420', N'QUALITY CONTROL', 12, '2024-06-15', N'Internal'),
    (N'Hardness Tester',        N'QA-HT-001',  N'SN-00070', N'PHASE II', N'PHT-1800',  N'HRA/HRB/HRC',       N'QA Lab Room B',        8,  N'420', N'QUALITY CONTROL', 12, '2024-07-01', N'External'),
    (N'Crimp Force Monitor',    N'CC-CFM-001', N'SN-00080', N'Schleuniger', N'CFA-02', N'0-1000 N',          N'Cutting & Crimping Area', 7, N'410', N'CUTTING & CRIMPING', 6, '2024-09-01', N'Internal'),
    (N'Pull Test Machine',      N'QC-PT-001',  N'SN-00090', N'Mecmesin', N'MultiTest', N'0-500 N',           N'Assembly Line 1',       9, N'450', N'ASSEMBLY',       12, '2024-03-01', N'External')
) AS source (
    equipment_name, control_no, serial_no, brand, model, range,
    location, section_id, section_code, section_name,
    calib_interval_months, last_calib_date, calib_type
)
ON target.control_no = source.control_no
WHEN NOT MATCHED THEN
    INSERT (equipment_name, control_no, serial_no, brand, model, range,
            location, section_id, section_code, section_name,
            calib_interval_months, last_calib_date, calib_type, equipment_status, created_by)
    VALUES (source.equipment_name, source.control_no, source.serial_no, source.brand, source.model, source.range,
            source.location, source.section_id, source.section_code, source.section_name,
            source.calib_interval_months, source.last_calib_date, source.calib_type, N'Active', N'220021');
GO

-- ============================================================
-- END OF SCRIPT
-- ============================================================
PRINT 'Calibreq database setup complete.';
GO