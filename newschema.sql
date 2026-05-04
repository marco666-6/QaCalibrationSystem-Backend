IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'QaCalibMS')
BEGIN
    CREATE DATABASE QaCalibMS;
END;

GO

USE [QaCalibMS];

GO

-- ============================
-- DROP (safe order)
-- ============================
/* IF OBJECT_ID('', 'U') IS NOT NULL DROP TABLE ;
IF OBJECT_ID('', 'U') IS NOT NULL DROP TABLE ;
IF OBJECT_ID('', 'U') IS NOT NULL DROP TABLE ;
IF OBJECT_ID('', 'U') IS NOT NULL DROP TABLE ;
IF OBJECT_ID('', 'U') IS NOT NULL DROP TABLE ;*/
-- And so on for all tables, in the correct order to avoid FK constraint issues (drop child tables before parent tables)
GO


-- ============================
-- SECTIONS
-- ============================
CREATE TABLE dbo.sections (
    section_id          INT IDENTITY PRIMARY KEY,
    section_code        NVARCHAR(6) NOT NULL UNIQUE,
    section_name        NVARCHAR(100) NOT NULL,
    is_active           BIT NOT NULL DEFAULT 1,
    created_at          DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2 NULL
);
GO


-- ============================
-- POSITIONS
-- ============================
CREATE TABLE dbo.positions (
    position_id         INT IDENTITY PRIMARY KEY,
    position_code       NVARCHAR(6) NOT NULL UNIQUE,
    position_name       NVARCHAR(100) NOT NULL,
    is_active           BIT NOT NULL DEFAULT 1,
    created_at          DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2 NULL
);
GO


-- ============================
-- DEFAULT LOCATIONS
-- ============================
CREATE TABLE dbo.default_locations (
    default_location_id         INT IDENTITY PRIMARY KEY,
    default_location_name       NVARCHAR(200) NOT NULL,
    is_active               BIT NOT NULL DEFAULT 1,
    created_at              DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at              DATETIME2 NULL
);
GO


-- ============================
-- USERS
-- ============================
CREATE TABLE dbo.users (
    user_id                         INT IDENTITY PRIMARY KEY,
    employee_id                     INT NULL,  -- ref: Shared.dbo.employees.employee_id; cross-database No-FK reference intentionally omitted
    username                        NVARCHAR(100) NOT NULL UNIQUE,
    password_hash                   NVARCHAR(500) NOT NULL,
    email                           NVARCHAR(200) NOT NULL,
    role                            NVARCHAR(50) NOT NULL,
    is_active                       BIT NOT NULL DEFAULT 1,
    failed_login_attempts           INT NOT NULL DEFAULT 0,
    must_change_password            BIT NOT NULL DEFAULT 1,
    last_login                      DATETIME2 NULL,
    lockout_until                   DATETIME2 NULL,
    refresh_token                   NVARCHAR(MAX) NULL,
    refresh_token_expires_at        DATETIME2 NULL,
    created_at                      DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at                      DATETIME2 NULL
);
GO


-- ============================
-- PASSWORD RESET TOKENS
-- ============================
CREATE TABLE dbo.password_reset_tokens (
    id              BIGINT IDENTITY PRIMARY KEY,
    user_id         INT NOT NULL,  -- ref: users.user_id; No-FK reference required to associate the token with a specific user in users table
    token           NVARCHAR(200) NOT NULL UNIQUE,
    expires_at      DATETIME2 NOT NULL,
    created_at      DATETIME2 NOT NULL DEFAULT GETDATE(),
    consumed_at     DATETIME2 NULL,

    CONSTRAINT FK_password_reset_tokens_users
        FOREIGN KEY (user_id)
        REFERENCES dbo.users(user_id)
        ON DELETE CASCADE
);

CREATE INDEX IX_password_reset_tokens_user_status
ON dbo.password_reset_tokens(user_id, consumed_at, expires_at);
GO


