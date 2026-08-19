using TUnit.Core.Interfaces;
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

        if (testContext.StateBag.ContainsKey(RecordVideoAttribute.StateBagKey))
        {
            options.RecordVideoDir = "playwright-artifacts/";

            options.ViewportSize = new ViewportSize
            { Width = 1280, Height = 1400 };
        }

        return options;
    }

    public string WebAppUrl => Fixture.App.GetEndpoint("webapp").ToString();
}

public static class PageExtensions
{
    public static async Task Login(this IPage page, string username, string password)
    {
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Username" })
                                .FillAsync(username);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" })
                                .ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" })
                                .FillAsync(password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
    }
}

// The StateBag is used to avoid reflection and use TUnit source generation approach
[AttributeUsage(AttributeTargets.Method)]
public sealed class RecordVideoAttribute : Attribute, ITestDiscoveryEventReceiver
{
    internal const string StateBagKey = "CarvedRock.RecordVideo";

    public ValueTask OnTestDiscovered(DiscoveredTestContext discoveredTestContext)
    {
        discoveredTestContext.TestContext.StateBag[StateBagKey] = true;
        return default;
    }
}