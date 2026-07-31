/*
    NLIP SQL Server schema — generated directly from the real EF Core migration via:
        dotnet ef migrations script --idempotent -p src/NLIP.Persistence -s src/NLIP.API

    This is the actual DDL src/NLIP.Persistence/Migrations/SqlServer/*_InitialCreate.cs produces,
    not a hand-maintained approximation — regenerate it with the command above any time the model
    changes and a new migration is added, so it never drifts from what the app really runs.
    Provided for DBAs who want to review/provision the schema without running the app; NLIP.API
    itself applies migrations directly via Database.MigrateAsync() at startup (see Program.cs),
    it does not read this file.

    For the Postgres deployment path (Render — see docs/DEPLOYMENT.md), the equivalent script is:
        NLIP_MIGRATION_PROVIDER=Postgres dotnet ef migrations script --idempotent \
            -p src/NLIP.Persistence -s src/NLIP.API
*/

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [ActivityLogs] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [Description] nvarchar(max) NOT NULL,
        [Category] nvarchar(max) NOT NULL,
        [Timestamp] datetimeoffset NOT NULL,
        CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [ApiCallLogs] (
        [Id] uniqueidentifier NOT NULL,
        [CorrelationId] uniqueidentifier NOT NULL,
        [Endpoint] nvarchar(max) NOT NULL,
        [HttpMethod] nvarchar(max) NOT NULL,
        [RequestHeaders] nvarchar(max) NULL,
        [RequestPayload] nvarchar(max) NULL,
        [ResponseHeaders] nvarchar(max) NULL,
        [ResponsePayload] nvarchar(max) NULL,
        [StatusCode] int NULL,
        [DurationMs] bigint NOT NULL,
        [IsSuccessful] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_ApiCallLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [UserName] nvarchar(max) NULL,
        [Action] nvarchar(max) NOT NULL,
        [EntityName] nvarchar(450) NOT NULL,
        [EntityId] nvarchar(450) NULL,
        [Details] nvarchar(max) NULL,
        [IpAddress] nvarchar(max) NULL,
        [Timestamp] datetimeoffset NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [BackgroundJobRecords] (
        [Id] uniqueidentifier NOT NULL,
        [HangfireJobId] nvarchar(max) NOT NULL,
        [JobType] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [EnqueuedAt] datetimeoffset NOT NULL,
        [StartedAt] datetimeoffset NULL,
        [CompletedAt] datetimeoffset NULL,
        [Error] nvarchar(max) NULL,
        CONSTRAINT [PK_BackgroundJobRecords] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [Branches] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NULL,
        [State] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Branches] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [Customers] (
        [Id] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [MiddleName] nvarchar(max) NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [Gender] nvarchar(max) NOT NULL,
        [NationalIdNumber] nvarchar(max) NULL,
        [Bvn] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [Employers] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [RcNumber] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [ContactPerson] nvarchar(max) NULL,
        [ContactEmail] nvarchar(max) NULL,
        [ContactPhone] nvarchar(max) NULL,
        [TaxIdentificationNumber] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Employers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [ErrorLogs] (
        [Id] uniqueidentifier NOT NULL,
        [Source] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [StackTrace] nvarchar(max) NULL,
        [CorrelationId] uniqueidentifier NULL,
        [Severity] nvarchar(max) NOT NULL,
        [OccurredAt] datetimeoffset NOT NULL,
        [Resolved] bit NOT NULL,
        CONSTRAINT [PK_ErrorLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [Title] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [Channel] int NOT NULL,
        [Status] int NOT NULL,
        [RelatedPolicyId] uniqueidentifier NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [SentAt] datetimeoffset NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [Permissions] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [BusinessType] int NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsSystemRole] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [SystemSettings] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(200) NOT NULL,
        [Value] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [IsSecret] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Username] nvarchar(100) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [MustChangePassword] bit NOT NULL,
        [LastLoginAt] datetimeoffset NULL,
        [LastLoginIp] nvarchar(max) NULL,
        [MfaEnabled] bit NOT NULL,
        [MfaSecret] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [Agents] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Agents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Agents_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [Id] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        [PermissionId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Token] nvarchar(200) NOT NULL,
        [ExpiresAt] datetimeoffset NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [RevokedAt] datetimeoffset NULL,
        [ReplacedByToken] nvarchar(max) NULL,
        [CreatedByIp] nvarchar(max) NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [Policies] (
        [Id] uniqueidentifier NOT NULL,
        [PolicyNumber] nvarchar(50) NOT NULL,
        [CorePolicyId] nvarchar(100) NOT NULL,
        [NaicomPolicyId] nvarchar(100) NULL,
        [BusinessType] int NOT NULL,
        [Status] int NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [AgentId] uniqueidentifier NULL,
        [CustomerId] uniqueidentifier NULL,
        [EmployerId] uniqueidentifier NULL,
        [SumAssured] decimal(18,2) NOT NULL,
        [PremiumAmount] decimal(18,2) NOT NULL,
        [PremiumFrequency] nvarchar(20) NOT NULL,
        [CoverageStartDate] datetime2 NOT NULL,
        [CoverageEndDate] datetime2 NOT NULL,
        [CurrentSubmissionStatus] int NOT NULL,
        [RetryCount] int NOT NULL,
        [LastSubmittedAt] datetimeoffset NULL,
        [RowVersion] rowversion NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_Policies] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Policies_Agents_AgentId] FOREIGN KEY ([AgentId]) REFERENCES [Agents] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Policies_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Policies_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Policies_Employers_EmployerId] FOREIGN KEY ([EmployerId]) REFERENCES [Employers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Policies_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [GroupMembers] (
        [Id] uniqueidentifier NOT NULL,
        [PolicyId] uniqueidentifier NOT NULL,
        [EmployeeId] nvarchar(max) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [Gender] nvarchar(max) NOT NULL,
        [SumAssured] decimal(18,2) NOT NULL,
        [Designation] nvarchar(max) NULL,
        [DateJoined] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_GroupMembers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroupMembers_Policies_PolicyId] FOREIGN KEY ([PolicyId]) REFERENCES [Policies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [NaicomTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [PolicyId] uniqueidentifier NOT NULL,
        [Action] int NOT NULL,
        [NaicomPolicyId] nvarchar(max) NULL,
        [CorrelationId] uniqueidentifier NOT NULL,
        [RequestPayload] nvarchar(max) NOT NULL,
        [ResponsePayload] nvarchar(max) NULL,
        [HttpStatusCode] int NULL,
        [IsSuccessful] bit NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [DurationMs] bigint NOT NULL,
        [AttemptedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_NaicomTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NaicomTransactions_Policies_PolicyId] FOREIGN KEY ([PolicyId]) REFERENCES [Policies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [PolicyBeneficiaries] (
        [Id] uniqueidentifier NOT NULL,
        [PolicyId] uniqueidentifier NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [Relationship] nvarchar(max) NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [SharePercentage] decimal(18,2) NOT NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_PolicyBeneficiaries] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PolicyBeneficiaries_Policies_PolicyId] FOREIGN KEY ([PolicyId]) REFERENCES [Policies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [PolicyHistories] (
        [Id] uniqueidentifier NOT NULL,
        [PolicyId] uniqueidentifier NOT NULL,
        [PreviousStatus] int NOT NULL,
        [NewStatus] int NOT NULL,
        [Notes] nvarchar(max) NULL,
        [ChangedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_PolicyHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PolicyHistories_Policies_PolicyId] FOREIGN KEY ([PolicyId]) REFERENCES [Policies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE TABLE [SubmissionQueue] (
        [Id] uniqueidentifier NOT NULL,
        [PolicyId] uniqueidentifier NOT NULL,
        [Action] int NOT NULL,
        [Payload] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [RetryCount] int NOT NULL,
        [MaxRetries] int NOT NULL,
        [NextAttemptAt] datetimeoffset NULL,
        [LastAttemptAt] datetimeoffset NULL,
        [LastError] nvarchar(max) NULL,
        [CorrelationId] uniqueidentifier NOT NULL,
        [HangfireJobId] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [ProcessedAt] datetimeoffset NULL,
        CONSTRAINT [PK_SubmissionQueue] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SubmissionQueue_Policies_PolicyId] FOREIGN KEY ([PolicyId]) REFERENCES [Policies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ActivityLogs_Timestamp] ON [ActivityLogs] ([Timestamp]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Agents_BranchId] ON [Agents] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Agents_Code] ON [Agents] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ApiCallLogs_CorrelationId] ON [ApiCallLogs] ([CorrelationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ApiCallLogs_CreatedAt] ON [ApiCallLogs] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityName_EntityId] ON [AuditLogs] ([EntityName], [EntityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_Timestamp] ON [AuditLogs] ([Timestamp]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Branches_Code] ON [Branches] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ErrorLogs_OccurredAt] ON [ErrorLogs] ([OccurredAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GroupMembers_PolicyId] ON [GroupMembers] ([PolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NaicomTransactions_AttemptedAt] ON [NaicomTransactions] ([AttemptedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NaicomTransactions_PolicyId] ON [NaicomTransactions] ([PolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_CreatedAt] ON [Notifications] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissions_Name] ON [Permissions] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_AgentId] ON [Policies] ([AgentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_BranchId] ON [Policies] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_BusinessType] ON [Policies] ([BusinessType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Policies_CorePolicyId] ON [Policies] ([CorePolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_CustomerId] ON [Policies] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_EmployerId] ON [Policies] ([EmployerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_NaicomPolicyId] ON [Policies] ([NaicomPolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Policies_PolicyNumber] ON [Policies] ([PolicyNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_ProductId] ON [Policies] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Policies_Status] ON [Policies] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PolicyBeneficiaries_PolicyId] ON [PolicyBeneficiaries] ([PolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PolicyHistories_PolicyId] ON [PolicyHistories] ([PolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_Code] ON [Products] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RolePermissions_RoleId_PermissionId] ON [RolePermissions] ([RoleId], [PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Name] ON [Roles] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SubmissionQueue_PolicyId] ON [SubmissionQueue] ([PolicyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SubmissionQueue_Status_NextAttemptAt] ON [SubmissionQueue] ([Status], [NextAttemptAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SystemSettings_Key] ON [SystemSettings] ([Key]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserRoles_UserId_RoleId] ON [UserRoles] ([UserId], [RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731163711_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731163711_InitialCreate', N'9.0.1');
END;

COMMIT;
GO

