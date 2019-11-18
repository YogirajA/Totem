MERGE INTO [ContractSchema] TargetTable
USING (VALUES ('Boolean','{
	"type": "boolean",
	"example": false
}','1.0')) AS PrimitiveTypes (SchemaName,SchemaString,VersionNumber)
ON TargetTable.SchemaName = PrimitiveTypes.SchemaName
WHEN NOT MATCHED THEN
INSERT (SchemaName,SchemaString,VersionNumber) VALUES (PrimitiveTypes.SchemaName, PrimitiveTypes.SchemaString, PrimitiveTypes.VersionNumber);