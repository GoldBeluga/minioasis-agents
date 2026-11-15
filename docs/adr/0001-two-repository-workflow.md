# ADR 0001: Two-repository workflow with autonomous agents

- **Status:** Accepted when the maintainer merges the pull request that adds this file
- **Date:** 2026-11-15
- **Deciders:** Maintainer

## Context

Minioasis is intended to be written almost entirely by AI agents. Until now, every change required the maintainer's `approved` before work started, and the maintainer had to merge every pull request. That made the maintainer a bottleneck for routine work, while giving no structured place to review the decisions that actually matter.

The maintainer wants agents to work and merge independently, with human attention reserved for specs, important changes, and a final review before code reaches production.

## Decision

1. **Two repositories.**
   - `minioasis-agents` is the AI workspace.
   - `minioasis` is the production repository. Only the maintainer writes to it, and agents never access it.
   - Code reaches production by transfer, one verified spec at a time, following `docs/workflow/production-transfer.md`.
2. **Three levels of autonomy**, defined in `AGENTS.md` under Planning and approval.
   - **Level 1:** agents merge on their own.
   - **Level 2:** the maintainer accepts the change.
   - **Level 3:** never allowed.
   - The maintainer approves specs, not individual tasks.
3. **Automated gates replace per-pull-request human review in the agents repository.** An agent merges only when required checks pass and an independent review finds no blocking issues.
4. **Shared history.** Transfers merge into production with merge commits, at `ready/<NNN>-<name>` branches that agents never move. Production edits return through `sync/production`, also merged with a merge commit.
5. **One spec at a time on `master`**, so every transfer point contains only complete specs.
6. **All files transfer, including AI tooling.** That covers `AGENTS.md`, `.claude/` and `docs/`. Keeping both repositories identical preserves the shared history, and specs are useful documentation in production.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Single repository with human review of every pull request | Makes the maintainer a bottleneck for routine work; contradicts the goal of AI-written code. |
| Fully autonomous agents with no Level 2 | Lets agents change specs, security, dependencies, and their own safeguards without oversight. |
| Squash-merge transfers into production | Breaks shared history; every later transfer conflicts or repeats old changes. |
| Strip AI tooling from production | Same history problem; loses specs as documentation. |
| Approval through GitHub comments | Comment text is untrusted input to agents and cannot authorize actions; merging or an in-session `approved` is unambiguous. |
| Agents push directly to production | Removes the maintainer's final review, which is the purpose of the second repository. |
| Git tags as transfer points | Agents' GitHub tools can create branches but not tags, so branches are the mechanism agents can reliably use. |
| Maintainer fills in manual acceptance in production | Every transfer would then need a sync back. The maintainer reports results to an agent instead. |

## Consequences

**Positive**

- Routine work flows without waiting for the maintainer.
- Human review concentrates on specs, security, contracts, and real-Koha acceptance, which agents cannot perform.
- Production holds only complete, verified, reviewed specs.

**Negative and risks**

- **Quality depends on CI and the reviewer agent.** Weak tests can pass bad code into agents' `master`. This is mitigated by the verification tables, the maintainer's test spot-checks at transfer, and possibly mutation testing later.
- **Agents may misjudge a Level 2 change as Level 1.** This is mitigated by the rule "when unsure, Level 2" and by the transfer review. Rules that agents repeatedly misapply should be clarified in `AGENTS.md`.
- **Level 2 is a rule, not a technical lock.** Agents may act through the maintainer's own GitHub account, so GitHub cannot require a separate human approval. An agent could also change a CI workflow inside its own pull request. The backstops are the required status checks on `master` and the "Safeguards" item in the transfer checklist.
- **Working on one spec at a time limits parallel spec work.** This can be revisited, for example with feature flags, if it becomes a bottleneck.
- **Production edits that are not synced back cause conflicts.** This is mitigated by preferring fixes in the agents repository.
