IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Contract')
BEGIN
    CREATE TABLE Contract
    (
        Id             UNIQUEIDENTIFIER NOT NULL,
        Description    NVARCHAR(MAX),
        ContractString NVARCHAR(MAX),
        VersionNumber  NVARCHAR(MAX),
        UpdateInst     DATETIME2        NOT NULL,
        CreatedDate    DATETIME2        NOT NULL,
        Namespace      NVARCHAR(MAX)    NOT NULL,
        Type           NVARCHAR(MAX)    NOT NULL,
        CONSTRAINT PK_Contract
            PRIMARY KEY (Id)
    )

    ALTER TABLE Contract ADD CONSTRAINT DF_Contract_Id DEFAULT NEWID() FOR Id

    ALTER TABLE Contract ADD CONSTRAINT DF_Contract_CreatedDate DEFAULT SYSUTCDATETIME() FOR CreatedDate

    ALTER TABLE Contract ADD CONSTRAINT DF_Contract_Namespace DEFAULT N'' FOR Namespace

    ALTER TABLE Contract ADD CONSTRAINT DF_Contract_Type DEFAULT N'' FOR Type

END;
