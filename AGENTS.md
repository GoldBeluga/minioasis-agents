# Minioasis repository instructions

This is an AI development repository for the Minioasis .NET 10 API backed by Koha. The current HTTP capability is `GET /api/v1/items/{id}`.

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

## Architecture

Projects remain at the repository root. Handwritten code is organized first by role and then by subdomain.

| Project | Responsibility |
| --- | --- |
| `Minioasis.Application` | Use cases, business models, validation, gateway ports, results, and typed error definitions. |
| `Minioasis.Api` | HTTP endpoints, response mapping, Problem Details, English error text, and composition. |
| `Minioasis.Koha` | Replaceable Koha gateway, generated clients, OAuth, and provider-specific translation. |

Application is the source of truth. It must not reference API, Koha implementations, ASP.NET/HTTP types, provider URLs, or generated DTOs. API endpoints stay thin and call Application use cases. Koha implements Application-owned ports. Preserve this dependency direction unless an approved plan explicitly changes it.

Preserve existing routes, status codes, JSON shapes, Problem Details fields, and error text unless an approved task explicitly changes the public contract. Do not add databases, caches, queues, providers, or other infrastructure unless they are included in the approved plan. Do not introduce dependencies unless necessary and approved.

Every production or reusable shared-test top-level type belongs in its own file named after the type. Namespaces follow folders. Private nested implementation types and test helpers may remain with their owner. Test folders mirror production folders; reusable doubles live under `TestDoubles/<subdomain>`.

Current organization includes:

- Application: `Abstractions/Item`, `Models/Item`, `Results/Item`, `UseCases/Item`, `Errors/Item`, `Errors/Catalog`, `Errors/System`, plus shared results, errors, and extensions.
- API: `Endpoints/Item` plus shared abstractions, endpoints, models, services, mappers, extensions, and resources.
- Koha: `Adapters/Item` plus authentication abstractions, services, handlers, exceptions, shared configuration, and extensions.

Create a subdomain folder only when it has code to contain. Do not create empty placeholders.

## Errors

Application owns stable typed error definitions; API owns presentation and HTTP status mapping.

To add an error:

1. Add a `public static readonly ApplicationErrorDefinition` to the appropriate group under `Minioasis.Application/Errors/<subdomain>`. For a new domain, create a separate `[ErrorDefinitions]` group.
2. Add the exact matching, case-sensitive key to `Minioasis.Api/Resources/errors.en.json` with nonblank `title` and `detail` values.
3. Return an `ApplicationError` built from the definition, supplying metadata for placeholders such as `{id}`.

The registry and JSON resource must remain in exact parity. Preserve startup rejection of blank or duplicate identifiers, null definitions, invalid categories, empty registries, and missing, unknown, duplicate, malformed, null, or incomplete messages.

Preserve the category mapping: validation 400, not found 404, invalid dependency response 502, unavailable dependency 503, and dependency authentication or unexpected failures 500. Preserve the `code` and `traceId` Problem Details extensions. Only English is currently supported.

## Koha contracts and generated code

`contracts/official_koha_openapi.json` is an immutable checked-in reference. Never edit it. Use it to update the focused `contracts/items.openapi.json` or `contracts/oauth.openapi.json`. If a required operation is absent or ambiguous, stop and ask rather than inventing a protocol.

`Minioasis.Koha/Generated` is owned by NSwag. Do not hand-edit generated files. When an approved task changes a focused contract or `nswag.json`, restore the local tool and regenerate the affected client:

```powershell
dotnet tool restore
dotnet tool run nswag run nswag.json /variables:Contract=items,Feature=Items,FeatureSingular=Item
dotnet tool run nswag run nswag.json /variables:Contract=oauth,Feature=OAuth,FeatureSingular=OAuth
```

Run only the generation command relevant to the changed contract, inspect the generated diff, and commit the source contract, generator configuration when applicable, and resulting generated files together.

Keep generated Koha types inside `Minioasis.Koha`; do not leak them into Application or API.

## OAuth behavior

Preserve these established rules unless an approved plan explicitly changes authentication behavior:

- Use OAuth 2 client credentials with a least-privilege Koha service identity.
- Use separate HTTP clients for token acquisition and protected API calls. The token client must not use the bearer handler.
- Validate that acquired access tokens are nonblank, have a positive lifetime, and use the Bearer token type.
- Cache tokens until shortly before expiration and coordinate concurrent refreshes.
- On a protected GET returning 401, invalidate the cached token, recreate the request, and retry exactly once.
- Never replay non-GET requests automatically and never loop retries.
- Propagate cancellation. Map token network, timeout, validation, and other acquisition failures through the existing authentication error path to HTTP 500.

## Tests

Tests must not require a live Koha server or developer credentials. Use in-process interface fakes and `HttpMessageHandler` doubles. Do not build or extend a fake network server for routine tests.

API tests use their own `TestInfrastructure/Shared/ApiFactory`, which supplies dummy Koha configuration and supports test service overrides. Do not mutate process-wide environment variables in tests.

Add focused tests for changed behavior and meaningful failure paths. Do not fabricate results or claim that a command passed unless it completed successfully.

## Configuration and running

Koha reads these required values through ASP.NET configuration:

- `MINIOASIS_KOHA_BASE_URL`
- `MINIOASIS_KOHA_CLIENT_ID`
- `MINIOASIS_KOHA_CLIENT_SECRET`

For native Windows development, set them in the PowerShell process that starts the API:

```powershell
$env:MINIOASIS_KOHA_BASE_URL = "https://your-koha-server/"
$env:MINIOASIS_KOHA_CLIENT_ID = "your-client-id"
$env:MINIOASIS_KOHA_CLIENT_SECRET = "your-client-secret"
dotnet run --project Minioasis.Api
```

For Bash:

```bash
export MINIOASIS_KOHA_BASE_URL='https://your-koha-server/'
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
