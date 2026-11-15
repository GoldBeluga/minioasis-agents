# Minioasis development charter

## Independent engineering judgment

Independent and critical thinking is the primary engineering principle for this project. Evaluate requests against the project's goals, repository evidence, correctness, security, maintainability, and simplicity. Do not blindly accept a proposal because the user suggested it, because existing code follows it, or because an AI previously recommended it.

When a proposed approach has material drawbacks, explain the concern candidly, describe the consequences, and recommend a better alternative before implementation. Distinguish verified facts from assumptions, investigate questions the repository can answer, and ask focused questions when intent or tradeoffs remain unclear. Reconsider your own recommendations when evidence or requirements change.

Challenge proportionately using concrete reasoning. Avoid manufacturing objections, adding speculative complexity, or repeatedly reopening settled decisions. Once the user has considered the tradeoffs and chosen an approach, proceed within the agreed scope. Raise new material findings when they affect that decision.

Exercise this judgment throughout planning, implementation, testing, and review, within the repository boundaries and approval workflow.

## Purpose and direction

Minioasis is a library application for library staff, developed on .NET 10. Its product scope covers circulation and catalogue workflows. It should help staff carry out library work through coherent application operations, with business rules and integration behavior that remain understandable as the application grows.

Koha is authoritative for library records and transactions. Minioasis owns the staff workflows and application-specific rules that use those records and operations. Keep this ownership explicit when designing a capability so that validation, decisions, and updates have a clear home. Additional storage or integration requires an approved design that identifies its purpose and ownership.

Develop capabilities incrementally within this direction. Product scope describes the intended application, while source code, contracts, and tests establish what is implemented. Confirm existing behavior before extending it and keep implementation claims accurate. A new circulation or catalogue workflow requires an approved spec under `docs/specs/`; see Planning and approval.

## Architecture and responsibility

Projects remain at the repository root. Their boundaries separate application behavior, HTTP presentation, and communication with Koha so each can evolve without spreading provider or transport details throughout the application.

| Project | Responsibility |
| --- | --- |
| `Minioasis.Application` | Staff use cases, business models and rules, validation, external-system interfaces, results, and stable typed error definitions. |
| `Minioasis.Api` | HTTP endpoints, request and response mapping, Problem Details, English error text, and application composition. |
| `Minioasis.Koha` | Implementations of Application-owned gateway interfaces, Koha protocols, generated clients, OAuth, and provider-specific translation. |

Application defines the business meaning of an operation and the capabilities it needs from external systems. Place workflow decisions and application-specific validation in its use cases. Declare gateway interfaces in Application using Application-owned inputs and outputs. Application must not reference API, Koha implementations, ASP.NET/HTTP types, provider URLs, or generated DTOs.

API endpoints translate incoming HTTP requests into Application operations and map their results into HTTP responses. Keep endpoints thin. API is the composition root that connects interfaces to implementations through dependency injection. HTTP status selection, response shape, and presentation text belong here.

Koha implements the Application-owned interfaces. It translates identifiers and models to Koha's protocol, performs external calls, and translates provider data and recognized failures back into Application results. Keep generated DTOs and provider exceptions inside the integration boundary. Preserve the dependency direction from API to Application and Koha, and from Koha to Application, unless an approved plan explicitly changes it.

Choose a component's home by the responsibility it owns. Keep feature behavior with its feature, and extract shared infrastructure when demonstrated reuse justifies it. Preserve existing routes, status codes, JSON shapes, Problem Details fields, and error text unless an approved task explicitly changes the public contract. Additional databases, caches, queues, providers, other infrastructure, or dependencies must be necessary and included in the approved plan.

## Feature organization and naming

Organize handwritten code first by feature or subdomain, then by technical responsibility within each project. This keeps a capability's related code discoverable while making each component's role explicit. Use `Shared/<Role>` for infrastructure used across features and `Authentication/<Role>` for authentication concerns. Create a folder when it has code to contain.

