using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Totem.Features.Contracts;
using Totem.Models;
using static Totem.Tests.Testing;

namespace Totem.Tests.Features.Contracts
{
    public class IndexTests
    {
        public async Task ShouldBeEqualToDisplayOnContractListCountInDatabase()
        {
            var databaseContractsCount = Query<Contract>().Count(x => x.DisplayOnContractList);

            var query = new Index.Query();
            var contracts = await Send(query);

            contracts.Count.ShouldBe(databaseContractsCount);
        }
    }
}
