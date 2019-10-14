using System.Json;
using Shouldly;
using Totem.Infrastructure;
using Totem.Models;
using Totem.Services;
using static Totem.Tests.TestingConvention;
using static Totem.Tests.TestDataGenerator;

namespace Totem.Tests.Services
{
    public class TesterServiceTests
    {
        [Input(SampleContractString)]
        public void DeserializeContractToTupleTest(string contract)
        {
            var testDictionary = new CaseInsensitiveDictionary<SchemaObject>();
            var jsonObjectDictionary = (JsonObject)JsonValue.Parse(contract);
            foreach (var key in jsonObjectDictionary.Keys)
            {
                var jsonObject = jsonObjectDictionary[key];
                var schemaObjectString = jsonObject.ToString();
                JsonValue.Parse(schemaObjectString);
                var schemaObject = SchemaObject.ConvertJsonToSchema(schemaObjectString);
                testDictionary.Add(key, schemaObject);
            }

            testDictionary.ShouldNotBeEmpty();
            testDictionary.Count.ShouldBe(2);
        }

        [Input(SampleContractString, @"{}")]
        public void MessageShouldNotBeEmptyJSON(string contract, string message)
        {
            var testService = new TesterService();

            var result = testService.Execute(contract, message);

            result.IsMessageValid.ShouldBeFalse();

            result.MessageErrors[0].ShouldBe("Message contains empty JSON.");
        }

        [Input(SampleContractString, @"\\}")]
        public void MessageShouldNotBeInvalidJSON(string contract, string message)
        {
            var testService = new TesterService();

            var result = testService.Execute(contract, message);

            result.IsMessageValid.ShouldBeFalse();

            result.MessageErrors[0].ShouldBe("Message contains invalid JSON.");
        }

        [Input(SampleContractString, @"{""Id"":""01234567-abcd-0123-abcd-0123456789ab"",""Timestamp"":""2019-05-12T18:14:29"",""Name"":""Emil"",""Age"":""31""}")]
        public void MessageShouldBeAValidContract(string contract, string message)
        {
            var testService = new TesterService();

            var result = testService.Execute(contract, message);

            result.IsMessageValid.ShouldBeTrue();
        }

        [Input(@"{""test"":""id""}",
            @"{""Id"":""01234567-abcd-0123-abcd-0123456789ab"",""Timestamp"":""2019-05-12T18:14:29"",""Name"":""Emil"",""Age"":""31""}")]
        public void ContractShouldHaveValidOpenAPISchema(string contract, string message)
        {
            var testService = new TesterService();

            var result = testService.Execute(contract, message);

            result.IsMessageValid.ShouldBeFalse();

            result.MessageErrors[0].ShouldBe("Unable to test; contract is not a valid OpenAPI schema.");
        }


        [Input(SampleContractStringWithReferenceError, @"{""Id"":""01234567-abcd-0123-abcd-0123456789ab"",""Timestamp"":""2019-05-12T18:14:29"",""Name"":""Emil"",""Age"":""31""}")]
        public void ContractShouldHaveAValidReferenceDefinition(string contract, string message)
        {
            var testService = new TesterService();

            var result = testService.Execute(contract, message);

            result.IsMessageValid.ShouldBeFalse();

            result.MessageErrors[0].ShouldBe("Unable to test; reference definition is invalid.");
        }

