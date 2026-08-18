namespace CarvedRock.AppTests;

public partial class WebAppTests : CustomPageTest
{
    [Test]
    public async Task HomePageWorks()
    {
        await Page.GotoAsync(WebAppUrl);

        await Expect(Page).ToHaveTitleAsync("Carved Rock Fitness");

        var bannerTextLocator = Page.GetByText("GET A GRIP");
        await Expect(bannerTextLocator).ToBeVisibleAsync();
    }
}
