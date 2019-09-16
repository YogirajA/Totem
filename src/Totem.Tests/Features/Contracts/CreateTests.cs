using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Totem.Features.Contracts;
using Totem.Models;
using static Totem.Tests.Testing;
using static Totem.Tests.TestDataGenerator;
using static Totem.Tests.TestingConvention;

namespace Totem.Tests.Features.Contracts
{
    public class CreateTests
    {
        public async Task ShouldCreateWhenValid()
        {
            var oldCount = CountRecords<Contract>();
            var newContract = SampleContract(true);

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
            var createdContract = Query<Contract>(newContractId, command.VersionNumber);
            createdContract.UpdateInst = newContract.UpdateInst;
            createdContract.CreatedDate = newContract.CreatedDate;

            createdContract.ShouldMatch(newContract);
        }

        public async Task ShouldGetLatestVersionDataOnCreateNewContract()
        {
            var command = await BuildAndPersistContract();

            var createContactQuery = new Create.Query
            {
                ContractId = command.Id.GetValueOrDefault(),
                VersionNumber = default // Get latests
            };

            var newVersionCommand = await Send(createContactQuery);

            newVersionCommand.ShouldMatch(new Create.Command
            {
                VersionNumber = command.VersionNumber,
                Id = command.Id,
                DeprecationDate = null,
                ContractString = command.ContractString,
                Description = command.Description,
                Namespace = command.Namespace,
                Type = command.Type
            });
        }

        public async Task ShouldGetSpecificVersionDataOnCreateNewContract()
        {
            const int contractsCount = 5;
            var contractId = Guid.NewGuid();

            var commands = await BuildAndPersistContracts(contractsCount, contractId);
            var baseContractCommand = commands.ToArray()[new Random().Next(0, contractsCount - 1)];

            var createContactQuery = new Create.Query
            {
                ContractId = contractId,
                VersionNumber = baseContractCommand.VersionNumber // Get this version data
            };

            var newVersionCommand = await Send(createContactQuery);

            newVersionCommand.ShouldMatch(new Create.Command
            {
                VersionNumber = baseContractCommand.VersionNumber,
                Id = contractId,
                DeprecationDate = null,
                ContractString = baseContractCommand.ContractString,
                Description = baseContractCommand.Description,
                Namespace = baseContractCommand.Namespace,
                Type = baseContractCommand.Type
            });
        }

        public async Task ShouldCreateWithDisplayOnContractListAsTrueWhenValid()
        {
            var oldCount = CountRecords<Contract>();
            var newContract = SampleContract(true);

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
            var createdContract = Query<Contract>(newContractId, newContract.VersionNumber);
            createdContract.UpdateInst = newContract.UpdateInst;
            createdContract.CreatedDate = newContract.CreatedDate;

            newContract.DisplayOnContractList.ShouldBeTrue();
            createdContract.ShouldMatch(newContract);
        }

        [Input("1.0.1")]
        [Input("1.0.0-alpha")]
        [Input("1.0.0-alpha.1")]
        [Input("1.0.0-0.3.7")]
        [Input("1.0.0-x.7.z.92")]
        [Input("1.0.0-alpha+001")]
        [Input("1.0.0+20130313144700")]
        [Input("1.0.0-beta+exp.sha.5114f85")]
        [Input("1.0.0-alpha.beta")]
        [Input("1.0.0-beta")]
        [Input("1.0.0-beta.2")]
        [Input("1.0.0-beta.11")]
        [Input("1.0.0-rc.1")]
        public void ShouldValidateDifferentFormsOfVersionNumber(string versionNumber)
        {
            var newContract = SampleContract(true);

            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = newContract.ContractString,
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = versionNumber
            };

            command.ShouldValidate();
        }

