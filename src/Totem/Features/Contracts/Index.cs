using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Totem.Infrastructure;
using Totem.Models;

namespace Totem.Features.Contracts
{
    public class Index
    {
        public class Query : IRequest<List<ViewModel>>
        {
        }

        public class Handler : IRequestHandler<Query, List<ViewModel>>
        {
            private readonly TotemContext _db;
            private readonly IMapper _mapper;

            public Handler(TotemContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<List<ViewModel>> Handle(Query request, CancellationToken cancellationToken)
            {
                var contracts = await _db.Contract.Where(x => x.DisplayOnContractList).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken: cancellationToken);

                return _mapper.Map<List<ViewModel>>(contracts);
            }
        }

        public class ViewModel
        {
            public Guid Id { get; set; }

            public string Description { get; set; }

            [DisplayName("Contract")]
            public string ContractString { get; set; }
            public CaseInsensitiveDictionary<SchemaObject> ContractObject { get; set; }

            [DisplayName("Properties")]
            public List<string> ContractProperties { get; set; }

            [DisplayName("Version Number")]
            public string VersionNumber { get; set; }

            public string Namespace { get; set; }

            public string Type { get; set; }

            [DisplayName("Update Time")]
            public DateTime UpdateInst { get; set; }

            [DisplayName("Created Date")]
            public DateTime CreatedDate { get; set; }

        }
    }
}
