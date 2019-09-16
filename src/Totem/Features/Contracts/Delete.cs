using System;
using System.Threading;
using System.Threading.Tasks;
using Totem.Infrastructure;
using MediatR;

namespace Totem.Features.Contracts
{
    public class Delete
    {
        public class Command : IRequest
        {
            public Guid ContractId { get; set; }
            public string VersionNumber { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly TotemContext _dbContext;

            public Handler(TotemContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var contract = await _dbContext.Contract.FindAsync(command.ContractId, command.VersionNumber);
                _dbContext.Contract.Remove(contract);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}