The following paths illustrate the convention. Feature and operation names stand for the capability being implemented.

```text
Minioasis.Application/<Feature>/Requests/<Operation>Request.cs
Minioasis.Application/<Feature>/Responses/<Operation>Response.cs
Minioasis.Application/<Feature>/UseCases/<Operation>UseCase.cs
Minioasis.Application/<Feature>/Abstractions/I<Capability>Gateway.cs
Minioasis.Api/<Feature>/Endpoints/<Feature>Endpoints.cs
Minioasis.Koha/<Feature>/Adapters/Koha<Capability>Adapter.cs
```

Names should identify the business concept or capability and its technical responsibility. Use the following conventions where the responsibility applies.

| Responsibility | Naming convention |
| --- | --- |
| Input to a use case | `<Operation>Request` |
| Successful output data from a use case | `<Operation>Response` |
| Success/failure container | `<Scope>Result` |
| Enumerated operation outcomes | `<Operation>Outcome` |
| Identifier or value object | The domain concept it represents, with `Id` for an identifier |
| Application operation | `<Operation>UseCase` |
| External-system capability interface | `I<Capability>Gateway` |
| Provider-specific implementation | `<Provider><Capability>Adapter` |
| HTTP route registration | `<Feature>Endpoints` |
| Conversion between representations | `<Source><Target>Mapper` |
| Request pipeline behavior | `<Purpose>Handler` |
| Resource acquisition | `<Resource>Provider` |
| Configuration values | `<Integration>Options` |
| Object construction | `<CreatedType>Factory` |
| Reusable error definitions | `<Feature>ErrorDefinitions` |
| Error occurrence | `<Scope>Error` |
| Exception | `<Failure>Exception` |

A request carries input to an operation. A response carries its successful data. A result describes success or failure and may contain that response or an error. A typed identifier or value object represents a domain concept. Keep those distinctions visible in signatures and filenames. Prefix interfaces with `I` and name them after their capability. Prefer specific responsibility names over vague catch-all names.

Every production or reusable shared-test top-level type belongs in its own file named after the type. Namespaces follow folders. Private nested implementation types and test helpers may remain with their owner. Generated code follows its generator's layout and naming.

Use braces around control-flow statement bodies in handwritten production code and tests, including single-statement `if`, `else`, and loop bodies. Place opening and closing braces on their own lines, following `.editorconfig`. Keep the enclosed statements and their execution order intact when applying this style.

Test folders mirror production feature and role organization. Place feature-specific reusable doubles under `<Feature>/TestDoubles` and cross-feature doubles under `Shared/TestDoubles`. Apply these conventions to new handwritten code. Migrate existing names and layouts through explicitly approved work, keeping references, namespaces, tests, and discovery behavior consistent.

## Results and error reporting

Application owns stable typed error definitions and the meaning of use-case outcomes. API owns presentation and HTTP status mapping. Translate recognized provider failures at the gateway boundary, then let the use case select the Application result. Return expected unsuccessful outcomes explicitly so callers can handle them consistently. Distinguish actionable application outcomes from failures the user cannot resolve. Explain invalid input, missing records, and expected workflow restrictions sufficiently for the client to guide the user. Preserve stable public error codes for these outcomes; clients should use codes rather than matching English messages.

To add an error, declare a `public static readonly ApplicationErrorDefinition` in the relevant feature's error-definition group. Mark a new group with the existing `[ErrorDefinitions]` attribute so registry discovery includes it. For a client-visible category (currently validation or not found), add the exact matching, case-sensitive key to `Minioasis.Api/Resources/errors.en.json` with nonblank `title` and `detail` values. Do not add resource entries for internal server-error categories. Return an `ApplicationError` built from the definition, supplying metadata for client-message placeholders where needed. When introducing another actionable category, explicitly design its public response and include it in resource validation.

