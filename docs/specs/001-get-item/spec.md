# 001 — Get item

- **Status:** approved
- **Kind:** read
- **Related:** ADR 0001

> This spec was written after the fact. It documents behavior already on `master` at commit `7122045`, taken from the code and tests. It designs nothing new. Merging the pull request that adds it is the maintainer's approval of this behavior. It is not yet `verified`, because the coverage review found test gaps; see [verification.md](verification.md).

## Goal

Let staff look up a single Koha item by its internal Koha item number and see its basic identifying details. It is also the first end-to-end capability, establishing the patterns for Koha integration, errors, and tests that later specs copy.

## Background

The capability already exists on `master`. Koha exposes `GET /items/{item_id}`, which needs the `catalogue` permission. Minioasis calls it through a service account using OAuth 2 client credentials.

## Staff and roles

Any staff client of the API. **No staff authentication exists yet** (see Security and privacy).

## Staff workflow

1. The staff client sends the Koha internal item number.
2. Minioasis returns the item's identifiers, barcode, and call number, or explains why it cannot.

## Decisions

These decisions were read from the existing code. Merging this spec locks them.

**Locked**

- **D1.** Items are identified by Koha's internal item number, not by barcode. — 2026-11-15
- **D2.** Identifiers are returned as strings, although Koha stores numbers. — 2026-11-15
- **D3.** The response contains only `id`, `bibliographicRecordId`, `barcode`, and `callNumber`. — 2026-11-15
- **D4.** Staff never see why a server failure happened, only that one did, with a `traceId`. — 2026-11-15

**Agent's discretion**

- None.

**Deferred**

- Lookup by barcode.
- Item status, availability, and location.

## Requirements

- **R1.** Given an item exists in Koha, when staff request it by its item number, then Minioasis returns 200 with the item's `id`, `bibliographicRecordId`, `barcode`, and `callNumber`.
- **R2.** Given a blank or whitespace-only identifier, when staff request an item, then Minioasis returns 400 `items.invalid_identifier` without calling Koha.
- **R3.** Given an identifier that is not a whole number, when staff request an item, then Minioasis returns 400 `items.invalid_identifier` without calling Koha.
- **R4.** Given an identifier that is zero or negative, when staff request an item, then Minioasis returns 400 `items.invalid_identifier` without calling Koha.
- **R5.** Given Koha has no item with that number, when staff request it, then Minioasis returns 404 `items.not_found`, and the detail names the requested identifier.
- **R6.** Given Koha is unavailable (429, any 5xx, connection failure, response cut off, or the deadline in R10 exceeded while requesting the item), when staff request an item, then Minioasis returns 503 `internal_error`.
- **R7.** Given Minioasis cannot authenticate to Koha (OAuth token acquisition fails for any reason, including a timeout; or Koha answers 401 after the retry in R11; or Koha answers 403), when staff request an item, then Minioasis returns 500 `internal_error`.
- **R8.** Given Koha returns an unusable answer (any other 4xx, a body that cannot be parsed, or an item without a positive `item_id` and `biblio_id`), when staff request an item, then Minioasis returns 502 `internal_error`.
- **R9.** Given any server failure (500, 502, or 503), when Minioasis responds, then the body contains only `status`, `title: "Internal error"`, `code: "internal_error"`, and `traceId`, with no `detail`, and the internal error code or exception is logged with the same `traceId`.
- **R10.** Given Koha is slow, when staff request an item, then the whole Koha interaction (token acquisition, the request, and reading the response) is bounded by a 10-second deadline.
- **R11.** Given the cached Koha token is rejected with 401, when Minioasis requests the item, then it discards the token, obtains a new one, and retries exactly once.
- **R12.** Given the staff client abandons the request, when Minioasis is waiting on Koha, then the work is cancelled and is not logged or reported as an internal failure.
- **R13.** Given Koha returns dates in ISO format, when the server's culture is not English, then the item is still read correctly.

## Edge cases

