using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Totem.Infrastructure;
using Totem.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Totem.Features.Contracts
{
    public class History
    {
        public class Query : IRequest<List<ViewModel>>
        {
            public Guid ContractId { get; set; }
        }

        public class Handler : IRequestHandler<Query, List<ViewModel>>
        {
            private readonly TotemContext _dbContext;
            private readonly IMapper _mapper;

            public Handler(TotemContext dbContext, IMapper mapper)
            {
                _dbContext = dbContext;
                _mapper = mapper;
            }

            public async Task<List<ViewModel>> Handle(Query request, CancellationToken cancellationToken)
            {
                var contracts = await _dbContext.Contract.Where(x => x.Id == request.ContractId).OrderByDescending(x => x.VersionNumber).ToListAsync(cancellationToken);
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
