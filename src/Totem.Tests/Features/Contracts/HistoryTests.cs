using System.Threading.Tasks;
using Totem.Features.Contracts;
using Shouldly;
using static Totem.Tests.Testing;
using static Totem.Tests.TestDataGenerator;

namespace Totem.Tests.Features.Contracts
{
    public class HistoryTests
    {
        public async Task ShouldReturnVersionsForContractId()
        {
            var newContract = await AlreadyInDatabaseContract(x => x.VersionNumber = "1.0.0");
            var newContractVersion2 = await AlreadyInDatabaseContract(x =>
            {
                x.Id = newContract.Id;
                x.VersionNumber = "2.0.0";
            });
            var newContractVersion3 = await AlreadyInDatabaseContract(x =>
            {
                x.Id = newContract.Id;
                x.VersionNumber = "3.0.0";
            });

            var contract2 = await AlreadyInDatabaseContract();
            var contract3 = await AlreadyInDatabaseContract();

            var query = new History.Query {ContractId = newContract.Id};
            var results = await Send(query);

            results.Count.ShouldBe(3);
            results.ShouldContain(x => x.Id == newContract.Id && x.VersionNumber == newContract.VersionNumber);
            results.ShouldContain(x => x.Id == newContractVersion2.Id && x.VersionNumber == newContractVersion2.VersionNumber);
            results.ShouldContain(x => x.Id == newContractVersion3.Id && x.VersionNumber == newContractVersion3.VersionNumber);
            results.ShouldNotContain(x => x.Id == contract2.Id && x.VersionNumber == contract2.VersionNumber);
            results.ShouldNotContain(x => x.Id == contract3.Id && x.VersionNumber == contract3.VersionNumber);
        }
    }
}