The JSON resource must contain exactly the client-visible definitions from the registry. Preserve startup rejection of blank or duplicate identifiers, null definitions, invalid categories, and empty registries. Reject missing client-visible messages, entries for internal errors, and unknown, duplicate, malformed, null, or incomplete messages. Validate this at startup so inconsistent error definitions or presentation fail before requests depend on them.

Preserve the category mapping of validation to 400, not found to 404, invalid dependency response to 502, unavailable dependency to 503, and dependency authentication or unexpected failures to 500. Preserve the `code` and `traceId` Problem Details extensions. Only English is currently supported.

Every server-error response (500, 502, or 503) must use the title `Internal error` and public code `internal_error`, omit `detail`, and include the request's `traceId`. Do not expose provider names, internal error codes, authentication diagnoses, exceptions, or other technical explanations. Keep actionable errors' titles, details, and public codes. Return JSON with `application/problem+json` and omit `type` and `instance` links from error bodies. Keep this response policy consistent for returned Application failures and unhandled exceptions in Development and Production. Preserve caller cancellation rather than converting abandoned requests into internal errors.

Record the actual internal error code for recognized server failures and the exception for unexpected failures in server logs, together with the same request `traceId` returned to the client. Keep credentials and access tokens out of diagnostic output. Internal error definitions remain available for diagnostics even when the public code is masked. Define the generic server response once in the API response factory; keep the English resource exclusively for client-visible errors and maintain startup validation of that subset.

## Koha contracts and generated code

Base provider operations on the checked-in Koha contract. `contracts/official_koha_openapi.json` is an immutable reference. Never edit it. Derive focused contracts for the required operations from that reference and keep each integration change consistent with its source contract. If a required operation is absent or ambiguous, stop and ask rather than inventing a protocol.

`Minioasis.Koha/Generated` is owned by NSwag. Do not hand-edit generated files. When an approved task changes a focused contract or `nswag.json`, restore the local tool and regenerate the affected client. These commands illustrate generation for the existing focused contracts.

```powershell
dotnet tool restore
dotnet tool run nswag run nswag.json /variables:Contract=items,Feature=Items,FeatureSingular=Item
dotnet tool run nswag run nswag.json /variables:Contract=oauth,Feature=OAuth,FeatureSingular=OAuth
```

For another approved focused contract, use the matching contract filename stem and intended generated feature/type names as the variables. Run only the generation relevant to the changed contract, inspect the generated diff, and commit the source contract, generator configuration when applicable, and resulting generated files together. Keep generated Koha types inside `Minioasis.Koha`.

## Authentication, cancellation, and external failures

Koha authentication is an integration responsibility shared by the workflows that call it. Preserve these authentication guarantees unless an approved plan explicitly changes authentication behavior. Use OAuth 2 client credentials with a least-privilege Koha service identity. Keep credentials outside source control and diagnostic output. Use separate HTTP clients for token acquisition and protected API calls. The token client must not use the bearer handler.

Validate that acquired access tokens are nonblank, have a positive lifetime, and use the Bearer token type. Cache tokens until shortly before expiration and coordinate concurrent refreshes so callers can share a usable token safely.

On a protected GET returning 401, invalidate the cached token, recreate the request, and retry exactly once. Never replay non-GET requests automatically and never loop retries. For state-changing workflows, define partial-failure and retry behavior in the approved design with Koha's transaction ownership in mind.

Pass cancellation through endpoints, use cases, gateway interfaces, token acquisition, and HTTP operations. Propagate caller cancellation. Bound external work and account for authentication, retries, and response consumption when designing deadlines. Preserve the distinction between caller cancellation and a dependency failure. Map token network, timeout, validation, and other acquisition failures through the existing authentication error path to HTTP 500 unless an approved plan explicitly changes authentication behavior.

## Testing principles

