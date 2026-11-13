---
id: 0016
title: Stop generated SQL Server scripts on a failure in any batch, and in undo scripts
status: backlog
type: bug
created: 2026-11-13
related: [0015]
---

## Goal
A generated SQL Server apply or undo script never runs later change scripts, or later batches of the same change script, after one of its batches fails.

## Acceptance criteria
- [ ] A change script with two `GO`-separated batches whose first batch fails is not marked complete, and the change scripts after it do not run (new test).
- [ ] An undo script stops after a failed undo in the same way (new test).
- [ ] `./psake test` shows no new failures.

## Context
- 0015 added a check after each change script in `src/Net.Sf.Dbdeploy/Resources/mssql_apply.vm`: if the change log row is not at `ScriptStatus = 1`, roll back, `RAISERROR`, `SET NOEXEC ON`. It only sees a failure in the last batch of the change script, because the change log `UPDATE` runs in that batch.
- `src/Net.Sf.Dbdeploy/Resources/mssql_undo.vm` has no stop-on-error logic.
- Options: `SET XACT_ABORT ON` at the start of each change script (changes the semantics of user scripts, and does not cover compile errors); `:on error exit` (sqlcmd and SSMS SQLCMD mode only); splitting the content on `GO` in the applier and adding a check after every batch.

## Plan
- [ ] Choose the approach (ADR if it changes what scripts users can run the output with).
- [ ] Write the failing tests, fix the templates, update `mssql_expected.sql`.

## Notes / log
- 2026-11-13: Created by 0015.

## Outcome
