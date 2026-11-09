# Minioasis development charter

## Independent engineering judgment

Independent and critical thinking is the primary engineering principle for this project. Evaluate requests against the project's goals, repository evidence, correctness, security, maintainability, and simplicity. Do not blindly accept a proposal because the user suggested it, because existing code follows it, or because an AI previously recommended it.

When a proposed approach has material drawbacks, explain the concern candidly, describe the consequences, and recommend a better alternative before implementation. Distinguish verified facts from assumptions, investigate questions the repository can answer, and ask focused questions when intent or tradeoffs remain unclear. Reconsider your own recommendations when evidence or requirements change.

Challenge proportionately using concrete reasoning. Avoid manufacturing objections, adding speculative complexity, or repeatedly reopening settled decisions. Once the user has considered the tradeoffs and chosen an approach, proceed within the agreed scope. Raise new material findings when they affect that decision.

Exercise this judgment throughout planning, implementation, testing, and review, within the repository boundaries and approval workflow.

## Purpose and direction

Minioasis is a library application for library staff, developed on .NET 10. Its product scope covers circulation and catalogue workflows. It should help staff carry out library work through coherent application operations, with business rules and integration behavior that remain understandable as the application grows.

Koha is authoritative for library records and transactions. Minioasis owns the staff workflows and application-specific rules that use those records and operations. Keep this ownership explicit when designing a capability so that validation, decisions, and updates have a clear home. Additional storage or integration requires an approved design that identifies its purpose and ownership.

Develop capabilities incrementally within this direction. Product scope describes the intended application, while source code, contracts, and tests establish what is implemented. Confirm existing behavior before extending it and keep implementation claims accurate. A new circulation or catalogue workflow requires its own agreed behavior and approved implementation plan.

## Architecture and responsibility

Projects remain at the repository root. Their boundaries separate application behavior, HTTP presentation, and communication with Koha so each can evolve without spreading provider or transport details throughout the application.

| Project | Responsibility |
| --- | --- |
| `Minioasis.Application` | Staff use cases, business models and rules, validation, external-system interfaces, results, and stable typed error definitions. |
| `Minioasis.Api` | HTTP endpoints, request and response mapping, Problem Details, English error text, and application composition. |
| `Minioasis.Koha` | Implementations of Application-owned gateway interfaces, Koha protocols, generated clients, OAuth, and provider-specific translation. |

Application defines the business meaning of an operation and the capabilities it needs from external systems. Place workflow decisions and application-specific validation in its use cases. Declare gateway interfaces in Application using Application-owned inputs and outputs. Application must not reference API, Koha implementations, ASP.NET/HTTP types, provider URLs, or generated DTOs.

API endpoints translate incoming HTTP requests into Application operations and map their results into HTTP responses. Keep endpoints thin. API is the composition root that connects interfaces to implementations through dependency injection. HTTP status selection, response shape, and presentation text belong here.

Koha implements the Application-owned interfaces. It translates identifiers and models to Koha's protocol, performs external calls, and translates provider data and recognized failures back into Application results. Keep generated DTOs and provider exceptions inside the integration boundary. Preserve the dependency direction from API to Application and Koha, and from Koha to Application, unless an approved plan explicitly changes it.

Choose a component's home by the responsibility it owns. Keep feature behavior with its feature, and extract shared infrastructure when demonstrated reuse justifies it. Preserve existing routes, status codes, JSON shapes, Problem Details fields, and error text unless an approved task explicitly changes the public contract. Additional databases, caches, queues, providers, other infrastructure, or dependencies must be necessary and included in the approved plan.

## Feature organization and naming

Organize handwritten code first by feature or subdomain, then by technical responsibility within each project. This keeps a capability's related code discoverable while making each component's role explicit. Use `Shared/<Role>` for infrastructure used across features and `Authentication/<Role>` for authentication concerns. Create a folder when it has code to contain.

The following paths illustrate the convention. Feature and operation names stand for the capability being implemented.

```text
Minioasis.Application/<Feature>/Requests/<Operation>Request.cs
Minioasis.Application/<Feature>/Responses/<Operation>Response.cs
Minioasis.Application/<Feature>/UseCases/<Operation>UseCase.cs
Minioasis.Application/<Feature>/Abstractions/I<Capability>Gateway.cs
Minioasis.Api/<Feature>/Endpoints/<Feature>Endpoints.cs
Minioasis.Koha/<Feature>/Adapters/Koha<Capability>Adapter.cs
```

Names should identify the business concept or capability and its technical responsibility. Use the following conventions where the responsibility applies.

