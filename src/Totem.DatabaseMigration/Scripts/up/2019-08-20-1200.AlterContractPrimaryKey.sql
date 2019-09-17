UPDATE [Contract] SET [VersionNumber]='1.0' WHERE [VersionNumber] IS NULL

ALTER TABLE [Contract]
ALTER COLUMN [VersionNumber] NVARCHAR(40) NOT NULL

GO

ALTER TABLE [Contract]
DROP CONSTRAINT PK_Contract

GO

ALTER TABLE [Contract]
ADD CONSTRAINT PK_Contract PRIMARY KEY ([Id], [VersionNumber])