# 001 — Get item: verification

- **Spec:** [spec.md](spec.md)
- **Verified at commit:** not yet; coverage reviewed against `7122045` on 2026-11-15; gaps closed on `ai/spec/001-test-gaps` on 2026-11-15
- **Transfer branch:** not yet

## Requirement coverage

Test paths are shortened. `Api` means `Minioasis.Api.Tests`, and the same for the other projects.

"Seen failing" is "not recorded" for tests written before that rule existed. The follow-up work must record it for every row before the spec can be `verified`. Tests added to close gaps record the deliberate code change that made them fail; each change was reverted afterwards.

| Req | Proves (Given / When / Then) | Test | Seen failing | Result |
| --- | --- | --- | --- | --- |
| R1 | Koha returns an item with id 12, biblio 34, barcode ABC, call number QA1 → adapter returns those four values | `Koha::KohaGetItemAdapterTests::Maps_generated_item_to_get_item_response` | not recorded | ✅ pass |
| R1 | Gateway finds an item → use case returns it unchanged | `Application::GetItemUseCaseTests::Returns_get_item_response_when_gateway_finds_item` | not recorded | ✅ pass |
| R1 | Use case succeeds → API returns 200 with exactly the fields `barcode`, `bibliographicRecordId`, `callNumber`, `id` | `Api::ItemEndpointsTests::Returns_item_json_from_application_use_case` | not recorded | ✅ pass |
| R2 | Identifier `"  "` → `items.invalid_identifier`, and the gateway is called 0 times | `Application::GetItemUseCaseTests::Rejects_blank_identifier_without_calling_gateway` | not recorded | ✅ pass |
| R2, R3, R4 | Validation error → 400 with the correct title, detail, code, `traceId`, `application/problem+json`, and no `type` or `instance` | `Api::ItemEndpointsTests::Maps_application_errors_to_problem_details` (validation row) | not recorded | ✅ pass |
| R3 | Identifier `"koha-12"` → invalid identifier, and 0 network calls | `Koha::KohaGetItemAdapterTests::Rejects_non_numeric_identifier_without_network` | not recorded | ✅ pass |
| R3, R4 | Gateway reports an invalid identifier → `items.invalid_identifier` / Validation | `Application::GetItemUseCaseTests::Translates_gateway_outcomes` (InvalidIdentifier row) | not recorded | ✅ pass |
| R4 | Identifier `"0"` or `"-5"` → invalid identifier, and 0 network calls | `Koha::KohaGetItemAdapterTests::Rejects_non_positive_or_out_of_range_identifier_without_network` (`"0"` and `"-5"` rows) | yes: removing `kohaId <= 0` from the adapter made both rows fail (a network call was attempted) | ✅ pass |
| R5 | Koha 404 → not found | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (404 row) | not recorded | ✅ pass |
| R5 | Not found → `items.not_found` / NotFound | `Application::GetItemUseCaseTests::Translates_gateway_outcomes` (NotFound row) | not recorded | ✅ pass |
| R5 | Not found → 404, detail "No item was found with identifier 12." | `Api::ItemEndpointsTests::Maps_application_errors_to_problem_details` (not-found row) | not recorded | ✅ pass |
| R6 | Koha 429 and 502 → unavailable | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (429 and 502 rows) | not recorded | ✅ pass |
| R6 | Item response stream breaks → unavailable | `Koha::KohaGetItemAdapterFailureTests::Item_response_disconnection_is_unavailable` | not recorded | ✅ pass |
| R6 | Unavailable → `items.get.unavailable` | `Application::GetItemUseCaseTests::Translates_gateway_outcomes` (Unavailable row) | not recorded | ✅ pass |
| R6 | `items.get.unavailable` → 503 masked | `Api::ApiErrorHandlingTests::Masks_server_failures_and_logs_internal_code_with_response_trace_id` | not recorded | ✅ pass |
| R7 | Koha 403 → authentication failed | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (403 row) | not recorded | ✅ pass |
| R7 | Koha 401 → authentication failed | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (401 row) | yes: mapping only 403 to authentication made it fail with `InvalidResponse` | ✅ pass |
| R7 | Koha answers 401 to the request and to its single retry, through the real bearer handler and token provider → authentication failed, with 2 item requests and 2 token acquisitions | `Koha::KohaGetItemAdapterFailureTests::Koha_401_that_persists_after_retry_is_an_authentication_failure` | yes: mapping only 403 to authentication made it fail with `InvalidResponse` | ✅ pass |
| R7 | OAuth failure → authentication failed | `Koha::KohaGetItemAdapterTests::Translates_oauth_failure_to_authentication_outcome` | not recorded | ✅ pass |
| R7 | OAuth never answers → authentication failed after the deadline | `Koha::KohaGetItemAdapterFailureTests::OAuth_timeout_is_an_authentication_failure` | not recorded | ✅ pass |
| R7 | OAuth response stream breaks → authentication failed | `Koha::KohaGetItemAdapterFailureTests::OAuth_response_disconnection_is_an_authentication_failure` | not recorded | ✅ pass |
| R7 | OAuth 4xx/5xx, malformed JSON, network failure, or timeout → authentication exception | `Koha::KohaAccessTokenProviderTests::Converts_*` (four tests) | not recorded | ✅ pass |
| R7 | `items.get.authentication_failed` → 500 masked | `Api::ApiErrorHandlingTests::Masks_server_failures_and_logs_internal_code_with_response_trace_id` | not recorded | ✅ pass |
| R8 | Koha 400 → invalid response | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (400 row) | not recorded | ✅ pass |
| R8 | Item with an unparseable date → invalid response | `Koha::KohaGetItemAdapterFailureTests::Invalid_date_is_an_invalid_response` | not recorded | ✅ pass |
| R8 | Koha 200 with `item_id` 0 or -1, or `biblio_id` 0 or -3 → invalid response | `Koha::KohaGetItemAdapterTests::Non_positive_identifiers_in_koha_item_are_an_invalid_response` | yes: removing the identifier check made all four rows fail with `Found` | ✅ pass |
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