| Edge case | State | Requirement, reason, or manual scenario |
| --- | --- | --- |
| Identifier is blank or only spaces | covered | R2 |
| Identifier has letters or symbols | covered | R3 |
| Identifier is zero or negative | covered | R4 |
| Identifier has spaces around a valid number | covered | R1: the use case trims it |
| Identifier is larger than a 32-bit number | covered | R3: it is not a usable whole number |
| Item exists but has no barcode or call number | covered | R1: both may be `null` |
| Koha answers slowly | covered | R10 |
| The Koha token expires between requests | covered | R11 |
| Many staff request items while the token is being refreshed | covered | R11: callers share one refresh |
| The staff client disconnects mid-request | covered | R12 |
| The server runs under a non-English culture | covered | R13 |
| The real Koha returns item fields in a form the contract does not describe | manual | Manual scenario 1 |
| The Koha service account lacks the `catalogue` permission | manual | Manual scenario 5 |

## Prohibitions

| The system must not… | State | Requirement, reason, or manual scenario |
| --- | --- | --- |
| Call Koha for an identifier it can already tell is invalid | covered | R2, R3, R4 |
| Reveal Koha, internal error codes, or exception text to staff | covered | R9 |
| Retry more than once after a 401, or retry anything that is not a GET | covered | R11 |
| Report an abandoned request as a server failure | covered | R12 |
| Log the Koha client secret or an access token | manual | Manual scenario 5: inspect the log |

## API contract

| Method | Route | Success | Request | Response |
| --- | --- | --- | --- | --- |
| GET | `/api/v1/items/{id}` | 200 | `id`: Koha internal item number, as a path segment | JSON object below |

```json
{
  "id": "12",
  "bibliographicRecordId": "34",
  "barcode": "ABC",
  "callNumber": "QA1"
}
```

- `id` and `bibliographicRecordId` are strings containing Koha's numeric identifiers.
- `barcode` (Koha `external_id`) and `callNumber` (Koha `callnumber`) may be `null`.
- Error bodies use `application/problem+json`, with no `type` or `instance` fields.

## Errors

| Situation | HTTP status | Public code | Title / detail shown to staff |
| --- | --- | --- | --- |
| Blank, non-numeric, zero, or negative id | 400 | `items.invalid_identifier` | "Invalid item identifier" / "The item identifier must be a valid positive identifier." |
| Item not in Koha | 404 | `items.not_found` | "Item not found" / "No item was found with identifier {id}." |
| Koha unusable response | 502 | `internal_error` | "Internal error" (internal code `items.get.invalid_response`) |
| Koha unavailable | 503 | `internal_error` | "Internal error" (internal code `items.get.unavailable`) |
| Koha authentication failure | 500 | `internal_error` | "Internal error" (internal code `items.get.authentication_failed`) |
| Unexpected failure | 500 | `internal_error` | "Internal error" (internal code `system.unexpected`) |

## Koha operations

| Koha operation | Read or write | Required Koha permission | Focused contract |
| --- | --- | --- | --- |
| `GET /items/{item_id}` (`getItem`) | read | `catalogue` | `contracts/items.openapi.json` |
| `POST /oauth/token` (client credentials) | — | — | `contracts/oauth.openapi.json` |

## Koha configuration

None known. The Koha service account needs the `catalogue` permission.

## Failure and retry

This is a read operation. The only retry is the single token-refresh retry in R11. Staff may simply repeat the request after a 503.

## Security and privacy

- **No inbound authentication.** Anyone who can reach the API can look up items through the Koha service account. This is a known gap, to be addressed by spec 002.
- No patron data is involved.
- The Koha client secret and access tokens are never logged.

## Out of scope

- Search by barcode or any other field.
- Item status, availability, location, or holdings details.
- Bibliographic record details.

## Open questions

None. This spec documents existing behavior.

## Notes

- **2026-11-15:** Documented from `master` at `7122045`. Noted for a future spec: a Koha `400` currently maps to 502 rather than to a validation error. This is defensible, because identifiers are validated before calling Koha, but it is undecided.

## Ready for approval

- [x] The goal is specific enough to check.
- [x] Every requirement is testable.
- [x] Every edge case and prohibition has a state.
- [x] "Out of scope" names the nearest things this spec does not do.
- [x] For a write spec, "Failure and retry" is complete. (Not a write spec.)
- [x] There are no open questions.