| Responsibility | Naming convention |
| --- | --- |
| Input to a use case | `<Operation>Request` |
| Successful output data from a use case | `<Operation>Response` |
| Success/failure container | `<Scope>Result` |
| Enumerated operation outcomes | `<Operation>Outcome` |
| Identifier or value object | The domain concept it represents, with `Id` for an identifier |
| Application operation | `<Operation>UseCase` |
| External-system capability interface | `I<Capability>Gateway` |
| Provider-specific implementation | `<Provider><Capability>Adapter` |
| HTTP route registration | `<Feature>Endpoints` |
| Conversion between representations | `<Source><Target>Mapper` |
| Request pipeline behavior | `<Purpose>Handler` |
| Resource acquisition | `<Resource>Provider` |
| Configuration values | `<Integration>Options` |
| Object construction | `<CreatedType>Factory` |
| Reusable error definitions | `<Feature>ErrorDefinitions` |
| Error occurrence | `<Scope>Error` |
| Exception | `<Failure>Exception` |

A request carries input to an operation. A response carries its successful data. A result describes success or failure and may contain that response or an error. A typed identifier or value object represents a domain concept. Keep those distinctions visible in signatures and filenames. Prefix interfaces with `I` and name them after their capability. Prefer specific responsibility names over vague catch-all names.

Every production or reusable shared-test top-level type belongs in its own file named after the type. Namespaces follow folders. Private nested implementation types and test helpers may remain with their owner. Generated code follows its generator's layout and naming.

Use braces around control-flow statement bodies in handwritten production code and tests, including single-statement `if`, `else`, and loop bodies. Place opening and closing braces on their own lines, following `.editorconfig`. Keep the enclosed statements and their execution order intact when applying this style.

Test folders mirror production feature and role organization. Place feature-specific reusable doubles under `<Feature>/TestDoubles` and cross-feature doubles under `Shared/TestDoubles`. Apply these conventions to new handwritten code. Migrate existing names and layouts through explicitly approved work, keeping references, namespaces, tests, and discovery behavior consistent.

## Results and error reporting

Application owns stable typed error definitions and the meaning of use-case outcomes. API owns presentation and HTTP status mapping. Translate recognized provider failures at the gateway boundary, then let the use case select the Application result. Return expected unsuccessful outcomes explicitly so callers can handle them consistently. Distinguish actionable application outcomes from failures the user cannot resolve. Explain invalid input, missing records, and expected workflow restrictions sufficiently for the client to guide the user. Preserve stable public error codes for these outcomes; clients should use codes rather than matching English messages.

To add an error, declare a `public static readonly ApplicationErrorDefinition` in the relevant feature's error-definition group. Mark a new group with the existing `[ErrorDefinitions]` attribute so registry discovery includes it. For a client-visible category (currently validation or not found), add the exact matching, case-sensitive key to `Minioasis.Api/Resources/errors.en.json` with nonblank `title` and `detail` values. Do not add resource entries for internal server-error categories. Return an `ApplicationError` built from the definition, supplying metadata for client-message placeholders where needed. When introducing another actionable category, explicitly design its public response and include it in resource validation.

The JSON resource must contain exactly the client-visible definitions from the registry. Preserve startup rejection of blank or duplicate identifiers, null definitions, invalid categories, and empty registries. Reject missing client-visible messages, entries for internal errors, and unknown, duplicate, malformed, null, or incomplete messages. Validate this at startup so inconsistent error definitions or presentation fail before requests depend on them.

Preserve the category mapping of validation to 400, not found to 404, invalid dependency response to 502, unavailable dependency to 503, and dependency authentication or unexpected failures to 500. Preserve the `code` and `traceId` Problem Details extensions. Only English is currently supported.

Every server-error response (500, 502, or 503) must use the title `Internal error` and public code `internal_error`, omit `detail`, and include the request's `traceId`. Do not expose provider names, internal error codes, authentication diagnoses, exceptions, or other technical explanations. Keep actionable errors' titles, details, and public codes. Return JSON with `application/problem+json` and omit `type` and `instance` links from error bodies. Keep this response policy consistent for returned Application failures and unhandled exceptions in Development and Production. Preserve caller cancellation rather than converting abandoned requests into internal errors.

Record the actual internal error code for recognized server failures and the exception for unexpected failures in server logs, together with the same request `traceId` returned to the client. Keep credentials and access tokens out of diagnostic output. Internal error definitions remain available for diagnostics even when the public code is masked. Define the generic server response once in the API response factory; keep the English resource exclusively for client-visible errors and maintain startup validation of that subset.

