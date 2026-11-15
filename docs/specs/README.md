# Specs

A spec is the agreed description of one capability: what staff need, how Minioasis behaves, and how that behavior is proven. Agents build only what approved specs describe. The maintainer controls the product mainly by approving specs.

The binding rules are in `AGENTS.md` under "Spec rules", "Implementation rules", and "Verification rules". This file explains how to apply them. Changing this file or `_template/` is a Level 2 change.

## The steps for one capability

| Step | What happens | Who | Output |
| --- | --- | --- | --- |
| 1. Discuss | The agent asks how the capability should behave, challenges vague answers, and makes wishes concrete. | Agent and maintainer | Decisions recorded in `spec.md` |
| 2. Specify | The agent writes numbered requirements, edge cases, and prohibitions. Nothing may stay open. | Agent | `spec.md`, status `draft` |
| 3. Approve | The maintainer reads the spec and merges its pull request. | Maintainer | Status `approved` |
| 4. Plan | The agent maps requirements to files and tests, and marks one-way decisions. | Agent | `plan.md` |
| 5. Check the plan | A different agent tries to find holes in the plan before any code exists. | Second agent | "Plan check" section of `plan.md` |
| 6. Build | Tests first, then code, in small pull requests that agents merge themselves. | Agent | Code, tests, merged pull requests |
| 7. Verify | A different agent checks every requirement against the code and tests. | Second agent | `verification.md`, status `verified`, branch `ready/<NNN>-<name>` |
| 8. Transfer | The maintainer reviews, tests against a real Koha, and merges into production. | Maintainer | Status `transferred` |

The maintainer acts at steps 1, 3, and 8. Steps 4 to 7 need no human unless a Level 2 change comes up.

Small fixes that restore already-specified behavior skip this process; see Level 1 in `AGENTS.md`.

## Folder layout

```text
docs/specs/<NNN>-<short-name>/
├── spec.md           what and why: reviewed and approved by the maintainer
├── plan.md           how: written by agents, checked by a second agent
└── verification.md   proof: requirement → test → result, plus manual acceptance
```

Copy the files from `_template/` to start a new spec.

## Numbering

- Use three digits, assigned in order: `001`, `002`, and so on. A number is never reused.
- The short name is lowercase kebab-case and names the capability, for example `002-staff-auth-design`.
- A rejected spec keeps its folder and number, with status `rejected` and the reason recorded, so the reasoning is not lost.

## Status

```text
draft ──► approved ──► verified ──► transferred
  │
  └──► rejected
```

| Status | Meaning | Who changes it |
| --- | --- | --- |
| `draft` | Being discussed and written. | Agent |
| `approved` | The maintainer accepted the spec. Agents may implement it. | Written as `approved` in the Level 2 spec pull request; it takes effect when the maintainer merges or accepts that pull request |
| `verified` | Every requirement has a passing test in `verification.md`, all checks pass, and the `ready/<NNN>-<name>` branch exists. | Agent, after an independent verification |
| `transferred` | The maintainer merged the spec into production and completed manual acceptance. | Agent, recording what the maintainer reports |
| `rejected` | Not pursued. The reason is recorded in `spec.md`. | Agent, on the maintainer's decision |

The status is the `Status:` line at the top of `spec.md`. `docs/ROADMAP.md` and `docs/STATE.md` mirror it.

## How to discuss (step 1)

- Start open: let the maintainer describe the capability in their own words.
- Ask for a walk-through: "A patron is at the desk with a book. What does the staff member do, and what do they see?"
- Challenge vague words. "Quickly", "simple", and "the usual way" each need a concrete meaning.
- Offer choices where the maintainer may not know the options: "When Koha refuses, should staff see Koha's reason or a general message?"
- Stop when you know what staff do, what they see, what happens when things go wrong, and what is out of scope.

Record each outcome in the spec's "Decisions" section as one of:

