/*
    NLIP reference schema (hand-authored).

    This mirrors the EF Core model in src/NLIP.Persistence/Configurations as of this scaffold, for
    DBAs who want to review/provision the schema without running the app, and as a sanity check
    against the real migration once generated. It is NOT applied by the application at runtime —
    NLIP.API applies EF Core migrations via Database.MigrateAsync() on startup (see Program.cs).

    IMPORTANT: no EF Core migration exists yet in this repository (the sandbox this scaffold was
    built in has no .NET SDK, so `dotnet ef migrations add` could not be run). Before first deploy:
        dotnet tool install --global dotnet-ef
        dotnet ef migrations add InitialCreate -p src/NLIP.Persistence -s src/NLIP.API
    Then diff the generated migration's Up() against this file and reconcile any drift.
*/

SET NOCOUNT ON;
GO

IF DB_ID('NlipDb') IS NULL
BEGIN
    PRINT 'Run this script while connected to the NlipDb database (create it first: CREATE DATABASE NlipDb;)';
END
GO

-- ===================== Master data =====================

CREATE TABLE Branches (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(20) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Address NVARCHAR(500) NULL,
    State NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy NVARCHAR(200) NULL,
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(200) NULL,
    CONSTRAINT UQ_Branches_Code UNIQUE (Code)
);

CREATE TABLE Agents (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(20) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    BranchId UNIQUEIDENTIFIER NOT NULL REFERENCES Branches(Id),
    PhoneNumber NVARCHAR(50) NULL,
    Email NVARCHAR(256) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy NVARCHAR(200) NULL,
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(200) NULL,
    CONSTRAINT UQ_Agents_Code UNIQUE (Code)
);

CREATE TABLE Employers (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    RcNumber NVARCHAR(50) NULL,
    Address NVARCHAR(500) NULL,
    ContactPerson NVARCHAR(200) NULL,
    ContactEmail NVARCHAR(256) NULL,
    ContactPhone NVARCHAR(50) NULL,
    TaxIdentificationNumber NVARCHAR(50) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy NVARCHAR(200) NULL,
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(200) NULL
);

CREATE TABLE Customers (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    MiddleName NVARCHAR(100) NULL,
    DateOfBirth DATETIME2 NOT NULL,
    Gender NVARCHAR(20) NOT NULL,
    NationalIdNumber NVARCHAR(50) NULL,
    Bvn NVARCHAR(20) NULL,
    PhoneNumber NVARCHAR(50) NOT NULL,
    Email NVARCHAR(256) NULL,
    Address NVARCHAR(500) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy NVARCHAR(200) NULL,
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(200) NULL
);

CREATE TABLE Products (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(20) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    BusinessType INT NOT NULL, -- 1 = IndividualLife, 2 = GroupLife
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy NVARCHAR(200) NULL,
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(200) NULL,
    CONSTRAINT UQ_Products_Code UNIQUE (Code)
);
GO

-- ===================== Identity =====================

CREATE TABLE Users (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    AccessFailedCount INT NOT NULL DEFAULT 0,
    LockoutEnd DATETIMEOFFSET NULL,
    MustChangePassword BIT NOT NULL DEFAULT 0,
    LastLoginAt DATETIMEOFFSET NULL,
    LastLoginIp NVARCHAR(50) NULL,
    MfaEnabled BIT NOT NULL DEFAULT 0,
    MfaSecret NVARCHAR(200) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy NVARCHAR(200) NULL,
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(200) NULL,
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

CREATE TABLE Roles (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsSystemRole BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy NVARCHAR(200) NULL,
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(200) NULL,
    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);

CREATE TABLE Permissions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    CONSTRAINT UQ_Permissions_Name UNIQUE (Name)
);

CREATE TABLE RolePermissions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    RoleId UNIQUEIDENTIFIER NOT NULL REFERENCES Roles(Id) ON DELETE CASCADE,
    PermissionId UNIQUEIDENTIFIER NOT NULL REFERENCES Permissions(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_RolePermissions UNIQUE (RoleId, PermissionId)
);

