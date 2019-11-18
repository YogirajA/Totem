MERGE INTO [ContractSchema] TargetTable
USING (VALUES ('Number','{
	"type": "number",
	"example": "5.5"
}','1.0'),('Number(Float)','{
	"type": "number",
	"format": "float",
	"example": "10.5"
}','1.0'),('Number(Double)','{
	"type": "number",
	"format": "double",
	"example": "123456789012.34567"
}','1.0')) AS PrimitiveTypes (SchemaName,SchemaString,VersionNumber)
ON TargetTable.SchemaName = PrimitiveTypes.SchemaName
WHEN NOT MATCHED THEN
INSERT (SchemaName,SchemaString,VersionNumber) VALUES (PrimitiveTypes.SchemaName, PrimitiveTypes.SchemaString, PrimitiveTypes.VersionNumber);