using FoodyApiTests.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FoodyApiTests
{
    [TestFixture]
    public class FoodyTests
    {

        private RestClient client;
        private static string lastCreatedFoodId;

        private const string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86 ";

        [OneTimeSetUp]
        public void Setup()
        {
            string accessToken = GetJwtToken("MA1234", "MA1234");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(accessToken)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password) 
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });
            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);   

            return json.GetProperty("accessToken").GetString()?? string.Empty;    
          
        }

        [Order(1)]
        [Test]
        public void CreatedNewFood()
        {
            var createtFood = new
            {

                Name = "New food",
                Description = "New recipe",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(createtFood);

            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            //var responseObjectDTO = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            //var responseObject = JsonSerializer.Deserialize<JsonObject>(response.Content);
            //bool jsonObjectContainsKey = responseObject.ContainsKey("foodId");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Content, Does.Contain("foodId"));
            // Assert.That(responseObjectDTO, Has.Property("FoodId"));
            // Assert.IsTrue(jsonObjectContainsKey);
            // Assert.IsTrue(responseObject.ContainsKey("foodId"), $"JSON object should contain key '{"foodId"}'.");

            lastCreatedFoodId = json.GetProperty("foodId").GetString() ?? string.Empty; 
        }

        [Order(2)]
        [Test]
        public void EditTitleOfCreatedFood()
        {
            var editedFoody = new[]
            { 
                new
                {
                  path ="/name",
                  op = "replace",
                  value = "Edited Title"
                }
            };

             var request = new RestRequest($"/api/Food/Edit/{lastCreatedFoodId}", Method.Patch);
                 
            request.AddJsonBody(editedFoody);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"), "Editing food was unsuccessful.");

        }

        [Order(3)]
        [Test]
        public void GetAllFoods()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);
            var response = this.client.Execute(request);

            var responseAllFoods = JsonSerializer.Deserialize<List<FoodDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseAllFoods, Is.Not.Empty, "There is no foods.");
        }

        [Order(4)]
        [Test]

        public void DeleteFoodThatEdited()
        {
            var request = new RestRequest("/api/Food/Delete/{foodId}", Method.Delete);
            request.AddUrlSegment("foodId", lastCreatedFoodId);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"), "Deleting food was unsuccessful.");
        }

        [Order(5)]
        [Test]
        public void CreatedNewFoodWithoutARequiredFields()
        {
            var createtFood = new
            {

                Name = "",
                Description = "",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(createtFood);

            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditNonExistingFood()
        {
            string nonExistingFoodId = "123dfd";

            var editedFoody = new[]
            {
                new
                {
                  path ="/name",
                  op = "replace",
                  value = "Edited Title"
                }
            };

            var request = new RestRequest($"/api/Food/Edit/{nonExistingFoodId}", Method.Patch);

            request.AddJsonBody(editedFoody);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No food revues..."), "Editing food was unsuccessful.");
        }


        [Order(7)]
        [Test]
        public void DeleteNonExistingFood()
        {
            string nonExistingFoodId = "123dfd"; 

            var request = new RestRequest("/api/Food/Delete/{foodId}", Method.Delete);
            request.AddUrlSegment("foodId", nonExistingFoodId);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this food revue!"), "Deleting food was unsuccessful.");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}