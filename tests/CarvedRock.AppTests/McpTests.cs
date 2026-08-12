using System.Text.Json;
using ModelContextProtocol.Protocol;

namespace CarvedRock.AppTests;

[ClassDataSource<AppFixture>(Shared = SharedType.PerTestSession)]
public class McpTests(AppFixture fixture)
{
    public readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Test]
    public async Task GetToolsIncludesGetProducts()
    {
        var mcpClient = await fixture.GetAnonymousMcpClient();

        var tools = await mcpClient.ListToolsAsync();

        var getProductsTool = tools.FirstOrDefault(t => t.Name == "get_products");
        await Assert.That(getProductsTool).IsNotNull();

        var setPriceTool = tools.FirstOrDefault(t => t.Name == "set_product_price");
        await Assert.That(setPriceTool).IsNull();
    }

    [Test]
    public async Task CallGetProductsToolReturnsProducts()
    {
        var mcpClient = await fixture.GetAnonymousMcpClient();

        var getProductsResponse = await mcpClient.CallToolAsync("get_products");

        await Assert.That(getProductsResponse).IsNotNull()
            .And.Member(r => r.IsError, isErr => isErr.IsFalse().Or.IsNull());

        var productJson = getProductsResponse.Content
                        .First(c => c.Type == "text") as TextContentBlock;
        var products = JsonSerializer.Deserialize<List<ProductModel>>(
            productJson?.Text ?? "[]", CamelCaseOptions);

        await Assert.That(products).IsNotNull()
            .And.Contains(p => p.Name == "Alpine Trekker");
        // or check against fixture.InitialProducts
    }

    [Test]
    public async Task GetToolsAsAdminIncludesSetProductPrice()
    {
        var mcpClient = await fixture.GetAdminMcpClient();

        var tools = await mcpClient.ListToolsAsync();

        var setPriceTool = tools.FirstOrDefault(t => t.Name == "set_product_price");
        await Assert.That(setPriceTool).IsNotNull();

        var getProductsTool = tools.FirstOrDefault(t => t.Name == "get_products");
        await Assert.That(getProductsTool).IsNotNull();
    }

    [Test]
    public async Task SetPriceAsAdminWorks()
    {
        var mcpClient = await fixture.GetAdminMcpClient();

        var response = await mcpClient.CallToolAsync("set_product_price",
            new Dictionary<string, object?>
            {
                { "id", 22 },
                { "newPrice", 88.88 }
            });

        var responseJson = response.Content.First(c => c.Type == "text") as TextContentBlock;
        var opResult = JsonSerializer.Deserialize<OperationResult>(responseJson?.Text ?? "{}",
            CamelCaseOptions);
    }

    [Test]
    public async Task SetInvalidPriceAsAdminFails()
    {
        var mcpClient = await fixture.GetAdminMcpClient();

        var response = await mcpClient.CallToolAsync("set_product_price",
            new Dictionary<string, object?>
            {
                { "id", 22 },
                { "newPrice", 99_120.88 } // invalid for all product types
            });

        var responseJson = response.Content.First(c => c.Type == "text") as TextContentBlock;
        var opResult = JsonSerializer.Deserialize<OperationResult>(responseJson?.Text ?? "{}",
            CamelCaseOptions);

        await Assert.That(opResult).IsNotNull()
            .And.Member(r => r.Status, s => s.IsEqualTo("error"))
            .And.Member(r => r.Message, m => m.IsNotNull().And.Contains("validation error"));
    }
}
public record ProductModel(int Id, string Name, string Description, string Category, double Price);
public record OperationResult(string Status, string? Message = null);
