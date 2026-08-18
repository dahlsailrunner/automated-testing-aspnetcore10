using TUnit.Playwright;

namespace CarvedRock.AppTests;

[ClassDataSource<AppFixture>(Shared = SharedType.PerTestSession)]
public partial class WebAppTests(AppFixture fixture) : PageTest
{
    private readonly string WebAppUrl = fixture.App.GetEndpoint("webapp")!.ToString();

    [Test]
    public async Task HomePageWorks()
    {
        await Page.GotoAsync(WebAppUrl);

        await Expect(Page).ToHaveTitleAsync("Carved Rock Fitness");

        var bannerTextLocator = Page.GetByText("GET A GRIP");
        await Expect(bannerTextLocator).ToBeVisibleAsync();
    }
}
