using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Totem.Features.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Semver;
using Totem.Infrastructure;
using Totem.Models;

namespace Totem.Features.Contracts
{
    public class Create
    {
        public class Query : IRequest<Command>
        {
            public Guid ContractId { get; set; }
            public string VersionNumber { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Command>
        {
            private readonly TotemContext _dbContext;
            private readonly IMapper _mapper;

            public QueryHandler(TotemContext dbContext, IMapper mapper)
            {
                _dbContext = dbContext;
                _mapper = mapper;
            }

            public async Task<Command> Handle(Query request, CancellationToken cancellationToken)
            {
                var command = new Command();

                if (request.ContractId == Guid.Empty)
                {
                    return command;
                }

                var contracts = await _dbContext.Contract
                    .Where(x => x.Id == request.ContractId &&
                    (x.VersionNumber == request.VersionNumber || request.VersionNumber == default))
                    .ToListAsync(cancellationToken);

                var maxVersionContract =
                    contracts.OrderByDescending(x => SemVersion.Parse(x.VersionNumber)).First();

                _mapper.Map(maxVersionContract, command);

                command.DeprecationDate = null;

                return command;
            }
        }

        public class Command : IRequest<Guid>
        {
            public Guid? Id { get; set; }

            public string Description { get; set; }

            [DisplayName("Contract")]
            public string ContractString { get; set; }

            [DisplayName("Version Number")]
            public string VersionNumber { get; set; }

            public string Namespace { get; set; }

            public string Type { get; set; }

            [DisplayName("Deprecation Date")]
            public DateTime? DeprecationDate { get; set; }
        }

        public class Handler : RequestHandler<Command, Guid>
        {
            private readonly TotemContext _db;
            private readonly IMapper _mapper;

            public Handler(TotemContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            protected override Guid Handle(Command request)
            {
                if (request.Id.GetValueOrDefault() == default)
                {
                    request.Id = Guid.NewGuid();
                }

                var contract = _mapper.Map<Contract>(request);

                contract.UpdateInst = DateTime.UtcNow;

                _db.Contract.Add(contract);

                _db.SaveChanges();

                return contract.Id;
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(TotemContext dbContext)
            {

                RuleFor(m => m.Description).NotEmpty();
                RuleFor(m => m.ContractString).NotEmpty().StringMustBeValidContract();
                RuleFor(m => m.Namespace).NotEmpty();
                RuleFor(m => m.Type).NotEmpty();
                RuleFor(m => m.VersionNumber).NotEmpty().Must(BeAValidVersion).WithMessage("'Version Number' must follow semantic version system.");
                RuleFor(m => m).MustAsync((m, cancellationToken) => ValidationExtensions.IsUniqueContract(m.Id, m.VersionNumber, dbContext, cancellationToken))
                    .WithMessage(m => $"Contract Id '{m.Id}' with Version '{m.VersionNumber}' already exist.");
            }

            private static bool BeAValidVersion(Command contract, string versionNumberString)
            {
                const string semanticVersionRegex = "^((([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9a-zA-Z-]+(?:\\.[0-9a-zA-Z-]+)*))?)(?:\\+([0-9a-zA-Z-]+(?:\\.[0-9a-zA-Z-]+)*))?)$";
                var match = Regex.Match(versionNumberString, semanticVersionRegex, RegexOptions.IgnoreCase);
                return match.Success;
            }
        }
    }
}
