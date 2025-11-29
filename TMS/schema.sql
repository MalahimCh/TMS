--Users Table
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20),
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'customer',
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    IsEmailVerified BIT NOT NULL DEFAULT 0
);

--default admin user 
--password is admin@123
INSERT INTO Users (FullName, Email, PhoneNumber, PasswordHash, Role)
VALUES (
    'Admin',
    'admin@gmail.com',
    '03000000000',
    '$2a$11$zI6bG/l7/ThMb/23mHV14eTM/wrjal0YPwxh5sx9y4BH7i4si8rx.',
    'admin'
);

INSERT INTO Users (FullName, Email, PhoneNumber, PasswordHash, Role)
VALUES (
    'Customer',
    'customer@gmail.com',
    '03000000000',
    '$2a$11$q4FCOgL0n86JGZUdOdWqaumclr0HsqiNN4pDOOP1O5Ls7KLtY3fDa',
    'customer'
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



--update admin and test customer as verified
UPDATE Users
SET IsEmailVerified = 1
WHERE Email IN ('admin@gmail.com', 'customer@gmail.com');

CREATE TABLE Buses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BusNumber NVARCHAR(20) NOT NULL UNIQUE,
    BusType NVARCHAR(50) NOT NULL,
    TotalSeats INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Seats (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BusId INT NOT NULL FOREIGN KEY REFERENCES Buses(Id),
    SeatNumber NVARCHAR(10) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Available',
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT UQ_Seat_Bus UNIQUE (BusId, SeatNumber)
);

CREATE TABLE Locations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_LocationName UNIQUE (Name)
);

INSERT INTO Locations (Name) VALUES 
('Karachi'),
('Lahore'),
('Faisalabad'),
('Rawalpindi'),
('Gujranwala'),
('Peshawar'),
('Multan'),
('Hyderabad'),
('Islamabad'),
('Quetta'),
('Sargodha'),
('Sialkot'),
('Bahawalpur'),
('Sheikhupura'),
('Sukkur'),
('Larkana'),
('Rahim Yar Khan'),
('Okara'),
('Kasur'),
('Gujrat'),
('Jhelum'),
('Dera Ghazi Khan'),
('Muzaffargarh'),
('Bahawalnagar'),
('Sahiwal'),
('Gojra'),
('Mardan'),
('Nawabshah'),
('Chiniot'),
('Mirpur Khas'),
('Mingora'),
('Dera Ismail Khan'),
('Swat'),
('Abbottabad'),
('Kohat'),
('Chaman'),
('Taxila'),
('Murree'),
('Pakpattan'),
('Dadu'),
('Kotri'),
('Jacobabad'),
('Shikarpur'),
('Haripur'),
('Narowal'),
('Kamoke'),
('Samundri'),
('Jaranwala'),
('Vehari'),
('Wah Cantt'),
('Kot Addu'),
('Attock'),
('Chakwal'),
('Khuzdar');




CREATE TABLE Routes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OriginId INT NOT NULL,
    DestinationId INT NOT NULL,
    DistanceKm INT NOT NULL,
    EstimatedTimeMinutes INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Route UNIQUE (OriginId, DestinationId),
    CONSTRAINT FK_Route_Origin FOREIGN KEY (OriginId) REFERENCES Locations(Id),
    CONSTRAINT FK_Route_Destination FOREIGN KEY (DestinationId) REFERENCES Locations(Id)
);



CREATE TABLE RecurringSchedules (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    RouteId INT NOT NULL FOREIGN KEY REFERENCES Routes(Id),
    BusId INT NOT NULL FOREIGN KEY REFERENCES Buses(Id),

    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Frequency NVARCHAR(50) NOT NULL,
    DepartureTime TIME NOT NULL,
    ArrivalTime TIME NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    SelectedDays NVARCHAR(20) NULL,

    CONSTRAINT UQ_Recurring UNIQUE (BusId, StartDate, DepartureTime)
);

CREATE TABLE Schedules (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    RouteId INT NOT NULL FOREIGN KEY REFERENCES Routes(Id),
    BusId INT NOT NULL FOREIGN KEY REFERENCES Buses(Id),

    RecurringScheduleId INT NULL FOREIGN KEY REFERENCES RecurringSchedules(Id),

    DepartureTime DATETIME2(7) NOT NULL,
    ArrivalTime DATETIME2(7) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    Completed BIT NOT NULL DEFAULT 0,

    CONSTRAINT UQ_BusSchedule UNIQUE (BusId, DepartureTime)
);









--CREATE TABLE Bookings (
--    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
--    UserId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Users(Id),
--    ScheduleId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Schedules(Id),
--    BookingDate DATETIME2(7) DEFAULT GETUTCDATE(),

--    -- Amounts
--    TotalAmount DECIMAL(10,2) NOT NULL,
--    DiscountAmount DECIMAL(10,2) DEFAULT 0,
--    PromotionCode NVARCHAR(50) NULL,
--    FinalAmount AS (TotalAmount - DiscountAmount) PERSISTED,

--    -- Status
--    BookingStatus NVARCHAR(50) DEFAULT 'Pending', 
--    PaymentStatus NVARCHAR(50) DEFAULT 'Pending',

--    -- Payment tracking
--    TransactionId NVARCHAR(100) NULL,
--    PaymentMethod NVARCHAR(50) NULL,

--    BookingReference NVARCHAR(50) NOT NULL UNIQUE
--);



--CREATE TABLE BookingSeats (
--    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
--    BookingId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Bookings(Id),
--    SeatId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Seats(Id),
--    SeatPrice DECIMAL(10,2) NOT NULL
--);


--CREATE TABLE Promotions (
--    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
--    Code NVARCHAR(50) NOT NULL UNIQUE,
--    DiscountPercentage INT NOT NULL,
--    MinOrderAmount DECIMAL(10,2) NULL,
--    ValidFrom DATETIME2(7) NOT NULL,
--    ValidTo DATETIME2(7) NOT NULL,
--    MaxUsage INT NULL,
--    UsageCount INT NOT NULL DEFAULT 0,
--    IsActive BIT NOT NULL DEFAULT 1,
--    CreatedAt DATETIME2(7) DEFAULT GETUTCDATE()
--);


--CREATE TABLE CancellationPolicies (
--    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
--    RefundPercentage INT NOT NULL,
--    CutoffHoursBeforeDeparture INT NOT NULL,
--    IsActive BIT NOT NULL DEFAULT 1,
--    CreatedAt DATETIME2(7) DEFAULT GETUTCDATE()
--);
