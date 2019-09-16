using System.Linq;
using System.Threading.Tasks;
using Totem.Features.Contracts;
using Totem.Models;
using Shouldly;
using static Totem.Tests.Testing;
using static Totem.Tests.TestDataGenerator;

namespace Totem.Tests.Features.Contracts
{
    public class DeleteTests
    {
        public async Task ShouldDeleteContract()
        {
            await AlreadyInDatabaseContract();
            var contractToDelete = await AlreadyInDatabaseContract();
            var contractCount = CountRecords<Contract>();

            var command = new Delete.Command
            {
                ContractId = contractToDelete.Id,
                VersionNumber = contractToDelete.VersionNumber
            };

            await Send(command);

            var updatedCount = CountRecords<Contract>();
            updatedCount.ShouldBe(contractCount - 1);

            var deletedContract = Query<Contract>().SingleOrDefault(x => x.Id == contractToDelete.Id && x.VersionNumber == contractToDelete.VersionNumber);
            deletedContract.ShouldBeNull();
        }
    }
}