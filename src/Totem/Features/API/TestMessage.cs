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
                var contract = await _db.Contract.SingleAsync(x => x.Id == request.ContractId,
                    cancellationToken: cancellationToken);

                var message = JsonConvert.SerializeObject(request.Message);

                var testResult = _testerService.Execute(contract.ContractString, message);

                return new Result
                {
                    Contract = _mapper.Map<ContractDto>(contract),
                    TestMessage = message,
                    TestMessageValid = testResult.IsMessageValid ? "Valid" : "Invalid",
                    MessageErrors = testResult.MessageErrors
                };
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
            public DateTime CreatedDate { get; set; }
        }

        public class Result
        {
            public ContractDto Contract { get; set; }
            public string TestMessage { get; set; }
            public string TestMessageValid { get; set; }
            public List<string> MessageErrors { get; set; } = new List<string>();
        }
    }
}
