--CREATE DATABASE TestSale;
--GO

USE TestSale;
GO
--UNIQUE trong SQL là một ràng buộc (constraint) dùng để đảm bảo rằng giá trị trong một cột hoặc nhóm cột là duy nhất trong bảng, tức là không có hai hàng nào có giá trị giống nhau ở cột đó (hoặc tổ hợp các cột đó).
--Để cho phép khóa ngoại có thể NULL hoặc không bắt buộc phải có (nullable foreign key), bạn chỉ cần không thêm NOT NULL vào cột đó.
--REFERENCES	Tạo ràng buộc khóa ngoại để đảm bảo liên kết giữa các bảng dữ liệu
--ON DELETE	Quy định hành vi khi bản ghi cha bị xóa
--ON UPDATE	Quy định hành vi khi giá trị khóa chính được cập nhật
--UNIQUE: dữ liệu không trùng nhau
--UNIQUE NOT NULL: dữ liệu bắt buộc và không được trùng lặp

-- Bảng Roles
CREATE TABLE Roles (  
    RoleID INT IDENTITY(1,1) PRIMARY KEY,  
    RoleName NVARCHAR(50) UNIQUE NOT NULL,  
    Description NVARCHAR(255)  
);
GO

-- Bảng Permissions
CREATE TABLE Permissions (  
    PermissionID INT IDENTITY(1,1) ,
    PermissionCode NVARCHAR(20) PRIMARY KEY,  
    PermissionName NVARCHAR(200)  UNIQUE NOT NULL,  
    Description NVARCHAR(255)  
);
GO

-- Bảng RolePermissions
CREATE TABLE RolePermissions (
    RoleID INT NOT NULL,  
    PermissionCode NVARCHAR(20) NOT NULL,
    PRIMARY KEY (RoleID, PermissionCode),  
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID),  
    FOREIGN KEY (PermissionCode) REFERENCES Permissions(PermissionCode)  
);
GO

-- Trigger để tự động chèn quyền cho RoleName SuperAdmin khi thêm quyền mới
CREATE TRIGGER trg_InsertPermissionForSuperAdmin
ON Permissions
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Lấy RoleID của SuperAdmin
    DECLARE @SuperAdminRoleID INT;
    SELECT @SuperAdminRoleID = RoleID FROM Roles WHERE RoleName = 'SuperAdmin';

    -- Chèn các quyền mới vào RolePermissions cho SuperAdmin
    INSERT INTO RolePermissions (RoleID, PermissionCode)
    SELECT @SuperAdminRoleID, i.PermissionCode
    FROM inserted i;
END;
GO

-- Bảng Users
--IDENTITY(1000,1) nghĩa là:
--Bắt đầu từ 1000
--Mỗi lần tăng 1 (1000, 1001, 1002, 1003, ...)
--Gender là giới tính của người dùng, có thể là Male, Female hoặc Other nếu không xác định.
--ALTER TABLE Users
-- ADD FailedLoginAttempts INT NOT NULL DEFAULT 0,
--     LastFailedLoginAttempt DATETIME NULL;
CREATE TABLE Users (  
    UserID INT IDENTITY(1000,1) PRIMARY KEY,
    UserCode NVARCHAR(20) UNIQUE NOT NULL,  
    UserName NVARCHAR(50) UNIQUE NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Gender NVARCHAR(10) CHECK (Gender IN ('Male', 'Female', 'Other')) DEFAULT 'Other',
    Email NVARCHAR(100) UNIQUE,
    Birthdate DATE,
    Phone NVARCHAR(15),
    Password NVARCHAR(255) NOT NULL,
    Status NVARCHAR(20) CHECK (Status IN ('ACTIVE', 'STOP')) DEFAULT 'ACTIVE',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FailedLoginAttempts INT NOT NULL DEFAULT 0,  -- Số lần đăng nhập thất bại
    LastFailedLoginAttempt DATETIME NULL  -- Thời gian lần đăng nhập thất bại
);
GO
-- Trigger để tự động cập nhật UpdatedAt khi có thay đổi
CREATE TRIGGER trg_UpdateUpdatedAt
ON Users
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE U
    SET UpdatedAt = GETDATE()
    FROM Users U
    INNER JOIN inserted i ON U.UserID = i.UserID;
END;
GO

