using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Totem.Infrastructure;
using Totem.Models;
using Totem.Services;

namespace Totem.Features.Contracts
{
    public class TestMessage
    {
        public class Query : IRequest<ViewModel>
        {
            public Guid ContractId { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, ViewModel>
        {
            private readonly TotemContext _db;
            private readonly IMapper _mapper;

            public QueryHandler(TotemContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var contract = await _db.Contract.SingleAsync(x => x.Id == request.ContractId, cancellationToken: cancellationToken);

                return _mapper.Map<ViewModel>(contract);
            }
        }

        public class Command : IRequest<ViewModel>
        {
            public Guid ContractId { get; set; }
            public string SampleMessage { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, ViewModel>
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

            public async Task<ViewModel> Handle(Command request, CancellationToken cancellationToken)
            {
                var contract = await _db.Contract.SingleAsync(x => x.Id == request.ContractId, cancellationToken: cancellationToken);
                var testResult = _testerService.Execute(contract.ContractString, request.SampleMessage);

                var result = _mapper.Map<ViewModel>(contract);

                result.TestMessage = request.SampleMessage;
                result.TestMessageValid = testResult.IsMessageValid ? "Valid" : "Invalid";
                result.MessageErrors = testResult.MessageErrors;

                return result;
            }
        }

        public class ViewModel
        {
            [DisplayName("Contract ID")]
            public Guid ContractId { get; set; }
            [DisplayName("Description")]
            public string ContractDescription { get; set; }
            [DisplayName("Properties")]
            public CaseInsensitiveDictionary<SchemaObject> ContractObject { get; set; }
            public string ContractString { get; set; }
            public string TestMessageValid { get; set; }
            public string TestMessage { get; set; }
            public IEnumerable<string> MessageErrors { get; set; }
        }
    }
}
