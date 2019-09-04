using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Json;
using System.Linq;
using Newtonsoft.Json;
using Headspring;
using Totem.Infrastructure;

namespace Totem.Models
{
    public class SchemaObject
    {
        public string Type { get; set; }
        public CaseInsensitiveDictionary<SchemaObject> Properties { get; set; }
        public SchemaObject Items { get; set; }
        public int MaxItems { get; set; }
        public int MinItems { get; set; }
        public int MaxLength { get; set; }
        public int MinLength { get; set; }
        public string Pattern { get; set; }
        public string Format { get; set; }
        public object Example { get; set; }
        [JsonProperty(PropertyName = "$ref")]
        public string Reference { get; set; }

        public static SchemaObject ConvertJsonToSchema(string schema)
        {
            return JsonConvert.DeserializeObject<SchemaObject>(schema,
                new JsonSerializerSettings
                { // allows the $ref property to be parsed as data
                    MetadataPropertyHandling = MetadataPropertyHandling.Ignore
                });
        }

        public static CaseInsensitiveDictionary<SchemaObject> BuildSchemaDictionary<T>(string contractString, Func<T> handleRefError, Func<T> handleFailure)
        {
            var schemaDictionary = new CaseInsensitiveDictionary<SchemaObject>();

            try
            {
                var schemaObject = (JsonObject)JsonValue.Parse(contractString);
                foreach (var key in schemaObject.Keys)
                {
                    var contractObjectString = schemaObject[key].ToString();
                    var contractObject = ConvertJsonToSchema(contractObjectString);
                    schemaDictionary.Add(key, contractObject);
                }

                if (schemaDictionary["Contract"].Properties.Any(p => p.Value.Reference != null))
                { // References are used in the schema, so populate them
                    var populatedDictionary = PopulateReferences(schemaDictionary);
                    if (populatedDictionary.HasReferenceError)
                    {
                        handleRefError();
                    }
                    schemaDictionary = populatedDictionary.SchemaDictionary;
                }
            }
            catch
            {
                handleFailure();
            }

            return schemaDictionary;
        }

        public static PopulatedSchema PopulateReferences(CaseInsensitiveDictionary<SchemaObject> schema)
        {
            var propertyList = schema["Contract"].Properties.ToList();
            var error = false;
            foreach (var property in propertyList)
            {
                if (property.Value.Type != null) continue;
                if (property.Value.Reference == null)
                {
                    error = true;
                    continue;
                }
                var referenceName = ParseReferenceName(property.Value.Reference);
                schema.TryGetValue(referenceName, out var reference);

                if (reference == null)
                {
                    error = true;
                }
                else
                {
                    schema["Contract"].Properties[property.Key] = reference;
                    schema["Contract"].Properties[property.Key].Reference = referenceName;
                }
            }

            return new PopulatedSchema
            {
                SchemaDictionary = schema,
                HasReferenceError = error
            };
        }

        public static string ParseReferenceName(string reference)
        {
            // Expects format of "#/ReferenceName"
            return reference.Substring(2);
        }
    }

    public class DataType : Enumeration<DataType, string>
    {
        public static readonly DataType Object = new DataType("object", "Object");
        public static readonly DataType Integer = new DataType("integer", "Integer");
        public static readonly DataType Number = new DataType("number", "Number");
        public static readonly DataType String = new DataType("string", "String");
        public static readonly DataType Boolean = new DataType("boolean", "Boolean");
        public static readonly DataType Array = new DataType("array", "Array");

        private DataType(string value, string displayName) : base(value, displayName) { }
    }

    public class Format : Enumeration<Format, string>
    {
        // String Formats
        public static readonly Format Date = new Format("date", "Date");
        public static readonly Format DateTime = new Format("date-time", "DateTime");
        public static readonly Format Password = new Format("password", "Password");
        public static readonly Format Byte = new Format("byte", "Byte");
        public static readonly Format Binary = new Format("binary", "Binary");

        // Number/Integer Formats
        public static readonly Format Float = new Format("float", "Float");
        public static readonly Format Double = new Format("double", "Double");
        public static readonly Format Int32 = new Format("int32", "Int32");
        public static readonly Format Int64 = new Format("int64", "Int64");

        private Format(string value, string displayName) : base(value, displayName) { }
    }

    public class PopulatedSchema
    {
        public CaseInsensitiveDictionary<SchemaObject> SchemaDictionary { get; set; }
        public bool HasReferenceError { get; set; }
    }

    public class ContractSchema
    {
        public Guid Id { get; set; }
        public string SchemaName { get; set; }
        public string SchemaString { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDate { get; set; }
    }
}
