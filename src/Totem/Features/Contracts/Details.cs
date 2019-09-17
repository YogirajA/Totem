using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Totem.Infrastructure;
using Totem.Models;

namespace Totem.Features.Contracts
{
    public class Details
    {
        public class Query : IRequest<ViewModel>
        {
            public Guid ContractId { get; set; }
            public string VersionNumber { get; set; }
        }

        public class ViewModel
        {
            [DisplayName("Contract ID")]
            public Guid Id { get; set; }

            public string Description { get; set; }

            [DisplayName("Contract")]
            public string ContractString { get; set; }
            [DisplayName("Properties")]
            public CaseInsensitiveDictionary<SchemaObject> ContractObject { get; set; }

            [DisplayName("Version Number")]
            public string VersionNumber { get; set; }

            public string Namespace { get; set; }

            public string Type { get; set; }

            [DisplayName("Update Time")]
            public DateTime UpdateInst { get; set; }

            [DisplayName("Created Date")]
            public DateTime CreatedDate { get; set; }

        }

        public class Handler : IRequestHandler<Query, ViewModel>
        {
            private readonly TotemContext _db;
            private readonly IMapper _mapper;

            public Handler(TotemContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var contract = await _db.Contract.SingleAsync(
                    x => x.Id == request.ContractId && x.VersionNumber == request.VersionNumber,
                    cancellationToken: cancellationToken);

                return _mapper.Map<ViewModel>(contract);
            }
        }
    }
}
