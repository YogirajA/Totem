using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fare;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Totem.Infrastructure;
using Totem.Models;

namespace Totem.Features.API
{
    public class SampleData
    {
        private static readonly Dictionary<string, string> JsonDictionary = new Dictionary<string, string>();

        public class Command : IRequest<Result>
        {
            public Guid ContractId { get; set; }
            public string VersionNumber { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, Result>
        {
            private readonly TotemContext _db;

            public CommandHandler(TotemContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var contract = await _db.Contract.SingleAsync(x => x.Id == request.ContractId && x.VersionNumber == request.VersionNumber,
                    cancellationToken);

                var schemaDictionary = SchemaObject.BuildSchemaDictionary(contract.ContractString, HandleError, HandleError);

                var sampleString = GenerateSampleString(schemaDictionary, schemaDictionary["Contract"].Properties);

                return new Result
                {
                    SampleData = sampleString
                };
            }

            private static string GetExample(CaseInsensitiveDictionary<SchemaObject> schemaDictionary, SchemaObject schemaObject)
            {
                string sampleString;
                string referenceKey = null;
                var dataType = schemaObject.GetDataType();

                if (!string.IsNullOrEmpty(schemaObject.Reference))
                {
                    referenceKey = schemaObject.Reference.Replace(@"#/", "");
                }

                // use the Example as the sample data if given
                if (dataType == DataType.Object)
                {
                    sampleString = GenerateSampleString(schemaDictionary, schemaObject.Properties);
                }
                else if (schemaObject.Example != null)
                {
                    if (dataType == DataType.String && schemaObject.GetFormat() == Format.DateTime)
                    {
                        var date = (DateTime)schemaObject.Example;
                        sampleString = $"\"{date:yyyy-MM-ddTHH:mm:ssZ}\"";
                    }
                    else if (dataType == DataType.Array)
                    {
                        var arrayExample = schemaObject.Example.ToString().Replace(Environment.NewLine, "");
                        if (schemaObject.Example.ToString()[0] != '[') // example of a single item instead of a list
                        {
                            arrayExample = $"[{arrayExample}]";
                        }

                        sampleString = arrayExample;
                    }
                    else if (dataType == DataType.String)
                    {
                        sampleString = $"\"{schemaObject.Example}\"";
                    }
                    else
                    {
                        sampleString = schemaObject.Example.ToString();
                    }
                }
                else if (!string.IsNullOrEmpty(referenceKey) && schemaDictionary[referenceKey].Example != null)
                {
                    sampleString = $"\"{schemaDictionary[referenceKey].Example}\"";
                }
                else
                {
                    var sampleValue = GenerateSampleData(schemaObject.GetDataType(), schemaObject.GetFormat(), schemaObject.Pattern, schemaObject.Properties,
                        schemaObject.Items, schemaObject.MinItems, schemaObject.MaxItems);
                    sampleString = sampleValue;
                }

                return sampleString;
            }

            private static string GenerateSampleString(CaseInsensitiveDictionary<SchemaObject> schemaDictionary, CaseInsensitiveDictionary<SchemaObject> properties)
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.Append("{");

                foreach (var item in properties.Select((property, index) => new {property, index}))
                {
                    var index = item.index;
                    var (propertyKey, propertySchemaObject) = item.property;

                    var exampleString = index == properties.Count - 1
                        ? $"{GetExample(schemaDictionary, propertySchemaObject)}"
                        : $"{GetExample(schemaDictionary, propertySchemaObject)}, ";

                    stringBuilder.Append($"\"{propertyKey}\": {exampleString}");
                }

                stringBuilder.Append("}");

                return stringBuilder.ToString();
            }
        }

        private static Result HandleError()
        {
            return new Result();
        }

