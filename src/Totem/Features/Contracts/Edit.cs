using System;
using System.ComponentModel;
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
                var contract = await _db.Contract.SingleAsync(x => x.Id == request.ContractId, cancellationToken: cancellationToken);
                var viewModel = _mapper.Map<EditModel>(contract);

                return new Command
                {
                    InitialContract = viewModel,
                    ModifiedContract = viewModel
                };
            }
        }

        public class Command : IRequest
        {
            public EditModel InitialContract { get; set; }
            public EditModel ModifiedContract { get; set; }
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

            public Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var contract = _mapper.Map<Contract>(request.ModifiedContract);

                contract.UpdateInst = DateTime.UtcNow;

                _db.Update(contract);
                _db.SaveChanges();

                return Unit.Task;
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
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(m => m.ModifiedContract.Id).NotEmpty().WithName("Id");
                RuleFor(m => m.ModifiedContract.Description).NotEmpty().WithName("Description");
                RuleFor(m => m.ModifiedContract.ContractString).NotEmpty().StringMustBeValidContract();
                RuleFor(m => m.ModifiedContract.Namespace).NotEmpty().WithName("Namespace");
                RuleFor(m => m.ModifiedContract.Type).NotEmpty().WithName("Type");
                RuleFor(m => m.ModifiedContract.VersionNumber).NotEmpty().WithName("Version Number");
            }
        }
    }
}
