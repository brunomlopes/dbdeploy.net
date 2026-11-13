---
id: 0012
title: Decide how the Oracle tests are run (5 tests)
status: backlog
type: chore
created: 2026-11-13
related: [0002, 0003, 0005, 0007, 0010, 0011]
---

## Goal
The five Oracle tests either run against a real Oracle database or are deliberately kept out of the default result by a recorded decision, so they stop showing up as unexplained errors.

## Acceptance criteria
- [ ] An ADR records the decision: run them (and how), or exclude them from `./psake Test` until the Oracle provider is replaced (0003).
- [ ] If run: `./psake Test` shows no SetUp error for the five tests and the setup steps are documented.
- [ ] If excluded: the exclusion is visible in the test output (ignored or not run, with a reason), not a silent removal, and 0003 lists re-enabling them.
- [ ] Any test that then fails for a code reason has its own new backlog item.

## Context
- Tests (all fail in SetUp with `System.Exception: System.Data.OracleClient requires Oracle client software version 8.1.7 or greater.`), all in `Net.Sf.Dbdeploy.Database.OracleDatabaseSchemaVersionManagerTest`:
  - `ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted`
  - `TestCanRetrieveSchemaVersionFromDatabase`
  - `TestReturnsNoAppliedChangesWhenDatabaseTableDoesNotExist`
  - `TestShouldCreateChangeLogTableWhenToldToDoSo`
  - `TestShouldReturnEmptySetWhenTableHasNoRows`
- Cause: environment, with a code angle. The failure happens before any network access: `System.Data.OracleClient` (the provider built into .NET Framework, marked obsolete; the build warns CS0618) needs the native Oracle client installed on the machine, and it is not. Starting a database alone would not make these tests connect.
- `App.config` has `OracleConnString` with `data source=ORACLE`, a TNS alias, so a `tnsnames.ora` entry would also be needed. The fixture uses `OracleConnString-<MACHINENAME>` if present.
- This differs from 0010 and 0011, where starting a database is enough. The realistic choices are:
  1. Install the Oracle client machine-wide and provide an Oracle database (for example an Oracle container image).
  2. Replace `System.Data.OracleClient` with Oracle's managed provider, which needs no native client. That is a dependency and production-code change and is already in the scope of 0003.
  3. Exclude the Oracle fixture from the default run until option 2 is done.
- `./psake Test` is defined as "the entire suite" (0005, ADR 0002); option 3 has to respect or amend that decision.
- Which code paths in `Net.Sf.Dbdeploy` use the Oracle provider has not been reviewed.

## Plan
- [ ] Ask the maintainer to choose between the three options.
- [ ] Write the ADR.
- [ ] Carry out the chosen option, or hand it to 0003 and update that item's inventory.
- [ ] Run `./psake Test`; record the five results in Notes.

## Notes / log
- 2026-11-13: Created by 0002 from the first full run (92 run, 20 errors).
- 2026-11-13: Renumbered from 0009 to make room for 0007. Maintainer decision the same day: Oracle is out of scope for now because it needs native Oracle clients. The Docker Compose work (0007) does not include Oracle. Leave this item in the backlog until the maintainer picks it up; the five Oracle tests keep failing in SetUp meanwhile.

## Outcome
