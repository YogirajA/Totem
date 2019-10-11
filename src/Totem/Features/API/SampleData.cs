using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private static Dictionary<string, string> _jsonDictionary = new Dictionary<string, string>();

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
                    cancellationToken: cancellationToken);

                var sampleString = "{";
                var schemaDictionary = SchemaObject.BuildSchemaDictionary(contract.ContractString, HandleError, HandleError);

                foreach (var kv in schemaDictionary["Contract"].Properties.Select((value, idx) => new { idx, value}))
                {
                    var exampleString = kv.idx == schemaDictionary["Contract"].Properties.Count - 1 ? $"{GetExample(kv.value, sampleString, schemaDictionary)}" : $"{GetExample(kv.value, sampleString, schemaDictionary)}, ";
                    sampleString += $"\"{kv.value.Key}\": {exampleString}";
                }

                sampleString += "}";

                return new Result
                {
                    SampleData = sampleString
                };
            }

            private static string GetExample(KeyValuePair<string, SchemaObject> kv, string sampleString, CaseInsensitiveDictionary<SchemaObject> schemaDictionary)
            {
                string referenceKey = null;
                if (!string.IsNullOrEmpty(kv.Value.Reference))
                {
                    referenceKey = kv.Value.Reference.Replace(@"#/", "");
                }

                if (kv.Value.Example != null || kv.Value.Type.EqualsCaseInsensitive(DataType.Object.Value))
                {
                    // use the Example as the sample data if given
                    if (kv.Value.Type.EqualsCaseInsensitive(DataType.Object.Value))
                    {
                        var nestedSampleString = "{";
                        foreach (var nestedProp in kv.Value.Properties.Select((value, idx) => new { idx, value }))
                        {
                            var exampleString = nestedProp.idx == kv.Value.Properties.Count - 1 ? $"{GetExample(nestedProp.value, sampleString, schemaDictionary)}" : $"{GetExample(nestedProp.value, nestedSampleString, schemaDictionary)}, ";
                            nestedSampleString += $"\"{nestedProp.value.Key}\": {exampleString}";
                        }

                        nestedSampleString += "}";
                        sampleString = nestedSampleString;
                    }
                    else if (kv.Value.Type.EqualsCaseInsensitive(DataType.String.Value) &&
                        kv.Value.Format.EqualsCaseInsensitive(Format.DateTime.Value))
                    {
                        var date = (DateTime) kv.Value.Example;
                        sampleString = $"\"{date:yyyy-MM-ddTHH:mm:ssZ}\"";
                    }
                    else if (kv.Value.Type.EqualsCaseInsensitive(DataType.Array.Value))
                    {
                        var arrayExample = kv.Value.Example.ToString().Replace(System.Environment.NewLine, "");
                        if (kv.Value.Example.ToString()[0] != '[') // example of a single item instead of a list
                        {
                            arrayExample = $"[{arrayExample}]";
                        }

                        sampleString = arrayExample.ToString();
                    }
                    else if (kv.Value.Type.EqualsCaseInsensitive(DataType.String.Value))
                    {
                        sampleString = $"\"{kv.Value.Example}\"";
                    }
                    else
                    {
                        sampleString = kv.Value.Example.ToString();
                    }
                }
                else if (!string.IsNullOrEmpty(referenceKey) && schemaDictionary[referenceKey].Example != null)
                {
                    sampleString = $"\"{schemaDictionary[referenceKey].Example}\"";
                }
                else
                {
                    var sampleValue = GenerateSampleData(kv.Value.Type, kv.Value.Format, kv.Value.Pattern, kv.Value.Properties,
                        items: kv.Value.Items, minItems: kv.Value.MinItems, maxItems: kv.Value.MaxItems);
                    sampleString = sampleValue;
                }

                return sampleString;
            }
        }

        private static Result HandleError()
        {
            return new Result();
        }

        public static string GenerateSampleData(string type, string format, string pattern = null, CaseInsensitiveDictionary<SchemaObject> properties = null, SchemaObject items = null, int minItems = 0, int maxItems = 0)
        {
            if (type.EqualsCaseInsensitive(DataType.Integer.Value))
            {
                return GenerateInteger(format);
            }

            if (type.EqualsCaseInsensitive(DataType.Number.Value))
            {
                return GenerateNumber(format);
            }

            if (type.EqualsCaseInsensitive(DataType.Array.Value))
            {
                if (items != null)
                    return GenerateArray(items.Type, items.Format, minItems, maxItems, items.Pattern);
                return "[]";
            }

            if (type.EqualsCaseInsensitive(DataType.Object.Value))
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
            if (properties != null)
            {
                foreach (var (key, value) in properties)
                {
                    if (value.Type.EqualsCaseInsensitive(DataType.Integer.Value))
                    {
                        _jsonDictionary[key] = GenerateInteger(value.Format);
                    }
                    if (value.Type.EqualsCaseInsensitive(DataType.Number.Value))
                    {
                        _jsonDictionary[key] = GenerateNumber(value.Format);
                    }
                    if (value.Type.EqualsCaseInsensitive(DataType.String.Value))
                    {
                        //Removing extra quotes from string examples in nested objects
                        _jsonDictionary[key] = GenerateString(value.Format, value.Pattern).Replace("\"", ""); ;
                    }
                    if (value.Type.EqualsCaseInsensitive(DataType.Object.Value))
                    {
                        _jsonDictionary[key] = GenerateObject(value.Properties);
                    }
                }

                foreach (var (key, _) in properties)
                {
                    if (_jsonDictionary.ContainsKey(key))
                    {
                        objectDictionary.Add(key, _jsonDictionary[key].Replace(@"\", ""));
                    }
                }
            }

            return JsonConvert.SerializeObject(objectDictionary);
        }

        public static string GenerateString(string format, string pattern)
        {
            if (format != null && format.EqualsCaseInsensitive(Format.DateTime.Value))
            {
                var currentDate = DateTime.Now;
                return $"\"{currentDate.ToString("s", CultureInfo.InvariantCulture)}\"";
            }

            if (pattern == null) return "\"String text\"";

            try
            {
                // Generate a string based on the pattern regex
                Xeger regexGenerator = new Xeger(pattern, new Random());
                return $"\"{regexGenerator.Generate()}\"";
            }
            catch
            {
                return "\"Invalid pattern\"";
            }
        }

        public static string GenerateNumber(string format)
        {
            if (format != null && format.EqualsCaseInsensitive(Format.Float.Value))
            {
                return "10.50";
            }
            if (format != null && format.EqualsCaseInsensitive(Format.Double.Value))
            {
                return "123456789012.34567";
            }

            return "5.5"; // format not included or unknown
        }

        public static string GenerateInteger(string format)
        {
            if (format != null && format.EqualsCaseInsensitive(Format.Int32.Value))
            {
                return "5";
            }
            if (format != null && format.EqualsCaseInsensitive(Format.Int64.Value))
            {
                return "2147483650";
            }

            return "30"; // format not included or unknown
        }

        public static string GenerateArray(string itemType, string itemFormat, int minItems, int maxItems, string pattern)
        {
            var length = GetArrayLength(minItems, maxItems);
            var returnArray = new List<string>();
            if (itemType.EqualsCaseInsensitive(DataType.Integer.Value))
            {
                for (var i = 0; i < length; i++)
                {
                    returnArray.Add(GenerateInteger(itemFormat));
                }
            }

            if (itemType.EqualsCaseInsensitive(DataType.Number.Value))
            {
                for (var i = 0; i < length; i++)
                {
                    returnArray.Add(GenerateNumber(itemFormat));
                }
            }

            if (itemType.EqualsCaseInsensitive(DataType.String.Value))
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