-- Bảng UserRoles (nhiều-nhiều)
CREATE TABLE UserRoles (
    UserID INT NOT NULL,
    RoleID INT NOT NULL,
    PRIMARY KEY (UserID, RoleID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

CREATE TABLE RefreshToken (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Token NVARCHAR(MAX),
    JwtId NVARCHAR(MAX),
    IsUsed BIT NOT NULL DEFAULT 0,
    IsRevoked BIT NOT NULL DEFAULT 0,
    IssuedAt DATETIME NOT NULL,
    ExpiredAt DATETIME NOT NULL,
    CONSTRAINT FK_RefreshToken_Users FOREIGN KEY (UserId) REFERENCES Users(UserID)
);
-- Xem thông tin về bảng Users
-- sp_help là một stored procedure trong SQL Server dùng để lấy thông tin về cấu trúc của một đối tượng trong cơ sở dữ liệu, chẳng hạn như bảng, view, stored procedure, v.v.
EXEC sp_help 'Users';


-- Theo select UserID
--SELECT R.RoleID, R.RoleName
--FROM UserRoles UR
--JOIN Roles R ON UR.RoleID = R.RoleID
--WHERE UR.UserID = @UserID;  
-- Thay @UserID bằng giá trị cụ thể


--Category
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) UNIQUE NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Trigger để tự động cập nhật UpdatedAt khi có thay đổi
CREATE TRIGGER trg_UpdateCategoryUpdatedAt
ON Categories
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE C
    SET UpdatedAt = GETDATE()
    FROM Categories C
    INNER JOIN inserted i ON C.CategoryID = i.CategoryID;
END;
GO

-- Bảng Products
CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    ProductCode NVARCHAR(20) UNIQUE NOT NULL,
    ProductName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    Price DECIMAL(18, 2) NOT NULL CHECK (Price >= 0),
    Stock INT NOT NULL CHECK (Stock >= 0),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
-- Trigger để tự động cập nhật UpdatedAt khi có thay đổi
-- Trigger để tự động cập nhật UpdatedAt khi có thay đổi
CREATE TRIGGER trg_UpdateProductUpdatedAt
ON Products
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE P
    SET UpdatedAt = GETDATE()
    FROM Products P
    INNER JOIN inserted i ON P.ProductId = i.ProductId;
END;
GO

-- Bảng ProductCategories (nhiều-nhiều)
CREATE TABLE ProductCategories (
    ProductCode NVARCHAR(20) NOT NULL,
    CategoryID INT NOT NULL,
    PRIMARY KEY (ProductCode, CategoryID),
    FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);
GO


-- Bảng ProductImages
CREATE TABLE ProductImages (
    ImageID INT IDENTITY(1,1) PRIMARY KEY,
    ProductCode NVARCHAR(20) NOT NULL,
    ImageURL NVARCHAR(255) NOT NULL,
    IsPrimary BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode) ON DELETE CASCADE ON UPDATE CASCADE
);
GO
-- Trigger để tự động cập nhật UpdatedAt khi có thay đổi
CREATE TRIGGER trg_UpdateProductImageUpdatedAt
ON ProductImages
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE PI
    SET UpdatedAt = GETDATE()
    FROM ProductImages PI
    INNER JOIN inserted i ON PI.ImageID = i.ImageID;
END;
GO
-- Bảng ProoductColorsImages
-- CREATE TABLE ProductColorsImages (
--     ColorImageID INT IDENTITY(1,1) PRIMARY KEY,
--     ProductCode NVARCHAR(20) NOT NULL,
--     Color NVARCHAR(50) NOT NULL,
--     ImageURL NVARCHAR(255) NOT NULL,
--     CreatedAt DATETIME DEFAULT GETDATE(),
--     UpdatedAt DATETIME DEFAULT GETDATE(),
--     FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode) ON DELETE CASCADE ON UPDATE CASCADE
-- );
-- GO
-- Trigger để tự động cập nhật UpdatedAt khi có thay đổi
-- CREATE TRIGGER trg_UpdateProductColorImageUpdatedAt
-- ON ProductColorsImages
-- AFTER UPDATE
-- AS
-- BEGIN
--     SET NOCOUNT ON;

--     UPDATE PCI
--     SET UpdatedAt = GETDATE()
--     FROM ProductColorsImages PCI
--     INNER JOIN inserted i ON PCI.ColorImageID = i.ColorImageID;
-- END;
-- GO
-- Bảng Bookings
CREATE TABLE Bookings (
    BookingId INT IDENTITY(1,1) PRIMARY KEY,
    BookingCode NVARCHAR(20) UNIQUE NOT NULL,
    UserID INT NOT NULL,
    TotalAmount DECIMAL(18, 2) NOT NULL CHECK (TotalAmount >= 0),
    Status NVARCHAR(20) CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')) DEFAULT 'Pending',
    PaymentURL NVARCHAR(255),
    ExpiryDate DATETIME NOT NULL,
    CancelledDate DATETIME,
    CancelledReason NVARCHAR(255),
    BookingDate DATETIME DEFAULT GETDATE(),
    UpdatedBookingDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE ON UPDATE CASCADE
);

