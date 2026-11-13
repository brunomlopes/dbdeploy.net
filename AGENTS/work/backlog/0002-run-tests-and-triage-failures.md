---
id: 0002
title: Run the test suite, list failing tests and create fix work items
status: backlog
type: chore
created: 2026-11-13
related: [0001, 0003]
---

## Goal
Know exactly which tests fail on the .NET Framework 4.8 build and why, with one backlog work item per group of failures that share a solution. This item triages only; it fixes nothing.

## Acceptance criteria
- [ ] The full test run of `Test.Net.Sf.Dbdeploy` has been executed and the exact command used is recorded in Notes.
- [ ] Notes contain the totals (passed / failed / errored / ignored) and a list of every failing test by fully qualified name with its failure message.
- [ ] Every failing test is assigned to exactly one group, where a group is a set of tests that one fix would resolve.
- [ ] One backlog work item exists per group, listing its tests, the suspected cause and the proposed solution; their ids are listed in Outcome and in `related`.
- [ ] No production or test code was changed under this item.

## Context
- Depends on 0001 (run against the net48 build). If 0001 is not done, run against whatever builds and say so in Notes.
- Test project: `src/Test.Net.Sf.Dbdeploy` (NUnit 2.x, referenced from checked-in `lib/nunit.*.dll`; no runner is checked in, so one has to be obtained, e.g. the NUnit 2 console runner or an adapter).
- There is no test task in `psakefile.ps1`; `dbdeploy.build` (NAnt) and `go.bat` are the older build entry points and may show how tests used to be run.
- Many tests need live databases, configured in `src/Test.Net.Sf.Dbdeploy/App.config`:
  - SQL Server: `Server=.;Initial Catalog=DBDEPLOY` with SQL login `DBDeployUser`
  - MySQL: `192.168.1.112`, database `DBDEPLOY`
  - Oracle: data source `ORACLE`
  - `db/` and `scripts/` hold the schema scripts for the change-log table.
- Likely groups (to be confirmed by the run, not assumed): database not reachable per engine (environment, not code), MySQL compatibility (last commit is "Trying to get mysql compatibility"), `SqlCmdApplierTest` needing `sqlcmd` on the path, path/culture/line-ending assumptions.

## Plan
- [ ] Build the solution (`./psake build`) and locate the test assembly.
- [ ] Pick a way to run the NUnit 2 tests and record it; if this needs a new tool or package in the repo, note it rather than committing it.
- [ ] Run the whole suite once with no databases configured; save the result file outside the repo and copy the summary into Notes.
- [ ] Separate environment failures (no SQL Server / MySQL / Oracle available) from code failures. Ask the human which engines are available locally before treating a database failure as a code bug.
- [ ] Group the failures by shared solution.
- [ ] Create one backlog work item per group using the template; number them with the next unused ids.
- [ ] Add a test-run command to AGENTS.md "Commands" if a repeatable one now exists.

## Notes / log
- 2026-11-13: Item created. Tests have not been run yet.

## Outcome
