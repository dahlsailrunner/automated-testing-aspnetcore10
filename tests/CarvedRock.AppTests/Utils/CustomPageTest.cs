using TUnit.Playwright;

namespace CarvedRock.AppTests.Utils;

public class CustomPageTest : PageTest
{
    [ClassDataSource<AppFixture>(Shared = SharedType.PerTestSession)]
    public required AppFixture Fixture { get; init; }

    // playwright browsers on linux don't play well with the self-signed certs
    // this override is really only to support CI pipelines
    public override BrowserNewContextOptions ContextOptions(TestContext testContext)
    {
        var options = base.ContextOptions(testContext);
        options.IgnoreHTTPSErrors = true;       

        return options;
    }

    public string WebAppUrl => Fixture.App.GetEndpoint("webapp").ToString();
}