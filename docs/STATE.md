# State

A snapshot of where the work stands. Agents overwrite this file in the pull request that changes it; it is not a log. Read it first in every session.

This file holds only what GitHub and Git cannot show. Work in progress is the list of open pull requests, and the maintainer's queue is the open pull requests labelled `needs-human`.

**Last updated:** 2026-11-16

## Current spec

- **001 Get item:** `verified` on 2026-11-16. Pull request #12 (labelled `needs-human`) carries the verification and waits for the maintainer's acceptance. See `docs/specs/001-get-item/verification.md`.

## Next

- Maintainer accepts pull request #12; then `ready/001-get-item` is created at the merge commit.
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
