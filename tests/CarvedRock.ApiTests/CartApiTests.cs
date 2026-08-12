namespace CarvedRock.ApiTests;

public class CartApiTests : ApiTestsBase
{
    [Test]
    public async Task GetEmptyCartWorks()
    {
        var client = Factory.CreateClient();
        client.AddCustomerAuthHeaders();

        var response = await client.GetAsync("/cart");
        var jsonContent = await response.Content.ReadAsStringAsync();

        // var logger = TestContext.Current!.GetDefaultLogger();
        TestLogger.LogInformation($"Got the following response: --{jsonContent}--");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(jsonContent).IsEqualTo("[]"); // empty array
    }

    [Test]
    public async Task AddToCartWorks()
    {
        var client = Factory.CreateClient();
        client.AddCustomerAuthHeaders();

        var randomProduct = TestData.GeneralFaker.PickRandom(TestData.InitialProducts);

        var response = await client.PostAsJsonAsync("/cart",
                                new CartItem(randomProduct.Id, 1));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var cartCountResponse = await client.GetAsync("/cart/count");
        await Assert.That(cartCountResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var cartCount = await cartCountResponse.Content.ReadAsStringAsync();
        await Assert.That(Convert.ToInt32(cartCount)).IsEqualTo(1);

        // for you: assert about cart contents - different api call
    }
}

public record CartItem(int ProductId, int Quantity);