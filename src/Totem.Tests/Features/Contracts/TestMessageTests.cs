using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fixie.Internal;
using Shouldly;
using Totem.Features.Contracts;
using static Totem.Tests.Testing;
using static Totem.Tests.TestDataGenerator;

namespace Totem.Tests.Features.Contracts
{
    public class TestMessageTests
    {
        public async Task ShouldBeValidWhenMessageMatchesContract()
        {
            var addedContract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = SampleContractString);

            var query = new TestMessage.Query()
            {
                ContractId = addedContract.Id
            };
            var contract = await Send(query);

            var sampleMessage = "{\"id\": \"a21b2109-bd23-4205-ba53-b8df0fdd36bf\", \"Timestamp\": \"2019-07-23\",\"Name\":\"Saagar\",\"Age\":\"26\"}";

            var command = new TestMessage.Command()
            {
                ContractId = contract.ContractId,
                SampleMessage = sampleMessage
            };

            var result = await Send(command);

            result.TestMessageValid.ShouldBe("Valid");
            result.MessageErrors.ShouldBeEmpty();
        }

        public async Task ShouldBeValidWhenMessageWithNestedObjectMatchesContract()
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
                            },
                            ""LevelOne"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""LevelTwo"": {
                                        ""type"": ""object"",
                                        ""properties"": {
                                            ""LevelThree"": {
                                                ""type"": ""integer"",
                                                ""example"": ""232""
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }";

            var addedContract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = contractString);

            var query = new TestMessage.Query()
            {
                ContractId = addedContract.Id
            };
            var contract = await Send(query);

            var sampleMessage = "{\"id\": \"a21b2109-bd23-4205-ba53-b8df0fdd36bf\", \"Timestamp\": \"2019-07-23\",\"Name\":\"Saagar\",\"Age\":\"26\",\"LevelOne\":{\"LevelTwo\":{\"LevelThree\":\"232\"}}}";

            var command = new TestMessage.Command()
            {
                ContractId = contract.ContractId,
                SampleMessage = sampleMessage
            };

            var result = await Send(command);

            result.TestMessageValid.ShouldBe("Valid");
            result.MessageErrors.ShouldBeEmpty();
        }

        public async Task ShouldBeInvalidWhenMessageWithNestedObjectDoesNotMatchContract()
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
                            },
                            ""LevelOne"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""LevelThree"": {
                                        ""type"": ""object"",
                                        ""properties"": {
                                            ""LevelFour"": {
                                                ""type"": ""integer"",
                                                ""example"": ""232""
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }";

            var addedContract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = contractString);

            var query = new TestMessage.Query()
            {
                ContractId = addedContract.Id
            };
            var contract = await Send(query);

            var sampleMessage = "{\"id\": \"a21b2109-bd23-4205-ba53-b8df0fdd36bf\", \"Timestamp\": \"2019-07-23\",\"Name\":\"Saagar\",\"Age\":\"26\",\"LevelOne\":{\"LevelTwo\":{\"LevelThree\":\"232\"}}}";

            var command = new TestMessage.Command()
            {
                ContractId = contract.ContractId,
                SampleMessage = sampleMessage
            };

            var result = await Send(command);

            result.TestMessageValid.ShouldBe("Invalid");
            result.MessageErrors.ShouldBe(new List<string>()
            {"The value for field \"LevelOne-->LevelThree\" was not found."
            });
        }

        public async Task ShouldBeValidWhenMessageWithNestedArrayMatchesContract()
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
                            },
                            ""LevelOne"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""LevelTwo"": {
                                        ""type"": ""object"",
                                        ""properties"": {
                                            ""LevelThree"": {
                                                ""type"": ""array"",
                                                ""items"": {
                                                    ""type"": ""string""
                                                },
                                                ""example"": [""Item1"", ""Item2""]
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }";

            var addedContract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = contractString);

            var query = new TestMessage.Query()
            {
                ContractId = addedContract.Id
            };
            var contract = await Send(query);

            var sampleMessage = "{\"id\": \"a21b2109-bd23-4205-ba53-b8df0fdd36bf\", \"Timestamp\": \"2019-07-23\",\"Name\":\"Saagar\",\"Age\":\"26\",\"LevelOne\":{\"LevelTwo\":{\"LevelThree\":[\"Item1\",\"Item2\"]}}}";

            var command = new TestMessage.Command()
            {
                ContractId = contract.ContractId,
                SampleMessage = sampleMessage
            };

            var result = await Send(command);

            result.TestMessageValid.ShouldBe("Valid");
            result.MessageErrors.ShouldBeEmpty();
        }

        public async Task ShouldBeValidWhenMessageWithNestedArrayDoesNotMatchContract()
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
                            },
                            ""LevelOne"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""LevelTwo"": {
                                        ""type"": ""object"",
                                        ""properties"": {
                                            ""LevelThree"": {
                                                ""type"": ""array"",
                                                ""items"": {
                                                    ""type"": ""integer""
                                                },
                                                ""example"": [1, 2]
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }";

            var addedContract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = contractString);

            var query = new TestMessage.Query()
            {
                ContractId = addedContract.Id
            };
            var contract = await Send(query);

            var sampleMessage = "{\"id\": \"a21b2109-bd23-4205-ba53-b8df0fdd36bf\", \"Timestamp\": \"2019-07-23\",\"Name\":\"Saagar\",\"Age\":\"26\",\"LevelOne\":{\"LevelTwo\":{\"LevelThree\":[\"string123\"]}}}";

            var command = new TestMessage.Command()
            {
                ContractId = contract.ContractId,
                SampleMessage = sampleMessage
            };

            var result = await Send(command);

            result.TestMessageValid.ShouldBe("Invalid");
            result.MessageErrors.ShouldBe(new List<string>()
            {"An item in the Items array for LevelOne-->LevelTwo-->LevelThree does not match the required data type (integer)."
            });
        }

        public async Task ShouldBeValidWhenMessageWithArrayMatchesContract()
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
                            },
                            ""StringList"": {
                                ""type"": ""array"",
                                ""items"": {
                                    ""type"": ""string""
                                },
                                ""example"": [""Item1"", ""Item2""]
                           },
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }";

            var addedContract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = contractString);

            var query = new TestMessage.Query()
            {
                ContractId = addedContract.Id
            };
            var contract = await Send(query);

            var sampleMessage = "{\"id\": \"a21b2109-bd23-4205-ba53-b8df0fdd36bf\", \"Timestamp\": \"2019-07-23\",\"Name\":\"Saagar\",\"Age\":\"26\",\"StringList\": [\"Item1\",\"Item2\"]}";

            var command = new TestMessage.Command()
            {
                ContractId = contract.ContractId,
                SampleMessage = sampleMessage
            };

            var result = await Send(command);

            result.TestMessageValid.ShouldBe("Valid");
            result.MessageErrors.ShouldBeEmpty();
        }

        public async Task ShouldBeValidWhenMessageWithArrayDoesNotMatchContract()
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
                            },
                            ""IntList"": {
                                ""type"": ""array"",
                                ""items"": {
                                    ""type"": ""integer""
                                },
                                ""example"": [1, 2]
                           },
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }";

            var addedContract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = contractString);

            var query = new TestMessage.Query()
            {
                ContractId = addedContract.Id
            };
            var contract = await Send(query);

            var sampleMessage = "{\"id\": \"a21b2109-bd23-4205-ba53-b8df0fdd36bf\", \"Timestamp\": \"2019-07-23\",\"Name\":\"Saagar\",\"Age\":\"26\",\"IntList\": [\"Item1\",\"Item2\"]}";

            var command = new TestMessage.Command()
            {
                ContractId = contract.ContractId,
                SampleMessage = sampleMessage
            };

            var result = await Send(command);

            result.TestMessageValid.ShouldBe("Invalid");
            result.MessageErrors.ShouldBe(new List<string>()
            {"An item in the Items array for IntList does not match the required data type (integer)."
            });
        }

        public async Task ShouldBeInvalidWhenMessageDoesNotMatchContract()
        {
            var addedContract = await AlreadyInDatabaseContract(modifyFieldsForContract: x => x.ContractString = SampleContractString);

            var query = new TestMessage.Query()
            {
                ContractId = addedContract.Id
            };
            var contract = await Send(query);

            var sampleMessage = "{\"FirstName\":\"Saagar\",\"Age\":\"26\"}";

            var command = new TestMessage.Command()
            {
                ContractId = contract.ContractId,
                SampleMessage = sampleMessage
            };

            var result = await Send(command);

            result.TestMessageValid.ShouldBe("Invalid");
            result.MessageErrors.ShouldBe(new List<string>()
            {
                "Message property \"FirstName\" is not part of the contract.",
                "Message is missing expected property \"Id\".",
                "Message is missing expected property \"Name\".",
                "Message is missing expected property \"Timestamp\".",
                "The schema for \"FirstName\" was not found in the contract definition."
            });
        }
    }
}
