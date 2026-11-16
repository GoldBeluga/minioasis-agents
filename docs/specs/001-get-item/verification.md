# 001 — Get item: verification

- **Spec:** [spec.md](spec.md)
- **Verified at commit:** `c2685c0` (tests and code), on 2026-11-16, by an independent agent. History: coverage reviewed against `7122045`; gaps closed by pull requests #11 and #12
- **Transfer branch:** `ready/001-get-item`, to be created at the `master` commit that merges pull request #12

## Requirement coverage

Test paths are shortened. `Api` means `Minioasis.Api.Tests`, and the same for the other projects.

"Seen failing" names the deliberate code change that made the test fail. Changes added by pull request #11 are described inline. Changes `S01`–`S36` are listed under [Deliberate code changes](#deliberate-code-changes). Every change was reverted afterwards.

| Req | Proves (Given / When / Then) | Test | Seen failing | Result |
| --- | --- | --- | --- | --- |
| R1 | Koha returns an item with id 12, biblio 34, barcode ABC, call number QA1 → adapter returns those four values | `Koha::KohaGetItemAdapterTests::Maps_generated_item_to_get_item_response` | yes, S01 | ✅ pass |
| R1 | Gateway finds an item → use case returns it unchanged | `Application::GetItemUseCaseTests::Returns_get_item_response_when_gateway_finds_item` | yes, S02 | ✅ pass |
| R1 | Use case succeeds → API returns 200 with exactly the fields `barcode`, `bibliographicRecordId`, `callNumber`, `id` | `Api::ItemEndpointsTests::Returns_item_json_from_application_use_case` | yes, S03 | ✅ pass |
| R1, R3 | Production composition (real use case, adapter, handlers and dependency injection; only the network below the Koha HTTP clients is fake): `GET /api/v1/items/%2077%20` → 200 with id 77, Koha receives `/api/v1/items/77` with the bearer token; `GET /api/v1/items/abc` → 400 `items.invalid_identifier` with no Koha request | `Api::ItemEndpointsCompositionTests` (2 tests) | yes, V06 (route identifier ignored) and W1 (gateway registration removed) | ✅ pass |
| R1 | Koha item without barcode or call number → 200 whose JSON contains `"barcode": null` and `"callNumber": null` | `Api::ItemEndpointsCompositionTests::Item_without_barcode_or_call_number_returns_explicit_nulls` | yes, G4 (null fields left out of JSON) | ✅ pass |
| R2 | Identifier `"  "` → `items.invalid_identifier`, and the gateway is called 0 times | `Application::GetItemUseCaseTests::Rejects_blank_identifier_without_calling_gateway` | yes, S04 | ✅ pass |
| R2, R3, R4 | Validation error → 400 with the correct title, detail, code, `traceId`, `application/problem+json`, and no `type` or `instance` | `Api::ItemEndpointsTests::Maps_application_errors_to_problem_details` (validation row) | yes, S05 | ✅ pass |
| R3 | Identifier `"koha-12"` → invalid identifier, and 0 network calls | `Koha::KohaGetItemAdapterTests::Rejects_non_numeric_identifier_without_network` | yes, S06 | ✅ pass |
| R3 | Identifier `"12.5"`, `"1,000"` or `"1e3"` → invalid identifier, and 0 network calls | `Koha::KohaGetItemAdapterTests::Rejects_non_positive_or_out_of_range_identifier_without_network` (3 rows) | yes, G1 (decimal parsed and truncated: a network call was attempted) | ✅ pass |
| R3, R4 | Gateway reports an invalid identifier → `items.invalid_identifier` / Validation | `Application::GetItemUseCaseTests::Translates_gateway_outcomes` (InvalidIdentifier row) | yes, S07 | ✅ pass |
| R4 | Identifier `"0"` or `"-5"` → invalid identifier, and 0 network calls | `Koha::KohaGetItemAdapterTests::Rejects_non_positive_or_out_of_range_identifier_without_network` (`"0"` and `"-5"` rows) | yes: removing `kohaId <= 0` from the adapter made both rows fail (a network call was attempted) | ✅ pass |
| R5 | Koha 404 → not found | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (404 row) | yes, S08 | ✅ pass |
| R5 | Not found → `items.not_found` / NotFound | `Application::GetItemUseCaseTests::Translates_gateway_outcomes` (NotFound row) | yes, S09 | ✅ pass |
| R5 | Not found → 404, detail "No item was found with identifier 12." | `Api::ItemEndpointsTests::Maps_application_errors_to_problem_details` (not-found row) | yes, S10 | ✅ pass |
| R5 | Koha has no item 9001 → 404 with detail "No item was found with identifier 9001.", through the production composition; and the use case names `77` for a request for 77 | `Api::ItemEndpointsCompositionTests::Missing_item_returns_not_found_naming_the_requested_identifier`, `Application::GetItemUseCaseTests::Not_found_names_the_requested_identifier` | yes, G3 (detail always names 12) | ✅ pass |
| R6 | Koha 429 and 502 → unavailable | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (429 and 502 rows) | yes, S11 | ✅ pass |
| R6 | Koha 500 and 503 → unavailable | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (500 and 503 rows) | yes, X2 | ✅ pass |
| R6 | Koha 504 and 599 → unavailable ("any 5xx") | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (504 and 599 rows) | yes, K4 | ✅ pass |
| R6 | Item request fails to connect (`HttpRequestException`) → unavailable | `Koha::KohaGetItemAdapterTests::Connection_failure_is_unavailable` | yes, V19 | ✅ pass |
| R6, R10 | Item request never answers → unavailable at the deadline (300 ms in the test) | `Koha::KohaGetItemAdapterFailureTests::Unanswered_item_request_is_unavailable_at_the_deadline` | yes, V24 | ✅ pass |
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
| R8 | Koha 405, 409, 410 and 422 → invalid response ("any other 4xx") | `Koha::KohaGetItemAdapterTests::Translates_item_http_failures` (405, 409, 410 and 422 rows) | yes, K5 (410 treated as not found) | ✅ pass |
| R8 | Item with an unparseable date → invalid response | `Koha::KohaGetItemAdapterFailureTests::Invalid_date_is_an_invalid_response` | yes, S23 (S24 had no effect; see note 1) | ✅ pass |
| R8 | Koha 200 with body `null` or `{` → invalid response | `Koha::KohaGetItemAdapterTests::Unusable_success_body_is_an_invalid_response` | yes, X3 | ✅ pass |
| R8 | Koha 200 with `item_id` 0 or -1, or `biblio_id` 0 or -3 → invalid response | `Koha::KohaGetItemAdapterTests::Non_positive_identifiers_in_koha_item_are_an_invalid_response` | yes: removing the identifier check made all four rows fail with `Found` | ✅ pass |
| R8 | `items.get.invalid_response` → 502 masked | `Api::ApiErrorHandlingTests::Masks_server_failures_and_logs_internal_code_with_response_trace_id` | yes, S25 | ✅ pass |
| R9 | Each server-failure code → masked body, and the internal code is logged with the response `traceId` | `Api::ApiErrorHandlingTests::Masks_server_failures_and_logs_internal_code_with_response_trace_id` | yes: S26 (internal code not logged), S27 (body gains a `detail`) | ✅ pass |
| R9 | Unhandled exception, in Development and in Production → masked 500, and the exception is logged with `traceId` | `Api::ApiErrorHandlingTests::Masks_unhandled_exceptions_and_logs_exception_with_response_trace_id` | yes: S27 (body gains a `detail`), S28 (exception not logged) | ✅ pass |
| R10 | 300 ms OAuth plus a stalled body under a 500 ms deadline → unavailable in under 700 ms | `Koha::KohaGetItemAdapterFailureTests::Total_deadline_includes_oauth_and_item_response_body` | yes, S29 | ✅ pass |
| R10 | The registered deadline is 10 seconds | `Koha::KohaServiceCollectionExtensionsTests::Registered_koha_deadline_is_ten_seconds` | yes, V25 | ✅ pass |
| R10 | The configured deadline is the one applied: a 2-second deadline ends an unanswered request after 1.9–2.4 s | `Koha::KohaGetItemAdapterFailureTests::Deadline_is_applied_as_configured` | yes, G5 (1.25× deadline: took 2.52 s) | ✅ pass |
| R10 | The API client registered by `AddMinioasisKoha` puts token acquisition inside the deadline: OAuth never answers → authentication failure in under 5 s with a 300 ms deadline | `Koha::KohaServiceCollectionExtensionsTests::Registered_api_client_bounds_token_acquisition_by_the_deadline` | yes, V26 (handler order swapped) | ✅ pass |
| R11 | First 401 → fresh token, then a single retry | `Koha::KohaBearerHandlerTests::Retries_get_once_with_fresh_token_after_401` | yes, S30 | ✅ pass |
| R11 | Second 401 → no further retry | `Koha::KohaBearerHandlerTests::Does_not_retry_a_second_401` | yes, S31 | ✅ pass |
| R11 | 401 on POST, PUT, PATCH, DELETE, HEAD or OPTIONS → no retry | `Koha::KohaBearerHandlerTests::Does_not_retry_non_get_request` (6 rows) | yes: S32 (all methods retried), K11 (only POST excluded: the PUT, PATCH and DELETE rows failed), G7 (HEAD and OPTIONS retried: those rows failed) | ✅ pass |
| R12 | Client cancels during OAuth → the cancellation propagates with the caller's token | `Koha::KohaGetItemAdapterFailureTests::Caller_cancellation_during_oauth_is_propagated` | yes, S33 | ✅ pass |
| R12 | Client cancels during the item request → the cancellation propagates with the caller's token | `Koha::KohaGetItemAdapterFailureTests::Caller_cancellation_during_item_request_is_propagated` | yes, X1 | ✅ pass |
| R12 | The use case passes the caller's cancellation token to the gateway | `Application::GetItemUseCaseTests::Passes_caller_cancellation_token_to_gateway` | yes, V33 | ✅ pass |
| R12 | A cancellation thrown by the gateway leaves the use case as a cancellation, not as a failure result | `Application::GetItemUseCaseTests::Propagates_cancellation_from_gateway` | yes, G6 (use case returned unavailable) | ✅ pass |
| R12 | Client aborts → nothing is logged at Error level or above, and neither internal-failure message is logged | `Api::ApiErrorHandlingTests::Propagates_caller_cancellation_without_logging_an_internal_failure` | yes: P14 (the endpoint logs an error for the abandoned request; failed in 5 of 5 runs), and S34 (the test hung and was aborted; see note 2) | ✅ pass |
| R13 | Date `2026-09-06` under culture `ar-SA` → found | `Koha::KohaGetItemAdapterFailureTests::Iso_date_is_parsed_independently_of_current_culture` | yes, S35 | ✅ pass |

