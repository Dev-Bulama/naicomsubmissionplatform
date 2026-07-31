/*
    NLIP Postgres schema — generated directly from the real EF Core migration via:
        NLIP_MIGRATION_PROVIDER=Postgres dotnet ef migrations script --idempotent \
            -p src/NLIP.Persistence -s src/NLIP.API

    Used by the Render deployment path (docs/DEPLOYMENT.md). See docs/ROADMAP.md for why this
    migration was hand-assembled from `dotnet ef dbcontext script` output rather than scaffolded
    directly with `migrations add` (a confirmed EF Core 9.0.1 + Npgsql 9.0.4 differ bug, unrelated
    to this schema's correctness — dbcontext script and this script command both work fine).
*/

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731164601_PostgresInitialCreate') THEN
    CREATE TABLE "ActivityLogs" (
        "Id" uuid NOT NULL,
        "UserId" uuid,
        "Description" text NOT NULL,
        "Category" text NOT NULL,
        "Timestamp" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ActivityLogs" PRIMARY KEY ("Id")
    );


    CREATE TABLE "ApiCallLogs" (
        "Id" uuid NOT NULL,
        "CorrelationId" uuid NOT NULL,
        "Endpoint" text NOT NULL,
        "HttpMethod" text NOT NULL,
        "RequestHeaders" text,
        "RequestPayload" text,
        "ResponseHeaders" text,
        "ResponsePayload" text,
        "StatusCode" integer,
        "DurationMs" bigint NOT NULL,
        "IsSuccessful" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ApiCallLogs" PRIMARY KEY ("Id")
    );


    CREATE TABLE "AuditLogs" (
        "Id" uuid NOT NULL,
        "UserId" uuid,
        "UserName" text,
        "Action" text NOT NULL,
        "EntityName" text NOT NULL,
        "EntityId" text,
        "Details" text,
        "IpAddress" text,
        "Timestamp" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id")
    );


    CREATE TABLE "BackgroundJobRecords" (
        "Id" uuid NOT NULL,
        "HangfireJobId" text NOT NULL,
        "JobType" text NOT NULL,
        "Status" text NOT NULL,
        "EnqueuedAt" timestamp with time zone NOT NULL,
        "StartedAt" timestamp with time zone,
        "CompletedAt" timestamp with time zone,
        "Error" text,
        CONSTRAINT "PK_BackgroundJobRecords" PRIMARY KEY ("Id")
    );


    CREATE TABLE "Branches" (
        "Id" uuid NOT NULL,
        "Code" character varying(20) NOT NULL,
        "Name" text NOT NULL,
        "Address" text,
        "State" text,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "ModifiedAt" timestamp with time zone,
        "ModifiedBy" text,
        CONSTRAINT "PK_Branches" PRIMARY KEY ("Id")
    );


    CREATE TABLE "Customers" (
        "Id" uuid NOT NULL,
        "FirstName" character varying(100) NOT NULL,
        "LastName" character varying(100) NOT NULL,
        "MiddleName" text,
        "DateOfBirth" timestamp with time zone NOT NULL,
        "Gender" text NOT NULL,
        "NationalIdNumber" text,
        "Bvn" text,
        "PhoneNumber" text NOT NULL,
        "Email" text,
        "Address" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "ModifiedAt" timestamp with time zone,
        "ModifiedBy" text,
        CONSTRAINT "PK_Customers" PRIMARY KEY ("Id")
    );


    CREATE TABLE "Employers" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "RcNumber" text,
        "Address" text,
        "ContactPerson" text,
        "ContactEmail" text,
        "ContactPhone" text,
        "TaxIdentificationNumber" text,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "ModifiedAt" timestamp with time zone,
        "ModifiedBy" text,
        CONSTRAINT "PK_Employers" PRIMARY KEY ("Id")
    );


    CREATE TABLE "ErrorLogs" (
        "Id" uuid NOT NULL,
        "Source" text NOT NULL,
        "Message" text NOT NULL,
        "StackTrace" text,
        "CorrelationId" uuid,
        "Severity" text NOT NULL,
        "OccurredAt" timestamp with time zone NOT NULL,
        "Resolved" boolean NOT NULL,
        CONSTRAINT "PK_ErrorLogs" PRIMARY KEY ("Id")
    );


    CREATE TABLE "Notifications" (
        "Id" uuid NOT NULL,
        "UserId" uuid,
        "Title" text NOT NULL,
        "Message" text NOT NULL,
        "Channel" integer NOT NULL,
        "Status" integer NOT NULL,
        "RelatedPolicyId" uuid,
        "IsRead" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "SentAt" timestamp with time zone,
        CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id")
    );


    CREATE TABLE "Permissions" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" text,
        CONSTRAINT "PK_Permissions" PRIMARY KEY ("Id")
    );


    CREATE TABLE "Products" (
        "Id" uuid NOT NULL,
        "Code" character varying(20) NOT NULL,
        "Name" text NOT NULL,
        "BusinessType" integer NOT NULL,
        "Description" text,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "ModifiedAt" timestamp with time zone,
        "ModifiedBy" text,
        CONSTRAINT "PK_Products" PRIMARY KEY ("Id")
    );


    CREATE TABLE "Roles" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" text,
        "IsSystemRole" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "ModifiedAt" timestamp with time zone,
        "ModifiedBy" text,
        CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
    );


    CREATE TABLE "SystemSettings" (
        "Id" uuid NOT NULL,
        "Key" character varying(200) NOT NULL,
        "Value" text,
        "Description" text,
        "IsSecret" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "ModifiedAt" timestamp with time zone,
        "ModifiedBy" text,
        CONSTRAINT "PK_SystemSettings" PRIMARY KEY ("Id")
    );


    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "Username" character varying(100) NOT NULL,
        "Email" character varying(256) NOT NULL,
        "PasswordHash" text NOT NULL,
        "FullName" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "AccessFailedCount" integer NOT NULL,
        "LockoutEnd" timestamp with time zone,
        "MustChangePassword" boolean NOT NULL,
        "LastLoginAt" timestamp with time zone,
        "LastLoginIp" text,
        "MfaEnabled" boolean NOT NULL,
        "MfaSecret" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "ModifiedAt" timestamp with time zone,
        "ModifiedBy" text,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );


    CREATE TABLE "Agents" (
        "Id" uuid NOT NULL,
        "Code" character varying(20) NOT NULL,
        "FullName" text NOT NULL,
        "BranchId" uuid NOT NULL,
        "PhoneNumber" text,
        "Email" text,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "ModifiedAt" timestamp with time zone,
        "ModifiedBy" text,
        CONSTRAINT "PK_Agents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Agents_Branches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "Branches" ("Id") ON DELETE RESTRICT
    );


    CREATE TABLE "RolePermissions" (
        "Id" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        "PermissionId" uuid NOT NULL,
        CONSTRAINT "PK_RolePermissions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RolePermissions_Permissions_PermissionId" FOREIGN KEY ("PermissionId") REFERENCES "Permissions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RolePermissions_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
    );


    CREATE TABLE "RefreshTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Token" character varying(200) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "RevokedAt" timestamp with time zone,
        "ReplacedByToken" text,
        "CreatedByIp" text,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );


    CREATE TABLE "UserRoles" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        CONSTRAINT "PK_UserRoles" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_UserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserRoles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );


    CREATE TABLE "Policies" (
        "Id" uuid NOT NULL,
        "PolicyNumber" character varying(50) NOT NULL,
        "CorePolicyId" character varying(100) NOT NULL,
        "NaicomPolicyId" character varying(100),
        "BusinessType" integer NOT NULL,
        "Status" integer NOT NULL,
        "ProductId" uuid NOT NULL,
        "BranchId" uuid NOT NULL,
        "AgentId" uuid,
        "CustomerId" uuid,
        "EmployerId" uuid,
        "SumAssured" numeric(18,2) NOT NULL,
        "PremiumAmount" numeric(18,2) NOT NULL,
        "PremiumFrequency" character varying(20) NOT NULL,
        "CoverageStartDate" timestamp with time zone NOT NULL,
        "CoverageEndDate" timestamp with time zone NOT NULL,
        "CurrentSubmissionStatus" integer NOT NULL,
        "RetryCount" integer NOT NULL,
        "LastSubmittedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "ModifiedAt" timestamp with time zone,
        "ModifiedBy" text,
        CONSTRAINT "PK_Policies" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Policies_Agents_AgentId" FOREIGN KEY ("AgentId") REFERENCES "Agents" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Policies_Branches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "Branches" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Policies_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Policies_Employers_EmployerId" FOREIGN KEY ("EmployerId") REFERENCES "Employers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Policies_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
    );


    CREATE TABLE "GroupMembers" (
        "Id" uuid NOT NULL,
        "PolicyId" uuid NOT NULL,
        "EmployeeId" text NOT NULL,
        "FullName" text NOT NULL,
        "DateOfBirth" timestamp with time zone NOT NULL,
        "Gender" text NOT NULL,
        "SumAssured" numeric(18,2) NOT NULL,
        "Designation" text,
        "DateJoined" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "ModifiedAt" timestamp with time zone,
        "ModifiedBy" text,
        CONSTRAINT "PK_GroupMembers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_GroupMembers_Policies_PolicyId" FOREIGN KEY ("PolicyId") REFERENCES "Policies" ("Id") ON DELETE CASCADE
    );


    CREATE TABLE "NaicomTransactions" (
        "Id" uuid NOT NULL,
        "PolicyId" uuid NOT NULL,
        "Action" integer NOT NULL,
        "NaicomPolicyId" text,
        "CorrelationId" uuid NOT NULL,
        "RequestPayload" text NOT NULL,
        "ResponsePayload" text,
        "HttpStatusCode" integer,
        "IsSuccessful" boolean NOT NULL,
        "ErrorMessage" text,
        "DurationMs" bigint NOT NULL,
        "AttemptedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_NaicomTransactions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_NaicomTransactions_Policies_PolicyId" FOREIGN KEY ("PolicyId") REFERENCES "Policies" ("Id") ON DELETE CASCADE
    );


    CREATE TABLE "PolicyBeneficiaries" (
        "Id" uuid NOT NULL,
        "PolicyId" uuid NOT NULL,
        "FullName" text NOT NULL,
        "Relationship" text NOT NULL,
        "DateOfBirth" timestamp with time zone NOT NULL,
        "SharePercentage" numeric(18,2) NOT NULL,
        "PhoneNumber" text,
        "Email" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" text,
        "ModifiedAt" timestamp with time zone,
        "ModifiedBy" text,
        CONSTRAINT "PK_PolicyBeneficiaries" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PolicyBeneficiaries_Policies_PolicyId" FOREIGN KEY ("PolicyId") REFERENCES "Policies" ("Id") ON DELETE CASCADE
    );


    CREATE TABLE "PolicyHistories" (
        "Id" uuid NOT NULL,
        "PolicyId" uuid NOT NULL,
        "PreviousStatus" integer NOT NULL,
        "NewStatus" integer NOT NULL,
        "Notes" text,
        "ChangedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_PolicyHistories" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PolicyHistories_Policies_PolicyId" FOREIGN KEY ("PolicyId") REFERENCES "Policies" ("Id") ON DELETE CASCADE
    );


    CREATE TABLE "SubmissionQueue" (
        "Id" uuid NOT NULL,
        "PolicyId" uuid NOT NULL,
        "Action" integer NOT NULL,
        "Payload" text NOT NULL,
        "Status" integer NOT NULL,
        "RetryCount" integer NOT NULL,
        "MaxRetries" integer NOT NULL,
        "NextAttemptAt" timestamp with time zone,
        "LastAttemptAt" timestamp with time zone,
        "LastError" text,
        "CorrelationId" uuid NOT NULL,
        "HangfireJobId" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "ProcessedAt" timestamp with time zone,
        CONSTRAINT "PK_SubmissionQueue" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_SubmissionQueue_Policies_PolicyId" FOREIGN KEY ("PolicyId") REFERENCES "Policies" ("Id") ON DELETE CASCADE
    );


    CREATE INDEX "IX_ActivityLogs_Timestamp" ON "ActivityLogs" ("Timestamp");


    CREATE INDEX "IX_Agents_BranchId" ON "Agents" ("BranchId");


    CREATE UNIQUE INDEX "IX_Agents_Code" ON "Agents" ("Code");


    CREATE INDEX "IX_ApiCallLogs_CorrelationId" ON "ApiCallLogs" ("CorrelationId");


    CREATE INDEX "IX_ApiCallLogs_CreatedAt" ON "ApiCallLogs" ("CreatedAt");


    CREATE INDEX "IX_AuditLogs_EntityName_EntityId" ON "AuditLogs" ("EntityName", "EntityId");


    CREATE INDEX "IX_AuditLogs_Timestamp" ON "AuditLogs" ("Timestamp");


    CREATE UNIQUE INDEX "IX_Branches_Code" ON "Branches" ("Code");


    CREATE INDEX "IX_ErrorLogs_OccurredAt" ON "ErrorLogs" ("OccurredAt");


    CREATE INDEX "IX_GroupMembers_PolicyId" ON "GroupMembers" ("PolicyId");


    CREATE INDEX "IX_NaicomTransactions_AttemptedAt" ON "NaicomTransactions" ("AttemptedAt");


    CREATE INDEX "IX_NaicomTransactions_PolicyId" ON "NaicomTransactions" ("PolicyId");


    CREATE INDEX "IX_Notifications_CreatedAt" ON "Notifications" ("CreatedAt");


    CREATE INDEX "IX_Notifications_UserId" ON "Notifications" ("UserId");


    CREATE UNIQUE INDEX "IX_Permissions_Name" ON "Permissions" ("Name");


    CREATE INDEX "IX_Policies_AgentId" ON "Policies" ("AgentId");


    CREATE INDEX "IX_Policies_BranchId" ON "Policies" ("BranchId");


    CREATE INDEX "IX_Policies_BusinessType" ON "Policies" ("BusinessType");


    CREATE UNIQUE INDEX "IX_Policies_CorePolicyId" ON "Policies" ("CorePolicyId");


    CREATE INDEX "IX_Policies_CustomerId" ON "Policies" ("CustomerId");


    CREATE INDEX "IX_Policies_EmployerId" ON "Policies" ("EmployerId");


    CREATE INDEX "IX_Policies_NaicomPolicyId" ON "Policies" ("NaicomPolicyId");


    CREATE UNIQUE INDEX "IX_Policies_PolicyNumber" ON "Policies" ("PolicyNumber");


    CREATE INDEX "IX_Policies_ProductId" ON "Policies" ("ProductId");


    CREATE INDEX "IX_Policies_Status" ON "Policies" ("Status");


    CREATE INDEX "IX_PolicyBeneficiaries_PolicyId" ON "PolicyBeneficiaries" ("PolicyId");


    CREATE INDEX "IX_PolicyHistories_PolicyId" ON "PolicyHistories" ("PolicyId");


    CREATE UNIQUE INDEX "IX_Products_Code" ON "Products" ("Code");


    CREATE UNIQUE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens" ("Token");


    CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");


    CREATE INDEX "IX_RolePermissions_PermissionId" ON "RolePermissions" ("PermissionId");


    CREATE UNIQUE INDEX "IX_RolePermissions_RoleId_PermissionId" ON "RolePermissions" ("RoleId", "PermissionId");


    CREATE UNIQUE INDEX "IX_Roles_Name" ON "Roles" ("Name");


    CREATE INDEX "IX_SubmissionQueue_PolicyId" ON "SubmissionQueue" ("PolicyId");


    CREATE INDEX "IX_SubmissionQueue_Status_NextAttemptAt" ON "SubmissionQueue" ("Status", "NextAttemptAt");


    CREATE UNIQUE INDEX "IX_SystemSettings_Key" ON "SystemSettings" ("Key");


    CREATE INDEX "IX_UserRoles_RoleId" ON "UserRoles" ("RoleId");


    CREATE UNIQUE INDEX "IX_UserRoles_UserId_RoleId" ON "UserRoles" ("UserId", "RoleId");


    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");


    CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");



    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260731164601_PostgresInitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260731164601_PostgresInitialCreate', '9.0.1');
    END IF;
END $EF$;
COMMIT;