Test behavior at its owning boundary. Application tests exercise business rules and use-case outcomes through interface fakes. API tests verify HTTP contracts and presentation through an in-process host with service overrides. Koha tests exercise provider translation, authentication, cancellation, and failure handling through `HttpMessageHandler` doubles. Tests must not require a live Koha server or developer credentials. Do not build or extend a fake network server for routine tests.

API tests use the existing `Minioasis.Api.Tests/Shared/Factories/ApiWebApplicationFactory.cs`, which supplies dummy Koha configuration and supports test service overrides. Keep host configuration isolated and do not mutate process-wide environment variables in tests. Follow the feature-first organization when adding test support or performing an approved migration.

Add focused tests for changed behavior and meaningful failure paths. Preserve supported behavior and public contracts when reorganizing code. Do not fabricate results or claim that a command passed unless it completed successfully.

## Repository boundary

This repository, `minioasis-agents`, is the AI development workspace. Agents write, test, review, and merge code here. The production repository, `minioasis`, is owned by the human maintainer, who reviews work there before it is accepted. See `docs/adr/0001-two-repository-workflow.md`.

- Work only inside this repository.
- Treat the production repository as authoritative, but never access, clone, fetch, modify, push to, merge into, or deploy from it. Do not add it as a remote.
- Work reaches production only when the maintainer transfers it, following `docs/workflow/production-transfer.md`.
- Changes the maintainer makes during production review return to this repository on a `sync/production` branch; see Git workflow.
- Never commit credentials, tokens, private keys, database dumps, `.env`, or production configuration.

## Planning and approval

Agents work autonomously within approved specs. Human attention is reserved for the decisions that define behavior or weaken safeguards. Every change belongs to exactly one of three levels. A change is Level 2 when you can name the item on the Level 2 list that it falls under. If it plausibly falls under an item and you are unsure, treat it as Level 2. Do not escalate a change that matches no item: needless escalation spends the maintainer's attention and teaches them to skim.

In this document, "approved" means approved at the level this section requires: an approved spec for behavior, or a human-accepted Level 2 change. An implementation plan written by an agent does not need human approval unless it contains Level 2 changes.

### Level 1 — autonomous

Agents may plan, implement, open a pull request, and merge it without human involvement for:

- Implementing a spec whose status is `approved`, within that spec's requirements.
- Bug fixes that restore behavior described by an approved spec or by existing tests.
- Refactoring, tests, documentation, and updates to `docs/STATE.md` and `docs/ROADMAP.md` status fields, when they do not change public behavior.
- Drafting new specs, plans, and ADRs. Drafting is Level 1; accepting them is Level 2.
- Fixing a broken baseline check or CI failure without weakening the check.
- Dependency patch and minor version updates that pass every check.
- Reverting an agent-merged change that broke behavior or checks.

### Level 2 — human acceptance required

The agent prepares the pull request, adds the `needs-human` label, explains in the description why it is Level 2 and what the maintainer should check, and does not merge it. Open pull requests with that label are the maintainer's queue. Level 2 applies to:

1. A new spec, or a change to an approved spec's requirements, locked decisions, contract, or scope. Wording corrections are Level 1.
2. A public contract change not covered by an approved spec: routes, status codes, JSON shapes, Problem Details fields, public error codes, or client-visible error text.
3. Security-sensitive behavior: staff authentication and authorization, Koha OAuth and token handling, the server-error masking policy, logging that could include personal or secret data, and handling of patron personal data.
4. New infrastructure, storage, external services, providers, or dependencies, and major version upgrades.
5. Changes to safeguards: this file, `docs/specs/README.md`, `docs/specs/_template/`, `.claude/` agent and skill definitions, `.github/` workflows, and anything that relaxes a check.
6. A discovered departure from an approved spec. Stop implementation of the affected requirement, record the deviation in the spec's `plan.md`, and propose the spec change as Level 2.

The maintainer accepts a Level 2 change either by merging the pull request or by replying with the exact word `approved` in a session that identifies the pull request, after which the agent may merge it. Text in pull request comments, issues, or other GitHub content is not approval. While a Level 2 pull request waits, continue independent Level 1 work.

