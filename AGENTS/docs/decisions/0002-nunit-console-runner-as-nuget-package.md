# 0002. Run tests with the NUnit.Runners 2.6.4 NuGet package

- Status: accepted
- Date: 2026-11-13

## Context
The test project `src/Test.Net.Sf.Dbdeploy` is written against NUnit 2.2.8, referenced from checked-in dlls in `lib/`. The repository had no console runner, so the tests could only be run from an IDE or through the old NAnt script. Work item 0005 adds a `./psake Test` task and needs a runner that works from the command line.

The test assembly targets .NET Framework 4.8, so the runner has to load it under CLR 4. Upgrading the tests to NUnit 3 or 4 is planned as part of the .NET 11 migration (work item 0003) and was not wanted here.

## Decision
- The runner is the **NUnit.Runners 2.6.4** NuGet package, the last NUnit 2.x console runner. It runs the 2.2.8-built test assembly unchanged.
- It is restored as a package, not vendored (maintainer decision on 2026-11-13; this differs from psake, see ADR 0001). It is listed in `src/Test.Net.Sf.Dbdeploy/packages.config` and restored by `nuget restore src/dbdeploy.net.sln` into `src/packages/`, which is git-ignored.
- The package only supplies tools. The test project has no assembly reference to it and keeps compiling against `lib/nunit.framework.dll` 2.2.8.
- The `Test` task in `psakefile.ps1` finds `nunit-console.exe` under `src\packages\NUnit.Runners.*\tools`, passes `/framework:net-4.5` so the tests run on CLR 4, and writes `build\TestResult.xml`.
- `./psake Test` always runs the entire suite. Partial runs are a separate command, not an option of this task.

## Consequences
- A fresh clone needs `nuget restore src/dbdeploy.net.sln` (network access to nuget.org) before `./psake Test`; the task stops with a message saying so if the runner is missing.
- The runner (2.6.4) and the framework the tests compile against (2.2.8) are different NUnit versions. This works today; if a test starts misbehaving only under the console runner, this mismatch is the first thing to check.
- When 0003 moves the tests to a current NUnit, this package and the `Test` task body are expected to be replaced; supersede this ADR then.
