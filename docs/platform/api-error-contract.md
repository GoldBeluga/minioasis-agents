# API error contract

This document is binding. It records how the API presents errors today, for every capability. It was moved here unchanged in meaning from `AGENTS.md`, which now holds only the principles. Changing this file is a Level 2 change. A shared-behavior spec is intended to replace it; until then this is the contract.

## Status mapping and response shape

Preserve the category mapping of validation to 400, not found to 404, invalid dependency response to 502, unavailable dependency to 503, and dependency authentication or unexpected failures to 500. Preserve the `code` and `traceId` Problem Details extensions. Only English is currently supported.

Every server-error response (500, 502, or 503) must use the title `Internal error` and public code `internal_error`, omit `detail`, and include the request's `traceId`. Do not expose provider names, internal error codes, authentication diagnoses, exceptions, or other technical explanations. Keep actionable errors' titles, details, and public codes. Return JSON with `application/problem+json` and omit `type` and `instance` links from error bodies. Keep this response policy consistent for returned Application failures and unhandled exceptions in Development and Production. Preserve caller cancellation rather than converting abandoned requests into internal errors.

## Logging

Record the actual internal error code for recognized server failures and the exception for unexpected failures in server logs, together with the same request `traceId` returned to the client. Keep credentials and access tokens out of diagnostic output. Internal error definitions remain available for diagnostics even when the public code is masked. Define the generic server response once in the API response factory; keep the English resource exclusively for client-visible errors and maintain startup validation of that subset.

## Adding an error

To add an error, declare a `public static readonly ApplicationErrorDefinition` in the relevant feature's error-definition group. Mark a new group with the existing `[ErrorDefinitions]` attribute so registry discovery includes it. For a client-visible category (currently validation or not found), add the exact matching, case-sensitive key to `Minioasis.Api/Resources/errors.en.json` with nonblank `title` and `detail` values. Do not add resource entries for internal server-error categories. Return an `ApplicationError` built from the definition, supplying metadata for client-message placeholders where needed. When introducing another actionable category, explicitly design its public response and include it in resource validation.

## Startup validation

The JSON resource must contain exactly the client-visible definitions from the registry. Preserve startup rejection of blank or duplicate identifiers, null definitions, invalid categories, and empty registries. Reject missing client-visible messages, entries for internal errors, and unknown, duplicate, malformed, null, or incomplete messages. Validate this at startup so inconsistent error definitions or presentation fail before requests depend on them.
