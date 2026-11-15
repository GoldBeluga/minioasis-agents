# 001 — Get item: verification

- **Spec:** [spec.md](spec.md)
- **Verified at commit:** not yet; coverage reviewed against `7122045` on 2026-11-15; gaps closed by pull request #11 on 2026-11-15; "Seen failing" recorded for every row on 2026-11-15, against `0520881`
- **Transfer branch:** not yet

## Requirement coverage

Test paths are shortened. `Api` means `Minioasis.Api.Tests`, and the same for the other projects.

"Seen failing" names the deliberate code change that made the test fail. Changes added by pull request #11 are described inline. Changes `S01`–`S36` are listed under [Deliberate code changes](#deliberate-code-changes). Every change was reverted afterwards.

| Req | Proves (Given / When / Then) | Test | Seen failing | Result |
| --- | --- | --- | --- | --- |
| R1 | Koha returns an item with id 12, biblio 34, barcode ABC, call number QA1 → adapter returns those four values | `Koha::KohaGetItemAdapterTests::Maps_generated_item_to_get_item_response` | yes, S01 | ✅ pass |
| R1 | Gateway finds an item → use case returns it unchanged | `Application::GetItemUseCaseTests::Returns_get_item_response_when_gateway_finds_item` | yes, S02 | ✅ pass |
| R1 | Use case succeeds → API returns 200 with exactly the fields `barcode`, `bibliographicRecordId`, `callNumber`, `id` | `Api::ItemEndpointsTests::Returns_item_json_from_application_use_case` | yes, S03 | ✅ pass |
| R2 | Identifier `"  "` → `items.invalid_identifier`, and the gateway is called 0 times | `Application::GetItemUseCaseTests::Rejects_blank_identifier_without_calling_gateway` | yes, S04 | ✅ pass |
| R2, R3, R4 | Validation error → 400 with the correct title, detail, code, `traceId`, `application/problem+json`, and no `type` or `instance` | `Api::ItemEndpointsTests::Maps_application_errors_to_problem_details` (validation row) | yes, S05 | ✅ pass |
| R3 | Identifier `"koha-12"` → invalid identifier, and 0 network calls | `Koha::KohaGetItemAdapterTests::Rejects_non_numeric_identifier_without_network` | yes, S06 | ✅ pass |
| R3, R4 | Gateway reports an invalid identifier → `items.invalid_identifier` / Validation | `Application::GetItemUseCaseTests::Translates_gateway_outcomes` (InvalidIdentifier row) | yes, S07 | ✅ pass |
| R4 | Identifier `"0"` or `"-5"` → invalid identifier, and 0 network calls | `Koha::KohaGetItemAdapterTests::Rejects_non_positive_or_out_of_range_identifier_without_network` (`"0"` and `"-5"` rows) | yes: removing `kohaId <= 0` from the adapter made both rows fail (a network call was attempted) | ✅ pass |
| R5 | Koha 404 → not found | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (404 row) | yes, S08 | ✅ pass |
| R5 | Not found → `items.not_found` / NotFound | `Application::GetItemUseCaseTests::Translates_gateway_outcomes` (NotFound row) | yes, S09 | ✅ pass |
| R5 | Not found → 404, detail "No item was found with identifier 12." | `Api::ItemEndpointsTests::Maps_application_errors_to_problem_details` (not-found row) | yes, S10 | ✅ pass |
| R6 | Koha 429 and 502 → unavailable | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (429 and 502 rows) | yes, S11 | ✅ pass |
| R6 | Item response stream breaks → unavailable | `Koha::KohaGetItemAdapterFailureTests::Item_response_disconnection_is_unavailable` | yes, S12b (S12 had no effect; see note 1) | ✅ pass |
| R6 | Unavailable → `items.get.unavailable` | `Application::GetItemUseCaseTests::Translates_gateway_outcomes` (Unavailable row) | yes, S13 | ✅ pass |
| R6 | `items.get.unavailable` → 503 masked | `Api::ApiErrorHandlingTests::Masks_server_failures_and_logs_internal_code_with_response_trace_id` | yes, S14 | ✅ pass |
| R7 | Koha 403 → authentication failed | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (403 row) | yes, S15 | ✅ pass |
| R7 | Koha 401 → authentication failed | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (401 row) | yes: mapping only 403 to authentication made it fail with `InvalidResponse` | ✅ pass |
| R7 | Koha answers 401 to the request and to its single retry, through the real bearer handler and token provider → authentication failed, with 2 item requests and 2 token acquisitions | `Koha::KohaGetItemAdapterFailureTests::Koha_401_that_persists_after_retry_is_an_authentication_failure` | yes: mapping only 403 to authentication made it fail with `InvalidResponse` | ✅ pass |
| R7 | OAuth failure → authentication failed | `Koha::KohaGetItemAdapterTests::Translates_oauth_failure_to_authentication_outcome` | yes, S16 | ✅ pass |
| R7 | OAuth never answers → authentication failed after the deadline | `Koha::KohaGetItemAdapterFailureTests::OAuth_timeout_is_an_authentication_failure` | yes, S16 and S17 | ✅ pass |
| R7 | OAuth response stream breaks → authentication failed | `Koha::KohaGetItemAdapterFailureTests::OAuth_response_disconnection_is_an_authentication_failure` | yes, S16 and S18 | ✅ pass |
| R7 | OAuth 4xx/5xx, malformed JSON, network failure, or timeout → authentication exception | `Koha::KohaAccessTokenProviderTests::Converts_*` (four tests) | yes: S19 (HTTP failures and malformed JSON), S20 (network), S21 (timeout) | ✅ pass |
| R7 | `items.get.authentication_failed` → 500 masked | `Api::ApiErrorHandlingTests::Masks_server_failures_and_logs_internal_code_with_response_trace_id` | yes, S22 | ✅ pass |
| R8 | Koha 400 → invalid response | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (400 row) | yes, S23 | ✅ pass |
| R8 | Item with an unparseable date → invalid response | `Koha::KohaGetItemAdapterFailureTests::Invalid_date_is_an_invalid_response` | yes, S23 (S24 had no effect; see note 1) | ✅ pass |
| R8 | Koha 200 with `item_id` 0 or -1, or `biblio_id` 0 or -3 → invalid response | `Koha::KohaGetItemAdapterTests::Non_positive_identifiers_in_koha_item_are_an_invalid_response` | yes: removing the identifier check made all four rows fail with `Found` | ✅ pass |
| R8 | `items.get.invalid_response` → 502 masked | `Api::ApiErrorHandlingTests::Masks_server_failures_and_logs_internal_code_with_response_trace_id` | yes, S25 | ✅ pass |
| R9 | Each server-failure code → masked body, and the internal code is logged with the response `traceId` | `Api::ApiErrorHandlingTests::Masks_server_failures_and_logs_internal_code_with_response_trace_id` | yes: S26 (internal code not logged), S27 (body gains a `detail`) | ✅ pass |
| R9 | Unhandled exception, in Development and in Production → masked 500, and the exception is logged with `traceId` | `Api::ApiErrorHandlingTests::Masks_unhandled_exceptions_and_logs_exception_with_response_trace_id` | yes: S27 (body gains a `detail`), S28 (exception not logged) | ✅ pass |
| R10 | 300 ms OAuth plus a stalled body under a 500 ms deadline → unavailable in under 700 ms | `Koha::KohaGetItemAdapterFailureTests::Total_deadline_includes_oauth_and_item_response_body` | yes, S29 | ✅ pass |
| R11 | First 401 → fresh token, then a single retry | `Koha::KohaBearerHandlerTests::Retries_get_once_with_fresh_token_after_401` | yes, S30 | ✅ pass |
| R11 | Second 401 → no further retry | `Koha::KohaBearerHandlerTests::Does_not_retry_a_second_401` | yes, S31 | ✅ pass |
| R11 | Non-GET 401 → no retry | `Koha::KohaBearerHandlerTests::Does_not_retry_non_get_request` | yes, S32 | ✅ pass |
| R12 | Client cancels during OAuth → the cancellation propagates with the caller's token | `Koha::KohaGetItemAdapterFailureTests::Caller_cancellation_during_oauth_is_propagated` | yes, S33 | ✅ pass |
| R12 | Client aborts → nothing is logged as an internal failure | `Api::ApiErrorHandlingTests::Propagates_caller_cancellation_without_logging_an_internal_failure` | yes, S34: the test hung and was aborted (see note 2) | ✅ pass |
| R13 | Date `2026-09-06` under culture `ar-SA` → found | `Koha::KohaGetItemAdapterFailureTests::Iso_date_is_parsed_independently_of_current_culture` | yes, S35 | ✅ pass |

