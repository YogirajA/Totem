using Shouldly;
using Totem.Infrastructure;
using Totem.Models;

namespace Totem.Tests.Models
{
    public class SchemaTests
    {
        public void ShouldParseReferenceName()
        {
            const string reference = "#/Test";
            var result = SchemaObject.ParseReferenceName(reference);
            result.ShouldBe("Test");
        }

        public void ShouldPopulateReferencesDefinedSeparately()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Contract", new SchemaObject { Type = "Object" } },
                { "Custom", new SchemaObject { Type = "String", Example = "Custom example" } }
            };
            contractDictionary["Contract"].Properties = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Test", new SchemaObject { Reference = "#/Custom" } },
            };
            
            var result = SchemaObject.PopulateReferences(contractDictionary);

            result.HasReferenceError.ShouldBeFalse();
            result.SchemaDictionary["Contract"].Properties["Test"].Type.ShouldBe("String");
            result.SchemaDictionary["Contract"].Properties["Test"].Example.ShouldBe("Custom example");
        }

        public void ShouldHaveErrorWhenReferenceNotDefined()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Contract", new SchemaObject { Type = "Object" } }
            };
            contractDictionary["Contract"].Properties = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Test", new SchemaObject { Reference = "#/Custom" } },
            };

            var result = SchemaObject.PopulateReferences(contractDictionary);

            result.HasReferenceError.ShouldBeTrue();
        }

        public void ShouldFindReferencesWhenTheyExist()
        {
            SchemaObject.BuildSchemaDictionary(TestDataGenerator.SampleContractString, FailTest, FailTest);
        }

        private bool FailTest()
        {
            // If the test calls this function, it should fail
            true.ShouldBe(false); 
            return true;
        }

        public void ShouldHaveReferenceErrorWhenNoReferencesExist()
        {
            var contractString =@"{
                ""Contract"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""Id"": {
                            ""$ref"": ""#/Guid""
                        },
                        ""Timestamp"": {
                            ""type"": ""string"",
                            ""format"": ""date-time"",
                        }
                    }
                }
            }";
            var errorFound = false;
            SchemaObject.BuildSchemaDictionary(contractString, () => errorFound = ErrorFound(), FailTest);
            errorFound.ShouldBe(true);
        }

        public void ShouldHaveReferenceErrorIfReferenceUsedIsNotInTheList()
        {
            var contractString = @"{
                ""Contract"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""Id"": {
                            ""$ref"": ""#/Guid""
                        },
                        ""Timestamp"": {
                            ""type"": ""string"",
                            ""format"": ""date-time"",
                        }
                    }
                },
                ""NotMyReference"": {
                    ""type"": ""string""
                }
            }";
            var errorFound = false;
            SchemaObject.BuildSchemaDictionary(contractString, () => errorFound = ErrorFound(), FailTest);
            errorFound.ShouldBe(true);
        }

        private bool ErrorFound()
        {
            return true;
        }
    }
}
