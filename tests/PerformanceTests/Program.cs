using Duende.IdentityModel.Client;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Data;
using NBomber.Data.CSharp;
using NBomber.Http;
using NBomber.Http.CSharp;

using var httpClient = new HttpClient();

// TODO: possibly turn these into command line parameters?
var apiUrl = "https://localhost:7213/";
var authUrl = "https://demo.duendesoftware.com/";

string _globalToken = string.Empty;
IDataFeed<string> categories = DataFeed.Random(["all", "boots", "equip", "kayak"]);
IDataFeed<int> productIds = DataFeed.Random(Enumerable.Range(1, 50).ToArray());

var listScenario = Scenario.Create("get_product_list", async context =>
{
    var category = categories.GetNextItem(context.ScenarioInfo);
    var request = Http.CreateRequest("GET", $"{apiUrl}product?category={category}");

    return await Http.Send(httpClient, request);
})
.WithoutWarmUp()
.WithLoadSimulations(
    Simulation.RampingInject(rate: 1000, // 2000 caused some failures
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(120)),
    Simulation.RampingInject(rate: 100,
                             interval: TimeSpan.FromSeconds(1),
                             during: TimeSpan.FromSeconds(30))
);

var singleScenario = Scenario.Create("get_single_product", async context =>
{
    var id = productIds.GetNextItem(context.ScenarioInfo);
    var request = Http.CreateRequest("GET", $"{apiUrl}product/{id}")
                      .WithHeader("Authorization", $"Bearer {_globalToken}");

    return await Http.Send(httpClient, request);
})
.WithInit(async context =>
{
    var token = await httpClient.RequestClientCredentialsTokenAsync(
        new ClientCredentialsTokenRequest
        {
            Address = $"{authUrl}connect/token",
            ClientId = "m2m",
            ClientSecret = "secret",
            Scope = "api",
        });
    _globalToken = token.AccessToken!;
})
.WithoutWarmUp()
.WithLoadSimulations(
    Simulation.Inject(rate: 1000, // 2500 caused some failures
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(120)),
    Simulation.RampingInject(rate: 100,
                             interval: TimeSpan.FromSeconds(1),
                             during: TimeSpan.FromSeconds(30))
);

NBomberRunner
    .RegisterScenarios(listScenario, singleScenario)
    .WithWorkerPlugins(new HttpMetricsPlugin())
    .WithReportFormats(
        ReportFormat.Csv, ReportFormat.Html,
        ReportFormat.Md, ReportFormat.Txt
    )
    .WithTestSuite("CarvedRock")
    .WithTestName("API GET Requests")
    .Run();