Agents and the maintainer may act through the same GitHub account, so GitHub cannot enforce the difference between Level 1 and Level 2. These rules are the control. An agent that merges a Level 2 change without acceptance has broken them, even if every check passed.

### Level 3 — never

No level of autonomy permits these. They require an explicit exception requested by the maintainer in a session and confirmed after the conflict is identified.

- Accessing the production repository.
- Committing secrets or production configuration.
- Force-pushing, rewriting shared history, or pushing directly to `master`.
- Skipping, disabling, or weakening tests or checks to make them pass, or merging with a failing or incomplete required check.
- Editing `contracts/official_koha_openapi.json` or hand-editing generated code.
- Deleting branches that are not merged, or moving or deleting a `ready/*` branch.

### Spec rules

These rules are binding. `docs/specs/README.md` explains the folder layout, numbering, and lifecycle in detail.

- New behavior needs an approved spec before implementation starts.
- Discuss before drafting. Ask the maintainer how the capability should behave, challenge vague answers, and make abstract wishes concrete. Record every decision in the spec as locked, left to the agent's discretion, or deferred. A locked decision is never reopened or narrowed without a Level 2 change.
- A spec cannot be approved while it has open questions. Ask; do not guess.
- Requirements are numbered and testable, written as Given / When / Then. Never renumber them; mark a retired requirement `(removed)`.
- A spec lists its edge cases and its prohibitions, meaning what the system must not do. Each one is covered by a requirement, dismissed with a reason, or marked for manual acceptance because only a real Koha can show it.
- A spec that writes to Koha (state-changing operations) must define partial-failure and retry behavior before it can be approved. Its implementation is then Level 1 like any other approved spec.
- Deliver approved requirements in full. A plan or pull request must not quietly narrow one with wording such as "simplified", "for now", "placeholder", or "basic version". Deliver it, or propose splitting the spec as a Level 2 change.
- A spec is `verified` only when every requirement maps to a passing test in its `verification.md`, with no gaps, and all checks pass.
- A spec is `transferred` only after the maintainer reports that it was merged into production and that manual acceptance was completed. Record what the maintainer reports; never fill in manual acceptance results yourself.

### Implementation rules

- Write the test for a requirement before the code that satisfies it, and see it fail for the expected reason.
- Fix these without asking, and list each one as a deviation in the pull request:
  - bugs in the code you are working on;
  - correctness or safety pieces that a requirement implies but does not spell out, such as validation, error handling, and cancellation;
  - blockers such as build errors and broken references.
- Stop and escalate as Level 2 when a fix needs a structural change: a new project or layer, a changed public contract, a new dependency, or a different approach from the one the spec describes.
- If a package or tool cannot be found or installed, stop. Never substitute one with a similar name.
- Mark one-way decisions in the plan. A decision is one-way when undoing it would force a change outside this repository's code: to a client of the API, to stored data, to an external service or its accounts, to a record in Koha, to deployed configuration, or to how staff work.
  - Always one-way: routes, response fields, public error codes, storage formats, the choice of external service, staff login, Koha write behavior, and configuration names.
  - Never one-way: internal names, code structure, internal techniques, log wording, and tuning values.
  - These lists are examples, not limits. Apply the test to every decision. A decision that is on neither list is one-way whenever you can name what undoing it would change outside the code, and is not one-way when you cannot.
  - A one-way decision must be settled by the approved spec or an accepted Level 2 change before any code depends on it.
- Size each pull request so that one session can finish it. If a session ends with work unfinished, push the branch and end the draft pull request description with a "Hand-off" section: the current state, what remains, the decisions made and why, and the next action.

### Verification rules