CREATE TABLE UserRoles (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    RoleId UNIQUEIDENTIFIER NOT NULL REFERENCES Roles(Id),
    CONSTRAINT UQ_UserRoles UNIQUE (UserId, RoleId)
);

CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Token NVARCHAR(200) NOT NULL,
    ExpiresAt DATETIMEOFFSET NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    RevokedAt DATETIMEOFFSET NULL,
    ReplacedByToken NVARCHAR(200) NULL,
    CreatedByIp NVARCHAR(50) NULL,
    CONSTRAINT UQ_RefreshTokens_Token UNIQUE (Token)
);
GO

-- ===================== Policies =====================

CREATE TABLE Policies (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    PolicyNumber NVARCHAR(50) NOT NULL,
    CorePolicyId NVARCHAR(100) NOT NULL,
    NaicomPolicyId NVARCHAR(100) NULL,
    BusinessType INT NOT NULL,
    Status INT NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL REFERENCES Products(Id),
    BranchId UNIQUEIDENTIFIER NOT NULL REFERENCES Branches(Id),
    AgentId UNIQUEIDENTIFIER NULL REFERENCES Agents(Id),
    CustomerId UNIQUEIDENTIFIER NULL REFERENCES Customers(Id),
    EmployerId UNIQUEIDENTIFIER NULL REFERENCES Employers(Id),
    SumAssured DECIMAL(18,2) NOT NULL,
    PremiumAmount DECIMAL(18,2) NOT NULL,
    PremiumFrequency NVARCHAR(20) NOT NULL,
    CoverageStartDate DATETIME2 NOT NULL,
    CoverageEndDate DATETIME2 NOT NULL,
    CurrentSubmissionStatus INT NOT NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    LastSubmittedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy NVARCHAR(200) NULL,
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(200) NULL,
    CONSTRAINT UQ_Policies_PolicyNumber UNIQUE (PolicyNumber),
    CONSTRAINT UQ_Policies_CorePolicyId UNIQUE (CorePolicyId)
);
CREATE INDEX IX_Policies_NaicomPolicyId ON Policies(NaicomPolicyId);
CREATE INDEX IX_Policies_Status ON Policies(Status);
CREATE INDEX IX_Policies_BusinessType ON Policies(BusinessType);

CREATE TABLE PolicyBeneficiaries (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    PolicyId UNIQUEIDENTIFIER NOT NULL REFERENCES Policies(Id) ON DELETE CASCADE,
    FullName NVARCHAR(200) NOT NULL,
    Relationship NVARCHAR(50) NOT NULL,
    DateOfBirth DATETIME2 NOT NULL,
    SharePercentage DECIMAL(18,2) NOT NULL,
    PhoneNumber NVARCHAR(50) NULL,
    Email NVARCHAR(256) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy NVARCHAR(200) NULL,
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(200) NULL
);

CREATE TABLE GroupMembers (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    PolicyId UNIQUEIDENTIFIER NOT NULL REFERENCES Policies(Id) ON DELETE CASCADE,
    EmployeeId NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    DateOfBirth DATETIME2 NOT NULL,
    Gender NVARCHAR(20) NOT NULL,
    SumAssured DECIMAL(18,2) NOT NULL,
    Designation NVARCHAR(100) NULL,
    DateJoined DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy NVARCHAR(200) NULL,
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(200) NULL
);

