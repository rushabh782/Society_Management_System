USE societyDB;


CREATE TABLE gate_logs (
  gate_log_id BIGINT IDENTITY(1,1) PRIMARY KEY,
  society_id BIGINT,
  visitor_name VARCHAR(120) NOT NULL,
  vehicle_no VARCHAR(20),
  purpose VARCHAR(80),
  unit_id BIGINT,
  building_id BIGINT,  
  check_in DATETIME NOT NULL,
  check_out DATETIME,
  FOREIGN KEY (society_id) REFERENCES societies(society_id),
  FOREIGN KEY (building_id ) REFERENCES buildings(building_id),
  FOREIGN KEY (unit_id) REFERENCES units(unit_id)
);

-- =============================================
-- STEP 1: ALTER gate_logs table to add status column
-- =============================================
ALTER TABLE gate_logs 
ADD status VARCHAR(20) NOT NULL DEFAULT 'Pending';

GO

-- =============================================
-- STORED PROCEDURES FOR GATE LOGS
-- =============================================

-- Procedure 1: Get All Societies
-- =============================================
CREATE PROCEDURE sp_GetAllSocieties
AS
BEGIN
    SET NOCOUNT ON;
    SELECT society_id, name 
    FROM societies 
    ORDER BY name;
END
GO

-- =============================================
-- Procedure 2: Get Buildings by Society
-- =============================================
CREATE PROCEDURE sp_GetBuildingsBySociety
    @society_id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT building_id, name 
    FROM buildings 
    WHERE society_id = @society_id 
    ORDER BY name;
END
GO

-- =============================================
-- Procedure 3: Get Units by Building
-- =============================================
CREATE PROCEDURE sp_GetUnitsByBuilding
    @building_id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT unit_id, unit_no, floor_no 
    FROM units 
    WHERE building_id = @building_id 
    ORDER BY floor_no, unit_no;
END
GO

-- =============================================
-- Procedure 4: Get All Gate Logs (Admin View)
-- =============================================
CREATE PROCEDURE sp_GetAllGateLogs
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        gl.gate_log_id,
        gl.visitor_name,
        gl.vehicle_no,
        gl.purpose,
        gl.check_in,
        gl.check_out,
        gl.status,
        gl.society_id,
        gl.building_id,
        gl.unit_id,
        s.name AS society_name,
        b.name AS building_name,
        u.unit_no
    FROM gate_logs gl
    INNER JOIN societies s ON gl.society_id = s.society_id
    INNER JOIN buildings b ON gl.building_id = b.building_id
    INNER JOIN units u ON gl.unit_id = u.unit_id
    ORDER BY gl.check_in DESC;
END
GO

-- =============================================
-- Procedure 5: Get Gate Log by ID (for Edit)
-- =============================================
CREATE PROCEDURE sp_GetGateLogById
    @gate_log_id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        gate_log_id,
        society_id,
        building_id,
        unit_id,
        visitor_name,
        vehicle_no,
        purpose,
        check_in,
        check_out,
        status
    FROM gate_logs
    WHERE gate_log_id = @gate_log_id;
END
GO

-- =============================================
-- Procedure 6: Insert Gate Log
-- =============================================
CREATE PROCEDURE sp_InsertGateLog
    @society_id BIGINT,
    @building_id BIGINT,
    @unit_id BIGINT,
    @visitor_name VARCHAR(120),
    @vehicle_no VARCHAR(20),
    @purpose VARCHAR(80),
    @check_in DATETIME,
    @check_out DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO gate_logs (society_id, building_id, unit_id, visitor_name, vehicle_no, purpose, check_in, check_out, status)
        VALUES (@society_id, @building_id, @unit_id, @visitor_name, @vehicle_no, @purpose, @check_in, @check_out, 'Pending');
        
        SELECT SCOPE_IDENTITY() AS NewGateLogId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure 7: Update Gate Log
-- =============================================
CREATE PROCEDURE sp_UpdateGateLog
    @gate_log_id BIGINT,
    @society_id BIGINT,
    @building_id BIGINT,
    @unit_id BIGINT,
    @visitor_name VARCHAR(120),
    @vehicle_no VARCHAR(20),
    @purpose VARCHAR(80),
    @check_in DATETIME,
    @check_out DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE gate_logs
        SET 
            society_id = @society_id,
            building_id = @building_id,
            unit_id = @unit_id,
            visitor_name = @visitor_name,
            vehicle_no = @vehicle_no,
            purpose = @purpose,
            check_in = @check_in,
            check_out = @check_out
        WHERE gate_log_id = @gate_log_id;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure 8: Delete Gate Log
-- =============================================
CREATE PROCEDURE sp_DeleteGateLog
    @gate_log_id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM gate_logs 
        WHERE gate_log_id = @gate_log_id;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure 9: Get Pending Gate Logs by Unit (Member View)
-- =============================================
CREATE PROCEDURE sp_GetPendingGateLogsByUnit
    @unit_id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        gl.gate_log_id,
        gl.visitor_name,
        gl.vehicle_no,
        gl.purpose,
        gl.check_in,
        gl.status,
        s.name AS society_name,
        b.name AS building_name,
        u.unit_no
    FROM gate_logs gl
    INNER JOIN societies s ON gl.society_id = s.society_id
    INNER JOIN buildings b ON gl.building_id = b.building_id
    INNER JOIN units u ON gl.unit_id = u.unit_id
    WHERE gl.unit_id = @unit_id 
    AND gl.status = 'Pending'
    ORDER BY gl.check_in DESC;
END
GO

-- =============================================
-- Procedure 10: Approve Gate Log
-- =============================================
CREATE PROCEDURE sp_ApproveGateLog
    @gate_log_id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE gate_logs
        SET status = 'Approved'
        WHERE gate_log_id = @gate_log_id;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO