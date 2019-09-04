using System.Threading.Tasks;
using Shouldly;
using Totem.Features.Contracts;
using Totem.Models;
using static Totem.Tests.Testing;
using static Totem.Tests.TestDataGenerator;

namespace Totem.Tests.Features.Contracts
{
    public class CreateTests
    {
        public async Task ShouldCreateWhenValid()
        {
            var oldCount = CountRecords<Contract>();
            var newContract = SampleContract();

            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = newContract.ContractString,
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldValidate();
            var newContractId = await Send(command);
            newContract.Id = newContractId;

            CountRecords<Contract>().ShouldBe(oldCount + 1);
            var createdContract = Query<Contract>(newContractId);
            createdContract.UpdateInst = newContract.UpdateInst;
            createdContract.CreatedDate = newContract.CreatedDate;

            createdContract.ShouldMatch(newContract);
        }

        public void ShouldNotCreateWhenRequiredFieldsEmpty()
        {
            var command = new Create.Command()
            {
                Description = "",
                ContractString = "",
                Namespace = "",
                Type = "",
                VersionNumber = ""
            };

            command.ShouldNotValidate("Contract must be valid JSON.",
                "'Contract' must not be empty.",
                "'Description' must not be empty.",
                "'Namespace' must not be empty.",
                "'Type' must not be empty.",
                "'Version Number' must not be empty.");
        }

        public void ShouldNotCreateWhenContractStringIsNotValidJson()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = "Not a valid JSON string",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("Contract must be valid JSON.");
        }

