# <NNN> — <Capability name>: implementation plan

- **Spec:** [spec.md](spec.md)
- **Plan status:** draft | checked | in progress | complete

## Approach

<A few paragraphs: how the capability fits the architecture in `AGENTS.md`, and the key design choices, including the choices made under "Agent's discretion".>

## One-way decisions

<Decisions whose undoing would force a change outside this repository's code; see the test and examples in `docs/specs/README.md`. Each must be settled by a locked decision or a requirement in the spec. Write "None" if there are none.>

| Decision | What undoing it would break | Settled by |
| --- | --- | --- |
| | | D<n> / R<n> |

## Files

| Project | File | New or changed | Purpose |
| --- | --- | --- | --- |
| `Minioasis.Application` | | | |
| `Minioasis.Koha` | | | |
| `Minioasis.Api` | | | |
| Tests | | | |

## Contracts and generation

<Which focused contract is created or changed, which operations it derives from `contracts/official_koha_openapi.json`, and which NSwag command regenerates the client. Write "None" if not applicable.>

## Error definitions

| Code | Category | Client-visible resource entry |
| --- | --- | --- |
| | | |

## Tests

<Every requirement, and every "covered" edge case and prohibition, needs at least one planned test. "Fails when" says what would make the test fail; a test that cannot fail proves nothing.>

| Requirement | Layer | Planned test | Fails when |
| --- | --- | --- | --- |
| R1 | Application / Api / Koha | `<Class>::<Method>` | |

## Pull request breakdown

<Small steps, each one finishable in a single session and mergeable on its own. Tests come first.>

1. …

## Risks

- <What could go wrong and how the plan reduces it.>

## Plan check

<Findings of a plan review by an agent other than the plan's author, using the six questions in `docs/specs/README.md`, and how each finding was resolved.>

## Deviations

<Unplanned fixes and additions made during implementation. A departure from the spec itself needs a Level 2 spec change.>