**Gaps: 0 known.** Every row in this table has a passing test that was seen to fail when its behavior was broken. The seven gaps found by verification round 3 are closed by the rows marked G1–G7. A final independent check is recorded under [Independent verification](#independent-verification).

Independent verification round 1 found seven code changes that no test caught (V06, W1, V19, V24, V25, V26, V33), and round 2 found four more (K4, K5, K11, P14). The rows marked with those identifiers, and the X rows, were added to close them.

**R11** is also proven end to end by `Koha::KohaGetItemAdapterFailureTests::Koha_401_that_persists_after_retry_is_an_authentication_failure`: through the real bearer handler and token provider, it asserts 2 item requests and 2 token acquisitions. Seen failing: yes, S30 (no retry) and S31 (a third attempt).

## Edge cases and prohibitions

| Item from the spec | State | Proven by |
| --- | --- | --- |
| Identifier has spaces around a valid number | covered | `Application::GetItemUseCaseTests::Trims_identifier_before_calling_gateway`: `"  12 "` reaches the gateway as `"12"`, and the not-found detail names `"12"`. Seen failing: yes, removing `Trim()` made it fail |
| Identifier is larger than a 32-bit number | covered | `Koha::KohaGetItemAdapterTests::Rejects_non_positive_or_out_of_range_identifier_without_network` (`"2147483648"` and `"4294967308"` rows). Seen failing: yes, parsing as a 64-bit number made the first fail, and G2 (64-bit parse with a wrapping cast) made the second fail |
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
| V06 | `ItemEndpoints.cs` | Route identifier ignored; always asks for `"12"` | `ItemEndpointsCompositionTests` (both tests) |
| W1 | `KohaServiceCollectionExtensions.cs` | `IGetItemGateway` registration removed | `ItemEndpointsCompositionTests` (both tests) |
| V19 | `KohaGetItemAdapter.cs` | `HttpRequestException` caught only when it has an inner exception | `Connection_failure_is_unavailable` |
| V24 | `KohaBearerHandler.cs` | Item request sent with `CancellationToken.None` | `Unanswered_item_request_is_unavailable_at_the_deadline` |
| V25 | `KohaOptions.cs` | Default deadline 60 seconds | `Registered_koha_deadline_is_ten_seconds` |
| V26 | `KohaServiceCollectionExtensions.cs` | Bearer handler registered outside the timeout handler | `Registered_api_client_bounds_token_acquisition_by_the_deadline` |
| V33 | `GetItemUseCase.cs` | Gateway given `CancellationToken.None` | `Passes_caller_cancellation_token_to_gateway` |
| X1 | `KohaGetItemAdapter.cs` | Caller cancellation treated as unavailable | `Caller_cancellation_during_item_request_is_propagated` |
| X2 | `KohaGetItemAdapter.cs` | Only 429 and 502 treated as unavailable | `Translates_item_http_failures` (500 and 503 rows) |
| X3 | `KohaGetItemAdapter.cs` | Unusable Koha responses treated as unavailable | `Unusable_success_body_is_an_invalid_response` (both rows) |
| K4 | `KohaGetItemAdapter.cs` | Only 500, 502 and 503 treated as unavailable | `Translates_item_http_failures` (504 and 599 rows) |
| K5 | `KohaGetItemAdapter.cs` | Koha 410 treated as not found | `Translates_item_http_failures` (410 row) |
| K11 | `KohaBearerHandler.cs` | Only POST excluded from the 401 retry | `Does_not_retry_non_get_request` (PUT, PATCH and DELETE rows) |
| P14 | `ItemEndpoints.cs` | Endpoint logs an error for an abandoned request, then rethrows | `Propagates_caller_cancellation_without_logging_an_internal_failure` (5 of 5 runs) |
| K8 | `KohaRequestTimeoutHandler.cs` | Body no longer buffered inside the deadline | `Total_deadline_includes_oauth_and_item_response_body`: fails cleanly after 5 s (it hung before this change) |
| G1 | `KohaGetItemAdapter.cs` | Identifier parsed as a decimal and truncated | `Rejects_non_positive_or_out_of_range_...` (`"12.5"`, `"1,000"`, `"1e3"` rows) |
| G2 | `KohaGetItemAdapter.cs` | Identifier parsed as 64-bit and cast with wrap-around | `Rejects_non_positive_or_out_of_range_...` (`"4294967308"` row) |
| G3 | `GetItemUseCase.cs` | Not-found detail always names 12 | `Not_found_names_the_requested_identifier` (expected "77", got "12"), `Missing_item_returns_not_found_naming_the_requested_identifier` |
| G4 | `Program.cs` | Null fields left out of all JSON responses | `Item_without_barcode_or_call_number_returns_explicit_nulls` |
| G5 | `KohaRequestTimeoutHandler.cs` | Deadline applied as 1.25× the setting | `Deadline_is_applied_as_configured` (2.52 s, outside 1.9–2.4 s) |
| G6 | `GetItemUseCase.cs` | Cancellation swallowed and returned as unavailable | `Propagates_cancellation_from_gateway` (no exception thrown) |
| G7 | `KohaBearerHandler.cs` | HEAD and OPTIONS retried after a 401 | `Does_not_retry_non_get_request` (HEAD and OPTIONS rows: expected 1 call, got 2) |
| K24 | `KohaAccessTokenProvider.cs` | OAuth called with `CancellationToken.None` | `OAuth_timeout_is_an_authentication_failure`, `Caller_cancellation_during_oauth_is_propagated`: both fail cleanly after 5 s (they hung before this change) |

**Notes**

1. **S12 and S24 had no effect, because those catch blocks are not on the path the failures take.** The timeout handler buffers the body. A cut-off body therefore reaches the adapter as an `HttpRequestException`, not an `HttpIOException`, so S12b is the change that proves R6. The NSwag client reports an unreadable body as a `KohaServiceException`, not a `JsonException`, so S23 proves R8. Both catch blocks look unreachable in the configured handler chain. They were left in place because removing them is a refactor outside this verification.
2. **S34 hangs instead of failing cleanly.** Without the cancellation token, the stub use case never finishes. The test's own 10-second wait fails, but disposing the test host then waits for the abandoned request. The run still fails (the hang detector aborts it, and CI would time out), but a cleaner failure would be better. This is a possible test improvement, not a gap. The three Koha tests that hung in the same way under round 2's changes (K8, K24) now wait at most 5 seconds and fail cleanly.
3. **The concurrency test was strengthened in this change.** Its fake token server answered instantly, so the 20 callers ran one after another and S36 had no effect. The token response is now held until all 20 calls are waiting. The test passed 10 runs in a row.
4. **Known limits recorded by round 2, not spec violations.** A caller waiting behind a token refresh does not react to its own cancellation until the refresh ends, which is bounded by the deadline (K23). No test pins the token provider as a singleton in the real composition; the shared refresh is proven on a directly constructed provider (K25). The timeout handler's rethrow of the caller's cancellation is redundant, because `HttpClient` re-attaches the caller's token itself (K13).

## Wiring

| Piece | Registered in | Reached from |
| --- | --- | --- |
| `GetItemUseCase` as `IGetItemUseCase` | `AddMinioasisApplication` | `ItemEndpoints.GetItemAsync` |
| `KohaGetItemAdapter` as `IGetItemGateway` | `AddMinioasisKoha` | `GetItemUseCase` |
| `ItemEndpoints` | `AddEndpointModules` (found automatically) | `MapEndpointModules` in `Program.cs` |
| `KohaRequestTimeoutHandler`, `KohaBearerHandler` | `AddMinioasisKoha`, on the API HTTP client | Every Koha API request |

## Checks

Run at the head of pull request #12, on 2026-11-16:

| Command | Result |
| --- | --- |
| `dotnet format minioasis.slnx --verify-no-changes --no-restore` | passed |
| `dotnet test minioasis.slnx --configuration Release --no-restore` | passed: 130 tests (Application 19, Koha 76, Api 35), 0 failed. The Koha suite passed 10 repeated runs and the Api suite 5 |
| `dotnet publish Minioasis.Api/Minioasis.Api.csproj --configuration Release --no-restore` | passed |

## Independent verification

**Round 1 (2026-11-15, at `72b7e35`): not verified.** A separate agent independently broke at least one aspect of every requirement and edge case. Seven changes went uncaught by the full suite: V06, W1, V19, V24, V25, V26 and V33. It confirmed notes 1–3, the wiring, and that nothing only a real Koha can show is recorded as a pass. Tests were added for all seven.

**Round 2 (2026-11-15, at `d5ecf44`): not verified.** A second, fresh agent devised its own changes, at least two per requirement, including the composition and wiring. Four changes went uncaught: K4 (any 5xx), K5 (any other 4xx), K11 (non-GET methods other than POST) and P14 (error-level log for an abandoned request). It also reported tests that hang rather than fail (K8, K24, S34) and that the Checks section did not match its commit. Tests were added for all four, the Koha tests that hung now fail cleanly, and the Checks section was corrected.

**Round 3 (2026-11-15, at `f08d017`): not verified.** A third, fresh agent applied 78 deliberate code changes. Every wiring and composition change was caught. Seven changes that violate explicit statements in spec.md still passed all 119 tests:

| # | Spec statement | Surviving change | Test that would close it |
| --- | --- | --- | --- |
| 1 | R3: an identifier that is not a whole number is rejected without calling Koha | Parse as a decimal and truncate: `12.5` and `1,000` reach Koha | Adapter rows for `12.5`, `1,000` and `1e3` |
| 2 | Edge: an identifier larger than a 32-bit number is rejected | `long` parse with an unchecked cast: `4294967308` reaches Koha as item 12 | Adapter row `4294967308` |
| 3 | R5: the detail names the requested identifier | Detail hard-coded to `"12"`; every test uses 12 | A not-found test with another identifier, at use-case or composition level |
| 4 | R1 / edge: `barcode` and `callNumber` may be `null` | Null fields left out of the JSON | An API or composition test asserting `"barcode": null` and `"callNumber": null` are present |
| 5 | R10: a 10-second deadline | Deadline applied as 1.25× the setting | A tighter bound in `Total_deadline_...`, or a test of the applied deadline with a fake time provider |
| 6 | R12: an abandoned request is not reported as a failure | The use case catches the cancellation and returns Unavailable | A use-case test that cancellation propagates, or a composition-level abort test |
| 7 | Prohibition: retry nothing that is not a GET | HEAD and OPTIONS retried after a 401 | `HEAD` and `OPTIONS` rows in `Does_not_retry_non_get_request` |

Non-blocking findings from round 3: `KohaAccessTokenProviderTests.Preserves_caller_cancellation` hangs instead of failing when the provider ignores the caller's token; the adapter's `item is null` check is also unreachable; no test shows that `id` comes from Koha's `item_id` rather than the request (the spec does not say which); the full suite passed 8 of 8 runs on unmodified source.

**The review-and-fix loop stopped here.** `AGENTS.md` stops it after three rounds, or sooner if a round does not reduce the blocking findings. Round 3 was the third round, and it found 7 blocking findings after round 2's 4.

**After round 3 (2026-11-16), on the maintainer's instruction in session:** tests were added for all seven findings (G1–G7), each seen to fail under the round-3 change. The maintainer also approved one final independent check against a fixed bar: every explicit Given/When/Then, edge case and prohibition in spec.md has a test that fails when it is directly broken.

**Final check (2026-11-16, at `c2685c0`): verified.** A fresh agent walked the spec clause by clause and applied 119 of its own direct breaks, one at a time. Every clause of R1–R13, every covered edge case and every covered prohibition had a test that failed. Blocking findings: none. It confirmed the checks (130 tests), the test names and counts in this file, and that nothing only a real Koha can show is recorded as a pass.

Non-blocking findings from the final check:

1. With the cancellation token dropped at the endpoint, `Propagates_caller_cancellation_without_logging_an_internal_failure` hangs instead of failing (note 2). The CI workflow has no hang timeout, so such a regression would stall CI until the job times out. Adding one changes `.github/`, which is a Level 2 change.
2. `Total_deadline_includes_oauth_and_item_response_body` (700 ms bound) failed once while three build-and-test runs shared 4 cores. Unloaded, the Koha suite passed every repeated run.
3. An endpoint that swallows the cancellation, logs only a warning and returns 500 to the already-disconnected client passes all tests. Nothing is observable to staff and no failure is logged.

### Clause checklist

| Spec clause | Proving test |
| --- | --- |
| R1: 200 with `id`, `bibliographicRecordId`, `barcode`, `callNumber` | `Maps_generated_item_to_get_item_response`, `Returns_item_json_from_application_use_case`, `Route_identifier_reaches_koha_through_the_production_composition` |
| R2: blank or whitespace → 400, no Koha call | `Rejects_blank_identifier_without_calling_gateway`, `Maps_application_errors_to_problem_details` |
| R3: not a whole number → 400, no Koha call | `Rejects_non_numeric_identifier_without_network`, `Rejects_non_positive_or_out_of_range_...` (`12.5`, `1,000`, `1e3`), `Invalid_route_identifier_is_rejected_without_calling_koha` |
| R4: zero or negative → 400, no Koha call | `Rejects_non_positive_or_out_of_range_...` (`0`, `-5`) |
| R5: 404 `items.not_found`, detail names the identifier | `Translates_item_http_failures` (404), `Missing_item_returns_not_found_naming_the_requested_identifier`, `Not_found_names_the_requested_identifier` |
| R6: 429 / any 5xx / connection failure / cut-off response / deadline → 503 | `Translates_item_http_failures` (429, 500, 502, 503, 504, 599), `Connection_failure_is_unavailable`, `Item_response_disconnection_is_unavailable`, `Unanswered_item_request_is_unavailable_at_the_deadline`, `Masks_server_failures_...` (503) |
| R7: OAuth failure / 401 after retry / 403 → 500 | `Translates_item_http_failures` (401, 403), `Koha_401_that_persists_after_retry_...`, `OAuth_*`, `Converts_*`, `Masks_server_failures_...` (500) |
| R8: other 4xx / unparseable body / no positive ids → 502 | `Translates_item_http_failures` (400, 405, 409, 410, 422), `Unusable_success_body_...`, `Invalid_date_...`, `Non_positive_identifiers_...`, `Masks_server_failures_...` (502) |
| R9: masked body, logged with the same `traceId` | `Masks_server_failures_...`, `Masks_unhandled_exceptions_...` |
| R10: 10-second deadline over token, request and response | `Registered_koha_deadline_is_ten_seconds`, `Deadline_is_applied_as_configured`, `Registered_api_client_bounds_token_acquisition_by_the_deadline`, `Total_deadline_includes_oauth_and_item_response_body` |
| R11: 401 → new token, exactly one retry | `Retries_get_once_with_fresh_token_after_401`, `Does_not_retry_a_second_401`, `Koha_401_that_persists_after_retry_...` |
| R12: abandoned request cancelled, not logged or reported as a failure | `Caller_cancellation_during_oauth_is_propagated`, `Caller_cancellation_during_item_request_is_propagated`, `Passes_caller_cancellation_token_to_gateway`, `Propagates_cancellation_from_gateway`, `Propagates_caller_cancellation_without_logging_an_internal_failure` |
| R13: ISO dates under a non-English culture | `Iso_date_is_parsed_independently_of_current_culture` |
| Edge: spaces around a valid number | `Trims_identifier_before_calling_gateway`, `Route_identifier_reaches_koha_...` |
| Edge: larger than 32 bits | `Rejects_non_positive_or_out_of_range_...` (`2147483648`, `4294967308`) |
| Edge: no barcode or call number | `Maps_missing_barcode_and_call_number_to_null`, `Item_without_barcode_or_call_number_returns_explicit_nulls` |
| Edge: many requests share one token refresh | `Concurrent_callers_share_one_refresh` |
| Prohibition: no Koha call for an invalid identifier | the R2, R3 and R4 tests (each asserts 0 calls) |
| Prohibition: reveal nothing internal | the R9 tests |
| Prohibition: no second retry, no non-GET retry | `Does_not_retry_a_second_401`, `Does_not_retry_non_get_request` (POST, PUT, PATCH, DELETE, HEAD, OPTIONS) |
| Prohibition: abandoned request is not a server failure | the R12 tests |
| Manual only: real Koha field forms, `catalogue` permission, secrets in logs | Manual acceptance scenarios 1 and 5 |

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