**Gaps: 0.** The three requirement gaps (R4, R7 for a 401 at the adapter, R8 for non-positive identifiers in the response) are closed. The spec stays `approved`, not `verified`, until "Seen failing" is recorded for every remaining row and an independent agent verifies it.

## Edge cases and prohibitions

| Item from the spec | State | Proven by |
| --- | --- | --- |
| Identifier has spaces around a valid number | covered | `Application::GetItemUseCaseTests::Trims_identifier_before_calling_gateway`: `"  12 "` reaches the gateway as `"12"`, and the not-found detail names `"12"`. Seen failing: yes, removing `Trim()` made it fail |
| Identifier is larger than a 32-bit number | covered | `Koha::KohaGetItemAdapterTests::Rejects_non_positive_or_out_of_range_identifier_without_network` (`"2147483648"` row). Seen failing: yes, parsing as a 64-bit number made it fail (a network call was attempted) |
| Item has no barcode or call number | covered | `Koha::KohaGetItemAdapterTests::Maps_missing_barcode_and_call_number_to_null`: an explicit `null` barcode and an absent call number both come back as `null`. Seen failing: yes, substituting an empty string made it fail |
| Many requests share one token refresh | covered | `Koha::KohaAccessTokenProviderTests::Concurrent_callers_share_one_refresh` |
| All other covered edge cases and prohibitions | covered | The requirement rows above |
| Real Koha returns fields in an unexpected form | manual | Manual scenario 1 |
| Service account lacks the `catalogue` permission | manual | Manual scenario 5 |
| Secrets and tokens are not logged | manual | Manual scenario 5 |

Writing this table found 2 more gaps and 1 weak test, in addition to the 3 above. All are now closed.

## Wiring

| Piece | Registered in | Reached from |
| --- | --- | --- |
| `GetItemUseCase` as `IGetItemUseCase` | `AddMinioasisApplication` | `ItemEndpoints.GetItemAsync` |
| `KohaGetItemAdapter` as `IGetItemGateway` | `AddMinioasisKoha` | `GetItemUseCase` |
| `ItemEndpoints` | `AddEndpointModules` (found automatically) | `MapEndpointModules` in `Program.cs` |
| `KohaRequestTimeoutHandler`, `KohaBearerHandler` | `AddMinioasisKoha`, on the API HTTP client | Every Koha API request |

## Checks

Run on `ai/spec/001-test-gaps` (based on `e850a93`), on 2026-11-15:

| Command | Result |
| --- | --- |
| `dotnet format minioasis.slnx --verify-no-changes --no-restore` | passed |
| `dotnet test minioasis.slnx --configuration Release --no-restore` | passed: 98 tests (Application 16, Koha 51, Api 31), 0 failed |
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