GO
-- Trigger để tự động cập nhật BookingDate khi có thay đổi
CREATE TRIGGER trg_UpdateBookingDate
ON Bookings
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE B
    SET UpdatedBookingDate = GETDATE()
    FROM Bookings B
    INNER JOIN inserted i ON B.BookingId = i.BookingId;
END;
GO

-- Bảng BookingProducts
CREATE TABLE BookingProducts (
    BookingProductsID INT IDENTITY(1,1) PRIMARY KEY,
    BookingCode NVARCHAR(20) NOT NULL,
    ProductCode NVARCHAR(20) NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    Price DECIMAL(18, 2) NOT NULL CHECK (Price >= 0),
    Status NVARCHAR(20) CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')) DEFAULT 'Pending',
    BookingProductDate DATETIME DEFAULT GETDATE(),
	FOREIGN KEY (BookingCode) REFERENCES Bookings(BookingCode) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode) ON DELETE CASCADE ON UPDATE CASCADE
);

GO
-- Trigger để tự động cập nhật BookingProductDate khi có thay đổi
CREATE TRIGGER trg_UpdateBookingProductDate
ON BookingProducts
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE BP
    SET BookingProductDate = GETDATE()
    FROM BookingProducts BP
    INNER JOIN inserted i ON BP.BookingProductsID = i.BookingProductsID;
END;
GO