## Koha contracts and generated code

Base provider operations on the checked-in Koha contract. `contracts/official_koha_openapi.json` is an immutable reference. Never edit it. Derive focused contracts for the required operations from that reference and keep each integration change consistent with its source contract. If a required operation is absent or ambiguous, stop and ask rather than inventing a protocol.

`Minioasis.Koha/Generated` is owned by NSwag. Do not hand-edit generated files. When an approved task changes a focused contract or `nswag.json`, restore the local tool and regenerate the affected client. These commands illustrate generation for the existing focused contracts.

```powershell
dotnet tool restore
dotnet tool run nswag run nswag.json /variables:Contract=items,Feature=Items,FeatureSingular=Item
dotnet tool run nswag run nswag.json /variables:Contract=oauth,Feature=OAuth,FeatureSingular=OAuth
```

For another approved focused contract, use the matching contract filename stem and intended generated feature/type names as the variables. Run only the generation relevant to the changed contract, inspect the generated diff, and commit the source contract, generator configuration when applicable, and resulting generated files together. Keep generated Koha types inside `Minioasis.Koha`.

## Authentication, cancellation, and external failures

Koha authentication is an integration responsibility shared by the workflows that call it. Preserve these authentication guarantees unless an approved plan explicitly changes authentication behavior. Use OAuth 2 client credentials with a least-privilege Koha service identity. Keep credentials outside source control and diagnostic output. Use separate HTTP clients for token acquisition and protected API calls. The token client must not use the bearer handler.

Validate that acquired access tokens are nonblank, have a positive lifetime, and use the Bearer token type. Cache tokens until shortly before expiration and coordinate concurrent refreshes so callers can share a usable token safely.

On a protected GET returning 401, invalidate the cached token, recreate the request, and retry exactly once. Never replay non-GET requests automatically and never loop retries. For state-changing workflows, define partial-failure and retry behavior in the approved design with Koha's transaction ownership in mind.

Pass cancellation through endpoints, use cases, gateway interfaces, token acquisition, and HTTP operations. Propagate caller cancellation. Bound external work and account for authentication, retries, and response consumption when designing deadlines. Preserve the distinction between caller cancellation and a dependency failure. Map token network, timeout, validation, and other acquisition failures through the existing authentication error path to HTTP 500 unless an approved plan explicitly changes authentication behavior.

## Testing principles

Test behavior at its owning boundary. Application tests exercise business rules and use-case outcomes through interface fakes. API tests verify HTTP contracts and presentation through an in-process host with service overrides. Koha tests exercise provider translation, authentication, cancellation, and failure handling through `HttpMessageHandler` doubles. Tests must not require a live Koha server or developer credentials. Do not build or extend a fake network server for routine tests.

API tests use the existing `Minioasis.Api.Tests/TestInfrastructure/Shared/ApiFactory.cs`, which supplies dummy Koha configuration and supports test service overrides. Keep host configuration isolated and do not mutate process-wide environment variables in tests. Follow the feature-first organization when adding test support or performing an approved migration.

Add focused tests for changed behavior and meaningful failure paths. Preserve supported behavior and public contracts when reorganizing code. Do not fabricate results or claim that a command passed unless it completed successfully.

## Repository boundary

- Work only inside this repository.
- Treat the separate production repository as authoritative, but do not access, modify, push to, merge into, or deploy from it.
- Changes made here are reviewed and transferred to production manually.
- Do not add a production remote or attempt a production deployment.
- Never commit credentials, tokens, private keys, database dumps, `.env`, or production configuration.

## Planning and approval

Before changing any repository file:

1. Inspect the relevant code, tests, configuration, contracts, and current Git state.
2. For architecture, behavior, security, dependencies, persistence, infrastructure, public contracts, or ambiguous scope, ask detailed concrete questions until the intended result and tradeoffs are clear.
3. Present a decision-complete implementation plan covering behavior, important files, tests, and material risks.
4. Wait until the user replies with the exact word `approved` before creating the task branch or editing files.

Small, obvious changes need fewer questions, but still require a plan and the exact approval word. If implementation reveals a material departure from the approved plan, stop, explain the discovery, present a revised plan, and wait for `approved` again.

If a user request conflicts with these instructions, ask whether the exception is intentional before proceeding.

## Git workflow

Apply this workflow to every task that changes repository files. Read-only explanations, reviews, and diagnostics do not require a branch.