        public void ShouldNotCreateWhenContractStringIsNotValidSchema()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = "{\"id\": \"Guid\", \"Timestamp\": \"DateTime\", \"Name\": \"String\", \"Age\": \"Int32\"}",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("Contract must be defined as a valid OpenAPI schema.");
        }

        public void ShouldNotCreateWhenContractStringDoesNotHaveTimestamp()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
                    ""Contract"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""Id"": {
                                ""$ref"": ""#/Guid""
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
                        ""type"": ""string""
                    }
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("Contract must include a property Timestamp of format date-time.");
        }

        public void ShouldNotCreateWhenContractStringDoesNotHaveId()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
                    ""Contract"": {
                        ""type"": ""object"",
                        ""properties"": {
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
                    }
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("Contract must include a property ID of type Guid.");
        }

        public void ShouldNotCreateWhenContractStringTimestampDoesNotHaveFormat()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
                    ""Contract"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""Id"": {
                                ""$ref"": ""#/Guid""
                            },
                            ""Timestamp"": {
                                ""type"": ""string"",
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
                        ""type"": ""string""
                    }
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("The Timestamp property must have a format of date-time.");
        }

        public async Task ShouldCreateWhenValidWithArray()
        {
            var oldCount = CountRecords<Contract>();
            var newContract = SampleContract();
            newContract.ContractString = @"{
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
                            ""PhoneNumbers"": {
                                ""type"": ""array"",
                                ""items"": {
                                    ""type"": ""string""
                                }                                            
                            }
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }";

            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = newContract.ContractString,
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldValidate();
            var newContractId = await Send(command);
            newContract.Id = newContractId;

            CountRecords<Contract>().ShouldBe(oldCount + 1);
            var createdContract = Query<Contract>(newContractId);
            createdContract.UpdateInst = newContract.UpdateInst;
            createdContract.CreatedDate = newContract.CreatedDate;

            createdContract.ShouldMatch(newContract);
        }

        public async Task ShouldCreateWhenValidWithNestedObject()
        {
            var oldCount = CountRecords<Contract>();
            var newContract = SampleContract();
            newContract.ContractString = @"{
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

            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = newContract.ContractString,
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldValidate();
            var newContractId = await Send(command);
            newContract.Id = newContractId;

            CountRecords<Contract>().ShouldBe(oldCount + 1);
            var createdContract = Query<Contract>(newContractId);
            createdContract.UpdateInst = newContract.UpdateInst;
            createdContract.CreatedDate = newContract.CreatedDate;

            createdContract.ShouldMatch(newContract);
        }

        public async Task ShouldCreateWhenValidWithArrayAndNestedObject()
        {
            var oldCount = CountRecords<Contract>();
            var newContract = SampleContract();
            newContract.ContractString = @"{
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
                            },
                            ""PhoneNumbers"": {
                                ""type"": ""array"",
                                ""items"": {
                                    ""type"": ""string""
                                }                                            
                            }
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }";

            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = newContract.ContractString,
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldValidate();
            var newContractId = await Send(command);
            newContract.Id = newContractId;

            CountRecords<Contract>().ShouldBe(oldCount + 1);
            var createdContract = Query<Contract>(newContractId);
            createdContract.UpdateInst = newContract.UpdateInst;
            createdContract.CreatedDate = newContract.CreatedDate;

            createdContract.ShouldMatch(newContract);
        }

        public async Task ShouldCreateWhenValidWithNestedArray()
        {
            var oldCount = CountRecords<Contract>();
            var newContract = SampleContract();
            newContract.ContractString = @"{
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
                                                }
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

            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = newContract.ContractString,
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldValidate();
            var newContractId = await Send(command);
            newContract.Id = newContractId;

            CountRecords<Contract>().ShouldBe(oldCount + 1);
            var createdContract = Query<Contract>(newContractId);
            createdContract.UpdateInst = newContract.UpdateInst;
            createdContract.CreatedDate = newContract.CreatedDate;

            createdContract.ShouldMatch(newContract);
        }

        public void ShouldNotCreateWhenNestedArrayDoesNotHaveItems()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
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
                                               ""type"": ""array""
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
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("The definition of \"LevelThree\" is incorrect. It is an array data type and requires an Items sub-property.");
        }

        public void ShouldNotCreateWhenContractStringArrayDoesNotHaveItems()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
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
                            ""PhoneNumbers"": {
                                ""type"": ""array""
                            }
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("The definition of \"PhoneNumbers\" is incorrect. It is an array data type and requires an Items sub-property.");
        }

        public void ShouldNotCreateWhenContractStringArrayDoesNotHaveAValidItemsType()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
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
                            ""PhoneNumbers"": {
                                ""type"": ""array"",
                                ""items"": {
                                    ""type"": ""not-a-type""
                                }
                            }
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("The definition of \"PhoneNumbers\" is incorrect. A valid type is required for the Items sub-property.");
        }

        public void ShouldNotCreateWhenNestedArrayDoesNotHaveAValidItemsType()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
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
                                                    ""type"": ""not-a-type""
                                                }
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
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("The definition of \"LevelThree\" is incorrect. A valid type is required for the Items sub-property.");
        }

        public void ShouldNotCreateWhenContractStringDoesNotHaveValidType()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
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
                                ""type"": ""not-a-type"",
                                ""format"": ""int32"",
                                ""example"": ""30""
                            },
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("The definition of \"Age\" is incorrect. \"not-a-type\" is not an allowed data type.");
        }

        public void ShouldNotCreateWhenContractStringDoesNotHaveValidFormat()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
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
                                ""format"": ""not-a-format"",
                                ""example"": ""30""
                            },
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("The definition of \"Age\" is incorrect. \"not-a-format\" is not an allowed format.");
        }

        public void ShouldNotCreateWhenContractStringDoesNotHaveAProperFormat()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
                    ""Contract12"": {
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
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("Contract must be defined as a valid OpenAPI schema.",
                "Contract must have a properly defined and formatted schema.");
        }

        public void ShouldNotCreateWhenContractStringPropertyDoesNotHaveAType()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
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
                                ""example"": ""test""
                            }
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("Reference definition not found.",
                "The definition of \"Name\" is incorrect. A type is required.");
        }

        public void ShouldNotCreateWhenNonIntegerTypeExampleForIntegerType()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
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
                                ""example"": ""not-an-integer-example""
                            }
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("The example 'not-an-integer-example' for 'Age' does not match the required data type or format 'integer'.");
        }

        public void ShouldNotCreateWhenNonDateTimeExampleForDateTimeFormat()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
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
                            ""Birthdate"": {
                                ""type"": ""string"",
                                ""format"": ""date-time"",
                                ""example"": ""not-a-date-example""
                            }
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string""
                    }
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("The example 'not-a-date-example' for 'Birthdate' does not match the required data type or format 'date-time'.");
        }

        public void ShouldNotCreateWhenNonGuidExampleForGuid()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
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
                                ""example"": ""32""
                            }
                        }
                    },
                    ""Guid"": {
                        ""type"": ""string"",
                        ""pattern"": ""^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$"",
                        ""minLength"": 36,
                        ""maxLength"": 36,
                        ""example"": ""not-a-guid-example""
                    }
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("The example 'not-a-guid-example' for 'Id' does not match the required data type or format 'Guid'.");
        }

        public void ShouldNotCreateWhenNestedExamplesDoNotMatchType()
        {
            var newContract = SampleContract();
            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = @"{
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
                            ""LevelOne"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""LevelTwo"": {
                                        ""type"": ""object"",
                                        ""properties"": {
                                            ""LevelThree"": {
                                                ""type"": ""integer"",
                                                ""example"": ""not-an-integer""
                                            },
                                            ""InvalidType"": {
                                                ""type"": ""not-a-type""
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
                }",
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldNotValidate("The definition of \"InvalidType\" is incorrect. \"not-a-type\" is not an allowed data type.", "The example 'not-an-integer' for 'LevelThree' does not match the required data type or format 'integer'.");
        }
    }
}
