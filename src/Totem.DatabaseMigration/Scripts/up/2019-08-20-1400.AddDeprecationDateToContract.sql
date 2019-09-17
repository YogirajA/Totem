IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Contract]') 
         AND name = 'DeprecationDate')
BEGIN
    ALTER TABLE [Contract]
	ADD [DeprecationDate] DATETIME2
END