-- ============================
-- CALIBRATION EQUIPMENTS
-- ============================
CREATE TABLE dbo.equipments (
    id                          INT IDENTITY PRIMARY KEY,
    equipment_name              NVARCHAR(200) NOT NULL,  -- equipment's name, it will be heavily used across the system — especially in the app level for grouping identical equipment name and others
    control_no                  NVARCHAR(100) NOT NULL UNIQUE,  -- equipment's control number, unique identifier for each equipment (e.g., "DC-XX", "EQ-001", "AS/MT/002", etc)
    serial_no                   NVARCHAR(100) NULL,
    brand                       NVARCHAR(100) NULL,
    model                       NVARCHAR(100) NULL,
    location                    NVARCHAR(200) NOT NULL,  -- equipment's location, choose from existing default locations table data through autocomplete or enter a custom one (e.g., "Dekat Blabla", etc)
    section_id                  INT NOT NULL,  -- ref: sections.section_id; No-FK reference required to associate the equipment with a specific section in sections table
    pic_id                      INT NOT NULL,  -- ref: Shared.dbo.employees.employee_id; cross-database No-FK reference intentionally omitted
    pic_code                    NVARCHAR(6) NOT NULL,  -- denormalize: Shared.dbo.employees.employee_code; via pic_id > users > employees
    pic_name                    NVARCHAR(200) NOT NULL,  -- denormalize: Shared.dbo.employees.employee_full_name; via pic_id > users > employees
    calib_interval_months       INT NOT NULL,  -- equipment's calibration interval in months (i.e., 1 = monthly, 3 = every 3 months, 12 = yearly, 20 = every 20 months, etc)
    last_calib_date             DATE NULL,  -- equipment's last calibration date, required for existing manual records, if no last calibration date or for new stuff then select 'No Record' in the front end, and this field may remain NULL
    last_calib_month            AS MONTH(last_calib_date) PERSISTED,
    last_calib_year             AS YEAR(last_calib_date) PERSISTED,
    next_calib_date             AS DATEADD(MONTH, calib_interval_months, last_calib_date) PERSISTED,  -- equipment's next calibration date, computed automatically from last_calib_date + calib_interval_months
    next_calib_month            AS MONTH(DATEADD(MONTH, calib_interval_months, last_calib_date)) PERSISTED,
    next_calib_year             AS YEAR(DATEADD(MONTH, calib_interval_months, last_calib_date)) PERSISTED,
    calib_type                  CHAR(1) NOT NULL DEFAULT 'I',  -- calibration type maps to calibration type enum (i.e., 'I' = internal, 'E' = external)
    equipment_status            CHAR(1) NOT NULL DEFAULT 'A',  -- equipment's status maps to equipment status enum (i.e., 'A' = active, 'O' = out for service, 'S' = scrapped)
    remarks                     NVARCHAR(MAX) NULL,
    created_at                  DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at                  DATETIME2 NULL,
    created_by                  NVARCHAR(6) NOT NULL,  -- ref: Shared.dbo.employees.employee_id; cross-database No-FK reference intentionally omitted, via logged in user > employees
    updated_by                  NVARCHAR(6) NULL, -- ref: Shared.dbo.employees.employee_id; cross-database No-FK reference intentionally omitted, via logged in user > employees

    CONSTRAINT CK_equipments_calib_type CHECK (calib_type IN ('I', 'E')),
    CONSTRAINT CK_equipments_equipment_status CHECK (equipment_status IN ('A', 'O', 'S'))
);
GO

-- =========================
-- CALIBRATION APPROVERS
-- =========================
CREATE TABLE dbo.qa_calib_approvers (
    id                  INT IDENTITY PRIMARY KEY,
    approver_id         INT NOT NULL,  -- ref: users.user_id; No-FK reference required to associate the approver with a specific pic user in users table
    approver_code       NVARCHAR(6) NOT NULL,  -- denormalize: Shared.dbo.employees.employee_code; via approver_id > users > employees
    approver_name       NVARCHAR(200) NOT NULL,  -- denormalize: Shared.dbo.employees.employee_full_name; via approver_id > users > employees
    step_no             CHAR(1) NOT NULL DEFAULT '4',  -- approver's step_no maps to approval step enum (i.e., '1' = prepared/preparer, '2' = checked/checker, '3' = approved/approver, '4' = none)
    is_active           BIT NOT NULL DEFAULT 1,
    created_at          DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at          DATETIME2 NULL,
    created_by          NVARCHAR(6) NOT NULL,  -- ref: Shared.dbo.employees.employee_id; cross-database No-FK reference intentionally omitted, via logged in user > employees
    updated_by          NVARCHAR(6) NULL,  -- ref: Shared.dbo.employees.employee_id; cross-database No-FK reference intentionally omitted, via logged in user > employees

    CONSTRAINT UQ_approvers_employee_step UNIQUE (approver_id, step_no),
    CONSTRAINT CK_approvers_step_no CHECK (step_no IN ('1', '2', '3', '4'))
);


