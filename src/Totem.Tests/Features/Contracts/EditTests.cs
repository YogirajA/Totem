using System;
using System.Threading.Tasks;
using Shouldly;
using Totem.Features.Contracts;
using Totem.Models;
using static Totem.Tests.Testing;
using static Totem.Tests.TestDataGenerator;

namespace Totem.Tests.Features.Contracts
{
    public class EditTests
    {
        public async Task ShouldEditWhenValid()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var oldCount = CountRecords<Contract>();

            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description, // Edited
                ContractString = sampleContract.ContractString, // Edited
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = sampleContract.VersionNumber // Edited
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldValidate();
            await Send(command);

            CountRecords<Contract>().ShouldBe(oldCount);

            var modifiedContract = Query<Contract>(initialContract.Id);

            modifiedContract.Id.ShouldBe(initialContract.Id);
            modifiedContract.Description.ShouldBe(sampleContract.Description);
            modifiedContract.ContractString.ShouldBe(sampleContract.ContractString);
            modifiedContract.Namespace.ShouldBe(initialContract.Namespace);
            modifiedContract.Type.ShouldBe(initialContract.Type);
            modifiedContract.VersionNumber.ShouldBe(sampleContract.VersionNumber);
        }

        public async Task ShouldNotEditWhenRequiredFieldsEmpty()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var modifiedContractModel = new Edit.EditModel()
            {
                Id = Guid.Empty,
                Description = "",
                ContractString = "",
                Namespace = "",
                Type = "",
                VersionNumber = ""
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldNotValidate("Contract must be valid JSON.",
                "'Contract' must not be empty.",
                "'Description' must not be empty.",
                "'Id' must not be empty.",
                "'Namespace' must not be empty.",
                "'Type' must not be empty.",
                "'Version Number' must not be empty.");
        }

