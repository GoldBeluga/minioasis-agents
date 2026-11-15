# Roadmap

Planned and completed specs, in intended order. Agents may add `proposed` rows and update status. Only the maintainer's acceptance of a spec moves it to `approved` (see `docs/specs/README.md`). Deferred ideas from specs are added under "Ideas not yet scheduled".

**Product scope** (from `AGENTS.md`): staff circulation and catalogue workflows on top of Koha. Anything outside that scope needs the scope changed first, as a Level 2 change to `AGENTS.md`.

| # | Spec | Status | Why / notes |
| --- | --- | --- | --- |
| 001 | [Get item](specs/001-get-item/spec.md) | approved | Existing behavior, documented after the fact. Not yet verified: independent verification stopped after three rounds with 7 open findings, awaiting the maintainer. |
| 002 | Staff authentication: design | proposed | The largest gap: the API is open to anyone who can reach it. Decides staff identity (for example Cloudflare Access, an OIDC provider, or Supabase) and how staff actions are audited, because Koha attributes every call to the service account. A design and ADR only; implementation is a follow-up spec. |
| 003 | Health endpoint | proposed | Needed for container and infrastructure monitoring. |
| — | Circulation and catalogue workflows | to be decided | Maintainer to choose and order them, for example check out, check in, renew, holds, item search, and record edits. |

## Ideas not yet scheduled

- Mutation testing (Stryker.NET) to measure test strength. Needs a new tool dependency (Level 2).
- Decide whether a Koha 400 should map to 502 or to a validation error (noted in spec 001).
- Structured logging or OpenTelemetry.
- Staff frontend (React, TypeScript, Vite, Tailwind). Needs a scope and architecture decision.