-- =========================
-- CALIBRATION HEADER
-- =========================
-- Calibration phase maps to calibration phase enum (i.e., 'P' = plan, 'A' = actual) and their data just separate table
CREATE TABLE dbo.qa_calib_main_headers (
    id              INT IDENTITY PRIMARY KEY,
    calib_no        NVARCHAR(100) NOT NULL UNIQUE,  -- calibration's identification number, unique identifier for each calibration record (e.g., "CALIB-2024-001", "CALIB-2024-002", etc)
    calib_type      CHAR(1) NOT NULL,  -- calibration type maps to calibration type enum (i.e., 'I' = internal, 'E' = external)
    remarks         NVARCHAR(MAX) NULL,
    created_at      DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2 NULL,
    created_by      NVARCHAR(6) NOT NULL,  -- ref: Shared.dbo.employees.employee_id; cross-database No-FK reference intentionally omitted, via logged in user > employees
    updated_by      NVARCHAR(6) NULL,  -- ref: Shared.dbo.employees.employee_id; cross-database No-FK reference intentionally omitted, via logged in user > employees

    CONSTRAINT CK_main_headers_calib_type CHECK (calib_type IN ('I', 'E'))
);


-- =========================
-- CALIBRATION PLANS
-- =========================
CREATE TABLE dbo.qa_calib_plans (
    id                  INT IDENTITY PRIMARY KEY,
    header_id           INT NOT NULL UNIQUE,  -- ref: qa_calib_main_headers.id; FK reference required to associate the calibration plan with a specific calibration header in qa_calib_main_headers table
    calib_status        CHAR(1) NOT NULL DEFAULT 'D',  -- calibration status maps to calibration status enum (i.e., 'D' = draft, 'P' = prepared, 'C' = checked, 'A' = approved, 'L' = locked)
    calib_month         INT NOT NULL,  -- calibration month, required to indicate the month of the calibration (i.e., between 1 and 12)
    calib_year          INT NOT NULL,  -- calibration year, required to indicate the year of the calibration (e.g., 2020, 2024, etc)
    locked_at           DATETIME2 NULL,  -- lock date, can be null, should be updated to the date when the plan is locked (i.e., when the plan status is updated to 'L')
    locked_by           NVARCHAR(6) NULL,  -- ref: Shared.dbo.employees.employee_id; cross-database No-FK reference intentionally omitted, via logged in user > employees
    
    CONSTRAINT FK_plans_header FOREIGN KEY (header_id) REFERENCES dbo.qa_calib_main_headers(id) ON DELETE CASCADE,
    CONSTRAINT CK_plans_calib_month CHECK (calib_month BETWEEN 1 AND 12),
    CONSTRAINT CK_plans_calib_year CHECK (calib_year BETWEEN 1900 AND 9999),
    CONSTRAINT CK_plans_calib_status CHECK (calib_status IN ('D', 'S', 'L'))
);


-- =========================
-- CALIBRATION ACTUALS
-- =========================
CREATE TABLE dbo.qa_calib_actuals (
    id                  INT IDENTITY PRIMARY KEY,
    header_id           INT NOT NULL UNIQUE,  -- ref: qa_calib_main_headers.id; FK reference required to associate the calibration actual with a specific calibration header in qa_calib_main_headers table
    calib_status        CHAR(1) NOT NULL DEFAULT 'N',  -- calibration status maps to calibration status enum ('N' = not yet (plan is not locked yet), 'W' = wait (until planned calib_month), 'G' = ongoing, 'X' = completed)
    completed_dt        DATETIME2 NULL,  -- completion date, can be null, should be updated to the date when the actual calibration is completed (i.e., when the actual calibration status is updated to 'X')
    completed_by        NVARCHAR(6) NULL,  -- ref: Shared.dbo.employees.employee_id; cross-database No-FK reference intentionally omitted, via logged in user > employees
    
    CONSTRAINT FK_actuals_header FOREIGN KEY (header_id) REFERENCES dbo.qa_calib_main_headers(id) ON DELETE CASCADE,
    CONSTRAINT CK_actuals_calib_status CHECK (calib_status IN ('N', 'W', 'G', 'X'))
);