        public async Task ShouldNotEditWhenContractStringNotValidJson()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
                ContractString = "Not a valid contract string",
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };

            command.ShouldNotValidate("Contract must be valid JSON.");
        }

        public async Task ShouldNotEditWhenContractStringNotValidSchema()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
                ContractString = "{\"id\": \"Guid\", \"Timestamp\": \"DateTime\", \"Name\": \"String\", \"Age\": \"Int32\"}",
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };

            command.ShouldNotValidate("Contract must be defined as a valid OpenAPI schema.");
        }

        public async Task ShouldNotEditWhenContractStringDoesNotHaveTimestamp()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };

            command.ShouldNotValidate("Contract must include a property Timestamp of format date-time.");
        }

        public async Task ShouldNotEditWhenContractStringTimestampDoesNotHaveFormat()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };

            command.ShouldNotValidate("The Timestamp property must have a format of date-time.");
        }

        public async Task ShouldNotEditWhenContractStringDoesNotHaveId()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };

            command.ShouldNotValidate("Contract must include a property ID of type Guid.");
        }

        public async Task ShouldEditWhenValidWithArray()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var oldCount = CountRecords<Contract>();

            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            sampleContract.ContractString = @"{
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
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description, // Edited
                ContractString = sampleContract.ContractString, // Edited
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = sampleContract.VersionNumber // Edited
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldValidate();
            await Send(command);

            CountRecords<Contract>().ShouldBe(oldCount);

            var modifiedContract = Query<Contract>(initialContract.Id);

            modifiedContract.Id.ShouldBe(initialContract.Id);
            modifiedContract.Description.ShouldBe(sampleContract.Description);
            modifiedContract.ContractString.ShouldBe(sampleContract.ContractString);
            modifiedContract.Namespace.ShouldBe(initialContract.Namespace);
            modifiedContract.Type.ShouldBe(initialContract.Type);
            modifiedContract.VersionNumber.ShouldBe(sampleContract.VersionNumber);
        }

        public async Task ShouldEditWhenValidWithNestedObject()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var oldCount = CountRecords<Contract>();

            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            sampleContract.ContractString = @"{
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
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description, // Edited
                ContractString = sampleContract.ContractString, // Edited
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = sampleContract.VersionNumber // Edited
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldValidate();
            await Send(command);

            CountRecords<Contract>().ShouldBe(oldCount);

            var modifiedContract = Query<Contract>(initialContract.Id);

            modifiedContract.Id.ShouldBe(initialContract.Id);
            modifiedContract.Description.ShouldBe(sampleContract.Description);
            modifiedContract.ContractString.ShouldBe(sampleContract.ContractString);
            modifiedContract.Namespace.ShouldBe(initialContract.Namespace);
            modifiedContract.Type.ShouldBe(initialContract.Type);
            modifiedContract.VersionNumber.ShouldBe(sampleContract.VersionNumber);
        }

        public async Task ShouldEditWhenValidWithArrayAndNestedObject()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var oldCount = CountRecords<Contract>();

            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            sampleContract.ContractString = @"{
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
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description, // Edited
                ContractString = sampleContract.ContractString, // Edited
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = sampleContract.VersionNumber // Edited
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldValidate();
            await Send(command);

            CountRecords<Contract>().ShouldBe(oldCount);

            var modifiedContract = Query<Contract>(initialContract.Id);

            modifiedContract.Id.ShouldBe(initialContract.Id);
            modifiedContract.Description.ShouldBe(sampleContract.Description);
            modifiedContract.ContractString.ShouldBe(sampleContract.ContractString);
            modifiedContract.Namespace.ShouldBe(initialContract.Namespace);
            modifiedContract.Type.ShouldBe(initialContract.Type);
            modifiedContract.VersionNumber.ShouldBe(sampleContract.VersionNumber);
        }

        public async Task ShouldEditWhenValidWithNestedArray()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var oldCount = CountRecords<Contract>();

            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            sampleContract.ContractString = @"{
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
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description, // Edited
                ContractString = sampleContract.ContractString, // Edited
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = sampleContract.VersionNumber // Edited
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldValidate();
            await Send(command);

            CountRecords<Contract>().ShouldBe(oldCount);

            var modifiedContract = Query<Contract>(initialContract.Id);

            modifiedContract.Id.ShouldBe(initialContract.Id);
            modifiedContract.Description.ShouldBe(sampleContract.Description);
            modifiedContract.ContractString.ShouldBe(sampleContract.ContractString);
            modifiedContract.Namespace.ShouldBe(initialContract.Namespace);
            modifiedContract.Type.ShouldBe(initialContract.Type);
            modifiedContract.VersionNumber.ShouldBe(sampleContract.VersionNumber);
        }

        public async Task ShouldNotEditWhenContractStringArrayDoesNotHaveItems()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldNotValidate("The definition of \"PhoneNumbers\" is incorrect. It is an array data type and requires an Items sub-property.");
        }

        public async Task ShouldNotEditWhenNestedArrayDoesNotHaveItems()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldNotValidate("The definition of \"LevelThree\" is incorrect. It is an array data type and requires an Items sub-property.");
        }

        public async Task ShouldNotEditWhenContractStringArrayDoesNotHaveAValidItemsType()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldNotValidate("The definition of \"PhoneNumbers\" is incorrect. A valid type is required for the Items sub-property.");
        }

        public async Task ShouldNotEditWhenNestedArrayDoesNotHaveAValidItemsType()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldNotValidate("The definition of \"LevelThree\" is incorrect. A valid type is required for the Items sub-property.");
        }

        public async Task ShouldNotEditWhenContractStringDoesNotHaveValidType()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldNotValidate("The definition of \"Age\" is incorrect. \"not-a-type\" is not an allowed data type.");
        }

        public async Task ShouldNotEditWhenContractStringDoesNotHaveValidFormat()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldNotValidate("The definition of \"Age\" is incorrect. \"not-a-format\" is not an allowed format.");
        }

        public async Task ShouldNotEditWhenContractStringDoesNotHaveAProperFormat()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldNotValidate("Contract must be defined as a valid OpenAPI schema.",
                "Contract must have a properly defined and formatted schema.");
        }

        public async Task ShouldNotEditWhenContractStringPropertyDoesNotHaveAType()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };
            command.ShouldNotValidate("Reference definition not found.",
                "The definition of \"Name\" is incorrect. A type is required.");
        }

        public async Task ShouldNotEditWhenNonIntegerTypeExampleForIntegerType()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };

            command.ShouldNotValidate("The example 'not-an-integer-example' for 'Age' does not match the required data type or format 'integer'.");
        }

        public async Task ShouldNotEditWhenNonDateTimeExampleForDateTimeFormat()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };

            command.ShouldNotValidate("The example 'not-a-date-example' for 'Birthdate' does not match the required data type or format 'date-time'.");
        }

        public async Task ShouldNotEditWhenNonGuidExampleForGuid()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };

            command.ShouldNotValidate("The example 'not-a-guid-example' for 'Id' does not match the required data type or format 'Guid'.");
        }

        public async Task ShouldNotEditWhenNestedExamplesDoNotMatchType()
        {
            var initialContract = await AlreadyInDatabaseContract();
            var initialContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = initialContract.Description,
                ContractString = initialContract.ContractString,
                Namespace = initialContract.Namespace,
                Type = initialContract.Type,
                VersionNumber = initialContract.VersionNumber
            };

            var sampleContract = SampleContract();
            var modifiedContractModel = new Edit.EditModel()
            {
                Id = initialContract.Id,
                Description = sampleContract.Description,
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
                Namespace = initialContract.Namespace,
                Type = sampleContract.Type,
                VersionNumber = sampleContract.VersionNumber
            };

            var command = new Edit.Command()
            {
                InitialContract = initialContractModel,
                ModifiedContract = modifiedContractModel
            };

            command.ShouldNotValidate("The definition of \"InvalidType\" is incorrect. \"not-a-type\" is not an allowed data type.", "The example 'not-an-integer' for 'LevelThree' does not match the required data type or format 'integer'.");
        }
    }
}