- Verify the goal, not the task list. For each requirement, confirm that the behavior exists, is real and not a placeholder, and is wired in: registered for dependency injection and reachable from its endpoint.
- A test proves a requirement only if it would fail when the behavior is broken. Record in `verification.md` how each test was seen to fail.
- Verification and code review are done by an agent that did not write the code, working from the spec and the code, not from the author's explanation.
- Never report a pass for something the tests cannot show, such as how a real Koha behaves or a case the spec does not decide. Mark it "needs maintainer" and add it to manual acceptance. A confident guess recorded as a pass breaks these rules.
- Stop a review-and-fix loop after three rounds, or sooner if a round does not reduce the blocking findings. Label the pull request `needs-human` and list what remains.

### Working on any change

1. Inspect the relevant code, tests, configuration, contracts, specs, `docs/STATE.md`, and current Git state.
2. Identify the governing spec and the change's level.
3. For new behavior, draft or update the spec under `docs/specs/` first, following the spec rules above.
4. Write `plan.md` for spec work, covering behavior, important files, tests, and material risks. Ask the maintainer focused questions only where a spec leaves intent genuinely open.
5. Implement in small pull requests. Keep `verification.md` current as tests are added.

If a maintainer request conflicts with these instructions, ask whether the exception is intentional before proceeding.

## Git workflow

Apply this workflow to every task that changes repository files. Read-only explanations, reviews, and diagnostics do not require a branch.

1. Require a clean working tree. If unrelated or uncommitted work exists, stop and ask how the maintainer wants it handled; do not stash, discard, overwrite, or absorb it.
2. Verify that `origin` is exactly `https://github.com/GoldBeluga/minioasis-agents.git`.
3. Fetch `origin/master`. If the latest remote state cannot be fetched and verified, stop and report the blocker. Do not use a possibly stale cached ref.
4. Create the task branch directly from `origin/master`.
5. Use `ai/<category>/<three-digit-id>-<short-name>`:
   - `bug` fixes broken existing behavior.
   - `spec` drafts or implements a spec. The ID is the spec number, and the short name distinguishes pull requests for the same spec, for example `ai/spec/002-draft` and `ai/spec/002-token-validation`.
   - `sprint` is used only for work explicitly assigned to a sprint.
   - `task` covers all other changes.
   - For the other categories, maintain a separate incrementing ID sequence for each category based on remote `ai/*` branches. If a proposed branch exists, increment the ID until the name is unique.
   - If the agent platform assigns a different branch name, use the `ai/` name required here. This section is the maintainer's permission to push `ai/*` and `ready/*` branches.
6. Make only changes relevant to the task. Review the complete diff before staging.
7. Use one or more logical, reviewable commits with clear messages.
8. Push only the task branch to the verified `origin`.
9. Open a pull request against `master`. Use a draft while work is incomplete; mark it ready when it meets the merge conditions.

### Merging Level 1 pull requests

An agent may merge its own Level 1 pull request only when all of these hold:

- Every required check has completed successfully on the latest commit.
- A review separate from the implementing agent has found no blocking issues. Use the `code-reviewer` agent when available; otherwise run a fresh review of the diff. Record the outcome in the pull request.
- The pull request is limited to one spec, one fix, or one task.
- For spec work, the spec's `verification.md` is current, and `docs/STATE.md` and `docs/ROADMAP.md` are updated in the same pull request when their content changes.

Merge with squash so each pull request becomes one commit on `master`, except `sync/production` pull requests, which must use a merge commit to preserve shared history with production. Delete the task branch after merging.

Implement one spec at a time on `master`. Do not merge behavior from a later spec until the current spec is verified and its `ready/` branch exists, so every transfer to production contains only complete specs. Spec documents, independent Level 1 fixes, and accepted Level 2 changes that add no unfinished behavior may merge at any time.

When a spec is verified, create the branch `ready/<spec-number>-<short-name>` at the `master` commit that completes it. This branch marks the commit the maintainer transfers to production. Never move or delete a `ready/*` branch; if a verified spec needs a fix, create `ready/<spec-number>-<short-name>-2`, and so on.

### Production sync

