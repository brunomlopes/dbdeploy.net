---
id: 0015
title: Fix SQL Server test where the second script runs after the first fails (1 test)
status: done
type: bug
created: 2026-11-13
related: [0002, 0007, 0010]
---

## Goal
It is known whether dbdeploy's generated SQL Server script really keeps going after a failed change script, and the test `TestDoesNotRunSecondScriptIfFirstScriptFails` passes for the right reason.

## Acceptance criteria
- [x] The cause is recorded in Notes: a product defect in the generated script, a test that depends on environment or tool version, or an outdated expectation.
- [x] With `./psake start-sqlserver` running, `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestDoesNotRunSecondScriptIfFirstScriptFails` passes in `./psake test`.
- [x] No other test changes result.

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
- [x] Reproduce with `./psake start-sqlserver` and `./psake test`; capture the generated script and the sqlcmd output.
- [x] Identify which of the candidate causes applies.
- [x] Fix it at the right level and re-run the suite.

## Notes / log
- 2026-11-13: Created by 0007.
- 2026-11-13: Cause: a product defect in `mssql_apply.vm`, not the environment or sqlcmd version. The test was committed in 2013 (427dec0, "Failing test for the template applier to skip over changesets if error occurred") as a deliberately failing test and the template was never fixed. The generated script had no stop-on-error logic: script 1's `INSERT` fails with Msg 208, which ends that batch (so its `UPDATE` of the change log is skipped) but leaves the transaction open; the next batch's `COMMIT` commits it, and script 2 then runs and creates the table. Reproduced by hand with sqlcmd against the 2022 container.
- 2026-11-13: Fix chosen in the template, in plain T-SQL, so the generated script stops wherever it is run (sqlcmd, SSMS without SQLCMD mode), rather than `:on error exit` (sqlcmd-only directive) or `-b` in `SqlCmdExecutor` (would only help when dbdeploy runs the script itself). After each change script, a batch checks that the change log row reached `ScriptStatus = 1`; if not, it rolls back the open transaction, raises an error naming the script, and sets `NOEXEC ON` so no later batch executes. The change log row stays at status 3 (started). Updated `Resources/Database/mssql_expected.sql` (used by `ScriptGenerationTest`) to match.
- 2026-11-13: Limitation, not fixed here: only a failure in the last batch of a change script (the batch that holds the change log `UPDATE`) is caught. A failure in an earlier `GO`-separated batch of the same change script still lets the rest run. The undo template `mssql_undo.vm` has no stop-on-error logic either. Both recorded in backlog item 0016.

## Outcome
`mssql_apply.vm` now stops the generated SQL Server script after a failed change script: it rolls back that script's transaction, raises an error, and sets `NOEXEC ON` so later scripts do not run. `TestDoesNotRunSecondScriptIfFirstScriptFails` (SQL Server) passes; `./psake test` with containers up: 92 run, 11 errors (6 MySQL, 0014; 5 Oracle, 0012), 0 failures, before 1 failure. `SqlCmdExecutor` and the test were not changed. Follow-up: 0016 (undo template and multi-batch change scripts).
