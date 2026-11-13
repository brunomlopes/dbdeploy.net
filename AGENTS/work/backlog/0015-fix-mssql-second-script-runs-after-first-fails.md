---
id: 0015
title: Fix SQL Server test where the second script runs after the first fails (1 test)
status: backlog
type: bug
created: 2026-11-13
related: [0002, 0007, 0010]
---

## Goal
It is known whether dbdeploy's generated SQL Server script really keeps going after a failed change script, and the test `TestDoesNotRunSecondScriptIfFirstScriptFails` passes for the right reason.

## Acceptance criteria
- [ ] The cause is recorded in Notes: a product defect in the generated script, a test that depends on environment or tool version, or an outdated expectation.
- [ ] With `./psake start-sqlserver` running, `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestDoesNotRunSecondScriptIfFirstScriptFails` passes in `./psake test`.
- [ ] No other test changes result.

## Context
- Found by 0007 once the tests could reach SQL Server (SQL Server 2022 CU28 container). The other eight SQL Server tests, including both `SqlCmdApplierTest` tests, pass.
- Failure: assertion `TableWeWillUse table was created.` at `AbstractDatabaseSchemaVersionManagerTest.AssertTableDoesNotExist` (line 110), called from `MsSqlDatabaseSchemaVersionManagerTest.cs` line 103.
- What the test does (`src/Test.Net.Sf.Dbdeploy/Database/MsSqlDatabaseSchemaVersionManagerTest.cs`, around lines 72 to 104): it generates one apply script with `TemplateBasedApplier` (template `Resources/mssql_apply.vm`) for two change scripts. Script 1 is `INSERT INTO TableWeWillUse VALUES (1);`, which must fail because the table does not exist. Script 2 is `CREATE TABLE dbo.TableWeWillUse (Id int NULL);`. It runs the generated text through `SqlCmdExecutor` (sqlcmd) and expects the table not to exist afterwards. The table exists, so script 2 ran after script 1 failed.
- Not investigated. Candidate causes:
  - The generated script does not stop on error (no `:on error exit`, or error handling in `mssql_apply.vm` that does not cover a compile-time error such as a missing table).
  - `SqlCmdExecutor` does not pass the option that makes sqlcmd abort (`-b`), or the sqlcmd it launches behaves differently from the one the test was written against. Check which sqlcmd is used: a copy is embedded in `src/Net.Sf.Dbdeploy/Resources/SQLCMD.EXE`, and the machine has a newer one on the path.
  - Behaviour difference on SQL Server 2022 compared with the version the test was written for.
- If it is a product defect, it matters beyond the test: a failed deployment would continue applying later scripts.
- This item authorises changes to the template, `SqlCmdExecutor` and the test as needed to fix this one behaviour.

## Plan
- [ ] Reproduce with `./psake start-sqlserver` and `./psake test`; capture the generated script and the sqlcmd output.
- [ ] Identify which of the candidate causes applies.
- [ ] Fix it at the right level and re-run the suite.

## Notes / log
- 2026-11-13: Created by 0007.

## Outcome
