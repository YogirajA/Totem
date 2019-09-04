MERGE INTO [ContractSchema] TargetTable
USING (VALUES ('Integer','{
	"type": "integer",
	"example": "123"
}','1.0'),('Integer(Int32)','{
	"type": "integer",
	"format": "int32",
	"example": "123"
}','1.0'),('Integer(Int64)','{
	"type": "integer",
	"format": "int64",
	"example": "4294967296"
}','1.0'),('String','{
	"type": "string",
	"example": "sample string"
}','1.0'),('Guid','{
	"type": "string",
	"maxLength": 36,
	"minLength": 36,
	"pattern": "^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$",
	"format": null,
	"example": "01234567-abcd-0123-abcd-0123456789ab",
	"reference": "Guid"
}','1.0'),('DateTime','{
	"type": "string",
	"format": "date-time",
	"example": "2019-01-01T18:14:29Z"
}','1.0')) AS PrimitiveTypes (SchemaName,SchemaString,VersionNumber)
ON TargetTable.SchemaName = PrimitiveTypes.SchemaName
WHEN NOT MATCHED THEN
INSERT (SchemaName,SchemaString,VersionNumber) VALUES (PrimitiveTypes.SchemaName, PrimitiveTypes.SchemaString, PrimitiveTypes.VersionNumber);