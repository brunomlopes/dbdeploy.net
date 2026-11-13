---
id: 0005
title: Add a psake task to run the tests from the command line
status: done
type: feature
created: 2026-11-13
related: [0002, 0003, 0004, 0006]
---

## Goal
One command, `./psake Test`, builds the solution and runs the entire `Test.Net.Sf.Dbdeploy` suite from the command line, printing a pass/fail summary and failing the build when a test fails.

## Acceptance criteria
- [x] `psakefile.ps1` has a `Test` task that depends on `Build` and has a description, so it is listed by `./psake Help`.
- [x] The test runner is a NuGet package listed in a `packages.config` that `nuget restore src/dbdeploy.net.sln` restores into `src/packages/`. No runner binaries are checked into the repo.
- [x] `./psake Test` on a fresh clone (after `nuget restore src/dbdeploy.net.sln`) runs the whole suite with no manual steps and nothing installed machine-wide beyond what `./psake build` already needs.
- [x] The console shows the totals (run / passed / failed / errors / ignored) and the name and message of each failing test.
- [x] A result file is written under an ignored folder (e.g. `build/`), not into tracked paths.
- [x] The psake run is reported as failed when at least one test fails or errors, and as succeeded when all pass (`./psake Test` returns a non-zero exit code; see 0006).
- [x] The `Test` task always runs the entire suite. It takes no filter, category or fixture option.
- [x] AGENTS.md "Commands" lists the test command.
- [x] An ADR records the runner package, its version and why that version (it is a new dependency).

## Context
- Build file: `psakefile.ps1` (psake 5.0.4 vendored in `Tools/psake/`, wrapper `psake.ps1` / `psake.cmd`, see ADR 0001). Existing tasks: `Build`, `Clean`, `BuildPackage`, `PublishPackage`, `Help`. There is no test task.
- Test project: `src/Test.Net.Sf.Dbdeploy`, output `src/Test.Net.Sf.Dbdeploy/bin/Debug/Test.Net.Sf.Dbdeploy.dll`, targets .NET Framework 4.8 since 0001.
- Test framework: NUnit **2.2.8** (`lib/nunit.framework.dll`, `lib/nunit.core.dll`, `lib/nunit.util.dll` are checked in). No console runner is checked in for it.
- How tests used to run: the NAnt script `dbdeploy.build` has `test` and `test-all` targets using the `<nunit2>` task with `lib/nunit.framework.dll`, an XML formatter, and an app config generated from `config/App.config.template`. Use them only to see which config the tests were run with; the new task runs everything, like `test-all`.
- `example_scripts/Tools/nant/bin/lib/common/{1.1,2.0}/nunit-console.exe` exist, but they belong to the bundled NAnt, target old CLRs and are not known to match NUnit 2.2.8 or to load a net48 assembly. Do not assume they work.
- Decided by the maintainer on 2026-11-13: the runner is restored as a NuGet package, not vendored (unlike psake, ADR 0001). This item is the authorisation to add that package.
- Which package and version is the main thing to find out. Start with a NUnit 2.x console runner package that can load the 2.2.8-built test assembly under CLR 4 (for example the last 2.6.x `NUnit.Runners`). The task should locate the runner under `src/packages/` without hard-coding more of the path than necessary.
- If no runner package can load the assembly as it is, stop and report: upgrading the test project's NUnit reference is a dependency and test-code change that overlaps with 0003 (move to NUnit 3/4) and is not authorised by this item.
- Decided by the maintainer on 2026-11-13: `./psake Test` runs the entire suite. Running part of the suite would be a separate command and is out of scope here; create a new work item if it is wanted.
- Many tests need live databases (connection strings in `src/Test.Net.Sf.Dbdeploy/App.config`: SQL Server, MySQL, Oracle). The task must still run and report when those are unreachable; making them pass is not part of this item.
- Exit code: since 0006, `.\psake.cmd <task>` returns 1 to the calling process when psake fails and 0 when it succeeds, so a failing test run can be checked through the exit code.
- Relationship to 0002 (run and triage the tests): 0002 needs a repeatable way to run the suite. Do this item first and have 0002 use `./psake Test`.
- 0003 will change the test stack; the `Test` task is expected to be updated then (e.g. to `dotnet test`).