**Gaps: 0.** Every requirement row has a passing test that was seen to fail when its behavior was broken.

**R11** is also proven end to end by `Koha::KohaGetItemAdapterFailureTests::Koha_401_that_persists_after_retry_is_an_authentication_failure`: through the real bearer handler and token provider, it asserts 2 item requests and 2 token acquisitions. Seen failing: yes, S30 (no retry) and S31 (a third attempt).

## Edge cases and prohibitions

| Item from the spec | State | Proven by |
| --- | --- | --- |
| Identifier has spaces around a valid number | covered | `Application::GetItemUseCaseTests::Trims_identifier_before_calling_gateway`: `"  12 "` reaches the gateway as `"12"`, and the not-found detail names `"12"`. Seen failing: yes, removing `Trim()` made it fail |
| Identifier is larger than a 32-bit number | covered | `Koha::KohaGetItemAdapterTests::Rejects_non_positive_or_out_of_range_identifier_without_network` (`"2147483648"` row). Seen failing: yes, parsing as a 64-bit number made it fail (a network call was attempted) |
| Item has no barcode or call number | covered | `Koha::KohaGetItemAdapterTests::Maps_missing_barcode_and_call_number_to_null`: an explicit `null` barcode and an absent call number both come back as `null`. Seen failing: yes, substituting an empty string made it fail |
| Many requests share one token refresh | covered | `Koha::KohaAccessTokenProviderTests::Concurrent_callers_share_one_refresh`: the token response is held until all 20 callers are waiting. Seen failing: yes, S36 (20 token requests instead of 1). See note 3 |
| All other covered edge cases and prohibitions | covered | The requirement rows above |
| Real Koha returns fields in an unexpected form | manual | Manual scenario 1 |
| Service account lacks the `catalogue` permission | manual | Manual scenario 5 |
| Secrets and tokens are not logged | manual | Manual scenario 5 |

