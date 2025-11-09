
use societyDB;
--Can expand later with vehicles, complaints, etc. Register Procedure no 2
GO
drop PROCEDURE sp_RegisterUser
    @full_name NVARCHAR(120),
    @email NVARCHAR(150),
    @phone NVARCHAR(20),
    @username NVARCHAR(80),
    @password_hash NVARCHAR(255),
    @society_id BIGINT,
    @role_id INT,
    @building_id BIGINT,
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

CREATE OR ALTER TRIGGER trg_AutoAssignBuildingPrefix
ON buildings
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @building_id BIGINT, @floors INT, @prefix CHAR(1), @i INT = 1;

    -- Step 1: Get newly inserted building details
    SELECT @building_id = i.building_id, @floors = i.floors
    FROM inserted i;

    -- Step 2: Assign prefix based on building_id
    SET @prefix = CHAR(64 + @building_id);

    UPDATE buildings
    SET prefix = @prefix
    WHERE building_id = @building_id;

    -- Step 3: Generate units dynamically based on floors
    WHILE @i <= @floors
    BEGIN
        DECLARE @unitNo NVARCHAR(10) = CONCAT(@prefix, '-', RIGHT('00' + CAST(@i AS VARCHAR(3)), 3));

        INSERT INTO units (building_id, unit_no, floor_no, carpet_area_sqft, is_parking_allocated)
        VALUES (@building_id, @unitNo, @i, 850, 1);

        SET @i += 1;
    END;
END;
GO

drop trigger trg_AutoAssignBuildingPrefix;
ALTER TABLE units
ADD CONSTRAINT UQ_unit_building UNIQUE (unit_no, building_id);
-- THIS TABLE IS ALTERED SO THAT IN EACH BUILDING UNIT NO IS UNIQUE (like A-101) allowed in different buildings, but not repeated in same building


ALTER TABLE units
DROP CONSTRAINT UQ_unit_building;

ALTER TABLE buildings
ADD prefix CHAR(1) NULL;

ALTER TABLE buildings
DROP COLUMN prefix;

select * from buildings;

-- Search across your entire database for any reference to 'prefix'
SELECT OBJECT_NAME(object_id) AS ObjectName, 
       OBJECT_TYPE_DESC, 
       definition
FROM sys.sql_modules
WHERE definition LIKE '%prefix%';

-- Search across your entire database for any reference to 'prefix'
SELECT OBJECT_NAME(object_id) AS ObjectName, 
       OBJECT_TYPE_DESC, 
       definition
FROM sys.sql_modules
WHERE definition LIKE '%prefix%';


select * from units;

-- change for unit table
ALTER TABLE units ADD society_id BIGINT;

ALTER TABLE units
ADD CONSTRAINT FK_Units_Societies FOREIGN KEY (society_id) REFERENCES societies(society_id);

SELECT 
    unit_id,
    unit_no,
    floor_no,               -- ✅ must be included
    carpet_area_sqft,
    is_parking_allocated,
    society_id
FROM units
WHERE building_id = @BuildingID;



EXEC sp_Members_Insert
    @SocietyID = 1,
    @FullName = 'Test Member',
    @Email = 'test@example.com',
    @Phone = '9876543210',
    @Status = 'Active',
    @BuildingID = 1,
    @UnitID = 101;