        [Input("1")]
        [Input("1.1")]
        [Input("1.0.0-.123")]
        [Input("1.0.0-...")]
        [Input("1.0.0-123.")]
        [Input("1.0.0-+")]
        [Input("1.0.0-+123")]
        [Input("1.0.0-")]
        [Input("1.0.0+.123")]
        [Input("1.0.0+...")]
        [Input("1.0.0+123.")]
        [Input("1.0.0+")]
        public void ShouldNotValidateInvalidFormsOfVersionNumber(string versionNumber)
        {
            var newContract = SampleContract(true);

            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = newContract.ContractString,
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = versionNumber
            };

            command.ShouldNotValidate(
                "'Version Number' must follow semantic version system.");
        }

        public void ShouldNotCreateWhenVersionIsNotValid()
        {
            var newContract = SampleContract(true);

            var command = new Create.Command()
            {
                Description = newContract.Description,
                ContractString = newContract.ContractString,
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = "not a valid version"
            };

            command.ShouldNotValidate("'Version Number' must follow semantic version system.");
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

            command.ShouldNotValidate(
                "Contract must be valid JSON.",
                "'Contract' must not be empty.",
                "'Description' must not be empty.",
                "'Namespace' must not be empty.",
                "'Type' must not be empty.",
                "'Version Number' must follow semantic version system.",
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
            var newContract = SampleContract(true);
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
            var createdContract = Query<Contract>(newContractId, command.VersionNumber);
            createdContract.UpdateInst = newContract.UpdateInst;
            createdContract.CreatedDate = newContract.CreatedDate;

            createdContract.ShouldMatch(newContract);
        }

        public async Task ShouldCreateWhenValidWithNestedObject()
        {
            var oldCount = CountRecords<Contract>();
            var newContract = SampleContract(true);
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
            var createdContract = Query<Contract>(newContractId, command.VersionNumber);
            createdContract.UpdateInst = newContract.UpdateInst;
            createdContract.CreatedDate = newContract.CreatedDate;

            createdContract.ShouldMatch(newContract);
        }

        public async Task ShouldCreateWhenValidWithArrayAndNestedObject()
        {
            var oldCount = CountRecords<Contract>();
            var newContract = SampleContract(true);
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
            var createdContract = Query<Contract>(newContractId, command.VersionNumber);
            createdContract.UpdateInst = newContract.UpdateInst;
            createdContract.CreatedDate = newContract.CreatedDate;

            createdContract.ShouldMatch(newContract);
        }

        public async Task ShouldCreateWhenValidWithNestedArray()
        {
            var oldCount = CountRecords<Contract>();
            var newContract = SampleContract(true);
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
            var createdContract = Query<Contract>(newContractId, command.VersionNumber);
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

            command.ShouldNotValidate("Contract must be defined as a valid OpenAPI schema.",
                "The definition of \"Name\" is incorrect. A type or reference is required.");
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

        public async Task ShouldNotCreateWhenVersionNumberForContractIdAlreadyExists()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var newContract = SampleContract(true);

            var command = new Create.Command()
            {
                Id = initialContract.Id,
                Description = newContract.Description,
                ContractString = newContract.ContractString,
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            command.ShouldNotValidate($"Contract Id '{initialContract.Id}' with Version '{initialContract.VersionNumber}' already exist.");
        }

        private static async Task<Create.Command> BuildAndPersistContract(Guid contractId = default, string version = default)
        {
            var newContract = SampleContract(true, contractId, version);

            var command = new Create.Command
            {
                Id = newContract.Id,
                Description = newContract.Description,
                ContractString = newContract.ContractString,
                Namespace = newContract.Namespace,
                Type = newContract.Type,
                VersionNumber = newContract.VersionNumber
            };

            command.ShouldValidate();
            await Send(command);

            return command;
        }

        private static async Task<IEnumerable<Create.Command>> BuildAndPersistContracts(int count, Guid contractId = default)
        {
            var buildTasks = Enumerable.Range(1, count)
                .Select(index =>
                {
                    var version = contractId == default ? default : $"1.0.{index}";

                    return BuildAndPersistContract(contractId, version);
                });

            return await Task.WhenAll(buildTasks);
        }
    }
}
