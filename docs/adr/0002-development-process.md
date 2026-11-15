# ADR 0002: Development process adapted from GSD Core

- **Status:** Accepted when the maintainer merges the pull request that adds this file
- **Date:** 2026-11-15
- **Deciders:** Maintainer

## Context

The maintainer was inspired by a project that appears to have been built with GSD ("Get Shit Done", now GSD Core), an open-source toolkit that drives AI coding agents through a discuss, plan, execute, and verify loop.

The maintainer wants a process that is sustainable, safe, and reliable rather than fashionable. GSD Core was reviewed at version 1.20.1 (MIT licence) by reading its rulebooks, without installing or running it.

## Decision

Do not install GSD Core. Adopt its strongest ideas as plain rules and templates owned by this repository: `AGENTS.md`, `docs/specs/README.md`, and `docs/specs/_template/`. The ideas are restated in this project's own words; no GSD text or code is included.

### Ideas adopted

| Idea from GSD | Where it lives here |
| --- | --- |
| Discuss before planning; record decisions as locked, agent's discretion, or deferred | Spec rules; "Decisions" in `spec.md` |
| Probe edge cases and prohibitions (must-not rules) before approval | "Edge cases" and "Prohibitions" in `spec.md` |
| Check the plan with a second agent before code exists, including a check for quietly reduced scope | Step 5 in `docs/specs/README.md`; Spec rules |
| Flag one-way decisions for a human; leave reversible ones alone | Implementation rules; "One-way decisions" in `plan.md` |
| Rules for unplanned work: fix bugs and blockers, stop for structural changes | Implementation rules |
| Never substitute a package that fails to install | Implementation rules |
| Map every requirement to an automated test before building | "Tests" in `plan.md`; `verification.md` |
| A test must be able to fail, and say how | "Fails when" in `plan.md`; "Seen failing" in `verification.md` |
| Verify the goal backwards: exists, real, wired | Verification rules; "Wiring" in `verification.md` |
| A verifier abstains instead of guessing, and routes the item to a human | Verification rules; "needs maintainer" result |
| Bounded review loops that escalate | Verification rules |
| Fresh agents for checking, so the author does not grade their own work | Verification rules |
| Hand-off notes so a new session can resume | Implementation rules; `docs/STATE.md` |
| Human acceptance testing recorded in the user's own words | "Manual acceptance" in `verification.md` |

### Ideas not adopted

| Idea | Why not |
| --- | --- |
| Installing the toolkit (Node.js 24, helper programs, hooks, about 60 agent definitions) | A fast-moving third-party dependency that steers every agent action. It releases every one to two weeks and has no stable or long-term-support version. |
| Running without asking ("yolo" mode) | Conflicts with Level 2 in `AGENTS.md`. |
| Numeric ambiguity scores | False precision. A short readiness checklist does the same job. |
| Parallel execution "waves" | This project builds one spec at a time so that each transfer is complete. |
| Machine-readable plan formats | Only useful with GSD's tooling. Plans here are plain Markdown tables. |
| Milestone archives, statistics, and user profiling | Not needed at this size. |

## Consequences

- The process has no dependency to install, upgrade, or lose.
- It works with any AI tool that reads `AGENTS.md`.
- Nothing enforces the steps automatically. They are followed because the rules say so, and checked at transfer.
- GSD Core keeps improving. Its ideas can be reviewed again later and adopted the same way, as Level 2 changes.
