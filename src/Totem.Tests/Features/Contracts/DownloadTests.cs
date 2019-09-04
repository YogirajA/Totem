using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Totem.Features.Contracts;
using static Totem.Tests.Testing;
using static Totem.Tests.TestDataGenerator;

namespace Totem.Tests.Features.Contracts
{
    public class DownloadTests
    {
        public async Task ShouldMatchDownloadedContractString()
        {
            var addedContract = await AlreadyInDatabaseContract(modifyFieldsForContract:x=>x.ContractString = SampleContractString);

            var query = new Download.Query()
            {
                ContractId = addedContract.Id
            };
            var contractFileStreamResult = await Send(query);

            contractFileStreamResult.ContentType.ShouldBe(MediaTypeNames.Text.Plain);
            contractFileStreamResult.FileStream.ToString().ShouldBe((new MemoryStream(Encoding.UTF8.GetBytes(SampleContractString)).ToString()));
            contractFileStreamResult.FileDownloadName.ShouldBe($"{addedContract.Id}_{addedContract.VersionNumber}.txt");
        }
    }
}
