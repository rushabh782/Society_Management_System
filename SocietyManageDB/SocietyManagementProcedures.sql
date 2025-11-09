USE societyDB;
GO

-- ===================================================================
-- 1. Dashboard
-- ===================================================================
PRINT 'Creating sp_GetAdminDashboardStats...';
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_GetAdminDashboardStats]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        (SELECT COUNT(*) FROM societies) AS TotalSocieties,
        (SELECT COUNT(*) FROM members) AS TotalMembers,
        (SELECT COUNT(*) FROM buildings) AS TotalBuildings,
        (SELECT COUNT(*) FROM complaints WHERE status = 'Open') AS OpenComplaints;
END
GO

-- ===================================================================
-- 2. Societies (CRUD)
-- ===================================================================
PRINT 'Creating Society CRUD Procedures...';
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Societies_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT society_id, name, city, state, pincode FROM societies ORDER BY name;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Societies_GetByID]
(
    @SocietyID BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM societies WHERE society_id = @SocietyID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Societies_Insert]
(
    @Name VARCHAR(150),
    @Address1 VARCHAR(200),
    @Address2 VARCHAR(200),
    @City VARCHAR(80),
    @State VARCHAR(80),
    @Pincode VARCHAR(10)
)
AS
BEGIN
    INSERT INTO societies (name, address_line1, address_line2, city, state, pincode, created_at)
    VALUES (@Name, @Address1, @Address2, @City, @State, @Pincode, GETDATE());
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Societies_Update]
(
    @SocietyID BIGINT,
    @Name VARCHAR(150),
    @Address1 VARCHAR(200),
    @Address2 VARCHAR(200),
    @City VARCHAR(80),
    @State VARCHAR(80),
    @Pincode VARCHAR(10)
)
AS
BEGIN
    UPDATE societies
    SET 
        name = @Name,
        address_line1 = @Address1,
        address_line2 = @Address2,
        city = @City,
        state = @State,
        pincode = @Pincode,
        updated_at = GETDATE()
    WHERE society_id = @SocietyID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Societies_Delete]
(
    @SocietyID BIGINT
)
AS
BEGIN
    -- Add pre-delete checks here if needed (e.g., delete buildings)
    DELETE FROM societies WHERE society_id = @SocietyID;
END
GO

-- ===================================================================
-- 3. Buildings (CRUD + Filter)
-- ===================================================================
PRINT 'Creating Building CRUD Procedures...';
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Buildings_GetBySocietyID]
(
    @SocietyID BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT building_id, name, floors 
    FROM buildings 
    WHERE society_id = @SocietyID 
    ORDER BY name;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Buildings_Insert]
(
    @SocietyID BIGINT,
    @Name VARCHAR(100),
    @Floors INT
)
AS
BEGIN
    INSERT INTO buildings (society_id, name, floors, created_at)
    VALUES (@SocietyID, @Name, @Floors, GETDATE());
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Buildings_Update]
(
    @BuildingID BIGINT,
    @Name VARCHAR(100),
    @Floors INT
)
AS
BEGIN
    UPDATE buildings
    SET 
        name = @Name,
        floors = @Floors
    WHERE building_id = @BuildingID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Buildings_Delete]
(
    @BuildingID BIGINT
)
AS
BEGIN
    DELETE FROM buildings WHERE building_id = @BuildingID;
END
GO

-- ===================================================================
-- 4. Units (CRUD + Filter)
-- ===================================================================
PRINT 'Creating Unit CRUD Procedures...';
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Units_GetByBuildingID]
(
    @BuildingID BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT unit_id, unit_no, floor_no, carpet_area_sqft, is_parking_allocated 
    FROM units 
    WHERE building_id = @BuildingID 
    ORDER BY floor_no, unit_no;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Units_Insert]
(
    @BuildingID BIGINT,
    @UnitNo VARCHAR(20),
    @FloorNo INT,
    @CarpetArea DECIMAL(10,2),
    @IsParking BIT
)
AS
BEGIN
    INSERT INTO units (building_id, unit_no, floor_no, carpet_area_sqft, is_parking_allocated)
    VALUES (@BuildingID, @UnitNo, @FloorNo, @CarpetArea, @IsParking);
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Units_Update]
(
    @UnitID BIGINT,
    @UnitNo VARCHAR(20),
    @FloorNo INT,
    @CarpetArea DECIMAL(10,2),
    @IsParking BIT
)
AS
BEGIN
    UPDATE units
    SET 
        unit_no = @UnitNo,
        floor_no = @FloorNo,
        carpet_area_sqft = @CarpetArea,
        is_parking_allocated = @IsParking
    WHERE unit_id = @UnitID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Units_Delete]
(
    @UnitID BIGINT
)
AS
BEGIN
    DELETE FROM units WHERE unit_id = @UnitID;
END
GO

-- ===================================================================
-- 5. Members (CRUD + Filter)
-- ===================================================================
PRINT 'Creating Member CRUD Procedures...';
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Members_GetBySocietyID]
(
    @SocietyID BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT member_id, full_name, email, phone, status 
    FROM members 
    WHERE society_id = @SocietyID 
    ORDER BY full_name;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Members_GetByID]