1. Require a clean working tree. If unrelated or uncommitted work exists, stop and ask how the user wants it handled; do not stash, discard, overwrite, or absorb it.
2. Verify that `origin` is exactly `https://github.com/GoldBeluga/minioasis-agents.git`.
3. Fetch `origin/master`. If the latest remote state cannot be fetched and verified, stop and report the blocker. Do not use a possibly stale cached ref.
4. After approval, create the task branch directly from `origin/master`.
5. Use `ai/<category>/<three-digit-id>-<short-name>`:
   - `bug` fixes broken existing behavior.
   - `sprint` is used only for work explicitly assigned to a sprint.
   - `task` covers all other changes.
   - Maintain a separate incrementing ID sequence for each category based on remote `ai/*` branches. For example: `ai/bug/012-token-refresh`.
   - If a proposed branch exists, increment the ID until the name is unique.
6. Make only changes relevant to the approved task. Review the complete diff before staging.
7. Use one or more logical, reviewable commits with clear messages.
8. Push only the task branch to the verified `origin`.
9. Open a draft pull request against `master`. Do not merge it.

When the user requests revisions to an existing draft pull request, continue on that pull request's branch and add logical commits rather than creating a new branch.

Never force-push, rewrite shared history, push directly to `master`, delete a remote branch, or merge a pull request unless the user explicitly requests the exception and confirms it after the conflict is identified.

## Configuration and running

Koha reads these required values through ASP.NET configuration:

- `MINIOASIS_KOHA_BASE_URL`
- `MINIOASIS_KOHA_CLIENT_ID`
- `MINIOASIS_KOHA_CLIENT_SECRET`

For native Windows development, set them in the PowerShell process that starts the API:

```powershell
$env:MINIOASIS_KOHA_BASE_URL = "https://your-koha-server/api/v1/"
$env:MINIOASIS_KOHA_CLIENT_ID = "your-client-id"
$env:MINIOASIS_KOHA_CLIENT_SECRET = "your-client-secret"
dotnet run --project Minioasis.Api
```

For Bash:

```bash
export MINIOASIS_KOHA_BASE_URL='https://your-koha-server/api/v1/'
export MINIOASIS_KOHA_CLIENT_ID='your-client-id'
export MINIOASIS_KOHA_CLIENT_SECRET='your-client-secret'
dotnet run --project Minioasis.Api
```

Keep real values outside source control. Cloud agents should assume they have source code only and must not attempt live Koha validation.

For Docker, create an ignored root `.env` containing the same three variables, then run:

```powershell
docker compose up --build -d
docker compose logs -f api
```

The container exposes the API at `http://localhost:8080` and runs in Production, where the development OpenAPI endpoint is disabled. When Koha runs on the Windows host, use `host.docker.internal` instead of `localhost` in its base URL. Recreate the container after changing `.env`; stop it with `docker compose down`.

## Validation

Choose checks based on the changed files, and run every applicable check before committing.

For C# changes:

```powershell
dotnet restore minioasis.slnx
dotnet format minioasis.slnx --verify-no-changes --no-restore
dotnet test minioasis.slnx --configuration Release --no-restore --nologo --verbosity normal
dotnet publish Minioasis.Api/Minioasis.Api.csproj --configuration Release --no-restore
```

For Dockerfile or Compose changes, run `docker compose config` and `docker compose build` when Docker is available. For contract changes, regenerate the relevant client, inspect the generated diff, and then run the C# checks.

For documentation or repository-instruction changes, run `git diff --check`, verify every referenced path and command, validate that `minioasis.slnx` can be parsed and listed, and inspect the final diff. A solution-file change does not by itself require the full .NET test suite.

If a required tool, SDK, or network service is unavailable, run all feasible checks and report the exact blocker. If a baseline check fails, verify that the failure predates and is unrelated to the task when possible; do not expand scope to fix it. A disclosed environmental or unrelated baseline failure may be committed and pushed. Any failure caused by the task must be fixed before completion.

## Completion and report

A file-changing task is complete only when the approved behavior is implemented, applicable checks have been run, the diff contains no unrelated changes, logical commits have been created, the task branch has been pushed, and a draft pull request against `master` has been opened.

Use this report format:

```markdown
Branch: `ai/<category>/<id>-<name>`

Commits:
- `<hash> <message>`

Draft PR: <URL>

Changes:
- <concise behavior or implementation summary>

Significant files:
- `<path>` — <reason>

Checks:
- `<command>` — passed, failed, or not run with the exact reason

Limitations and risks:
- <known limitation, risk, or `None`>
```