Writing this table found 2 more gaps and 1 weak test, in addition to the 3 above. All are now closed.

## Deliberate code changes

Each change was applied alone, only the named tests were run, and the file was restored with `git checkout` (S35's temporary file was deleted). "Failed" lists the tests that failed under the change.

| Id | File | Change | Failed |
| --- | --- | --- | --- |
| S01 | `KohaGetItemAdapter.cs` | Barcode filled from the call number | `Maps_generated_item_to_get_item_response` |
| S02 | `GetItemUseCase.cs` | A found item is not accepted (`Response is null` instead of `is not null`) | `Returns_get_item_response_when_gateway_finds_item` |
| S03 | `ApplicationResultHttpMapper.cs` | Item wrapped in an extra `item` object | `Returns_item_json_from_application_use_case` |
| S04 | `GetItemUseCase.cs` | Blank check reduced to a null check | `Rejects_blank_identifier_without_calling_gateway` |
| S05 | `ApplicationResultHttpMapper.cs` | Validation mapped to 422 | `Maps_application_errors_to_problem_details` (validation row) |
| S06 | `KohaGetItemAdapter.cs` | Letters stripped, so `"koha-12"` becomes 12 | `Rejects_non_numeric_identifier_without_network` |
| S07 | `GetItemUseCase.cs` | Invalid identifier mapped to `items.not_found` | `Translates_gateway_outcomes` (InvalidIdentifier row) |
| S08 | `KohaGetItemAdapter.cs` | Only 410 treated as not found | `Translates_item_http_failures` (404 row) |
| S09 | `GetItemUseCase.cs` | Not found mapped to unavailable | `Translates_gateway_outcomes` (NotFound row) |
| S10 | `ApplicationResultHttpMapper.cs` | `{id}` no longer filled into the detail | `Maps_application_errors_to_problem_details` (not-found row) |
| S11 | `KohaGetItemAdapter.cs` | Only 503 treated as unavailable | `Translates_item_http_failures` (429 and 502 rows) |
| S12 | `KohaGetItemAdapter.cs` | `HttpIOException` rethrown | none (note 1) |
| S12b | `KohaGetItemAdapter.cs` | `HttpRequestException` rethrown | `Item_response_disconnection_is_unavailable` |
| S13 | `GetItemUseCase.cs` | Unavailable mapped to invalid response | `Translates_gateway_outcomes` (Unavailable row) |
| S14 | `ApplicationResultHttpMapper.cs` | Unavailable mapped to 502 | `Masks_server_failures_...` (`items.get.unavailable` row) |
| S15 | `KohaGetItemAdapter.cs` | Only 401 treated as an authentication failure | `Translates_item_http_failures` (403 row) |
| S16 | `KohaGetItemAdapter.cs` | `KohaAuthenticationException` rethrown | `Translates_oauth_failure_to_authentication_outcome`, `OAuth_timeout_is_an_authentication_failure`, `OAuth_response_disconnection_is_an_authentication_failure` |
| S17 | `KohaRequestTimeoutHandler.cs` | An OAuth timeout no longer converted to an authentication failure | `OAuth_timeout_is_an_authentication_failure` (got `Unavailable`) |
| S18 | `KohaAccessTokenProvider.cs` | Cut-off OAuth response rethrown | `OAuth_response_disconnection_is_an_authentication_failure` |
| S19 | `KohaAccessTokenProvider.cs` | OAuth HTTP and parse failures not converted | `Converts_all_oauth_http_failures_...` (4 rows), `Converts_malformed_json_...` |
| S20 | `KohaAccessTokenProvider.cs` | Network failures not converted | `Converts_network_failure_to_authentication_failure` |
| S21 | `KohaAccessTokenProvider.cs` | Timeouts not converted | `Converts_timeout_to_authentication_failure` |
| S22 | `ApplicationResultHttpMapper.cs` | Other failures mapped to 501 | `Masks_server_failures_...` (authentication and unexpected rows) |
| S23 | `KohaGetItemAdapter.cs` | Unusable Koha responses treated as unavailable | `Translates_item_http_failures` (400 row), `Invalid_date_is_an_invalid_response` |
| S24 | `KohaGetItemAdapter.cs` | `JsonException` treated as unavailable | none (note 1) |
| S25 | `ApplicationResultHttpMapper.cs` | Invalid response mapped to 500 | `Masks_server_failures_...` (`items.get.invalid_response` row) |
| S26 | `ApplicationResultHttpMapper.cs` | Internal code left out of the log | `Masks_server_failures_...` (all 4 rows) |
| S27 | `ApiErrorResultFactory.cs` | Server-error body gains a `detail` | `Masks_server_failures_...` (all 4 rows), `Masks_unhandled_exceptions_...` (both rows) |
| S28 | `ApiExceptionHandler.cs` | Exception left out of the log | `Masks_unhandled_exceptions_...` (both rows) |
| S29 | `KohaRequestTimeoutHandler.cs` | Deadline tripled | `Total_deadline_includes_oauth_and_item_response_body` |
| S30 | `KohaBearerHandler.cs` | A 401 never retried | `Retries_get_once_with_fresh_token_after_401`, `Koha_401_that_persists_after_retry_...` |
| S31 | `KohaBearerHandler.cs` | A second 401 retried again | `Does_not_retry_a_second_401`, `Koha_401_that_persists_after_retry_...` |
| S32 | `KohaBearerHandler.cs` | Non-GET requests retried | `Does_not_retry_non_get_request` |
| S33 | `KohaRequestTimeoutHandler.cs` | Caller cancellation during OAuth turned into an authentication failure | `Caller_cancellation_during_oauth_is_propagated` |
| S34 | `ItemEndpoints.cs` | Use case given `CancellationToken.None` | `Propagates_caller_cancellation_...`: hung and was aborted by `--blame-hang-timeout 60s` (note 2) |
| S35 | new temporary file in `Minioasis.Koha/Items/Adapters/` | NSwag's `UpdateJsonSerializerSettings` hook adds a `DateOnly` converter that uses the current culture | `Iso_date_is_parsed_independently_of_current_culture` (`FormatException` under `ar-SA`) |
| S36 | `KohaAccessTokenProvider.cs` | Second cache check inside the lock removed | `Concurrent_callers_share_one_refresh` (expected 1 token request, got 20) |

**Notes**

1. **S12 and S24 had no effect, because those catch blocks are not on the path the failures take.** The timeout handler buffers the body. A cut-off body therefore reaches the adapter as an `HttpRequestException`, not an `HttpIOException`, so S12b is the change that proves R6. The NSwag client reports an unreadable body as a `KohaServiceException`, not a `JsonException`, so S23 proves R8. Both catch blocks look unreachable in the configured handler chain. They were left in place because removing them is a refactor outside this verification.
2. **S34 hangs instead of failing cleanly.** Without the cancellation token, the stub use case never finishes. The test's own 10-second wait fails, but disposing the test host then waits for the abandoned request. The run still fails (the hang detector aborts it, and CI would time out), but a cleaner failure would be better. This is a possible test improvement, not a gap.
3. **The concurrency test was strengthened in this change.** Its fake token server answered instantly, so the 20 callers ran one after another and S36 had no effect. The token response is now held until all 20 calls are waiting. The test passed 10 runs in a row.

## Wiring

| Piece | Registered in | Reached from |
| --- | --- | --- |
| `GetItemUseCase` as `IGetItemUseCase` | `AddMinioasisApplication` | `ItemEndpoints.GetItemAsync` |
| `KohaGetItemAdapter` as `IGetItemGateway` | `AddMinioasisKoha` | `GetItemUseCase` |
| `ItemEndpoints` | `AddEndpointModules` (found automatically) | `MapEndpointModules` in `Program.cs` |
| `KohaRequestTimeoutHandler`, `KohaBearerHandler` | `AddMinioasisKoha`, on the API HTTP client | Every Koha API request |

## Checks

Run for pull request #11 (based on `e850a93`), on 2026-11-15:

| Command | Result |
| --- | --- |
| `dotnet format minioasis.slnx --verify-no-changes --no-restore` | passed |
| `dotnet test minioasis.slnx --configuration Release --no-restore` | passed: 98 tests (Application 16, Koha 51, Api 31), 0 failed. Rerun after the concurrency test was strengthened: same result |
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