        public void AreAllElementsInContractContainedInMessageShouldReturnValidContract()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Name", new SchemaObject
                {
                    Type = "String"
                }},
                { "Age", new SchemaObject
                {
                    Type = "Integer"
                } }
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Name", "Robert" },
                { "Age", "31" }
            };

            var testService = new TesterService();

            var result = testService.AreAllElementsInContractContainedInMessage(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void AreAllElementsContractContainedInMessageShouldReturnInvalidContract()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Name", new SchemaObject
                {
                    Type = "String"
                }},
                { "Age", new SchemaObject
                {
                    Type = "Integer"
                } }
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Name", "Robert" }

            };

            var testService = new TesterService();

            var result = testService.AreAllElementsInContractContainedInMessage(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse("Message is missing expected property \"Age\".");
        }

        public void AreAllElementsInMessageContainedInContractShouldReturnValidContract()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Name", new SchemaObject
                {
                    Type = "String"
                }},
                { "Age", new SchemaObject
                {
                    Type = "Integer"
                } }
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Name", "Robert" },
                { "Age", "31" }
            };

            var testService = new TesterService();

            var result = testService.AreAllElementsInMessageContainedInContract(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void AreAllElementsInMessageContainedInContractShouldReturnValidContractWithWarnings()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Name", new SchemaObject
                {
                    Type = "String"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Name", "Robert" },
                { "Age", "31" }
            };

            var testService = new TesterService();

            var result = testService.AreAllElementsInMessageContainedInContract(messageKeyDictionary, contractDictionary);
            result.IsMessageValid.ShouldBeTrue();
            result.Warnings[0].ShouldBe("Message property \"Age\" is not part of the contract.");
        }

        public void AreAllElementsCorrectDataTypeShouldReturnValidContract()
        {
            var contractDictionary = SampleContractDictionary();

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Name", "Robert" },
                { "Age", "31" },
                { "BigNumber", "1000000" },
                { "Id",  "a21b2109-bd23-4205-ba53-b8df0fdd36bf" },
                { "Birthday", "2019-07-23"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void AreAllElementsCorrectDataTypeShouldReturnInvalidContract()
        {
            var contractDictionary = SampleContractDictionary();

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Name", "123" },
                { "Age", "NotANumber" },
                { "BigNumber", "NotANumber" },
                { "Id",  ""},
                { "Birthday", ""}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse();
            result.MessageErrors.ShouldBe(new[]
            {
                "\"NotANumber\" does not match the required data type for Age (Integer).",
                "\"NotANumber\" does not match the required data type for BigNumber (Integer).",
                "\"\" does not match the required format for Id (Guid)."
            });
        }

        public void ShouldValidateIntegerType()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Integer", new SchemaObject
                {
                    Type = "Integer"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Integer",  "15"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void ShouldValidateNumberType()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Number", new SchemaObject
                {
                    Type = "Number"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Number",  "4.5"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void ShouldFailValidationForNonIntegerType()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Integer", new SchemaObject
                {
                    Type = "Integer"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Integer",  "Not an integer"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse("\"Not an integer\" does not match the required data type for Integer (Integer).");
        }

        public void ShouldFailValidationForNonNumberType()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Number", new SchemaObject
                {
                    Type = "Number"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Number",  "Not a number"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse("\"Not a number\" does not match the required data type for Number (Number).");
        }

        public void ShouldValidateArrayType()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Array", new SchemaObject
                {
                    Type = "Array",
                    Items = new SchemaObject()
                    {
                        Type = "Integer"
                    }
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Array",  "[\"134435\"]"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void ShouldFailValidationForArrayTypeWithoutItems()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Array", new SchemaObject
                {
                    Type = "Array"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Array",  "[\"1\",\"2\",\"3\"]"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse();
            result.MessageErrors[0].ShouldBe("\"Array\" does not have the required property(Items) for type(Array).");
        }

        [Input("string", "date-time", "not a datetime")]
        [Input("integer", null, "not an integer")]
        [Input("integer", "int32", "not an integer")]
        [Input("integer", "int64", "not an integer")]
        [Input("number", "double", "not a number")]
        [Input("number", "float", "not a number")]
        [Input("number", null, "not a number")]
        public void ShouldFailValidationForArrayTypeWithIncorrectItemType(string dataType, string format, string itemString)
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Array", new SchemaObject
                {
                    Type = "Array",
                    Items = new SchemaObject
                    {
                        Type = dataType,
                        Format = format
                    }
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Array",  $"[\"{itemString}\"]"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse();
            result.MessageErrors[0].ShouldBe($"An item in the Items array for Array does not match the required data type ({format ?? dataType}).");
        }

        public void ShouldFailValidationForArrayTypeWithGreaterThanMaxItems()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Array", new SchemaObject
                {
                    Type = "Array",
                    Items = new SchemaObject()
                    {
                        Type = "Integer"
                    },
                    MaxItems = 2
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Array",  "[\"1\",\"2\",\"3\"]"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse();
            result.MessageErrors[0].ShouldBe("The Items array for Array has greater than the maximum number (2) of items allowed.");
        }

        public void ShouldFailValidationForArrayTypeWithLessThanMinItems()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Array", new SchemaObject
                {
                    Type = "Array",
                    Items = new SchemaObject()
                    {
                        Type = "Integer"
                    },
                    MinItems = 2
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Array",  "[\"1\"]"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse();
            result.MessageErrors[0].ShouldBe("The Items array for Array does not have the minimum number (2) of items required.");
        }

        public void ShouldValidateInt32Format()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Integer", new SchemaObject
                {
                    Type = "Integer",
                    Format = "int32"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Integer",  "15"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void ShouldFailValidationForNonInt32Format()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Integer", new SchemaObject
                {
                    Type = "Integer",
                    Format = "int32"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Integer",  "2150000000"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse("\"2150000000\" does not match the required format for Integer (Int32).");
        }

        public void ShouldValidateFloatFormat()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Number", new SchemaObject
                {
                    Type = "Number",
                    Format = "float"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Number",  "5.7"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void ShouldValidateDoubleFormat()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Number", new SchemaObject
                {
                    Type = "Number",
                    Format = "double"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Number",  "123456789012.34567"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void ShouldFailValidationForNonFloatFormat()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Number", new SchemaObject
                {
                    Type = "Number",
                    Format = "float"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Number", double.MinValue}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse("\"123456789012.34567\" does not match the required format for Number (Float).");
        }

        public void ShouldValidateDateTimeFormat()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Timestamp", new SchemaObject
                {
                    Type = "String",
                    Format = "date-time"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Timestamp",  "1/1/2019"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void ShouldFailValidationForDateTimeFormat()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Timestamp", new SchemaObject
                {
                    Type = "String",
                    Format = "date-time"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Timestamp",  "Not a datetime"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse("\"Not a datetime\" does not match the required format for Timestamp (DateTime).");

        }

        public void ShouldValidateInt64Format()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Integer", new SchemaObject
                {
                    Type = "Integer",
                    Format = "int64"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Integer",  "2150000000"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void ShouldValidateGuidFormat()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Id", new SchemaObject
                {
                    Reference = "#/Guid",
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Id",  "01234567-abcd-0123-abcd-0123456789ab"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void ShouldFailValidationForGuidFormat()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Id", new SchemaObject
                {
                    Reference = "Guid",
                    Type = "String"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Id",  "This isn't a guid"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse("\"This isn't a guid\" does not match the required format for Id (Guid).");

        }

        public void ShouldValidateObjectType()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Object", new SchemaObject
                {
                    Type = "Object"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Object",  new object()}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void ShouldFailValidationForNonObjectType()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Object", new SchemaObject
                {
                    Type = "Object"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "Object",  "not an object"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse();
            result.MessageErrors[0].ShouldBe("\"not an object\" does not match the required data type for Object (Object).");
        }

        public void ShouldNotFailValidationIfSchemaNotFound()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "FirstName", new SchemaObject
                {
                    Type = "String"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "LastName",  "Lastname"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void ShouldFailValidationWhenMissingNestedProperty()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                {
                    "User", new SchemaObject()
                    {
                        Type = "Object",
                        Properties = new CaseInsensitiveDictionary<SchemaObject>
                        {

                            { "FirstName", new SchemaObject { Type = "String" } },
                            { "LastName", new SchemaObject { Type = "String" } }
                        }
                    }
                    
                }
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "User",  new CaseInsensitiveDictionary<object>
                    {
                        { "LastName", new SchemaObject { Type = "String" } }
                    }
                }
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeFalse();
            result.MessageErrors[0].ShouldBe("The value for field \"User-->FirstName\" was not found.");
        }

        public void PropertyNameShouldBeCaseInsensitive()
        {
            var contractDictionary = new CaseInsensitiveDictionary<SchemaObject>
            {
                { "Id", new SchemaObject
                {
                    Type = "String"
                }}
            };

            var messageKeyDictionary = new CaseInsensitiveDictionary<object>
            {
                { "ID",  "testId"}
            };

            var testerService = new TesterService();

            var result = testerService.DoAllMessageValuesMatchDataTypes(messageKeyDictionary, contractDictionary);

            result.IsMessageValid.ShouldBeTrue();
        }

        public void TryParseJSON_Detects_Quote_Mismatch()
        {
            var testerService = new TesterService();

            var result1 = testerService.TryParseJSON("{\"Name\": Erin\"}", out var jsonObject1);
            var result2 = testerService.TryParseJSON("{\"Name\": \"Erin}", out var jsonObject2);
            var result3 = testerService.TryParseJSON("{\"Name: \"Erin\"}", out var jsonObject3);
            var result4 = testerService.TryParseJSON("{Name\": \"Erin\"}", out var jsonObject4);
            var result5 = testerService.TryParseJSON("{\"Age\": \"5}", out var jsonObject5);
            var result6 = testerService.TryParseJSON("{\"Age\": 5\"}", out var jsonObject6);

            result1.ShouldBeFalse();
            result2.ShouldBeFalse();
            result3.ShouldBeFalse();
            result4.ShouldBeFalse();
            result5.ShouldBeFalse();
            //            Assert.False(result6); // This test reveals the bug to be addressed by CT-7
        }

        public void TryParseJSON_Detects_Missing_Bracket()
        {
            var testerService = new TesterService();

            var result1 = testerService.TryParseJSON("{\"Name\": \"Erin\"", out var jsonObject1);
            var result2 = testerService.TryParseJSON("\"Name\": \"Erin\"}", out var jsonObject2);

            result1.ShouldBeFalse();
            result2.ShouldBeFalse();
        }

        public void TryParseJSON_Passes_With_Valid_JSON()
        {
            var testerService = new TesterService();

            var result1 = testerService.TryParseJSON("{\"Name\": \"Erin\"}", out var jsonObject1);
            var result2 = testerService.TryParseJSON("{\"Age\": 5}", out var jsonObject2);
            result1.ShouldBeTrue();
            result2.ShouldBeTrue();
        }
    }
}