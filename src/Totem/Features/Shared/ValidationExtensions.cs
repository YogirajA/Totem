using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using Totem.Infrastructure;
using Totem.Models;
using Microsoft.EntityFrameworkCore;

namespace Totem.Features.Shared
{
    public static class ValidationExtensions
    {
        private static readonly List<string> AllowedFormats = Format.GetAll().Select(x => x.Value).ToList();
        private static readonly List<string> AllowedDataTypes = DataType.GetAll().Select(x => x.Value).ToList();

        public static async Task<bool> IsUniqueContract(Guid? contractId, string versionNumber, TotemContext dbContext, CancellationToken cancellationToken)
        {
            var isDuplicateContract = await dbContext.Contract.AnyAsync(x => x.Id == contractId && x.VersionNumber == versionNumber, cancellationToken);
            return !isDuplicateContract;
        }

        public static IRuleBuilderInitial<T, string> StringMustBeValidContract<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Custom((contractString, context) =>
            {
                var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>();
                JsonObject jsonObjectDictionary = null;
                try
                {
                    jsonObjectDictionary = (JsonObject)JsonValue.Parse(contractString);
                }
                catch (Exception)
                {
                    context.AddFailure("Contract must be valid JSON.");
                }

                if (jsonObjectDictionary != null)
                {
                    const string refError = "Reference definition not found.";
                    const string contractError = "Contract must be defined as a valid OpenAPI schema.";
                    contractDictionary = SchemaObject.BuildSchemaDictionary(contractString,
                        () => HandleError(context, refError),
                        () => HandleError(context, contractError));
                }

                if (contractDictionary.Count > 0)
                { // At least one schema is defined
                    try
                    {
                        var contractObject = contractDictionary["Contract"];

                        if (string.IsNullOrEmpty(contractObject.Type))
                        {
                            context.AddFailure("Contract object must have a property \"type\" with value \"object\".");
                        }
                        else
                        {
                            if (!CheckDataType(contractObject.Type))
                            {
                                context.AddFailure("Contract does not have a valid type.");
                            }
                        }

                        if (contractObject.Properties.Count == 0)
                        {
                            context.AddFailure("An empty contract cannot be saved.");
                        }
                        else
                        {
                            var idKey = contractObject.Properties.Keys.FirstOrDefault(k => k.EqualsCaseInsensitive("ID"));

                            if (idKey != null)
                            {
                                var idProperty = contractObject.Properties[idKey];

                                if (idProperty.Type != null && !idProperty.Type.Equals(DataType.String.Value) ||
                                    idProperty.Reference != "Guid" ||
                                    contractDictionary["Guid"] == null)
                                {
                                    context.AddFailure("Contract must include a property ID of type Guid.");
                                }
                            }

                            var timestampKey = contractObject.Properties.Keys.FirstOrDefault(k => k.EqualsCaseInsensitive("Timestamp"));

                            if (timestampKey != null)
                            {
                                var timestampProperty = contractObject.Properties[timestampKey];

                                if (timestampProperty.Format == null || !timestampProperty.Format.Equals(Format.DateTime.Value))
                                {
                                    context.AddFailure("The Timestamp property must have a format of date-time.");
                                }
                            }

                            CheckProperties(contractObject.Properties, context);
                        }
                    }
                    catch (Exception)
                    {
                        context.AddFailure("Contract must have a properly defined and formatted schema.");
                    }
                }
            });
        }

        private static void CheckProperties(CaseInsensitiveDictionary<SchemaObject> properties, CustomContext context)
        {
            foreach (var (propertyName, propertyObject) in properties)
            {
                if (propertyObject == null) continue;
                var type = propertyObject.Type;
                var dataType = propertyObject.GetDataType();
                var format = propertyObject.Format;
                var example = propertyObject.Example?.ToString();
                var reference = propertyObject.Reference;

                if (type == null)
                {
                    if (string.IsNullOrEmpty(reference))
                    {
                        context.AddFailure(
                            $"The definition of \"{propertyName}\" is incorrect. A type or reference is required.");
                    }
                }
                else
                {
                    if (!CheckDataType(type))
                    {
                        context.AddFailure(
                            $"The definition of \"{propertyName}\" is incorrect. \"{type}\" is not an allowed data type.");
                    }

                    if (dataType == DataType.Object)
                    {
                        if (propertyObject.Properties == null)
                        {
                            context.AddFailure($"The definition of \"{propertyName}\" is incorrect. \"{type}\" data type requires a 'Properties' object.");
                        }
                        else
                        {
                            CheckProperties(propertyObject.Properties, context);
                        }
                    }

                    if (dataType == DataType.Array)
                    {
                        if (propertyObject.Items == null)
                        {
                            context.AddFailure($"The definition of \"{propertyName}\" is incorrect. It is an array data type and requires an Items sub-property.");
                        }
                        else
                        {
                            var hasValidDataType = propertyObject.Items.Type != null && CheckDataType(propertyObject.Items.Type);

                            if (propertyObject.Items.Reference == null && !hasValidDataType)
                            {
                                context.AddFailure($"The definition of \"{propertyName}\" is incorrect. A valid type is required for the Items sub-property.");
                            }

                            if (hasValidDataType && propertyObject.Items.GetDataType() == DataType.Object)
                            {
                                if (propertyObject.Items.Properties == null)
                                {
                                    context.AddFailure($"The definition of \"{propertyName}\" is incorrect. \"{DataType.Object.Value}\" data type requires a 'Properties' object.");
                                }
                                else
                                {
                                    CheckProperties(propertyObject.Items.Properties, context);
                                }
                            }
                        }
                    }
                }

                if (format != null && !CheckFormat(format))
                {
                    context.AddFailure(
                        $"The definition of \"{propertyName}\" is incorrect. \"{format}\" is not an allowed format.");
                }

                if (string.IsNullOrEmpty(example)) continue;
                if (dataType == DataType.Integer)
                {
                    var isExampleInteger = int.TryParse(example, out _) || long.TryParse(example, out _);
                    // Validate example is integer data type
                    if (!isExampleInteger)
                    {
                        AddExampleError(context, example, propertyName, type);
                    }
                }

                if (dataType == DataType.String)
                {
                    // Validate  example is date-time data type
                    if (format != null)
                    {
                        if (format.EqualsCaseInsensitive(Format.DateTime.Value))
                        {
                            var isExampleDate = DateTime.TryParse(example, out _);
                            if (!isExampleDate)
                                AddExampleError(context, example, propertyName, format);
                        }
                    }

                    // Validate  example is Guid type
                    if (propertyObject.Reference.EqualsCaseInsensitive("Guid"))
                    {
                        var isExampleGuid = Guid.TryParse(example, out _);
                        if (!isExampleGuid)
                            AddExampleError(context, example, propertyName, "Guid");
                    }

                }
            }
        }

        private static void AddExampleError(CustomContext context, string example, string propertyName, string type)
        {
            context.AddFailure(
                $"The example '{example}' for '{propertyName}' does not match the required data type or format '{type}'.");
        }

        private static object HandleError(CustomContext context, string error)
        {
            context.AddFailure(error);
            return null;
        }

        private static bool CheckFormat(string format)
        {
            return AllowedFormats.Contains(format);
        }

        private static bool CheckDataType(string type)
        {
            return AllowedDataTypes.Contains(type);
        }
    }
}
