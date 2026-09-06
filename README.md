# Minioasis

A .NET 10 API backed by Koha. The current endpoint is `GET /api/v1/items/{id}`.

## Projects and conventions

Projects stay at the repository root. `Minioasis.Api` owns HTTP endpoints, response mapping, and English error text. `Minioasis.Application` owns use cases, gateway abstractions, application models, and error definitions. `Minioasis.Koha` implements the gateway and OAuth integration. Each production project has a corresponding test project.

Handwritten code is organized by role, then subdomain:

| Project | Folders |
| --- | --- |
| Application | Abstractions/Item, Models/Item, Results/Item, UseCases/Item, Errors/Item, Errors/Catalog, Errors/System; Results/Shared, Errors/Shared, Extensions/Shared |
| API | Endpoints/Item; Abstractions/Shared, Endpoints/Shared, Models/Shared, Services/Shared, Mappers/Shared, Extensions/Shared; Resources |
| Koha | Adapters/Item; Abstractions/Authentication, Services/Authentication, Handlers/Authentication, Exceptions/Authentication; Configuration/Shared, Extensions/Shared |

Every top-level type belongs in its own file, named after the type. Namespaces follow folders. Private nested implementation types and test helpers stay with their owner. Test folders mirror production; reusable helpers live in `TestDoubles/<subdomain>`, with one top-level type per file. Generic HTTP test doubles use `TestDoubles/Shared`; OAuth doubles use `TestDoubles/Authentication`.

Item contains the current item-retrieval capability, including `IItemCatalogGateway`, `ItemLookupResult`, and `ItemLookupOutcome`. Catalog contains errors that can apply across catalog operations. Shared contains machinery used across subdomains, such as application results and error presentation. Add a subdomain folder beneath a role when it has code to contain; do not create empty placeholders for future sprints.

`Minioasis.Koha/Generated` is NSwag-owned output and is excluded from these splitting and formatting conventions. Edit generation inputs in `contracts` and `nswag.json`, not generated C# files. Project paths and the generation configuration remain unchanged.

The API composes the Application and Koha registrations through their service-collection extensions. Application does not depend on API or Koha implementations. HTTP status codes and message presentation belong to the API.

## Adding an error

1. Add a `public static readonly ApplicationErrorDefinition` to the appropriate group in `Minioasis.Application/Errors/<subdomain>`. Each definition supplies its stable string identifier and `ApplicationErrorCategory` exactly once. For a new domain, create a separate static group and annotate it with `[ErrorDefinitions]`. The definition and registry machinery lives in `Errors/Shared`.
2. Add the matching key to `Minioasis.Api/Resources/errors.en.json`, with nonblank `title` and `detail` values.
3. Return an `ApplicationError` constructed from that definition. Supply metadata when a detail contains a placeholder such as `{id}`.

For example, a use case can return:

```csharp
ApplicationResult<ItemDetails>.Failure(
    new ApplicationError(ItemErrors.NotFound,
        new Dictionary<string, string> { ["id"] = id }));
```

`ApplicationErrorDefinitionRegistry` discovers marked groups in the Application assembly once during API registration. It reads only declared public static readonly fields of the definition type. There is no separately maintained identifier checklist. Blank identifiers, duplicate identifiers, null definitions, invalid categories, and an empty registry are rejected.

`ErrorMessageProvider` loads the English resource during registration. Resource keys must match definitions exactly, using ordinal, case-sensitive comparison. Missing, unknown, duplicate, malformed, null, or incomplete messages prevent startup and identify the offending key where available. Remove the JSON entry when removing a definition.

The HTTP mapper derives status from the error category: validation 400, not found 404, invalid dependency response 502, unavailable dependency 503, and dependency authentication/unexpected failures 500. It preserves the existing `code` and `traceId` problem fields. Metadata substitution remains literal; an unsupplied placeholder remains in the text. Only English is currently supported.

## Build, test, and run

### Docker

Install Docker with Linux container support. Fill in the Koha URL, client ID, and client secret in the root `.env` file (copy `.env.example` if it is missing). Keep values single-quoted to preserve literal `$` characters. `.env` is excluded from Git and the Docker build context; credentials are supplied when the container starts.

From the repository root:

```powershell
docker compose up --build -d
docker compose logs -f api
```

The API listens at `http://localhost:8080`; for example, `GET http://localhost:8080/api/v1/items/123`. It runs in Production, so the development OpenAPI endpoint is disabled. Compose rejects missing or empty credentials before starting. When Koha runs on your Windows host, use `host.docker.internal` instead of `localhost` in its URL.

After changing credentials, run `docker compose up -d` to recreate the container with the updated configuration. To stop it, run `docker compose down`.

### Local .NET SDK

Install the .NET 10 SDK. From the repository root:

```powershell
dotnet restore minioasis.slnx
dotnet test minioasis.slnx --no-restore
dotnet publish Minioasis.Api/Minioasis.Api.csproj -c Release --no-restore
```

The publish output includes `Resources/errors.en.json`. Tests use fakes and do not require a live Koha server.

API tests create and dispose their own `TestInfrastructure/Shared/ApiFactory`. Its optional service-registration callback runs after production registrations through `ConfigureTestServices`. Item tests register `TestDoubles/Item/FakeGetItemUseCase`; future endpoint tests can supply their own use-case fakes through the same factory. The factory supplies dummy Koha settings in host configuration before application registration, with optional per-factory setting overrides. Tests do not set process-wide environment variables.

To run against Koha, provide `MINIOASIS_KOHA_BASE_URL`, `MINIOASIS_KOHA_CLIENT_ID`, and `MINIOASIS_KOHA_CLIENT_SECRET` in the process environment, then run `dotnet run --project Minioasis.Api`. Keep secrets outside source control. OpenAPI is exposed in Development.

`Program` passes `builder.Configuration` into `AddMinioasisKoha`. Koha reads the same setting names through `IConfiguration`, preserving required-value checks and base-URL normalization during registration. ASP.NET's default configuration includes environment variables; each test host can supply its own settings without modifying them.

This organization preserves the HTTP contract but changes C# namespaces to follow the role/subdomain layout. Consumers must update imports, rename `CatalogLookupResult` and `CatalogLookupOutcome` references to `ItemLookupResult` and `ItemLookupOutcome`, and pass configuration to `AddMinioasisKoha`. `ApplicationError` continues to accept an error definition.
