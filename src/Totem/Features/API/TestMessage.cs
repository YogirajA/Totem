using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Totem.Infrastructure;
using Totem.Services;

namespace Totem.Features.API
{
    public class TestMessage
    {
        public class Command : IRequest<Result>
        {
            public Guid ContractId { get; set; }
            public dynamic Message { get; set; }
            public bool AllowSubset { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, Result>
        {
            private readonly TotemContext _db;
            private readonly IMapper _mapper;
            private readonly TesterService _testerService;

            public CommandHandler(TotemContext db, IMapper mapper, TesterService testerService)
            {
                _db = db;
                _mapper = mapper;
                _testerService = testerService;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var contract = await _db.Contract.SingleAsync(x => x.Id == request.ContractId, cancellationToken);
                string warningMessage = null;
                var isValid = true;
                var message = JsonConvert.SerializeObject(request.Message);
                var result = new Result
                {
                    Contract = _mapper.Map<ContractDto>(contract),
                    TestMessage = message
                };

                if (contract.DeprecationDate.HasValue)
                {
                    if (contract.DeprecationDate <= DateTime.Today)
                    {
                        isValid = false;
                        result.MessageErrors.Add("This contract has been deprecated. Please check for a new version.");
                    }
                    else
                    {
                        warningMessage =
                            $"This contract will be deprecated on {contract.DeprecationDate}, please check for a new version.";
                    }
                }

                if (isValid)
                {
                    var testResult = _testerService.Execute(contract.ContractString, message, request.AllowSubset);

                    if (!testResult.IsMessageValid)
                    {
                        isValid = false;
                    }

                    result.MessageErrors = testResult.MessageErrors;
                }

                result.IsValid = isValid;
                result.WarningMessage = warningMessage;

                return result;
            }
        }

        public class ContractDto
        {
            public Guid Id { get; set; }
            public string Description { get; set; }
            public string ContractString { get; set; }
            public string VersionNumber { get; set; }
            public string Namespace { get; set; }
            public string Type { get; set; }
            public DateTime UpdateInst { get; set; }
            public DateTime? DeprecationDate { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class Result
        {
            public ContractDto Contract { get; set; }
            public string TestMessage { get; set; }
            public string WarningMessage { get; set; }
            public bool IsValid { get; set; }
            public List<string> MessageErrors { get; set; } = new List<string>();
        }
    }
}
