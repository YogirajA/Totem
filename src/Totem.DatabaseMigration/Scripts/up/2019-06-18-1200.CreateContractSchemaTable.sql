IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContractSchema')
BEGIN
    CREATE TABLE ContractSchema
    (
        Id             UNIQUEIDENTIFIER NOT NULL,
        SchemaName     NVARCHAR(MAX),
        SchemaString   NVARCHAR(MAX),
        VersionNumber  NVARCHAR(MAX),
        CreatedDate    DATETIME2	NOT NULL,
        CONSTRAINT	PK_ContractSchema
            PRIMARY KEY (Id)
    )

    ALTER TABLE ContractSchema ADD CONSTRAINT DF_ContractSchema_Id DEFAULT NEWID() FOR Id

    ALTER TABLE ContractSchema ADD CONSTRAINT DF_ContractSchema_CreatedDate DEFAULT SYSUTCDATETIME() FOR CreatedDate

END;
