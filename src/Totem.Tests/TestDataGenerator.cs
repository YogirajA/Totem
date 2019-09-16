using System;
using System.Text;
using System.Threading.Tasks;
using Totem.Features.Contracts;
using Totem.Infrastructure;
using Totem.Models;
using static Totem.Tests.Testing;

namespace Totem.Tests
{
    public static class TestDataGenerator
    {
        private static readonly Random Random = new Random();

        public const string SampleContractString = @"{
            ""Contract"": {
                ""type"": ""object"",
                ""properties"": {
                    ""Id"": {
                        ""$ref"": ""#/Guid""
                    },
                    ""Timestamp"": {
                        ""type"": ""string"",
                        ""format"": ""date-time"",
                        ""example"": ""2019-05-12T18:14:29Z""
                    },
                    ""Name"": {
                        ""type"": ""string"",
                        ""example"": ""John Doe""
                    },
                    ""Age"": {
                        ""type"": ""integer"",
                        ""format"": ""int32"",
                        ""example"": ""30""
                    }
                }
            },
            ""Guid"": {
                ""type"": ""string"",
                ""pattern"": ""^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$"",
                ""minLength"": 36,
                ""maxLength"": 36,
                ""example"": ""01234567-abcd-0123-abcd-0123456789ab""
            }
        }";

        public const string SampleContractStringWithReferenceError = @"{
            ""Contract"": {
                ""type"": ""object"",
                ""properties"": {
                    ""Id"": {
                        ""$ref"": ""#/Test""
                    },
                    ""Timestamp"": {
                        ""type"": ""string"",
                        ""format"": ""date-time"",
                        ""example"": ""2019-05-12T18:14:29Z""
                    },
                    ""Name"": {
                        ""type"": ""string"",
                        ""example"": ""John Doe""
                    },
                    ""Age"": {
                        ""type"": ""integer"",
                        ""format"": ""int32"",
                        ""example"": ""30""
                    }
                }
            },
            ""Guid"": {
                ""type"": ""string"",
                ""pattern"": ""^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$"",
                ""minLength"": 36,
                ""maxLength"": 36,
                ""example"": ""01234567-abcd-0123-abcd-0123456789ab""
            }
        }";

        public static CaseInsensitiveDictionary<SchemaObject> SampleContractDictionary()
        {
            return new CaseInsensitiveDictionary<SchemaObject>()
            {
                { "Name", new SchemaObject { Type = "String" } },
                { "Age", new SchemaObject { Type = "Integer" } },
                { "BigNumber", new SchemaObject { Type = "Integer" } },
                { "Id", new SchemaObject { Reference = "Guid", Type = "String" } },
                { "Birthday", new SchemaObject { Type = "String" } }
            };
        }

        public static CaseInsensitiveDictionary<SchemaObject> SampleSchemaDictionary()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>()
            {
                { "Name", new SchemaObject { Type = "String", Example = "John Doe" } },
                { "Age", new SchemaObject { Type = "Integer", Format = "Int32", Example = "30" } },
                { "Timestamp", new SchemaObject { Type = "String", Format = "DateTime", Example = "2019-05-12T18:14:29Z" } },
                { "Id", new SchemaObject { Reference = "#/Guid" } }
            };
            var contract = new SchemaObject()
            {
                Type = "Object",
                Properties = contractDictionary
            };
            return new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Contract", contract },
                { "Guid", new SchemaObject
                    {
                        Type = "String",
                        Pattern = "^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$",
                        MaxLength = 36,
                        MinLength = 36,
                        Example = "01234567-abcd-0123-abcd-0123456789ab"
                    }
                }
            };
        }

        public static Contract SampleContract(bool displayOnContractList = false, Guid contractId = default, string versionNumber = default)
        {
            return new Contract
            {
                Id = contractId == default ? Guid.NewGuid() : contractId,
                Description = RandomString(10),
                ContractString = SampleContractString,
                Namespace = RandomString(10),
                Type = RandomString(10),
                VersionNumber = versionNumber ?? "1.1.0",
                DisplayOnContractList = displayOnContractList
            };
        }

        public static async Task<Contract> AlreadyInDatabaseContract(Action<Create.Command> modifyFieldsForContract = null)
        {
            var command = new Create.Command()
            {
                Description = RandomString(10),
                ContractString = SampleContractString,
                Namespace = RandomString(10),
                Type = RandomString(10),
                VersionNumber = "1.0.0"
            };

            modifyFieldsForContract?.Invoke(command);

            var addedContractId = await Send(command);
            var addedContract = Query<Contract>(addedContractId, command.VersionNumber);

            return addedContract;
        }

        private static string RandomString(int size)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * Random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}
