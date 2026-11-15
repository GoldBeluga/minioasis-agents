# 001 — Get item: verification

- **Spec:** [spec.md](spec.md)
- **Verified at commit:** not yet; coverage reviewed against `7122045` on 2026-11-15
- **Transfer branch:** not yet

## Requirement coverage

Test paths are shortened. `Api` means `Minioasis.Api.Tests`, and the same for the other projects.

"Seen failing" is "not recorded" throughout, because these tests were written before that rule existed. The follow-up work must record it for every row before the spec can be `verified`.

| Req | Proves (Given / When / Then) | Test | Seen failing | Result |
| --- | --- | --- | --- | --- |
| R1 | Koha returns an item with id 12, biblio 34, barcode ABC, call number QA1 → adapter returns those four values | `Koha::KohaGetItemAdapterTests::Maps_generated_item_to_get_item_response` | not recorded | ✅ pass |
| R1 | Gateway finds an item → use case returns it unchanged | `Application::GetItemUseCaseTests::Returns_get_item_response_when_gateway_finds_item` | not recorded | ✅ pass |
| R1 | Use case succeeds → API returns 200 with exactly the fields `barcode`, `bibliographicRecordId`, `callNumber`, `id` | `Api::ItemEndpointsTests::Returns_item_json_from_application_use_case` | not recorded | ✅ pass |
| R2 | Identifier `"  "` → `items.invalid_identifier`, and the gateway is called 0 times | `Application::GetItemUseCaseTests::Rejects_blank_identifier_without_calling_gateway` | not recorded | ✅ pass |
| R2, R3, R4 | Validation error → 400 with the correct title, detail, code, `traceId`, `application/problem+json`, and no `type` or `instance` | `Api::ItemEndpointsTests::Maps_application_errors_to_problem_details` (validation row) | not recorded | ✅ pass |
| R3 | Identifier `"koha-12"` → invalid identifier, and 0 network calls | `Koha::KohaGetItemAdapterTests::Rejects_non_numeric_identifier_without_network` | not recorded | ✅ pass |
| R3, R4 | Gateway reports an invalid identifier → `items.invalid_identifier` / Validation | `Application::GetItemUseCaseTests::Translates_gateway_outcomes` (InvalidIdentifier row) | not recorded | ✅ pass |
| R4 | Identifier `"0"` or `"-5"` → invalid identifier without a network call | — | — | ⚠️ **gap**: the code rejects these, but no test proves it |
| R5 | Koha 404 → not found | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (404 row) | not recorded | ✅ pass |
| R5 | Not found → `items.not_found` / NotFound | `Application::GetItemUseCaseTests::Translates_gateway_outcomes` (NotFound row) | not recorded | ✅ pass |
| R5 | Not found → 404, detail "No item was found with identifier 12." | `Api::ItemEndpointsTests::Maps_application_errors_to_problem_details` (not-found row) | not recorded | ✅ pass |
| R6 | Koha 429 and 502 → unavailable | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (429 and 502 rows) | not recorded | ✅ pass |
| R6 | Item response stream breaks → unavailable | `Koha::KohaGetItemAdapterFailureTests::Item_response_disconnection_is_unavailable` | not recorded | ✅ pass |
| R6 | Unavailable → `items.get.unavailable` | `Application::GetItemUseCaseTests::Translates_gateway_outcomes` (Unavailable row) | not recorded | ✅ pass |
| R6 | `items.get.unavailable` → 503 masked | `Api::ApiErrorHandlingTests::Masks_server_failures_and_logs_internal_code_with_response_trace_id` | not recorded | ✅ pass |
| R7 | Koha 403 → authentication failed | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (403 row) | not recorded | ✅ pass |
| R7 | Koha 401 that persists after the retry → authentication failed | — | — | ⚠️ **gap**: only 403 is tested at the adapter |
| R7 | OAuth failure → authentication failed | `Koha::KohaGetItemAdapterTests::Translates_oauth_failure_to_authentication_outcome` | not recorded | ✅ pass |
| R7 | OAuth never answers → authentication failed after the deadline | `Koha::KohaGetItemAdapterFailureTests::OAuth_timeout_is_an_authentication_failure` | not recorded | ✅ pass |
| R7 | OAuth response stream breaks → authentication failed | `Koha::KohaGetItemAdapterFailureTests::OAuth_response_disconnection_is_an_authentication_failure` | not recorded | ✅ pass |
| R7 | OAuth 4xx/5xx, malformed JSON, network failure, or timeout → authentication exception | `Koha::KohaAccessTokenProviderTests::Converts_*` (four tests) | not recorded | ✅ pass |
| R7 | `items.get.authentication_failed` → 500 masked | `Api::ApiErrorHandlingTests::Masks_server_failures_and_logs_internal_code_with_response_trace_id` | not recorded | ✅ pass |
| R8 | Koha 400 → invalid response | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (400 row) | not recorded | ✅ pass |
| R8 | Item with an unparseable date → invalid response | `Koha::KohaGetItemAdapterFailureTests::Invalid_date_is_an_invalid_response` | not recorded | ✅ pass |
| R8 | Koha 200 with `item_id` or `biblio_id` ≤ 0 → invalid response | — | — | ⚠️ **gap**: the code checks this, but no test proves it |
| R8 | `items.get.invalid_response` → 502 masked | `Api::ApiErrorHandlingTests::Masks_server_failures_and_logs_internal_code_with_response_trace_id` | not recorded | ✅ pass |
| R9 | Each server-failure code → masked body, and the internal code is logged with the response `traceId` | `Api::ApiErrorHandlingTests::Masks_server_failures_and_logs_internal_code_with_response_trace_id` | not recorded | ✅ pass |
| R9 | Unhandled exception, in Development and in Production → masked 500, and the exception is logged with `traceId` | `Api::ApiErrorHandlingTests::Masks_unhandled_exceptions_and_logs_exception_with_response_trace_id` | not recorded | ✅ pass |
| R10 | 300 ms OAuth plus a stalled body under a 500 ms deadline → unavailable in under 700 ms | `Koha::KohaGetItemAdapterFailureTests::Total_deadline_includes_oauth_and_item_response_body` | not recorded | ✅ pass |
| R11 | First 401 → fresh token, then a single retry | `Koha::KohaBearerHandlerTests::Retries_get_once_with_fresh_token_after_401` | not recorded | ✅ pass |
| R11 | Second 401 → no further retry | `Koha::KohaBearerHandlerTests::Does_not_retry_a_second_401` | not recorded | ✅ pass |
| R11 | Non-GET 401 → no retry | `Koha::KohaBearerHandlerTests::Does_not_retry_non_get_request` | not recorded | ✅ pass |
| R12 | Client cancels during OAuth → the cancellation propagates with the caller's token | `Koha::KohaGetItemAdapterFailureTests::Caller_cancellation_during_oauth_is_propagated` | not recorded | ✅ pass |
| R12 | Client aborts → nothing is logged as an internal failure | `Api::ApiErrorHandlingTests::Propagates_caller_cancellation_without_logging_an_internal_failure` | not recorded | ✅ pass |
| R13 | Date `2026-09-06` under culture `ar-SA` → found | `Koha::KohaGetItemAdapterFailureTests::Iso_date_is_parsed_independently_of_current_culture` | not recorded | ✅ pass |

