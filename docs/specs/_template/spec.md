# <NNN> — <Capability name>

- **Status:** draft
- **Kind:** read | write (state-changing in Koha) | internal
- **Related:** <ADRs, earlier specs, issues>

## Goal

<One or two sentences, specific enough to check: "Staff can X, so that Y." Not "improve X".>

## Background

<What exists today in the code and in Koha, and what is missing. Based on the repository, not on assumptions.>

## Staff and roles

<Who uses this, for example circulation desk, cataloguer, or administrator, and in which situation.>

## Staff workflow

<Numbered steps, from the staff member's point of view. No implementation detail.>

1. …

## Decisions

<Outcomes of the discussion with the maintainer. Number them and never renumber.>

**Locked** (decided by the maintainer; changing one is a Level 2 change)

- **D1.** <decision> — <date>

**Agent's discretion** (the maintainer said "you decide"; the choice is recorded in `plan.md`)

- <area>

**Deferred** (not part of this spec; added to `docs/ROADMAP.md`)

- <idea>

## Requirements

<Each requirement is numbered, testable, and written as Given / When / Then. Never renumber; mark retired requirements `(removed)`.>

- **R1.** Given <situation>, when <action>, then <observable result>.

## Edge cases

<What happens at the edges of each requirement: empty, zero, very large, repeated, simultaneous, half-finished. Every row needs a state.>

| Edge case | State | Requirement, reason, or manual scenario |
| --- | --- | --- |
| | covered / dismissed / manual | |

## Prohibitions

<What the system must never do. Every row needs a state.>

| The system must not… | State | Requirement, reason, or manual scenario |
| --- | --- | --- |
| | covered / dismissed / manual | |

## API contract

| Method | Route | Success | Request | Response |
| --- | --- | --- | --- | --- |
| | | | | |

<Response field names and types. Note anything nullable.>

## Errors

| Situation | HTTP status | Public code | Title / detail shown to staff |
| --- | --- | --- | --- |
| | | | |

Server failures (500, 502, 503) always return `internal_error` without detail, as `docs/platform/api-error-contract.md` requires. List only the situations that cause them.

## Koha operations

| Koha operation | Read or write | Required Koha permission | Focused contract |
| --- | --- | --- | --- |
| | | | `contracts/<name>.openapi.json` |

## Koha configuration

<System preferences, circulation rules, or authorized values that change this behavior. Write "None known" if none.>

## Failure and retry

<Required for write specs; optional for read specs. Describe what happens if Koha fails, times out, or partly succeeds; whether the operation is safe to retry; and what staff see and should do.>

## Security and privacy

<Who is allowed to perform this operation; which patron personal data is read, returned, or logged; and what must never be logged.>

| Threat | Mitigation | Requirement |
| --- | --- | --- |
| | | |

## Out of scope

- <What this spec deliberately does not do, and why.>

## Open questions

<Must be empty before approval.>

- …

## Ready for approval

- [ ] The goal is specific enough to check.
- [ ] Every requirement is testable.
- [ ] Every edge case and prohibition has a state.
- [ ] "Out of scope" names the nearest things this spec does not do.
- [ ] For a write spec, "Failure and retry" is complete.
- [ ] There are no open questions.
