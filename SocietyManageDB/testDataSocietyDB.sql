USE societyDB;


INSERT INTO roles (name) VALUES ('member');


-- Insert Society
INSERT INTO societies (name, address_line1, city, state, pincode)
VALUES ('Test Society', '123 Main Street', 'Mumbai', 'MH', '400001');

-- Insert Member
INSERT INTO members (society_id, full_name, email, phone)
VALUES (1, 'Admin User', 'admin@example.com', '9999999999');

INSERT INTO members (society_id, full_name, email, phone)
VALUES (1, 'Dhruv User', 'dhruv@gmail.com', '8360176459');

-- Insert User
INSERT INTO users (member_id, username, password_hash)
VALUES (1, 'admin', 'admin123'); -- You can hash this later

-- Insert User
INSERT INTO users (member_id, username, password_hash)
VALUES (4, 'dhruv', 'dhruv123'); -- You can hash this later
-- Insert Role
INSERT INTO roles (name)
VALUES ('member');

-- Assign Role to admin
INSERT INTO user_roles (user_id, role_id, society_id)
VALUES (1, 5, 1);


-- Assign Role to user
INSERT INTO user_roles (user_id, role_id, society_id)
VALUES (1, 6, 1);


GO
CREATE PROCEDURE UserLoginProc
    @username VARCHAR(80),
    @password VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.user_id,
        u.username,
        r.name AS role
    FROM users u
    LEFT JOIN user_roles ur ON u.user_id = ur.user_id
    LEFT JOIN roles r ON ur.role_id = r.role_id
    WHERE u.username = @username
      AND u.password_hash = @password
      AND u.is_active = 1;
END
GO

INSERT INTO complaints (society_id, raised_by_user_id, unit_id, category, title, description, status)
VALUES 
(1, 1, 1, 'Plumbing', 'Leakage in bathroom', 'There is continuous water leakage from the ceiling.', 'Open'),

(1, 2, 2, 'Electricity', 'Power outage in bedroom', 'No power supply to the bedroom socket since morning.', 'In Progress'),

(1, 1, 1, 'Cleanliness', 'Garbage not collected', 'Common area trash bin not cleared for 2 days.', 'Open'),

(1, 2, 2, 'Security', 'Unauthorized visitor', 'Someone unknown was seen wandering on 5th floor.', 'Closed');


select * from societies;

select * from users;

select * from roles;

select * from members;

select * from user_roles;

select * from units;

select * from complaints;

select * from amenity_bookings;

select * from unit_occupancies;

select * from expenses;

select * from payments;

select * from buildings;

select * from gate_logs;

SELECT 
    usr.user_id,
    usr.username,
    m.full_name,
    uo.unit_id,
    u.unit_no,
    b.name as building,
    s.name as society
FROM users usr
INNER JOIN members m ON usr.member_id = m.member_id
INNER JOIN unit_occupancies uo ON m.member_id = uo.member_id
INNER JOIN units u ON uo.unit_id = u.unit_id
INNER JOIN buildings b ON u.building_id = b.building_id
INNER JOIN societies s ON b.society_id = s.society_id
WHERE usr.username = 'dhruv'  -- Replace with actual username
AND (uo.end_date IS NULL OR uo.end_date >= GETDATE());

CREATE TABLE notifications (
    notification_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,                   -- references the users table
    title NVARCHAR(200) NOT NULL,              -- title of the notification
    message NVARCHAR(MAX) NOT NULL,            -- notification message content
    link_url NVARCHAR(500) NULL,               -- optional link (nullable)
    created_at DATETIME DEFAULT GETDATE()      -- auto-generated timestamp
);

ALTER TABLE notifications
ADD CONSTRAINT FK_notifications_users
FOREIGN KEY (user_id) REFERENCES users(user_id)
ON DELETE CASCADE;

-- ✅ Add the 'is_read' column to track whether the user has read the notification
ALTER TABLE notifications
ADD is_read BIT NOT NULL DEFAULT 0;

select * from notifications;