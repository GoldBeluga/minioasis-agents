# 001 — Get item: implementation plan

- **Spec:** [spec.md](spec.md)
- **Plan status:** complete (recorded after the fact)

## Approach

The request passes through the three projects defined in `AGENTS.md`:

1. **`ItemEndpoints`** (`Minioasis.Api`) receives the request and calls `IGetItemUseCase`.
2. **`GetItemUseCase`** (`Minioasis.Application`) rejects blank identifiers and calls `IGetItemGateway`.
3. **`KohaGetItemAdapter`** (`Minioasis.Koha`) validates the numeric identifier, calls the NSwag-generated client, and translates Koha responses and failures into a `GetItemOutcome`.
4. **`GetItemUseCase`** turns the outcome into an `ApplicationResult`.
5. **`ApplicationResultHttpMapper`** (`Minioasis.Api`) turns the result into HTTP and Problem Details.

The API HTTP client runs two handlers:

- `KohaRequestTimeoutHandler` (outer) applies the 10-second deadline and buffers the response body.
- `KohaBearerHandler` (inner) adds the OAuth token and performs the single 401 retry for GET requests.

`KohaAccessTokenProvider` caches the token, refreshes it 30 seconds before it expires, and lets concurrent callers share one refresh.

## One-way decisions

| Decision | What undoing it would break | Settled by |
| --- | --- | --- |
| Route `/api/v1/items/{id}` and the four response field names | Any client already calling the API | D1, D3 |
| Identifiers as strings | Any client parsing the response | D2 |

## Files

| Project | File | Purpose |
| --- | --- | --- |
| `Minioasis.Application` | `Items/UseCases/GetItemUseCase.cs` | Use case and outcome translation |
| `Minioasis.Application` | `Items/Abstractions/IGetItemGateway.cs`, `IGetItemUseCase.cs` | Port and use-case interface |
| `Minioasis.Application` | `Items/Errors/GetItemErrorDefinitions.cs` | Error definitions |
| `Minioasis.Application` | `Items/Requests`, `Responses`, `Results`, `Identifiers` | Request, response, outcome, and `ItemId` |
| `Minioasis.Koha` | `Items/Adapters/KohaGetItemAdapter.cs` | Koha translation |
| `Minioasis.Koha` | `Authentication/**` | OAuth token provider and bearer handler |
| `Minioasis.Koha` | `Shared/Handlers/KohaRequestTimeoutHandler.cs` | Deadline |
| `Minioasis.Koha` | `Generated/Items/**`, `Generated/OAuth/**` | NSwag clients |
| `Minioasis.Api` | `Items/Endpoints/ItemEndpoints.cs` | Route |
| `Minioasis.Api` | `Shared/**` | Result mapping, Problem Details, exception handling, error messages |
| `Minioasis.Api` | `Resources/errors.en.json` | Client-visible error text |

## Contracts and generation

- `contracts/items.openapi.json` is derived from `getItem` in the official contract.
- `contracts/oauth.openapi.json` covers the token endpoint.
- Both are generated with the NSwag commands in `AGENTS.md`.

## Error definitions

| Code | Category | Client-visible resource entry |
| --- | --- | --- |
| `items.invalid_identifier` | Validation | yes |
| `items.not_found` | NotFound | yes |
| `items.get.unavailable` | DependencyUnavailable | no |
| `items.get.authentication_failed` | DependencyAuthentication | no |
| `items.get.invalid_response` | DependencyInvalidResponse | no |

## Tests

See [verification.md](verification.md).

## Follow-up pull requests (Level 1)

1. Add the missing tests listed as gaps in `verification.md`, and strengthen the weak one.
2. Record "Seen failing" for every row, by breaking each behavior on purpose, watching its test fail, and restoring it.
3. Have an agent that did not write the tests verify the spec.
4. When there are no gaps left, mark the spec `verified` and create the branch `ready/001-get-item`.

## Risks

- No inbound authentication (spec 002).

## Deviations

None.
