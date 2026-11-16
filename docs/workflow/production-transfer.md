# Production transfer runbook

This runbook is for the human maintainer. Agents never run it and never access the production repository.

| Repository | Purpose | Who writes |
| --- | --- | --- |
| `GoldBeluga/minioasis-agents` | AI development workspace | Agents (the maintainer pushes only `sync/production`) |
| `GoldBeluga/minioasis` | Production, reviewed code | Maintainer only |

Work moves one way, from agents to production, one verified spec at a time. Both repositories share the same commit history. Keeping that shared history is what makes each transfer show only the new work.

## Rules

1. **Transfer at a `ready/<NNN>-<name>` branch.** Agents create this branch when a spec is verified, and never move it afterwards. Never transfer the agents' `master` directly, because it may contain unfinished work.
2. **Merge transfer pull requests with "Create a merge commit".** Never use squash or rebase. Either one breaks the shared history, and every later transfer would then conflict.
3. **Prefer fixes in the agents repository.** If review finds a problem, ask an agent to fix it. The agent creates a new branch such as `ready/002-staff-auth-design-2`, and you transfer that one. Edit in production only for urgent or trivial fixes, and sync those edits back.

## One-time setup

In `minioasis-agents`, create a label named `needs-human` (**Issues → Labels → New label**). Agents put it on pull requests that wait for you.

Create an empty private repository `GoldBeluga/minioasis` on GitHub, and do not give the Claude GitHub App access to it. Then, on your computer:

```bash
git clone https://github.com/GoldBeluga/minioasis.git
cd minioasis
git remote add agents https://github.com/GoldBeluga/minioasis-agents.git
git remote set-url --push agents DISABLED
```

The push URL `DISABLED` stops you pushing to the agents repository by accident. The sync step below pushes with an explicit URL instead.

### First transfer

The production repository starts empty, so the first transfer creates its `master` from the first verified spec. Review the code on GitHub in the agents repository first, using the checklist below.

```bash
git fetch agents
git switch --no-track -c master agents/ready/001-get-item
git push -u origin master
```

Then apply the branch protection settings below, and tell an agent that spec 001 was transferred.

## Routine transfer

1. Find the verified spec's branch in `docs/STATE.md` of the agents repository, for example `ready/002-staff-auth-design`.

2. Create a review branch from it:

   ```bash
   git switch master
   git pull
   git fetch agents
   git switch --no-track -c review/002-staff-auth-design agents/ready/002-staff-auth-design
   git push -u origin review/002-staff-auth-design
   ```

3. On GitHub, open a pull request in `minioasis` from `review/002-staff-auth-design` into `master`.

4. Review it using the checklist below.

5. Merge with **Create a merge commit**.

6. Tell an agent that spec 002 was transferred, and give it your manual acceptance results. The agent records them in the spec's `verification.md`, marks the spec `transferred`, and updates `docs/STATE.md`. Reporting through an agent keeps your results in the spec without you editing files in production.

### Review checklist

- [ ] **Spec:** the behavior matches the approved `docs/specs/<NNN>/spec.md`.
- [ ] **Verification:** in `verification.md`, every requirement has a passing test, and the Given/When/Then column reads correctly.
- [ ] **Test spot-check:** open one or two test bodies and confirm they assert what their row claims.
- [ ] **Manual acceptance:** walk through the scenarios in the "Manual acceptance" section against a test Koha, and note what you observed. Required for any spec that writes to Koha. For a spec that only reads from Koha you may skip it; tell the agent you skipped it.
- [ ] **Files:** nothing outside what the spec's `plan.md` lists.
- [ ] **Security:** authentication, logging, configuration, `Dockerfile`, `compose.yaml`; no secrets or patron data in logs.
- [ ] **Public contract:** routes and `Minioasis.Api/Resources/errors.en.json`.
- [ ] **Dependencies:** `*.csproj` package references and `dotnet-tools.json`.
- [ ] **Safeguards:** every change to `AGENTS.md`, `docs/platform/`, `docs/specs/README.md`, `docs/specs/_template/`, `docs/workflow/`, `.claude/`, or `.github/` is one you accepted. GitHub cannot enforce this rule, so this check is the backstop.
- [ ] **CI:** passing on the pull request.

Style, naming, and layering are checked by CI and the reviewer agent. You do not need to review them.

## Syncing production edits back

Only needed if you committed directly in production, for example a fix during review.

```bash
git switch master
git pull
git push https://github.com/GoldBeluga/minioasis-agents.git master:sync/production
```

Then ask an agent to merge `sync/production`. The agents' rules require a merge commit for it, never a squash.

## Branch protection

Set these in GitHub under **Settings → Rules → Rulesets**, or under **Branches → Branch protection rules**.

### `minioasis-agents` — `master`

- Require a pull request before merging, with **0** required approvals. Agents merge their own Level 1 work, and GitHub does not let an author approve their own pull request.
- Require status checks to pass: the CI test job.
- Block force pushes and branch deletion.
- Allow both squash merging and merge commits. Merge commits are needed for `sync/production`.
- Do not enable "Require linear history".

### `minioasis-agents` — `ready/*`

- Block force pushes and branch deletion, so a transfer point cannot change after it is created.

### `minioasis` — `master`

GitHub's free plan offers branch protection only on public repositories. If `minioasis` is private on a free plan, either upgrade the plan, make the repository public, or go without protection and always merge through a pull request with a merge commit.

- Require a pull request before merging, with **0** required approvals. You are the only reviewer, and merging is your approval.
- Require status checks to pass: the same CI workflow runs here, because it is transferred with the code.
- Block force pushes and branch deletion.
- Allow merge commits. Disable squash and rebase merging, so a transfer cannot be merged the wrong way by accident.
- Do not enable "Require linear history".

## Troubleshooting

| Symptom | Cause | Fix |
| --- | --- | --- |
| The transfer pull request shows old, already-transferred changes | An earlier transfer was squashed or rebased | Ask an agent to diagnose. Usually the fix is a one-off merge of production `master` into the agents repository via `sync/production`. |
| Merge conflicts in the transfer pull request | Production edits were not synced back | Sync production edits back first, wait for the agents to merge them and create a new `ready/` branch, then transfer that. |
| The `ready/` branch is missing | The spec is not verified yet | Check `docs/STATE.md`. Transfer only verified specs. |
