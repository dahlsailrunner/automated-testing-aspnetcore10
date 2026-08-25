# Automated Testing Strategies for ASP.NET Core 10

This repo is meant to help with the understanding of automated testing
for ASP.NET Core 10, which is often part of a distributed application.
Key concepts include:

* Unit tests for business logic
* Integration tests using a variety of techniques:
  * `WebApplicationFactory` for in-process web app testing
  * TestContainers for mocked resources like databases
  * WireMock for mocked external API calls
  * Aspire's `DistributedApplicationTestingBuilder` for more complete integration testing
* End-to-end testing with Playwright
* Coverage reporting and CI pipeline
* Including tests in agentic development workflows

## Getting Started

You need the [Aspire prerequisites](https://aspire.dev/get-started/prerequisites/).

### VS Code Setup

You need the following extension:

* [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

Then just hit `F5` to run the app.

The [Aspire CLI](https://aspire.dev/get-started/install-cli/#install-the-aspire-cli) is highly recommended, along with the [Aspire VS Code Extension](https://aspire.dev/get-started/aspire-vscode-extension/).

### AI

It's not required, but if you want to use the AI features in the app, you
need to have an OpenAI key (use it to set the `openaiKey` parameter).

## Features

### UI

* The `Footwear`, `Equipment`, and `Kayaks` links will show listings of products
* The `Bad News` link will show an error page
* Authentication is required to all but the home page
* Products shown in listing pages can be added to the cart
* A shopping cart page shows products that have been added to the cart for the logged-in
  user
* The cart can be cleared
* Placing an order will save the order and send an email
* An `Admin` page is available for "bob" (the admin user)
* Navigating to `/admin` after logging in as `alice` will show an unauthorized page
* Product updates can be made on the admin page
* An AI chat is on the product listing page that can interact with the MCP server (see below) -- it uses the `agent` API

### API
  
* `GET /product` based on category (or "all") and by id allow anonymous requests
* `POST|PUT|DELETE /product` require authentication and an `admin` role (available with the `bob` login, but not `alice`)
* Validation with [FluentValidation](https://docs.fluentvalidation.net/en/latest/index.html) - try the `POST /product` method with a duplicate name or very high price
* A `GET /product` with a `category` of something other than "all", "boots", "equip", or "kayak" will throw an error
* An order can be placed with a `POST /order` request
* Shopping cart can be updated via API `/cart GET|POST|DEL`

### MCP

* Tools to get the list of products and to get a single product are available
  even to anonymous users
* A tool to delete a product is available to admin users
* A tool to update the product price is available to admin users

## Testing

This repo features automated testing, and it includes coverage reporting and CI pipeline integration.

Here are some helpful links for more information about various topics and techniques used:

* [TUnit](https://tunit.dev/docs/intro): The testing framework used, which focuses on an easy-to-read API and performance
* [Coverage configuration](https://github.com/microsoft/codecoverage/blob/main/docs/configuration.md): Excluding things that you don't intend to cover and other such configuration
* [CI Pipeline - Test Results](https://github.com/EnricoMi/publish-unit-test-result-action): How to include test results in CI pipeline summaries
* [CI Pipeline - Coverage Reporting](https://reportgenerator.io/usage): How to include coverage details in CI pipeline summaries (and more)

For the reportgenerator installation, use the following command:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool

```

### Recommendations

* Make your results easy to review - both pass rate and coverage percentage
* Even if initial coverage is low, get the full framework / pipeline established, then focus on improving the numbers
* [Exclude things](https://github.com/microsoft/codecoverage/blob/main/docs/configuration.md) that you don't intend to cover (e.g. source-generated code, EF core migrations, auto-properties, etc)
* Use [categories to easily segment tests](https://tunit.dev/docs/execution/test-filters) that you might want to run in isolation
* Run tests locally during development activities, ***and*** during CI pipelines / PR gates
* Don't skip tests - update them to be accurate or remove them
* Test things that matter and truly evaluate the behavior of your application
* Make execution as fast as you need it (in general, the faster, the better)

### Notes

* **Regarding `TUnit.Mocks.Http`:** If you can provide a replacement for an `HttpClient`,
    it's nice to use the TUnit library. If you just want to override the URL,
    then you should continue the use of WireMock.
* **TestContainers - flaky tests?** I've had some inconsistency with TestContainers
    and at least its PostgreSQL package on my local Windows machine, mostly with
    intermittent errors during startup and readiness of the PostgreSQL container.
    This may be to some corporate policy doing packet inspection on my laptop, but
    the tests have been consistently reliable in the CI pipeline.
* **`RecordVideo` attribute:** I created an attribute called `RecordVideo` that
    makes it easy to record a video on a Playwright test. This isn't strictly needed
    (you can still create videos using the code in the Playwright docs), but I
    find this a super easy way to enable that for a test.
* **Multiple test projects:** You may be wondering about the different test
    projects. With top-level statements, referring to a `Program` can be a
    bit ambiguous, and rather than switch from top-level statements, or fight
    the tooling, I opted to have multiple test projects.  

### Additional Tests to Create

Here are some additional tests that should be created to improve
coverage - they were intentionally ommitted so that you could
practice with creating different types of tests. If you add tests
for everything below, your coverage should be pretty close to 100%.

#### UnitTests

* Field length validations (e.g. name too long)
* Invalid `ImgUrl` property
* Negative price for a product

#### ApiTests

* Use API to clear cart (both with contents and empty)
* Verify that get product by id returns `NOT FOUND` when invalid id is passed
* Same `NOT FOUND` response should be returned for update and delete operations with invalid id
* Determine if possible to create a validation error on a product that isn't
  tied to a field / property on the product.  If not, remove uncovered code
  in the `ValidationExceptionHandler`
* Force a bad request on an order

#### AppTests

> [!TIP]
> Use the Playwright `codegen` tool for these!

* Add a test to verify that `alice` cannot use the same
  `/admin delete product 10` prompt successfully in the chat (she's
  not an admin and shouldn't be allowed to do this)
* Check the Current Promotion page with the link area on the home
  page (will require user to log in when clicked)
* Click the `Bad News` button on the home page - this should redirect to
  an error page that can be validated
* Try the UI pages for the Admin of products (create, update, delete) - both
  with and without validation errors
* Log in as alice (not an admin). Confirm that the Admin nav button is not
  visible.  Also try navigating to `/admin` and verify the Access Denied page
  is shown
* Use the chat as `bob` the admin with `/admin set price of desert trekker to 104.21`
  and confirm the results (and probably verify that `alice` cannot do this)
* After logging in, log out and verify the logged out page

#### Extra Credit

Use [Microsoft.Extentions.AI.Evaluation](https://developer.microsoft.com/blog/put-your-ai-to-the-test-with-microsoft-extensions-ai-evaluation/)
to test the quality of chat responses for a customer chat.

From a product listing page, use a prompt like the following in the
chat:

```txt
Recommend products for a forest hike
```

The response will list three (verify count of three) products that
could be confirmed to be real products from the original list - with
the correct prices, names, and probably descriptions.  Modify the prompt
and experiment with your results!

## Data and EF Core Migrations

The `dotnet ef` tool is used to manage EF Core migrations.  The following command was used to create migrations (from the `CarvedRock.Data` folder).

```bash
dotnet ef migrations add Initial -s ../CarvedRock.Api
```

The application uses PostgreSQL. Data is seeded by the `SeedData.json` contents in the `Data` project
