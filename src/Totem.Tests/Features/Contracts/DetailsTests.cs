using System.Threading.Tasks;
using Totem.Features.Contracts;
using static Totem.Tests.Testing;
using static Totem.Tests.TestDataGenerator;

namespace Totem.Tests.Features.Contracts
{
    public class DetailsTests
    {
        public async Task ShouldMatchDatabaseContract()
        {
            var addedContract = await AlreadyInDatabaseContract();

            var query = new Details.Query()
            {
                ContractId = addedContract.Id
            };
            var contract = await Send(query);

            contract.Id.ShouldMatch(addedContract.Id);
            contract.Description.ShouldMatch(addedContract.Description);
            contract.ContractString.ShouldMatch(addedContract.ContractString);
            contract.VersionNumber.ShouldMatch(addedContract.VersionNumber);
            contract.Namespace.ShouldMatch(addedContract.Namespace);
            contract.Type.ShouldMatch(addedContract.Type);
            contract.UpdateInst.ShouldMatch(addedContract.UpdateInst);
            contract.CreatedDate.ShouldMatch(addedContract.CreatedDate);
        }
    }
}