-- Bảng Passengers
CREATE TABLE Passengers (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    PassengerCode NVARCHAR(20) UNIQUE NOT NULL,
    BookingCode NVARCHAR(20) NOT NULL,
    CreateUserCode NVARCHAR(20) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Gender NVARCHAR(10) CHECK (Gender IN ('Male', 'Female', 'Other')),
    Email NVARCHAR(100),
    Phone NVARCHAR(15),
    Status NVARCHAR(20),
    Address NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (BookingCode) REFERENCES Bookings(BookingCode) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- Trigger cập nhật UpdatedAt khi Passengers được update
CREATE TRIGGER trg_UpdatePassengerUpdatedAt
ON Passengers
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE P
    SET UpdatedAt = GETDATE()
    FROM Passengers P
    INNER JOIN inserted i ON P.ID = i.ID;
END;
GO


-- Bảng Payments
CREATE TABLE Payments (
    paymentId INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    BookingCode NVARCHAR(20) NOT NULL,
    TotalAmount DECIMAL(18, 2) NOT NULL CHECK (TotalAmount >= 0),
    PaymentMethod NVARCHAR(50) NOT NULL,
    Transactions NVARCHAR(50),
    Description NVARCHAR(255),
    IsSuccess BIT NOT NULL DEFAULT 0,  -- 0: False, 1: True
    Status NVARCHAR(20) CHECK (Status IN ('Pending', 'Completed', 'Failed')) DEFAULT 'Pending',
    PaymentDate DATETIME DEFAULT GETDATE(),
    UpdatedPaymentDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (BookingCode) REFERENCES Bookings(BookingCode) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- Trigger cập nhật UpdatedPaymentDate khi Payments được update
CREATE TRIGGER trg_UpdatePaymentUpdatedDate
ON Payments
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE P
    SET UpdatedPaymentDate = GETDATE()
    FROM Payments P
    INNER JOIN inserted i ON P.paymentId = i.paymentId;
END;
GO





--insert Users
insert into Users (UserCode, UserName, FirstName, LastName, Gender, Email, Birthdate, Phone, Password, Status) 
values ('A1000', 'superadmin', 'super', 'admin', 'Male', 'superadmin@gmail.com', '2002-01-31', '1234567890', '$12$FcJorltaA5KwcbLkiy.UIOqQjcX0x8rRQQBO5KHbMCGBp2VwZAV/O', 'ACTIVE');--Mk đã được mã hóa bằng bcrypt Password: Password123
insert into Users (UserCode, UserName, FirstName, LastName, Gender, Email, Birthdate, Phone, Password, Status)
values ('A1001', 'admin', 'admin', 'test', 'Female', 'admin123@gmail.com', '2002-01-31', '0987654321', '$12$FcJorltaA5KwcbLkiy.UIOqQjcX0x8rRQQBO5KHbMCGBp2VwZAV/O', 'ACTIVE');--Mk đã được mã hóa bằng bcrypt Password: Password123

--insert Roles
insert into Roles (RoleName, Description) values ('SuperAdmin', N'Có quyền cao nhất trong hệ thống');
insert into Roles (RoleName, Description) values ('Admin', N'Có quyền quản lý người dùng');
insert into Roles (RoleName, Description) values ('Manager', N'Có quyền quản lý một số chức năng');
insert into Roles (RoleName, Description) values ('Editor', N'Có quyền chỉnh sửa nội dung');
insert into Roles (RoleName, Description) values ('User', N'Người dùng thông thường');

--insert UserRoles
select * from UserRoles;
insert into UserRoles (UserID, RoleID) values (1000, 1); 
insert into UserRoles (UserID, RoleID) values (1001, 2); 
select * from Permissions;

insert into Permissions values ('VIEW_USERS', 'View Users', N'Quyền xem thông tin người dùng');
insert into Permissions values ('CREATE_USERS', 'Create Users', N'Quyền tạo người dùng');
insert into Permissions values ('EDIT_USERS', 'Edit Users', N'Quyền chỉnh sửa thông tin người dùng');
insert into Permissions values ('DELETE_USERS', 'Delete', N'Quyền xóa người dùng');
insert into Permissions values ('VIEW_PRODUCTS', 'View Products', N'Quyền xem sản phẩm');




--insert RolePermissions
select * from RolePermissions;
insert into RolePermissions (RoleID, PermissionID) values (1, 1); -- SuperAdmin có tất cả quyền
insert into RolePermissions (RoleID, PermissionID) values (2, 2); -- Admin có quyền quản lý người dùng
insert into RolePermissions (RoleID, PermissionID) values (2, 3); -- Admin có quyền xem người dùng
insert into RolePermissions (RoleID, PermissionID) values (2, 4); -- Admin có quyền chỉnh sửa người dùng
insert into RolePermissions (RoleID, PermissionID) values (2, 5); -- Admin có quyền xóa người dùng

--select dữ liệu từ bảng Users join Roles join UserRoles join RolePermissions join Permissions
SELECT U.UserID,U.UserCode ,U.UserName, U.Status, U.Email,U.Password, UR.RoleID, R.RoleName, P.PermissionName
FROM Users U
JOIN UserRoles UR ON U.UserID = UR.UserID
JOIN Roles R ON UR.RoleID = R.RoleID
JOIN RolePermissions RP ON R.RoleID = RP.RoleID
JOIN Permissions P ON RP.PermissionCode = P.PermissionCode
WHERE U.UserID = 1000; -- Thay 1 bằng UserID cụ thể bạn muốn kiểm tra

--Cách 2: Gom các PermissionName thành 1 dòng
SELECT 
    U.UserID,
    U.UserCode,
    U.UserName,
    U.Status,
    U.Email,
    U.Password,
    UR.RoleID,
    R.RoleName,
    STRING_AGG(P.PermissionName, ', ') AS Permissions
FROM Users U
JOIN UserRoles UR ON U.UserID = UR.UserID
JOIN Roles R ON UR.RoleID = R.RoleID
JOIN RolePermissions RP ON R.RoleID = RP.RoleID
JOIN Permissions P ON RP.PermissionCode = P.PermissionCode
WHERE U.UserID = 1000
GROUP BY 
    U.UserID,
    U.UserCode,
    U.UserName,
    U.Status,
    U.Email,
    U.Password,
    UR.RoleID,
    R.RoleName;



-- Cách 3: Nếu 1 user có nhiều role → thêm STRING_AGG(DISTINCT ...) để gom không trùng lặp:
-- Cách 3: Nếu 1 user có nhiều role → thêm STRING_AGG(DISTINCT ...) để gom không trùng lặp:
--SELECT 
--    U.UserID,
--    U.UserCode,
--    U.UserName,
--    U.Status,
--    U.Email,
--    U.Password,
--    STRING_AGG(DISTINCT R.RoleName, ', ') AS RoleNames,
--    STRING_AGG(DISTINCT P.PermissionName, ', ') AS Permissions
--FROM Users U
--JOIN UserRoles UR ON U.UserID = UR.UserID
--JOIN Roles R ON UR.RoleID = R.RoleID
--JOIN RolePermissions RP ON R.RoleID = RP.RoleID
--JOIN Permissions P ON RP.PermissionID = P.PermissionID
--WHERE U.UserID = 1028
--GROUP BY 
--    U.UserID,
--    U.UserCode,
--    U.UserName,
--    U.Status,
--    U.Email,
--    U.Password;