## Plan
- [x] Read the `test` / `test-all` targets in `dbdeploy.build` and note what they ran and with which config.
- [x] Find a console runner that loads `Test.Net.Sf.Dbdeploy.dll` (NUnit 2.2.8, net48); try it by hand outside the repo first and record the exact command and result in Notes.
- [x] Add the runner package to a `packages.config` covered by `nuget restore src/dbdeploy.net.sln`, confirm a fresh restore brings it in, and write the ADR.
- [x] Add the `Test` task to `psakefile.ps1` (keep the file's CR-only line endings): depends on `Build`, runs the runner through `Exec`, writes the result file to an ignored folder.
- [x] Verify: all-pass or expected-failure run prints totals; a deliberately failing run makes psake report failure; `./psake Help` lists the task.
- [x] Update AGENTS.md "Commands" and the Context of 0002.

## Notes / log
- 2026-11-13: Item created. No runner was tried and no tests were run.
- 2026-11-13: Maintainer decisions: restore the runner as a NuGet package; the task runs the entire suite, with partial runs left to a separate command (not planned here); the `psake.cmd` exit code is fixed under 0006.
- 2026-11-13: Old NAnt targets (`dbdeploy.build`): `test` excluded the NUnit category `DbIntegration`; `test-all` ran everything. Both used an app config generated from `config/App.config.template`. The new task runs everything with the project's own `App.config` (copied to `bin\Debug\Test.Net.Sf.Dbdeploy.dll.config` by the build).
- 2026-11-13: Runner tried by hand outside the repo first: `nunit-console.exe` from the NuGet package NUnit.Runners 2.6.4, run as `nunit-console.exe src\Test.Net.Sf.Dbdeploy\bin\Debug\Test.Net.Sf.Dbdeploy.dll /nologo /xml:<file>`. It loaded the NUnit 2.2.8-built net48 assembly and ran 92 tests. Without `/framework` it reported "Execution Runtime: net-3.5", so the task passes `/framework:net-4.5`, after which it reports net-4.5.
- 2026-11-13: Added `NUnit.Runners` 2.6.4 to `src/Test.Net.Sf.Dbdeploy/packages.config` (tools-only package, no assembly reference added). `nuget restore src/dbdeploy.net.sln` installed it into `src/packages/NUnit.Runners.2.6.4`. ADR 0002 written.
- 2026-11-13: Added the `Test` task to `psakefile.ps1` (CR-only line endings kept). It depends on `Build`, locates the runner with a wildcard on the package version, and writes `build\TestResult.xml` (`build*/` is git-ignored).
- 2026-11-13: `./psake Test` on this machine: build 0 errors, then "Tests run: 92, Errors: 20, Failures: 0", about 1 min 47 s. psake reported failure and `psake.cmd` returned exit code 1. The 20 errors are all SetUp errors in the database fixtures: `SqlCmdApplierTest` (2), `MsSqlDatabaseSchemaVersionManagerTest` (7), `MySqlDatabaseSchemaVersionManagerTest` (6), `OracleDatabaseSchemaVersionManagerTest` (5). Triage is item 0002.
- 2026-11-13: Not verified: the all-tests-pass path. No run on this machine passes, so the "succeeds and returns 0 when all pass" criterion rests on `Exec` treating the runner's exit code 0 as success, not on an observed green run.
- 2026-11-13: Not verified on a separate fresh clone; the restore was run in this working tree, where the package was not present before.
- 2026-11-13: `./psake Help` lists `Test` with its description. AGENTS.md "Commands" now lists the test command.

## Outcome
`./psake Test` builds the solution and runs the entire suite with the NUnit.Runners 2.6.4 console runner, restored as a NuGet package. It prints the totals and each failing test, writes `build\TestResult.xml`, and fails the psake run (exit code 1) when a test fails or errors.

Current result on the maintainer's machine: 92 run, 20 errors, 0 failures; all errors are database fixture SetUp errors, left for 0002.

Deferred: a command to run part of the suite (out of scope by decision; no work item created). The green-run path is unverified until the database tests can pass.