-- =========================
-- CALIBRATION WORKERS
-- =========================
CREATE TABLE dbo.qa_calib_workers (
    id                          INT IDENTITY PRIMARY KEY,
    header_id                   INT NOT NULL,  -- ref: qa_calib_main_headers.id; FK reference required to associate the calibration worker with a specific calibration header either plan/actual in qa_calib_main_headers table
    worker_id                   INT NULL,  -- ref: users.user_id; No-FK reference required to associate the equipment with a specific worker user in users table if internal
    worker_code                 NVARCHAR(6) NULL,  -- denormalize: Shared.dbo.employees.employee_code; via worker_id > users > employees if internal
    worker_name                 NVARCHAR(200) NULL,  -- denormalize: Shared.dbo.employees.employee_full_name; via worker_id > users > employees if internal
    external_party_name         NVARCHAR(200) NULL,  -- name of the external party (not in employees table) if external, can be null if the technician is internal or if the name is unknown
    external_party_company      NVARCHAR(200) NULL,  -- name of the external party's company (not in employees table) if external, can be null if the technician is internal or if the name of company is unknown
    is_pic                      BIT NOT NULL DEFAULT 0,
    created_at                  DATETIME2 NOT NULL DEFAULT GETDATE(),
    created_by                  NVARCHAR(6) NOT NULL,  -- ref: Shared.dbo.employees.employee_id; cross-database No-FK reference intentionally omitted, via logged in user > employees
 
    CONSTRAINT FK_workers_header FOREIGN KEY (header_id) REFERENCES dbo.qa_calib_main_headers(id) ON DELETE CASCADE,
    CONSTRAINT CK_workers_identified CHECK (worker_id IS NOT NULL OR external_party_name IS NOT NULL)
);
GO


-- =========================
-- CALIBRATION APPROVALS
-- =========================
CREATE TABLE dbo.qa_calib_approvals (
    id                  INT IDENTITY PRIMARY KEY,
    header_id           INT NOT NULL,  -- ref: qa_calib_main_headers.id
    step_no             CHAR(1) NOT NULL,  -- '1', '2', '3'
    approver_id         INT NOT NULL,  -- ref: users.user_id (who is assigned this step)
    approver_code       NVARCHAR(6) NOT NULL,  -- denorm: employee_code
    approver_name       NVARCHAR(200) NOT NULL,  -- denormalize: Shared.dbo.employees.employee_full_name; via worker_id > users > employees
    action              CHAR(1) NOT NULL DEFAULT 'C',  -- calibration approval action maps to calibration approval action enum (i.e., 'C' = clear/cancel, 'S' = submit, 'R' = rejected)
    remarks             NVARCHAR(500) NULL,
    actioned_at         DATETIME2 NULL,  -- when the approval action was taken
    created_at          DATETIME2 NOT NULL DEFAULT GETDATE(),
    created_by          NVARCHAR(6) NOT NULL,
    updated_at          DATETIME2 NULL,
    updated_by          NVARCHAR(6) NULL,
 
    CONSTRAINT FK_approvals_header
        FOREIGN KEY (header_id)
        REFERENCES dbo.qa_calib_main_headers(id)
        ON DELETE CASCADE,

    CONSTRAINT CK_approvals_step_no
        CHECK (step_no IN ('1','2','3')),
 
    CONSTRAINT CK_approvals_action
        CHECK (action IN ('C','S')),

    CONSTRAINT UQ_approvals_header_step
        UNIQUE (header_id, step_no)
);


