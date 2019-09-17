IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Contract]') 
         AND name = 'DisplayOnContractList')
BEGIN
    ALTER TABLE [Contract]
	ADD [DisplayOnContractList] BIT
	CONSTRAINT DF_Contract_DisplayOnContractList DEFAULT 1 NOT NULL
END