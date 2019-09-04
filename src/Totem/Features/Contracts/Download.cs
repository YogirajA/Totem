using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Totem.Infrastructure;

namespace Totem.Features.Contracts
{
    public class Download
    {
        public class Query : IRequest<FileStreamResult>
        {
            public Guid ContractId { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, FileStreamResult>
        {
            private readonly TotemContext _db;

            public QueryHandler(TotemContext db)
            {
                _db = db;
            }

            public async Task<FileStreamResult> Handle(Query request, CancellationToken cancellationToken)
            {
                var contract = await _db.Contract.SingleAsync(x => x.Id == request.ContractId, cancellationToken: cancellationToken);

                byte[] byteArray = Encoding.UTF8.GetBytes(contract.ContractString);

                return new FileStreamResult(new MemoryStream(byteArray), MediaTypeNames.Text.Plain)
                {
                    FileDownloadName = $"{contract.Id}_{contract.VersionNumber}.txt"
                };
            }
        }
    }
}
