--Users Table
CREATE TABLE Users (
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [FullName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(100) NOT NULL,
    [PhoneNumber] NVARCHAR(20) NULL,
    [PasswordHash] NVARCHAR(255) NOT NULL,
    [Role] NVARCHAR(50) DEFAULT ('customer') NOT NULL,
    [CreatedAt] DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    UNIQUE NONCLUSTERED ([Email] ASC)
);

--default admin user 
--password is admin@123
INSERT INTO Users (Id, FullName, Email, PhoneNumber, PasswordHash, Role, CreatedAt)
VALUES (
    NEWID(),
    'Admin',
    'admin@gmail.com',
    '03000000000',
    '$2a$11$zI6bG/l7/ThMb/23mHV14eTM/wrjal0YPwxh5sx9y4BH7i4si8rx.',
    'admin',
    GETUTCDATE()
);


--otp table
CREATE TABLE OtpVerification (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL,
    OtpCode NVARCHAR(10) NOT NULL,
    Purpose NVARCHAR(50) NOT NULL,     -- 'Register' or 'ResetPassword'
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsUsed BIT NOT NULL DEFAULT 0
);


--alter users table
ALTER TABLE Users
ADD IsEmailVerified BIT NOT NULL DEFAULT 0;

--update admin and test customer as verified
UPDATE Users
SET IsEmailVerified = 1
WHERE Email IN ('admin@gmail.com', 'customer@gmail.com');
