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



CREATE TABLE Buses (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    BusNumber NVARCHAR(20) NOT NULL UNIQUE,
    BusType NVARCHAR(50) NOT NULL,       -- e.g., Sleeper, Seater, AC, Non-AC
    TotalSeats INT NOT NULL,
    CreatedAt DATETIME2(7) DEFAULT GETUTCDATE()
);

CREATE TABLE Seats (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    BusId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Buses(Id),
    SeatNumber NVARCHAR(10) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Available',  
    CreatedAt DATETIME2(7) DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Seat_Bus UNIQUE (BusId, SeatNumber)
);



CREATE TABLE Routes (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Origin NVARCHAR(100) NOT NULL,
    Destination NVARCHAR(100) NOT NULL,
    DistanceKm INT NOT NULL,
    EstimatedTimeMinutes INT NOT NULL,
    CreatedAt DATETIME2(7) DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Route UNIQUE (Origin, Destination)
);



CREATE TABLE RecurringSchedules (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),

    RouteId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Routes(Id),
    BusId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Buses(Id),

    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Frequency NVARCHAR(50) NOT NULL,
    DepartureTime TIME NOT NULL,
    ArrivalTime TIME NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    NextRunDate DATE NOT NULL,
    CreatedAt DATETIME2(7) DEFAULT GETUTCDATE(),

    CONSTRAINT UQ_Recurring UNIQUE (BusId, StartDate, DepartureTime)
);

CREATE TABLE Schedules (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    RouteId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Routes(Id),
    BusId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Buses(Id),

    RecurringScheduleId UNIQUEIDENTIFIER NULL
        FOREIGN KEY REFERENCES RecurringSchedules(Id),

    DepartureTime DATETIME2(7) NOT NULL,
    ArrivalTime DATETIME2(7) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    CreatedAt DATETIME2(7) DEFAULT GETUTCDATE(),

    CONSTRAINT UQ_BusSchedule UNIQUE (BusId, DepartureTime)
);

ALTER TABLE Schedules
ADD Completed BIT NOT NULL DEFAULT 0;





--CREATE TABLE Bookings (
--    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
--    UserId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Users(Id),
--    ScheduleId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Schedules(Id),
--    BookingDate DATETIME2(7) DEFAULT GETUTCDATE(),
--    TotalAmount DECIMAL(10,2) NOT NULL,
--    BookingStatus NVARCHAR(50) DEFAULT 'Pending', -- Pending, Confirmed, Cancelled
--    PaymentStatus NVARCHAR(50) DEFAULT 'Pending', -- Paid, Failed
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
--    ValidFrom DATETIME2(7) NOT NULL,
--    ValidTo DATETIME2(7) NOT NULL,
--    MaxUsage INT NULL,
--    CreatedAt DATETIME2(7) DEFAULT GETUTCDATE()
--);


--CREATE TABLE CancellationPolicies (
--    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
--    ScheduleId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Schedules(Id),
--    RefundPercentage INT NOT NULL,       -- % refunded
--    CutoffHoursBeforeDeparture INT NOT NULL,
--    CreatedAt DATETIME2(7) DEFAULT GETUTCDATE()
--);
