using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Totem.Features.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Totem.Infrastructure;
using Totem.Models;

namespace Totem.Features.Contracts
{
    public class Edit
    {
        public class Query : IRequest<Command>
        {
            public Guid ContractId { get; set; }
            public string VersionNumber { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Command>
        {
            private readonly TotemContext _db;
            private readonly IMapper _mapper;

            public QueryHandler(TotemContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Command> Handle(Query request, CancellationToken cancellationToken)
            {
                var contract = await _db.Contract.SingleAsync(
                    x => x.Id == request.ContractId && x.VersionNumber == request.VersionNumber, cancellationToken);

                var otherDisplayedVersionsCount = await _db.Contract
                    .CountAsync(x => x.Id == request.ContractId &&
                                     x.DisplayOnContractList &&
                                     x.VersionNumber != contract.VersionNumber, cancellationToken);

                var viewModel = _mapper.Map<EditModel>(contract);

                return new Command
                {
                    InitialContract = viewModel,
                    ModifiedContract = viewModel,
                    UniqueDisplayedContractVersion = otherDisplayedVersionsCount == 0
                };
            }
        }

        public class Command : IRequest
        {
            public EditModel InitialContract { get; set; }
            public EditModel ModifiedContract { get; set; }
            public bool UniqueDisplayedContractVersion { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command>
        {
            private readonly TotemContext _db;
            private readonly IMapper _mapper;

            public CommandHandler(TotemContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var contract = _mapper.Map<Contract>(request.ModifiedContract);
                var contractExists = await _db.Contract.AnyAsync(x => x.Id == contract.Id && x.VersionNumber == contract.VersionNumber, cancellationToken);

                if (contractExists)
                {
                    _db.Update(contract);
                }
                else
                {
                    await _db.Contract.AddAsync(contract, cancellationToken);
                }

                contract.UpdateInst = DateTime.UtcNow;

                if (request.UniqueDisplayedContractVersion)
                {
                    contract.DisplayOnContractList = true;
                }

                await _db.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }

        public class EditModel
        {
            public Guid Id { get; set; }

            public string Description { get; set; }

            [DisplayName("Contract")]
            public string ContractString { get; set; }

            [DisplayName("Version Number")]
            public string VersionNumber { get; set; }

            public string Namespace { get; set; }

            public string Type { get; set; }

            [DisplayName("Update Time")]
            public DateTime UpdateInst { get; set; }

            [DisplayName("Deprecation Date")]
            public DateTime? DeprecationDate { get; set; }

            [DisplayName("Include this version in the list of contracts")]
            public bool DisplayOnContractList { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(TotemContext dbContext)
            {
                RuleFor(m => m.ModifiedContract.Id).NotEmpty().WithName("Id");
                RuleFor(m => m.ModifiedContract.Description).NotEmpty().WithName("Description");
                RuleFor(m => m.ModifiedContract.ContractString).NotEmpty().StringMustBeValidContract();
                RuleFor(m => m.ModifiedContract.VersionNumber).NotEmpty().WithName("Version Number").Must(BeAValidVersion).WithMessage("'Version Number' must follow semantic version system.");
                RuleFor(m => m).MustAsync((m, cancellationToken) =>
                        ValidationExtensions.IsUniqueContract(m.ModifiedContract.Id, m.ModifiedContract.VersionNumber, dbContext, cancellationToken))
                    .WithMessage(m => $"Version {m.ModifiedContract.VersionNumber} is already in use by another contract.")
                    .When(x => x.ModifiedContract.Id == x.InitialContract.Id && x.ModifiedContract.VersionNumber != x.InitialContract.VersionNumber);
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