(
    @MemberID BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM members WHERE member_id = @MemberID;
END
GO


CREATE OR ALTER PROCEDURE [dbo].[sp_Members_Insert]
(
    @SocietyID BIGINT,
    @FullName VARCHAR(120),
    @Email VARCHAR(150),
    @Phone VARCHAR(20),
    @Status VARCHAR(20),
    @BuildingID BIGINT,
    @UnitID BIGINT,
    @Type VARCHAR(20) = 'Owner'  -- Default value if not provided
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MemberID BIGINT;

    -- 1️⃣ Insert member record
    INSERT INTO members (society_id, full_name, email, phone, status)
    VALUES (@SocietyID, @FullName, @Email, @Phone, @Status);

    SET @MemberID = SCOPE_IDENTITY();

    -- 2️⃣ Link this member to the selected unit with type and start_date
    INSERT INTO unit_occupancies (unit_id, member_id, type, start_date)
    VALUES (@UnitID, @MemberID, @Type, GETDATE());
END
GO


CREATE OR ALTER PROCEDURE [dbo].[sp_Members_Update]
(
    @MemberID BIGINT,
    @FullName VARCHAR(120),
    @Email VARCHAR(150),
    @Phone VARCHAR(20),
    @Status VARCHAR(20)
)
AS
BEGIN
    UPDATE members
    SET 
        full_name = @FullName,
        email = @Email,
        phone = @Phone,
        status = @Status
    WHERE member_id = @MemberID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_Members_Delete]
(
    @MemberID BIGINT
)
AS
BEGIN
    -- Add pre-delete logic (e.g., delete user, unit occupancy)
    DELETE FROM members WHERE member_id = @MemberID;
END
GO


-- Fetch buildings based on Society
CREATE OR ALTER PROCEDURE [dbo].[sp_Buildings_GetBySocietyID]
(
    @SocietyID BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT building_id, name 
    FROM buildings 
    WHERE society_id = @SocietyID 
    ORDER BY name;
END
GO

-- Fetch units based on Building
CREATE OR ALTER PROCEDURE [dbo].[sp_Units_GetByBuildingID]
(
    @BuildingID BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT unit_id, unit_no 
    FROM units 
    WHERE building_id = @BuildingID 
    ORDER BY unit_no;
END
GO

PRINT 'All Stored Procedures created successfully.';

CREATE OR ALTER PROCEDURE [dbo].[sp_Complaints_Insert]
(
    @SocietyID BIGINT,
    @RaisedByUserID BIGINT, -- The user_id of the member raising the complaint
    @UnitID BIGINT,         -- The unit_id associated with the complaint (can be NULL if it's about common areas)
    @Category VARCHAR(50),
    @Title VARCHAR(150),
    @Description TEXT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert the new complaint with default 'Open' status and current timestamp
    INSERT INTO complaints (society_id, raised_by_user_id, unit_id, category, title, description, status, created_at)
    VALUES (@SocietyID, @RaisedByUserID, @UnitID, @Category, @Title, @Description, 'Open', GETDATE());

    -- Optionally, you could return the newly created complaint_id
    -- SELECT SCOPE_IDENTITY() AS NewComplaintID;
END
GO

PRINT 'Complaint Insert Procedure created successfully.';



GO
alter PROCEDURE sp_RegisterUser
    @full_name NVARCHAR(120),
    @email NVARCHAR(150),
    @phone NVARCHAR(20),
    @username NVARCHAR(80),
    @password_hash NVARCHAR(255),
    @society_id BIGINT,
    @role_id INT,
    @building_id BIGINT = NULL,
    @unit_no NVARCHAR(20),
    @occupancy_type NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRAN;

        DECLARE @member_id BIGINT, @unit_id BIGINT, @user_id BIGINT;

        -- 🔹 1. Get the unit_id based on unit_no and building_id
        SELECT @unit_id = unit_id 
        FROM units 
        WHERE unit_no = @unit_no AND building_id = @building_id;

        IF @unit_id IS NULL
        BEGIN
            THROW 51000, 'Invalid Unit selected.', 1;
        END

        -- 🔹 2. Insert into members table
        INSERT INTO members (society_id, full_name, email, phone)
        VALUES (@society_id, @full_name, @email, @phone);

        SET @member_id = SCOPE_IDENTITY();

        -- 🔹 3. Insert into users table
        INSERT INTO users (member_id, username, password_hash)
        VALUES (@member_id, @username, @password_hash);

        SET @user_id = SCOPE_IDENTITY();

        -- 🔹 4. Assign role (Member = 2 by default)
        INSERT INTO user_roles (user_id, role_id, society_id)
        VALUES (@user_id, @role_id, @society_id);

        -- 🔹 5. Insert occupancy info
        INSERT INTO unit_occupancies (unit_id, member_id, type, start_date)
        VALUES (@unit_id, @member_id, @occupancy_type, GETDATE());

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN;
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        THROW 51000, @ErrorMessage, 1;
    END CATCH
END;
GO

EXEC sp_RegisterUser
    @full_name = 'John Doe',
    @email = 'john@example.com',
    @phone = '9876543210',
    @username = 'john123',
    @password_hash = 'hashedpasswordhere',
    @society_id = 1,
    @role_id = 2,
    @building_id = 101,
    @unit_no = '101',
    @occupancy_type = 'Owner';
