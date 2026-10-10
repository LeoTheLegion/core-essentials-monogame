# Code Coverage

The test project enforces a minimum **80% line coverage** on the core
`CoreEssentials` library. Coverage is collected with [Coverlet] (MSBuild-integrated),
measured on every `dotnet test` run, and the build fails if the threshold is not met.

Coverage is deliberately scoped to the **core assembly only**:

| Assembly | Measured? | Why |
| --- | --- | --- |
| `CoreEssentials` | Yes | The library under test. |
| `CoreEssentials.Tests` | No | Test code; measuring it would inflate the number. |
| `CoreEssentials.Playground` | No | Manual integration harness, not unit-testable without a device. |

## How it works

Coverage configuration lives in [`CoreEssentials.Tests.csproj`](../CoreEssentials.Tests/CoreEssentials.Tests.csproj):

```xml
<CollectCoverage>true</CollectCoverage>
<CoverletOutputFormat>Cobertura</CoverletOutputFormat>
<!-- Canonical [Assembly]Type filter — only the core assembly is instrumented. -->
<Include>[CoreEssentials]*</Include>
<ThresholdType>line</ThresholdType>
<ThresholdStat>total</ThresholdStat>
<ThresholdScope>project</ThresholdScope>
<Threshold>80</Threshold>
```

Key points:

- **`[CoreEssentials]*` is the load-bearing line.** Coverlet module filters require the
  canonical `[Assembly]Type` bracket glob. A bare string such as `CoreEssentials` is
  silently ignored, which would instrument *every* assembly (including the playground and
  test projects) and pollute the number. The bracket form matches only the core assembly.
- **`ThresholdStat=total` + `ThresholdScope=project`** apply the 80% gate to the total of
  all measured (core) lines, not per-file.
- When the gate fails, `dotnet test` exits non-zero *after* still writing the Cobertura
  report, so you can always see what's missing.

## Running it

```powershell
# Runs the tests, enforces the 80% gate, then prints a per-file coverage report.
./scripts/test.ps1
```

The `test.ps1` / `test.sh` scripts always run [`coverage-report.ps1`](../scripts/coverage-report.ps1)
afterwards, so you get actionable detail even when the gate fails. You can also run the
reporter standalone against the most recent report:

```powershell
./scripts/coverage-report.ps1
```

## Interpreting the report

`coverage-report.ps1` parses `CoreEssentials.Tests/coverage/coverage.cobertura.xml` and prints:

- **Overall** — total covered / valid lines and the percentage against the 80% target.
- **Top files by uncovered lines** — ranked list (up to 25) of where the biggest wins are.
  Each row shows `Covered`, `Total`, `Uncovered`, and `Pct` for that file; the numeric
  columns use the short leaf name so they stay visible next to long paths.
- **Files below 80%** — every core file under threshold, sorted ascending by `Pct`.
- **Delta** — how many more lines must be covered to reach 80%.

A file that is genuinely device-bound (e.g. thin `SpriteBatch` forwards) shows low `Pct`
but little real logic. For those, prefer testing the extracted pure logic over forcing
coverage of the GPU hop; see the test project for examples using internal seams and fakes.

[Coverlet]: https://github.com/coverlet-coverage/coverlet
