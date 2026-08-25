using Bogus;
using CarvedRock.Core;
using CarvedRock.Data;
using CarvedRock.Domain.Mapping;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace CarvedRock.ApiTests.Utils;

public class TestData : IAsyncInitializer, IAsyncDisposable
{
    public PostgreSqlContainer DbContainer { get; } =
        new PostgreSqlBuilder("postgres:18.3")
            .Build();

    public string ConnectionString => DbContainer.GetConnectionString() + ";SSL Mode=Disable";

    public List<Data.Entities.Product> InitialProducts { get; private set; } = null!;

    public readonly Faker<NewProductModel> NewProductFaker = new Faker<NewProductModel>()
        .UseSeed(2001) // will generate consistent data (with any fixed seed value)
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.Category, f => f.PickRandom("boots", "equip", "kayak"))
        .RuleFor(p => p.Price, (f, p) =>
                p.Category == "boots" ? f.Random.Double(50, 300) :
                p.Category == "equip" ? f.Random.Double(20, 150) :
                p.Category == "kayak" ? f.Random.Double(100, 500) : 0)
        .RuleFor(p => p.ImgUrl, f => f.Image.PicsumUrl());

    public readonly Faker GeneralFaker = new();
    public async Task InitializeAsync()
    {
        await DbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<LocalContext>()
                            .UseNpgsql(ConnectionString)
                            .Options;
        var context = new LocalContext(options);

        // Any data prep / migrations / setup can go here

        //context.MigrateAndCreateData(force: true); // or completely customize!!!
        await context.Database.EnsureCreatedAsync();

        var products = NewProductFaker.Generate(100);
        var productMapper = new ProductMapper();

        List<Data.Entities.Product> productsToCreate = [];
        foreach (var product in products)
        {
            productsToCreate.Add(productMapper.NewProductModelToProduct(product));
        }

        context.Products.AddRange(productsToCreate);
        await context.SaveChangesAsync();

        InitialProducts = await context.Products.ToListAsync();
    }

    public async ValueTask DisposeAsync()
    {
        // clean up / reset persistent data?
        GC.SuppressFinalize(this);
    }
}
