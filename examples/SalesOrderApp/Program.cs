using System.Threading.Tasks;

namespace SalesOrderApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var placeOrder = new PlaceOrderTests();
            await placeOrder.ShouldReturnBadRequestForInvalidContractIdForContract();
            await placeOrder.ShouldReturnBadRequestForInvalidMessageIdForContract();
            await placeOrder.ShouldReturnBadRequestForInvalidPropertyForContract();
            await placeOrder.ShouldReturnSuccessForValidContract();
        }
    }
}
