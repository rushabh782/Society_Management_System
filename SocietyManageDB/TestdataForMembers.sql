/*
=====================================================================
    ADDITIONAL SAMPLE DATA SCRIPT FOR Member Dashboard/Profile
    Run this *after* the main SampleData.sql script.
=====================================================================
*/

USE societyDB;
GO

PRINT 'Adding more sample data relevant to Member section...';

-- Add a new member without current occupancy to test profile LEFT JOIN
SET IDENTITY_INSERT [dbo].[members] ON;
INSERT INTO [dbo].[members] (member_id, society_id, full_name, email, phone, status, created_at) VALUES
(5, 1, 'Karan Verma', 'karan.verma@email.com', '9800055555', 'Active', GETDATE());
SET IDENTITY_INSERT [dbo].[members] OFF;
GO

SET IDENTITY_INSERT [dbo].[users] ON;
INSERT INTO [dbo].[users] (user_id, member_id, username, password_hash, is_active) VALUES
(5, 5, 'karan_v', 'karan123', 1); -- Member without unit (password 'karan123')
SET IDENTITY_INSERT [dbo].[users] OFF;
GO

-- Add Karan as a member of Society 1
INSERT INTO [dbo].[user_roles] (user_id, role_id, society_id) VALUES
(5, 6, 1); -- User 5 is Member (Role 6) of Society 1
GO
PRINT 'Added Member/User 5 (Karan Verma) without occupancy.';

-- Add another UNPAID bill for Riya's unit (Unit 1) for a different month
SET IDENTITY_INSERT [dbo].[maintenance_bills] ON;
INSERT INTO [dbo].[maintenance_bills] (bill_id, society_id, unit_id, bill_month, due_date, total_amount, status) VALUES
(3, 1, 1, '2025-11-01', '2025-11-20', 3600.00, 'Unpaid'); -- New UNPAID bill for Riya
SET IDENTITY_INSERT [dbo].[maintenance_bills] OFF;

INSERT INTO [dbo].[bill_items] (bill_id, description, amount) VALUES
(3, 'Monthly Maintenance', 2550.00),
(3, 'Water Charges', 1050.00);
GO
PRINT 'Added Unpaid Bill 3 for Riya (User 2).';

-- Add a CLOSED complaint raised by Riya (User 2)
SET IDENTITY_INSERT [dbo].[complaints] ON;
INSERT INTO [dbo].[complaints] (complaint_id, society_id, raised_by_user_id, unit_id, category, title, description, status, created_at) VALUES
(3, 1, 2, 1, 'Security', 'Parcel Misplaced', 'My parcel delivered yesterday is missing.', 'Closed', DATEADD(day, -5, GETDATE())); -- Closed 5 days ago
SET IDENTITY_INSERT [dbo].[complaints] OFF;
GO
PRINT 'Added Closed Complaint 3 for Riya (User 2).';


INSERT INTO [dbo].[amenity_bookings]
(amenity_id, user_id, start_time, end_time, status)
VALUES
(2, 2, DATEADD(day, -7, GETDATE()),
    DATEADD(hour, 1, DATEADD(day, -7, GETDATE())), 'Booked');

PRINT '✅ Booking inserted successfully (auto identity used)';

GO

-- Verification Queries (Optional - Run these in SSMS to check)
/*
-- Check Riya's (User 2) Dashboard Stats
EXEC sp_GetMemberDashboardStats @UserID = 2;
-- Expected: UnpaidBillsCount=1, OpenComplaintsCount=1, ActiveBookingsCount=1

-- Check Priya's (User 4) Dashboard Stats
EXEC sp_GetMemberDashboardStats @UserID = 4;
-- Expected: UnpaidBillsCount=1, OpenComplaintsCount=0, ActiveBookingsCount=0

-- Check Riya's (User 2) Profile
EXEC sp_Members_GetProfileDetailsByUserID @UserID = 2;
-- Expected: Riya's details, Society 1, Unit 101/A-Wing, Owner

-- Check Priya's (User 4) Profile
EXEC sp_Members_GetProfileDetailsByUserID @UserID = 4;
-- Expected: Priya's details, Society 1, Unit 501/B-Wing, Tenant

-- Check Karan's (User 5) Profile (Member without unit)
EXEC sp_Members_GetProfileDetailsByUserID @UserID = 5;
-- Expected: Karan's details, Society 1, NULLs for building/unit/occupancy
*/

SELECT * FROM [dbo].[amenities];

SET IDENTITY_INSERT [dbo].[amenities] ON;

INSERT INTO [dbo].[amenities] (amenity_id, society_id, name, booking_required) VALUES
(1, 1, 'Club House', 1),
(2, 1, 'Gym', 1),
(3, 1, 'Swimming Pool', 1);

GO

PRINT ' Amenities added successfully';
GO
GO


-- Reset identity insert for amenities table
SET IDENTITY_INSERT [dbo].[amenities] OFF;
GO

SELECT 'Amenity Identity Insert Status Reset' AS StatusMessage;
GO

PRINT 'Populating sample unit_occupancies data...';

INSERT INTO [dbo].[unit_occupancies] (unit_id, member_id, type, start_date, end_date)
VALUES
(1, 1, 'Owner', '2023-01-15', NULL),
(3, 4, 'Tenant', '2024-05-01', NULL),
(4, 3, 'Owner', '2022-11-20', NULL),
(2, 5, 'Tenant', '2023-06-01', '2024-05-31');

PRINT 'Sample unit_occupancies data inserted!';
GO


SELECT * FROM [dbo].[unit_occupancies];


GO
CREATE PROCEDURE sp_RegisterUser
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
select * from users;