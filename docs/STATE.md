# State

A snapshot of where the work stands. Agents overwrite this file in the pull request that changes it; it is not a log. Read it first in every session.

This file holds only what GitHub and Git cannot show. Work in progress is the list of open pull requests, and the maintainer's queue is the open pull requests labelled `needs-human`.

**Last updated:** 2026-11-15

## Current spec

- **001 Get item:** `approved`. All test gaps are closed. Before it is verified, "Seen failing" must be recorded for the older tests and an independent agent must verify it; see `docs/specs/001-get-item/verification.md`.

## Next

- Record "Seen failing" for the remaining 001 tests, have it verified independently, and create `ready/001-get-item`.
- Draft spec 002 (staff authentication design) for the maintainer.

## Recent decisions

- ADR 0001: two-repository workflow with autonomous agents.
- ADR 0002: development process adapted from GSD Core, without installing it.

## Open questions for the maintainer

- Staff identity provider and auditing of staff actions (spec 002).
- Whether Supabase is needed at all, and for what.
- Which circulation and catalogue workflows come first.

## Production transfers

| Spec | Transfer branch | Merged into production | Manual acceptance |
| --- | --- | --- | --- |
| — | — | none yet | — |

## Environment

- Cloud environment `minioasis-agents`: installs the .NET 10 SDK (10.0.115 from Ubuntu apt) and restores the solution at container start.
- Agents have no access to a live Koha.