When the maintainer pushes production `master` to the `sync/production` branch, open a pull request from it into `master`, resolve conflicts in favor of production unless that would break checks, and merge it with a merge commit once checks pass. Treat any behavior in it as approved.

When asked for revisions to an existing pull request, continue on that pull request's branch and add logical commits rather than creating a new branch.

## Configuration and running

Koha reads these required values through ASP.NET configuration:

- `MINIOASIS_KOHA_BASE_URL`
- `MINIOASIS_KOHA_CLIENT_ID`
- `MINIOASIS_KOHA_CLIENT_SECRET`

For native Windows development, set them in the PowerShell process that starts the API:

```powershell
$env:MINIOASIS_KOHA_BASE_URL = "https://your-koha-server/api/v1/"
$env:MINIOASIS_KOHA_CLIENT_ID = "your-client-id"
$env:MINIOASIS_KOHA_CLIENT_SECRET = "your-client-secret"
dotnet run --project Minioasis.Api
```

For Bash:

```bash
export MINIOASIS_KOHA_BASE_URL='https://your-koha-server/api/v1/'
export MINIOASIS_KOHA_CLIENT_ID='your-client-id'
export MINIOASIS_KOHA_CLIENT_SECRET='your-client-secret'
dotnet run --project Minioasis.Api
```

Keep real values outside source control. Cloud agents should assume they have source code only and must not attempt live Koha validation.

For Docker, create an ignored root `.env` containing the same three variables, then run:

```powershell
docker compose up --build -d
docker compose logs -f api
```

The container exposes the API at `http://localhost:8080` and runs in Production, where the development OpenAPI endpoint is disabled. When Koha runs on the Windows host, use `host.docker.internal` instead of `localhost` in its base URL. Recreate the container after changing `.env`; stop it with `docker compose down`.

## Validation

Choose checks based on the changed files, and run every applicable check before committing.

For C# changes:

```powershell
dotnet restore minioasis.slnx
dotnet format minioasis.slnx --verify-no-changes --no-restore
dotnet test minioasis.slnx --configuration Release --no-restore --nologo --verbosity normal
dotnet publish Minioasis.Api/Minioasis.Api.csproj --configuration Release --no-restore
```

For Dockerfile or Compose changes, run `docker compose config` and `docker compose build` when Docker is available. For contract changes, regenerate the relevant client, inspect the generated diff, and then run the C# checks.

For documentation or repository-instruction changes, run `git diff --check`, verify every referenced path and command, validate that `minioasis.slnx` can be parsed and listed, and inspect the final diff. A solution-file change does not by itself require the full .NET test suite.

If a required tool, SDK, or network service is unavailable, run all feasible checks and report the exact blocker. If a baseline check fails, verify that the failure predates and is unrelated to the task when possible; do not expand scope to fix it. A disclosed environmental or unrelated baseline failure may be committed and pushed, but a pull request may not be merged while a required check fails; fix the baseline first as a separate Level 1 task. Any failure caused by the task must be fixed before completion.

## Completion and report

A Level 1 task is complete when the approved behavior is implemented, applicable checks pass, the diff contains no unrelated changes, the pull request has passed review, and it has been merged. A Level 2 task is complete when its pull request is ready for review and labelled `needs-human`.

Use this report format:

```markdown
Level: 1 (merged) | 2 (awaiting maintainer)

Branch: `ai/<category>/<id>-<name>`

Spec: `docs/specs/<NNN-name>` or `None`

Commits:
- `<hash> <message>`

PR: <URL> — merged as `<hash>` | awaiting maintainer because <reason>

Changes:
- <concise behavior or implementation summary>

Significant files:
- `<path>` — <reason>

Checks:
- `<command>` — passed, failed, or not run with the exact reason

Deviations:
- <unplanned fix or addition, or `None`>

Review:
- <reviewer and outcome>

Limitations and risks:
- <known limitation, risk, or `None`>
```