CREATE TABLE PolicyHistories (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    PolicyId UNIQUEIDENTIFIER NOT NULL REFERENCES Policies(Id) ON DELETE CASCADE,
    PreviousStatus INT NOT NULL,
    NewStatus INT NOT NULL,
    Notes NVARCHAR(1000) NULL,
    ChangedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
GO

-- ===================== Integration / queue / logs =====================

CREATE TABLE NaicomTransactions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    PolicyId UNIQUEIDENTIFIER NOT NULL REFERENCES Policies(Id) ON DELETE CASCADE,
    Action INT NOT NULL,
    NaicomPolicyId NVARCHAR(100) NULL,
    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    RequestPayload NVARCHAR(MAX) NOT NULL,
    ResponsePayload NVARCHAR(MAX) NULL,
    HttpStatusCode INT NULL,
    IsSuccessful BIT NOT NULL,
    ErrorMessage NVARCHAR(2000) NULL,
    DurationMs BIGINT NOT NULL,
    AttemptedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE INDEX IX_NaicomTransactions_PolicyId ON NaicomTransactions(PolicyId);
CREATE INDEX IX_NaicomTransactions_AttemptedAt ON NaicomTransactions(AttemptedAt);

CREATE TABLE SubmissionQueue (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    PolicyId UNIQUEIDENTIFIER NOT NULL REFERENCES Policies(Id) ON DELETE CASCADE,
    Action INT NOT NULL,
    Payload NVARCHAR(MAX) NOT NULL,
    Status INT NOT NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 6,
    NextAttemptAt DATETIMEOFFSET NULL,
    LastAttemptAt DATETIMEOFFSET NULL,
    LastError NVARCHAR(2000) NULL,
    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    HangfireJobId NVARCHAR(100) NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    ProcessedAt DATETIMEOFFSET NULL
);
CREATE INDEX IX_SubmissionQueue_Status_NextAttempt ON SubmissionQueue(Status, NextAttemptAt);
CREATE INDEX IX_SubmissionQueue_PolicyId ON SubmissionQueue(PolicyId);

CREATE TABLE ApiCallLogs (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    Endpoint NVARCHAR(500) NOT NULL,
    HttpMethod NVARCHAR(10) NOT NULL,
    RequestHeaders NVARCHAR(MAX) NULL,
    RequestPayload NVARCHAR(MAX) NULL,
    ResponseHeaders NVARCHAR(MAX) NULL,
    ResponsePayload NVARCHAR(MAX) NULL,
    StatusCode INT NULL,
    DurationMs BIGINT NOT NULL,
    IsSuccessful BIT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE INDEX IX_ApiCallLogs_CorrelationId ON ApiCallLogs(CorrelationId);
CREATE INDEX IX_ApiCallLogs_CreatedAt ON ApiCallLogs(CreatedAt);

CREATE TABLE BackgroundJobRecords (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    HangfireJobId NVARCHAR(100) NOT NULL,
    JobType NVARCHAR(200) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    EnqueuedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    StartedAt DATETIMEOFFSET NULL,
    CompletedAt DATETIMEOFFSET NULL,
    Error NVARCHAR(2000) NULL
);

CREATE TABLE AuditLogs (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NULL,
    UserName NVARCHAR(100) NULL,
    Action NVARCHAR(200) NOT NULL,
    EntityName NVARCHAR(200) NOT NULL,
    EntityId NVARCHAR(100) NULL,
    Details NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(50) NULL,
    Timestamp DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
CREATE INDEX IX_AuditLogs_Entity ON AuditLogs(EntityName, EntityId);

CREATE TABLE ActivityLogs (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NULL,
    Description NVARCHAR(500) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Timestamp DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
CREATE INDEX IX_ActivityLogs_Timestamp ON ActivityLogs(Timestamp);

CREATE TABLE ErrorLogs (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Source NVARCHAR(200) NOT NULL,
    Message NVARCHAR(2000) NOT NULL,
    StackTrace NVARCHAR(MAX) NULL,
    CorrelationId UNIQUEIDENTIFIER NULL,
    Severity NVARCHAR(20) NOT NULL DEFAULT 'Error',
    OccurredAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    Resolved BIT NOT NULL DEFAULT 0
);
CREATE INDEX IX_ErrorLogs_OccurredAt ON ErrorLogs(OccurredAt);

CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    Channel INT NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    RelatedPolicyId UNIQUEIDENTIFIER NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    SentAt DATETIMEOFFSET NULL
);
CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt);

CREATE TABLE SystemSettings (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Key] NVARCHAR(200) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    Description NVARCHAR(500) NULL,
    IsSecret BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy NVARCHAR(200) NULL,
    ModifiedAt DATETIMEOFFSET NULL,
    ModifiedBy NVARCHAR(200) NULL,
    CONSTRAINT UQ_SystemSettings_Key UNIQUE ([Key])
);
GO

PRINT 'NLIP reference schema created. Remember: generate the real EF Core migration before relying on this in production — see the header comment.';
