# <NNN> — <Capability name>: verification

- **Spec:** [spec.md](spec.md)
- **Verified at commit:** `<sha>`
- **Transfer branch:** `ready/<NNN>-<name>`, or "not yet"

## Requirement coverage

Each row maps one requirement to a test that proves it.

- **Proves** is a plain-English summary of what the test asserts, so the maintainer can review behavior without reading C#.
- **Seen failing** records how the test was observed to fail: written before the code, or the behavior was broken on purpose and restored.
- **Result** is one of: ✅ pass, ❌ fail, ⚠️ gap (no test), or 🧑 needs maintainer (the tests cannot show it).

The spec cannot be `verified` while any row is a fail or a gap. "Needs maintainer" rows must appear in manual acceptance.

| Req | Proves (Given / When / Then) | Test (`Project::Class::Method`) | Seen failing | Result |
| --- | --- | --- | --- | --- |
| R1 | | | | |

## Edge cases and prohibitions

| Item from the spec | State | Proven by |
| --- | --- | --- |
| | covered / dismissed / manual | <test, reason, or manual scenario number> |

## Wiring

<For each new capability: where it is registered for dependency injection, and which endpoint reaches it. Confirms the code is connected, not merely present.>

## Checks

| Command | Result |
| --- | --- |
| `dotnet format minioasis.slnx --verify-no-changes --no-restore` | |
| `dotnet test minioasis.slnx --configuration Release --no-restore` | <passed / failed, counts> |
| `dotnet publish Minioasis.Api/Minioasis.Api.csproj --configuration Release --no-restore` | |

## Independent verification

<Findings of the agent that verified this spec without having written the code: what it confirmed, weak or misleading tests it found, placeholders or unwired code it found, and how each was resolved.>

## Manual acceptance (maintainer)

Agents cannot reach a real Koha. Agents write the scenarios and expected results. The maintainer runs them against a test Koha and reports what happened, and an agent records the report here word for word. Agents never fill in "Observed" or "Result" themselves. Required for write specs before the spec is `transferred`.

- **Koha instance and version:**
- **Date:**

| # | Scenario | Expected | Observed (maintainer's words) | Result |
| --- | --- | --- | --- | --- |
| 1 | <happy path> | | | pass / issue / skipped |
| 2 | <one failure case> | | | |

An "issue" result becomes a fix in the agents repository, followed by a new `ready/` branch.
