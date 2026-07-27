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

## Data and EF Core Migrations

The `dotnet ef` tool is used to manage EF Core migrations.  The following command was used to create migrations (from the `CarvedRock.Data` folder).

```bash
dotnet ef migrations add Initial -s ../CarvedRock.Api
```

The application uses PostgreSQL. Data is seeded by the `SeedData.json` contents in the `Data` project
