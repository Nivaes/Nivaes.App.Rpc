CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Users" (
    "IdUser" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
    "Identification" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "GivenName" TEXT NOT NULL,
    "FamilyName" TEXT NOT NULL,
    "PhoneNumber" TEXT NULL,
    "Email" TEXT NULL,
    "TimeStamp" TEXT NOT NULL,
    "TimeStampTicks" INTEGER NOT NULL
);

CREATE INDEX "IX_Users_FamilyName" ON "Users" ("FamilyName");

CREATE INDEX "IX_Users_GivenName" ON "Users" ("GivenName");

CREATE INDEX "IX_Users_Name" ON "Users" ("Name");

CREATE INDEX "IX_Users_TimeStampTicks" ON "Users" ("TimeStampTicks");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260819175954_InitialCreate', '10.0.11');

COMMIT;

