# Koha integration

This document is binding. It records how Minioasis authenticates to Koha and calls it today, for every capability that uses Koha. It was moved here unchanged in meaning from `AGENTS.md`, which now holds only the principles. Changing this file is a Level 2 change. A shared-behavior spec is intended to replace it; until then this is the contract.

## Authentication

Koha authentication is an integration responsibility shared by the workflows that call it. Preserve these authentication guarantees unless an approved plan explicitly changes authentication behavior. Use OAuth 2 client credentials with a least-privilege Koha service identity. Keep credentials outside source control and diagnostic output. Use separate HTTP clients for token acquisition and protected API calls. The token client must not use the bearer handler.

Validate that acquired access tokens are nonblank, have a positive lifetime, and use the Bearer token type. Cache tokens until shortly before expiration and coordinate concurrent refreshes so callers can share a usable token safely.

## Retry

On a protected GET returning 401, invalidate the cached token, recreate the request, and retry exactly once. Never replay non-GET requests automatically and never loop retries. For state-changing workflows, define partial-failure and retry behavior in the approved design with Koha's transaction ownership in mind.

## Cancellation, deadlines, and failures

Pass cancellation through endpoints, use cases, gateway interfaces, token acquisition, and HTTP operations. Propagate caller cancellation. Bound external work and account for authentication, retries, and response consumption when designing deadlines. Preserve the distinction between caller cancellation and a dependency failure. Map token network, timeout, validation, and other acquisition failures through the existing authentication error path to HTTP 500 unless an approved plan explicitly changes authentication behavior.

## Generating a Koha client

`Minioasis.Koha/Generated` is owned by NSwag. Do not hand-edit generated files. When an approved task changes a focused contract or `nswag.json`, restore the local tool and regenerate the affected client. The command takes the contract filename stem and the generated feature and type names as variables.

```powershell
dotnet tool restore
dotnet tool run nswag run nswag.json /variables:Contract=<contract>,Feature=<Feature>,FeatureSingular=<FeatureSingular>
```

`<contract>` is the stem of a file in `contracts/` named `<contract>.openapi.json`. Run only the generation relevant to the changed contract, inspect the generated diff, and commit the source contract, generator configuration when applicable, and resulting generated files together. Keep generated Koha types inside `Minioasis.Koha`.
