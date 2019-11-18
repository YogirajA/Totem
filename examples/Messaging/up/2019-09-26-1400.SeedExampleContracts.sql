  IF NOT EXISTS (
  SELECT
    *
  FROM
    [dbo].[Contract]
  WHERE
    [Id] = N'7820D508-26F6-4E9E-A4D6-EBD3EF7ED47B'
) BEGIN INSERT [dbo].[Contract] (
	   [Id]
      ,[Description]
      ,[ContractString]
      ,[VersionNumber]
	  ,[UpdateInst]
      ,[Namespace]
      ,[Type]
      ,[DisplayOnContractList]
)
VALUES
  (
    N'7820D508-26F6-4E9E-A4D6-EBD3EF7ED47B',
    N'Sales Order',
    N'{"Contract":{"type":"object","properties":{"Id":{"$ref":"#/Guid","example":"01234567-abcd-0123-abcd-0123456789ab"},"Timestamp":{"type":"string","format":"date-time","example":"2019-01-01T18:14:29Z"},"ItemName":{"type":"string","example":"sample string"},"OrderDate":{"type":"string","format":"date-time","example":"2019-01-01T18:14:29Z"},"OrderId":{"type":"string","example":"sample string"}}},"Guid":{"type":"string","pattern":"^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$","minLength":36,"maxLength":36,"example":"01234567-abcd-0123-abcd-0123456789ab"}}',
    N'1.0.0',
	GETDATE(),
    N'Sales Order',
	N'Sales Order',
    1
  ) END