        public static string GenerateSampleData(DataType dataType, Format format, string pattern = null, CaseInsensitiveDictionary<SchemaObject> properties = null, SchemaObject items = null, int minItems = 0, int maxItems = 0)
        {
            if (dataType == DataType.Integer)
            {
                return GenerateInteger(format);
            }

            if (dataType == DataType.Number)
            {
                return GenerateNumber(format);
            }

            if (dataType == DataType.Boolean)
            {
                return GenerateBoolean();
            }

            if (dataType == DataType.Array)
            {
                return items != null ? GenerateArray(dataType, items.GetFormat(), minItems, maxItems, items.Pattern) : "[]";
            }

            if (dataType == DataType.Object)
            {
                // Replace quotes inside nested JSON objects and remove slashes
                return GenerateObject(properties).Replace(@"\", "").Replace("\"{", "{").Replace("}\"", "}");
            }

            // Default is to treat as a string
            return GenerateString(format, pattern);
        }

        public static string GenerateObject(CaseInsensitiveDictionary<SchemaObject> properties)
        {
            var objectDictionary = new Dictionary<string, string>();

            if (properties == null)
            {
                return JsonConvert.SerializeObject(objectDictionary);
            }

            foreach (var (key, value) in properties)
            {
                var dataType = value.GetDataType();

                if (dataType == DataType.Integer)
                {
                    JsonDictionary[key] = GenerateInteger(value.GetFormat());
                }
                if (dataType == DataType.Number)
                {
                    JsonDictionary[key] = GenerateNumber(value.GetFormat());
                }
                if (dataType == DataType.Boolean)
                {
                    JsonDictionary[key] = GenerateBoolean();
                }
                if (dataType == DataType.String)
                {
                    //Removing extra quotes from string examples in nested objects
                    JsonDictionary[key] = GenerateString(value.GetFormat(), value.Pattern).Replace("\"", "");
                }
                if (dataType == DataType.Object)
                {
                    JsonDictionary[key] = GenerateObject(value.Properties);
                }
            }

            foreach (var key in properties.Keys)
            {
                if (JsonDictionary.ContainsKey(key))
                {
                    objectDictionary.Add(key, JsonDictionary[key].Replace(@"\", ""));
                }
            }

            return JsonConvert.SerializeObject(objectDictionary);
        }

        public static string GenerateString(Format format, string pattern)
        {
            if (format == Format.DateTime)
            {
                var currentDate = DateTime.Now;
                return $"\"{currentDate.ToString("s", CultureInfo.InvariantCulture)}\"";
            }

            if (pattern == null) return "\"String text\"";

            try
            {
                // Generate a string based on the pattern regex
                var regexGenerator = new Xeger(pattern, new Random());
                return $"\"{regexGenerator.Generate()}\"";
            }
            catch
            {
                return "\"Invalid pattern\"";
            }
        }

        public static string GenerateNumber(Format format)
        {
            if (format == Format.Float)
            {
                return "10.50";
            }
            if (format == Format.Double)
            {
                return "123456789012.34567";
            }

            return "5.5"; // format not included or unknown
        }

        public static string GenerateBoolean()
        {
            return "false";
        }

        public static string GenerateInteger(Format format)
        {
            if (format == Format.Int32)
            {
                return "5";
            }
            if (format == Format.Int64)
            {
                return "2147483650";
            }

            return "30"; // format not included or unknown
        }

        public static string GenerateArray(DataType dataType, Format itemFormat, int minItems, int maxItems, string pattern)
        {
            var length = GetArrayLength(minItems, maxItems);
            var returnArray = new List<string>();
            if (dataType == DataType.Integer)
            {
                for (var i = 0; i < length; i++)
                {
                    returnArray.Add(GenerateInteger(itemFormat));
                }
            }

            if (dataType == DataType.Number)
            {
                for (var i = 0; i < length; i++)
                {
                    returnArray.Add(GenerateNumber(itemFormat));
                }
            }

            if (dataType == DataType.Boolean)
            {
                for (var i = 0; i < length; i++)
                {
                    returnArray.Add(GenerateBoolean());
                }
            }

            if (dataType == DataType.String)
            {
                for (var i = 0; i < length; i++)
                {
                    returnArray.Add(GenerateString(itemFormat, pattern));
                }
            }

            var arrayString = $"[{string.Join(", ", returnArray.Select(x => $"{x}"))}]";
            return arrayString;
        }

        public class Result
        {
            public string SampleData { get; set; }
        }

        private static int GetArrayLength(int minItems, int maxItems)
        {
            return minItems != 0 && maxItems != 0 && minItems <= maxItems ? maxItems - minItems : 3;
        }
    }
}
