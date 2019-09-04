using System;
using System.ComponentModel;
using AutoMapper;
using Totem.Features.Shared;
using FluentValidation;
using MediatR;
using Totem.Infrastructure;
using Totem.Models;

namespace Totem.Features.Contracts
{
    public class Create
    {
        public class Command : IRequest<Guid>
        {
            public string Description { get; set; }

            [DisplayName("Contract")]
            public string ContractString { get; set; }

            [DisplayName("Version Number")]
            public string VersionNumber { get; set; }

            public string Namespace { get; set; }

            public string Type { get; set; }
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
                var contract = _mapper.Map<Contract>(request);

                contract.UpdateInst = DateTime.UtcNow;

                _db.Contract.Add(contract);

                _db.SaveChanges();

                return contract.Id;
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(m => m.Description).NotEmpty();
                RuleFor(m => m.ContractString).NotEmpty().StringMustBeValidContract();
                RuleFor(m => m.Namespace).NotEmpty();
                RuleFor(m => m.Type).NotEmpty();
                RuleFor(m => m.VersionNumber).NotEmpty();
            }
        }
    }
}