**Gaps: 3** (R4, R7 for a 401 at the adapter, R8 for non-positive identifiers in the response). The spec stays `approved`, not `verified`, until Level 1 pull requests add these tests and record "Seen failing" for every row.

## Edge cases and prohibitions

| Item from the spec | State | Proven by |
| --- | --- | --- |
| Identifier has spaces around a valid number | covered | ⚠️ gap: the code trims, but no test proves it |
| Identifier is larger than a 32-bit number | covered | ⚠️ gap: the code rejects it, but no test proves it |
| Item has no barcode or call number | covered | `Koha::KohaGetItemAdapterFailureTests::Iso_date_is_parsed_independently_of_current_culture` returns "found" for an item without them, but does not assert the `null` values: ⚠️ weak |
| Many requests share one token refresh | covered | `Koha::KohaAccessTokenProviderTests::Concurrent_callers_share_one_refresh` |
| All other covered edge cases and prohibitions | covered | The requirement rows above |
| Real Koha returns fields in an unexpected form | manual | Manual scenario 1 |
| Service account lacks the `catalogue` permission | manual | Manual scenario 5 |
| Secrets and tokens are not logged | manual | Manual scenario 5 |

Writing this table found **2 more gaps and 1 weak test**, in addition to the 3 above.

## Wiring

| Piece | Registered in | Reached from |
| --- | --- | --- |
| `GetItemUseCase` as `IGetItemUseCase` | `AddMinioasisApplication` | `ItemEndpoints.GetItemAsync` |
| `KohaGetItemAdapter` as `IGetItemGateway` | `AddMinioasisKoha` | `GetItemUseCase` |
| `ItemEndpoints` | `AddEndpointModules` (found automatically) | `MapEndpointModules` in `Program.cs` |
| `KohaRequestTimeoutHandler`, `KohaBearerHandler` | `AddMinioasisKoha`, on the API HTTP client | Every Koha API request |

## Checks

Run on `7122045`, on 2026-11-14:

| Command | Result |
| --- | --- |
| `dotnet format minioasis.slnx --verify-no-changes --no-restore` | passed |
| `dotnet test minioasis.slnx --configuration Release --no-restore` | passed: 87 tests (Application 15, Koha 41, Api 31), 0 failed |
| `dotnet publish Minioasis.Api/Minioasis.Api.csproj --configuration Release --no-restore` | passed |

## Independent verification

Not yet performed.

## Manual acceptance (maintainer)

- **Koha instance and version:**
- **Date:**

| # | Scenario | Expected | Observed (maintainer's words) | Result |
| --- | --- | --- | --- | --- |
| 1 | Request a known item number | 200 with the correct barcode and call number | | |
| 2 | Request an item number that does not exist | 404 `items.not_found` | | |
| 3 | Request `abc` | 400 `items.invalid_identifier` | | |
| 4 | Stop Koha or use a wrong base URL, then request | 503 `internal_error` with a `traceId` | | |
| 5 | Use a wrong client secret, or a service account without the `catalogue` permission, then request | 500 `internal_error`; the log shows an authentication failure and contains no secret or token | | |

**Notes:**
