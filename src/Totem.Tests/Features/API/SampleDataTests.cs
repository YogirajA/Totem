using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shouldly;
using Totem.Features.API;
using Totem.Infrastructure;
using Totem.Models;
using static Totem.Tests.Testing;
using static Totem.Tests.TestDataGenerator;

namespace Totem.Tests.Features.API
{
    public class SampleDataTests
    {
        public async Task ShouldGenerateTestDataForKnownDataTypesUsingExamples()
        {
            var contract = await AlreadyInDatabaseContract();

            var command = new SampleData.Command()
            {
                ContractId = contract.Id,
                VersionNumber = contract.VersionNumber
            };

            var result = await Send(command);

            var messageDictionary = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(result.SampleData);

            var date = $"{(DateTime)messageDictionary["Timestamp"]:yyyy-MM-ddTHH:mm:ssZ}";

            messageDictionary["ID"].ShouldBe("01234567-abcd-0123-abcd-0123456789ab");
            date.ShouldBe("2019-05-12T18:14:29Z");
            messageDictionary["Name"].ShouldBe("John Doe");
            messageDictionary["Age"].ShouldBe(30);
        }

        public async Task GeneratesDataWhenExamplesNotProvidedInContract()
        {
            const string contractString = @"{
                ""Contract"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""Id"": {
                            ""$ref"": ""#/Guid""
                        },
                        ""Timestamp"": {
                            ""type"": ""string"",
                            ""format"": ""date-time""
                        },
                        ""Name"": {
                            ""type"": ""string""
                        },
                        ""Age"": {
                            ""type"": ""integer"",
                            ""format"": ""int32""
                        }
                    }
                },
                ""Guid"": {
                    ""type"": ""string"",
                    ""pattern"": ""^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$""
                }
            }";
            var contract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = contractString);

            var command = new SampleData.Command()
            {
                ContractId = contract.Id,
                VersionNumber = contract.VersionNumber
            };

            var result = await Send(command);

            var messageDictionary = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(result.SampleData);
            var time = DateTime.Parse(messageDictionary["Timestamp"].ToString());
            var guid = Guid.Parse(messageDictionary["ID"].ToString());

            guid.GetType().ShouldBe(typeof(Guid));
            time.GetType().ShouldBe(typeof(DateTime));
            messageDictionary["Name"].ShouldBe("String text");
            messageDictionary["Age"].ShouldBe(5);
        }

        public void GeneratesProperIntegerSampleData()
        {
            var sampleInt32 = SampleData.GenerateSampleData(DataType.Integer, "Int32");
            var int32Value = int.Parse(sampleInt32);
            int32Value.GetType().ShouldBe(typeof(int));

            var sampleInt64 = SampleData.GenerateSampleData(DataType.Integer, "Int64");
            var int64Value = long.Parse(sampleInt64);
            int64Value.GetType().ShouldBe(typeof(long));

            var sampleInt = SampleData.GenerateSampleData(DataType.Integer, null);
            var intValue = long.Parse(sampleInt);
            intValue.GetType().ShouldBe(typeof(long));
        }

        public void GeneratesProperStringSampleData()
        {
            var dateTimeString = SampleData.GenerateSampleData(DataType.String, "date-time");
            dateTimeString = dateTimeString.Replace("\"", "");
            var sampleDateTime = DateTime.Parse(dateTimeString);
            sampleDateTime.GetType().ShouldBe(typeof(DateTime));

            var sampleData = SampleData.GenerateSampleData(DataType.String, null);
            sampleData.GetType().ShouldBe(typeof(string));
        }

        public void GeneratesProperGuidSampleData()
        {
            var guidString = SampleData.GenerateSampleData(DataType.String, null,
                "^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$");
            guidString = guidString.Replace("\"", "");
            var sampleGuid = Guid.Parse(guidString);
            sampleGuid.GetType().ShouldBe(typeof(Guid));
        }

        public void SampleDataHandlesInvalidPatternRegex()
        {
            // mismatched parens to test invalid pattern
            var guidString = SampleData.GenerateSampleData(DataType.String, null, "^(([0-9a-f]))-([0-9a-f]){12})$");
            guidString = guidString.Replace("\"", "");
            guidString.ShouldBe("Invalid pattern");
        }

        public void GeneratesProperObjectSampleData()
        {
            var propertyDictionary = new CaseInsensitiveDictionary<SchemaObject>();
            propertyDictionary.Add("IntegerProp", new SchemaObject()
            {
                Type = DataType.Integer.Value
            });
            propertyDictionary.Add("StringProp", new SchemaObject()
            {
                Type = DataType.String.Value
            });
            propertyDictionary.Add("ObjectProp", new SchemaObject()
            {
                Type = DataType.Object.Value,
                Properties = new CaseInsensitiveDictionary<SchemaObject>()
                    {{
                        "InnerProp", new SchemaObject()
                        {
                            Type = DataType.Integer.Value
                        }
                    }}
            });

            var objectString = SampleData.GenerateSampleData(DataType.Object, null, properties: propertyDictionary);
            objectString = objectString.Replace("\"\"", "\"");
            objectString.ShouldBe("{\"IntegerProp\":\"30\",\"StringProp\":\"String text\",\"ObjectProp\":{\"InnerProp\":\"30\"}}");
        }

        public void GeneratesProperStringArraySampleData()
        {
            var arrayItem = new SchemaObject()
            {
                Type = "String",
            };
            var sampleData = SampleData.GenerateSampleData(DataType.Array, null, items: arrayItem);
            sampleData = sampleData.Replace("\"\"", "\"");

            var items = JsonConvert.DeserializeObject<List<string>>(sampleData);
            foreach (var item in items)
            {
                item.GetType().ShouldBe(typeof(string));
            }
        }

        public void GeneratesProperIntegerArraySampleData()
        {
            var arrayItem = new SchemaObject()
            {
                Type = "Integer",
                Format = "int32"
            };
            var sampleData = SampleData.GenerateSampleData(DataType.Array, null, items: arrayItem);

            var items = JsonConvert.DeserializeObject<List<string>>(sampleData);
            foreach (var item in items)
            {
                var parsedItem = int.Parse(item);
                parsedItem.GetType().ShouldBe(typeof(int));
            }
        }

        public void GeneratesProperDateArraySampleData()
        {
            var arrayItem = new SchemaObject()
            {
                Type = "String",
                Format = "date-time"
            };
            var sampleData = SampleData.GenerateSampleData(DataType.Array, null, items: arrayItem);
            sampleData = sampleData.Replace("\"\"", "\"");

            var items = JsonConvert.DeserializeObject<List<string>>(sampleData);
            foreach (var item in items)
            {
                var parsedItem = DateTime.Parse(item);
                parsedItem.GetType().ShouldBe(typeof(DateTime));
            }
        }

        public async Task GeneratesProperArraySampleDataFromExamples()
        {
            const string contractString = @"{
                ""Contract"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""Id"": {
                            ""$ref"": ""#/Guid""
                        },
                        ""Timestamp"": {
                            ""type"": ""string"",
                            ""format"": ""date-time""
                        },
                        ""StringList"": {
                            ""type"": ""array"",
                            ""items"": {
                                ""type"": ""string""
                            },
                            ""example"": [""Item1"", ""Item2""]
                        },
                        ""IntList"": {
                            ""type"": ""array"",
                            ""items"": {
                                ""type"": ""integer""
                            },
                            ""example"": 5
                        },
                    }
                },
                ""Guid"": {
                    ""type"": ""string"",
                    ""pattern"": ""^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$""
                }
            }";
            var contract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = contractString);

            var command = new SampleData.Command()
            {
                ContractId = contract.Id,
                VersionNumber = contract.VersionNumber
            };

            var result = await Send(command);

            var messageDictionary = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(result.SampleData);
            var stringArray = JsonConvert.DeserializeObject<List<string>>(messageDictionary["StringList"].ToString());
            var intArray = JsonConvert.DeserializeObject<List<int>>(messageDictionary["IntList"].ToString());

            foreach (var item in stringArray)
            {
                item.GetType().ShouldBe(typeof(string));
            }
            foreach (var item in intArray)
            {
                item.GetType().ShouldBe(typeof(int));
            }
        }

        public async Task GeneratesProperNestedObjectSampleDataFromExamples()
        {
            const string contractString = @"{
                ""Contract"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""Id"": {
                            ""$ref"": ""#/Guid""
                        },
                        ""Timestamp"": {
                            ""type"": ""string"",
                            ""format"": ""date-time""
                        },
                        ""LevelOne"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""LevelTwo"": {
                                        ""type"": ""object"",
                                        ""properties"": {
                                            ""LevelThree"": {
                                                ""type"": ""string"",
                                                ""example"": ""My example""
                                            }
                                        }
                                    }
                               }
                           }
                    }
                },
                ""Guid"": {
                    ""type"": ""string"",
                    ""pattern"": ""^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$""
                }
            }";
            var contract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = contractString);

            var command = new SampleData.Command()
            {
                ContractId = contract.Id,
                VersionNumber = contract.VersionNumber
            };

            var result = await Send(command);

            var messageDictionary = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(result.SampleData);
            var levelOne = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(messageDictionary["LevelOne"].ToString());
            var levelTwo = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(levelOne["LevelTwo"].ToString());
            levelTwo["LevelThree"].ShouldBe("My example");
        }

        public async Task GeneratesProperNestedObjectsWithMultipleObjectNestingSampleDataFromExamples()
        {
            const string contractString = @"{
                ""Contract"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""Id"": {
                            ""$ref"": ""#/Guid""
                        },
                        ""Timestamp"": {
                            ""type"": ""string"",
                            ""format"": ""date-time""
                        },
                        ""LevelOne"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""LevelTwo(1)"": {
                                        ""type"": ""object"",
                                        ""properties"": {
                                            ""LevelThree"": {
                                                ""type"": ""string"",
                                                ""example"": ""My example L3""
                                            },
                                            ""LevelThree(1)"": {
                                                ""type"": ""object"",
                                                ""properties"": {
                                                    ""LevelFour"": {
                                                        ""type"": ""string"",
                                                        ""example"": ""My example L4""
                                                    }
                                                }
                                            }
                                        }
                                    },
                                   ""LevelTwo(2)"": {
                                        ""type"": ""object"",
                                        ""properties"": {
                                            ""LevelThree(2)"": {
                                                ""type"": ""string"",
                                                ""example"": ""My example L3(2)""
                                            }
                                        }
                                    }
                               }
                           }
                    }
                },
                ""Guid"": {
                    ""type"": ""string"",
                    ""pattern"": ""^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$""
                }
            }";
            var contract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = contractString);

            var command = new SampleData.Command()
            {
                ContractId = contract.Id,
                VersionNumber = contract.VersionNumber
            };

            var result = await Send(command);

            var messageDictionary = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(result.SampleData);
            var levelOne = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(messageDictionary["LevelOne"].ToString());
            var levelTwoOne = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(levelOne["LevelTwo(1)"].ToString());
            var levelThreeOne = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(levelTwoOne["LevelThree(1)"].ToString());
            var levelTwoTwo = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(levelOne["LevelTwo(2)"].ToString());
     
            levelTwoOne["LevelThree"].ShouldBe("My example L3");
            levelTwoTwo["LevelThree(2)"].ShouldBe("My example L3(2)");
            levelThreeOne["LevelFour"].ShouldBe("My example L4");
        }

        public async Task GeneratesProperNestedObjectSampleDataFromExamplesWithReferences()
        {
            const string contractString = @"{
                ""Contract"": {
                    ""type"": ""object"",
                    ""properties"": {
                        ""Id"": {
                            ""$ref"": ""#/Guid""
                        },
                        ""Timestamp"": {
                            ""type"": ""string"",
                            ""format"": ""date-time""
                        },
                        ""LevelOne"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""LevelTwo"": {
                                        ""type"": ""object"",
                                        ""properties"": {
                                            ""LevelThree"": {
                                                ""$ref"":  ""#/Guid""
                                            }
                                        }
                                    }
                               }
                           }
                    }
                },
                ""Guid"": {
                    ""type"": ""string"",
                    ""pattern"": ""^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$"",
                    ""example"": ""01234567-abcd-0123-abcd-0123456789ab""
                }
            }";
            var contract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = contractString);

            var command = new SampleData.Command()
            {
                ContractId = contract.Id,
                VersionNumber = contract.VersionNumber
            };

            var result = await Send(command);

            var messageDictionary = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(result.SampleData);
            var levelOne = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(messageDictionary["LevelOne"].ToString());
            var levelTwo = JsonConvert.DeserializeObject<CaseInsensitiveDictionary<object>>(levelOne["LevelTwo"].ToString());
            levelTwo["LevelThree"].ShouldBe("01234567-abcd-0123-abcd-0123456789ab");
        }

    }
}
