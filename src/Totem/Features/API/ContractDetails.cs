using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Totem.Infrastructure;
using Totem.Models;

namespace Totem.Features.API
{
    public class ContractDetails
    {
        public class Command : IRequest<Result>
        {
            public Guid ContractId { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, Result>
        {
            private readonly TotemContext _db;

            public CommandHandler(TotemContext db)
            {
                _db = db;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var contract = await _db.Contract.SingleAsync(x => x.Id == request.ContractId,
                    cancellationToken: cancellationToken);
                return new Result
                {
                    Contract = SchemaObject.BuildSchemaDictionary(contract.ContractString, NoOp, NoOp)
                };
            }

            private static bool NoOp() => true;
        }

        public class Result
        {
            public CaseInsensitiveDictionary<SchemaObject> Contract { get; set; }
        }
    }
}
