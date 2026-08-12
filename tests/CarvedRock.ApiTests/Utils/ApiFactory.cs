using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TUnit.AspNetCore;
using TUnit.Core.Logging;

namespace CarvedRock.ApiTests.Utils;

public class ApiFactory : TestWebApplicationFactory<Program>
{
    [ClassDataSource<TestData>(Shared = SharedType.PerTestSession)]
    public TestData TestData { get; init; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services => services
               .AddAuthentication(TestAuthHandler.SchemeName)
               .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>
                                (TestAuthHandler.SchemeName, _ => { }));
    }

    protected override void ConfigureStartupConfiguration(
                        IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:CarvedRockPostgres", 
                        TestData.DbContainer.GetConnectionString() }
        });
    }
}

public abstract class ApiTestsBase : WebApplicationTest<ApiFactory, Program>
{
    protected TestData TestData => GlobalFactory.TestData;
    protected static DefaultLogger TestLogger => TestContext.Current!.GetDefaultLogger();
}