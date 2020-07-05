using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shouldly;
using static SalesOrderApp.Testing;

namespace SalesOrderApp
{
    public class PlaceOrderTests
    {
        public static HttpClient Client = new HttpClient();
        private readonly TotemSettings _totemSettings;

        public PlaceOrderTests()
        {
            _totemSettings = Testing.GetTotemApplicationConfigurationSettings();
        }
        
        public async Task ShouldReturnSuccessForValidContract()
        {
            var placeOrder = new PlaceOrder
            {
                OrderId = "123abc",
                ItemName = "Frying Pan",
                OrderDate = DateTime.Now
            };

            var placeOrderMessage = new PlaceOrderContract
            {
                ContractId = Guid.Parse(_totemSettings.ValidPlaceOrderContractId),
                Message = new PlaceOrderMessageDetails
                {
                    Id = Guid.NewGuid(),
                    ItemName = placeOrder.ItemName,
                    OrderDate = placeOrder.OrderDate,
                    OrderId = placeOrder.OrderId,
                    Timestamp = DateTime.Now
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(placeOrderMessage), Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                response = await Client.PostAsync(_totemSettings.TestMessageApiUrl, stringContent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            await DisplayErrors(response);
            response.EnsureSuccessStatusCode();
        }

        public async Task ShouldReturnBadRequestForInvalidMessageIdForContract()
        {
            var placeOrder = new PlaceOrder
            {
                OrderId = "456def",
                ItemName = "Griddle",
                OrderDate = DateTime.Now
            };

            var placeOrderMessage = new PlaceOrderContract
            {
                ContractId = Guid.Parse(_totemSettings.ValidPlaceOrderContractId),
                Message = new PlaceOrderMessageDetails
                {
                    Id = null,
                    ItemName = placeOrder.ItemName,
                    OrderDate = placeOrder.OrderDate,
                    OrderId = placeOrder.OrderId,
                    Timestamp = DateTime.Now
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(placeOrderMessage), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(_totemSettings.TestMessageApiUrl, stringContent);
            await DisplayErrors(response);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldContain("Message does not match contract");
        }

        public async Task ShouldReturnBadRequestForInvalidContractIdForContract()
        {
            var placeOrder = new PlaceOrder
            {
                OrderId = "678ghi",
                ItemName = "Nu wave oven",
                OrderDate = DateTime.Now
            };

            var invalidGuid = Guid.NewGuid();

            var placeOrderMessage = new PlaceOrderContract
            {
                ContractId = invalidGuid,
                Message = new PlaceOrderMessageDetails
                {
                    Id = Guid.NewGuid(),
                    ItemName = placeOrder.ItemName,
                    OrderDate = placeOrder.OrderDate,
                    OrderId = placeOrder.OrderId,
                    Timestamp = DateTime.Now
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(placeOrderMessage), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(_totemSettings.TestMessageApiUrl, stringContent);
            await DisplayErrors(response);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            responseString.ShouldContain("Unable to test message; confirm that your request message is valid JSON.");
        }

        public async Task ShouldReturnBadRequestForInvalidPropertyForContract()
        {
            var placeOrder = new PlaceOrder
            {
                OrderId = "901jki",
                ItemName = "Cutting board",
                OrderDate = DateTime.Now
            };

            var placeOrderMessage = new PlaceOrderContract
            {
                ContractId = Guid.Parse(_totemSettings.ValidPlaceOrderContractId),
                Message = new MessageDetailsWithAdditionalField
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    InvalidField = "this should error out"
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(placeOrderMessage), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(_totemSettings.TestMessageApiUrl, stringContent);
            await DisplayErrors(response);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var responseString = await response.Content.ReadAsStringAsync();
            var messagePropertyInvalidfieldIsNotPartOfTheContract = "Message property \\\"InvalidField\\\" is not part of the contract.";

            var r = responseString.Contains(messagePropertyInvalidfieldIsNotPartOfTheContract);

            responseString.ShouldContain(messagePropertyInvalidfieldIsNotPartOfTheContract);
        }
    }

    public class PlaceOrderContract : ITotemTemplate
    {
        public Guid ContractId { get; set; }
        public IMessageDetails Message { get; set; }
    }

    public class PlaceOrderMessageDetails : IMessageDetails
    {
        public Guid? Id { get; set; }
        public string ItemName { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MessageDetailsWithAdditionalField : IMessageDetails
    {
        public string InvalidField { get; set; }
        public Guid? Id { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PlaceOrder
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string ItemName { get; set; }
    }
}