- **Locked:** the maintainer decided. Agents never reopen or narrow it without a Level 2 change.
- **Agent's discretion:** the maintainer said "you decide". The agent chooses and records the choice in `plan.md`.
- **Deferred:** a good idea that is not part of this spec. It goes to `docs/ROADMAP.md`.

## Edge cases and prohibitions (step 2)

For every requirement, ask what could go wrong at its edges (empty, zero, very large, repeated, simultaneous, half-finished) and what the system must never do (log a secret, repeat a write, reveal internal details). Each one ends in exactly one state:

| State | Meaning |
| --- | --- |
| Covered | A numbered requirement decides it, so a test will prove it. |
| Dismissed | It cannot happen or does not matter here. The reason is written down. |
| Manual | Only a real Koha can show it. It becomes a scenario in manual acceptance. |

An edge case with no state is an open question, and blocks approval.

## One-way decisions (step 4)

A decision is one-way when undoing it would force a change outside this repository's code. The agent finds these while planning and confirms that the approved spec settles each one. If the spec is silent, the agent stops and asks. Every other decision is two-way: undoing it changes only this repository's code, and the agent decides it.

| Decision | Kind | Who decides | Why |
| --- | --- | --- | --- |
| Names of internal classes, files, and folders | Two-way | Agent | Only the code changes, and tests catch mistakes |
| How code is split between methods or layers | Two-way | Agent | Same |
| Which internal technique is used, such as caching a token | Two-way | Agent | Invisible from outside |
| Wording of log messages | Two-way | Agent | Nothing depends on it |
| A tuning value, such as a timeout of 10 or 15 seconds | Two-way | Agent | One number in one place |
| A route such as `/api/v1/items/{id}` | One-way | Maintainer, in the spec | Every client calling it would break |
| Field names and types in a response | One-way | Maintainer, in the spec | Same |
| Public error codes such as `items.not_found` | One-way | Maintainer, in the spec | Clients decide what to show from them |
| What is stored, and in what shape | One-way | Maintainer, in the spec | Existing data would have to be converted |
| Which external service is used | One-way | Maintainer, in the spec | Accounts and data would have to be moved |
| How staff log in | One-way | Maintainer, in the spec | Every staff member and client is affected |
| What a write to Koha does | One-way | Maintainer, in the spec | Real library records change and cannot be un-run |
| Names of configuration values such as `MINIOASIS_KOHA_BASE_URL` | One-way | Maintainer, in the spec | A deployed server would stop starting |
| The exact words of error messages staff see | One-way | Maintainer, in the spec | Staff get used to them; `AGENTS.md` keeps them fixed |

The table gives examples, not limits. The test applies to every decision: if undoing it would change something outside the code, it is one-way, whether or not it is listed here; otherwise it is two-way.

## Plan check (step 5)

The checking agent reads `spec.md` and `plan.md`, not the author's reasoning, and answers:

1. Does every requirement, edge case, and prohibition have planned work and a planned test?
2. Does the plan deliver each locked decision in full, with no "simplified" or "for now" version?
3. Does anything in the plan go beyond the spec?
4. Does the plan follow the architecture and naming rules in `AGENTS.md`?
5. Are one-way decisions marked, and is each one settled by the spec?
6. Can every planned test actually fail?

Findings go back to the author. The loop stops after three rounds, as `AGENTS.md` requires.

## Verification (step 7)

The verifying agent did not write the code. For each requirement it confirms three things:

1. **Exists:** the behavior is implemented.
2. **Real:** it is not a placeholder, a hard-coded answer, or an empty method.
3. **Wired:** it is registered for dependency injection and reachable from its endpoint.

Then it checks that each test was seen to fail, runs the checks, and fills in `verification.md`. Anything the tests cannot show is marked "needs maintainer", never "pass".

## Worked example

`001-get-item` documents the existing item lookup and is the reference for writing new specs.

## Origin

Several of these practices are adapted from the open-source GSD Core project; see `docs/adr/0002-development-process.md`.