-- =========================
-- CALIBRATION PLAN ITEMS
-- =========================
CREATE TABLE dbo.qa_calib_items (
    id                INT IDENTITY PRIMARY KEY,
    header_id         INT NOT NULL,  -- ref: qa_calib_main_headers.id
    equipment_name    NVARCHAR(200) NOT NULL,  -- equipment type/group name (e.g., "Stainless Steel Ruler Assy")
    item_count        INT NOT NULL DEFAULT 0,  -- total number of equipment units under this group
    item_completed    INT NOT NULL DEFAULT 0,  -- how many units have been calibrated (for actual phase)
    std_used          NVARCHAR(200) NULL,  -- standard/reference used (internal) or vendor/lab name (external)
    remarks           NVARCHAR(MAX) NULL,
    created_at        DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at        DATETIME2 NULL,
    created_by        NVARCHAR(6) NOT NULL,
    updated_by        NVARCHAR(6) NULL,
 
    CONSTRAINT FK_items_header
        FOREIGN KEY (header_id)
        REFERENCES dbo.qa_calib_main_headers(id)
        ON DELETE CASCADE,

    CONSTRAINT CK_items_count_nonneg
        CHECK (item_count >= 0),
 
    CONSTRAINT CK_items_completed_nonneg
        CHECK (item_completed >= 0)
);


-- =========================
-- CALIBRATION ITEM DETAILS
-- =========================
CREATE TABLE dbo.qa_calib_item_details (
    id              INT IDENTITY PRIMARY KEY,
    item_id         INT NOT NULL,  -- ref: qa_calib_items.id (the group this unit belongs to)
    equipment_id    INT NOT NULL,  -- ref: equipments.id (specific equipment unit)
    calib_result    CHAR(1) NULL,  -- 'O' = OK, 'N' = NG, NULL = not yet done
    overdue_flag    BIT NOT NULL DEFAULT 0,  -- 1 = this equipment was past its next_calib_month when calibrated
    certificate_no  NVARCHAR(100) NULL,  -- external calibration certificate number (null for internal and possibly null/optional for external if not yet available)
    remarks         NVARCHAR(MAX) NULL,
    created_at      DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2 NULL,
    created_by      NVARCHAR(6) NOT NULL,
    updated_by      NVARCHAR(6) NULL,

    CONSTRAINT FK_details_item
        FOREIGN KEY (item_id)
        REFERENCES dbo.qa_calib_items(id)
        ON DELETE CASCADE,
 
    CONSTRAINT CK_details_result
        CHECK (calib_result IN ('O','N') OR calib_result IS NULL)
);
 
CREATE INDEX IX_details_item
    ON dbo.qa_calib_item_details(item_id);
GO


-- =========================
-- EQUIPMENT DETAILS SNAPSHOT (at time of calibration detail entry)
-- =========================
CREATE TABLE dbo.qa_calib_equipment_details (
    id                       INT IDENTITY PRIMARY KEY,
    detail_id                INT NOT NULL,  -- ref: qa_calib_item_details.id (1:1)
    equipment_id             INT NOT NULL,  -- original equipment id
    equipment_name           NVARCHAR(200) NOT NULL,
    control_no               NVARCHAR(100) NOT NULL,
    serial_no                NVARCHAR(100) NULL,
    brand                    NVARCHAR(200) NULL,
    model                    NVARCHAR(200) NULL,
    location                 NVARCHAR(200) NOT NULL,
    section_code             NVARCHAR(50) NOT NULL,
    section_name             NVARCHAR(200) NOT NULL,
    calib_interval_months    INT NOT NULL,
    last_calib_date          DATE NOT NULL,
    last_calib_month         INT NOT NULL,
    next_calib_date          DATE NOT NULL,  -- computed value at snapshot time
    next_calib_month         INT NOT NULL,  -- computed value at snapshot time
    pic_code                 NVARCHAR(6) NOT NULL,
    pic_full_name            NVARCHAR(200) NOT NULL,

    CONSTRAINT FK_equipmentsnapshots_detail
        FOREIGN KEY (detail_id)
        REFERENCES dbo.qa_calib_item_details(id)
        ON DELETE CASCADE,

    CONSTRAINT UQ_equipmentsnapshots_detail UNIQUE (detail_id)
);

-- UNTIL HERE HELP ME PLEASE ON WHAT TO DO
