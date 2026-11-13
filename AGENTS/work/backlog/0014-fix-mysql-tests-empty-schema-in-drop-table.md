---
id: 0014
title: Fix the MySQL tests failing with a syntax error near '.ChangeLog' (6 tests)
status: backlog
type: bug
created: 2026-11-13
related: [0002, 0007, 0011]
---

## Goal
The six MySQL schema-version tests run their bodies against the MySQL test container instead of failing while dropping a table, and whatever they then show about MySQL support is known.

## Acceptance criteria
- [ ] With `./psake start-mysql` running, `./psake test` reports no `You have an error in your SQL syntax ... near '.ChangeLog'` or `near '.TableWeWillUse'` error.
- [ ] The six tests listed below pass, or each one that still fails for a different reason has its own backlog item.
- [ ] No SQL Server test that passes today fails afterwards.

## Context
- Found by 0007 once the tests could reach a MySQL server (MySQL 8.0.46 container, MySql.Data 6.9.8 client).
- Tests, all in `Net.Sf.Dbdeploy.Database.MySqlDatabaseSchemaVersionManagerTest`:
  - `ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted`
  - `TestCanRetrieveSchemaVersionFromDatabase`
  - `TestReturnsNoAppliedChangesWhenDatabaseTableDoesNotExist`
  - `TestShouldCreateChangeLogTableWhenToldToDoSo`
  - `TestShouldReturnEmptySetWhenTableHasNoRows`
  - `TestDoesNotRunSecondScriptIfFirstScriptFails` (message says `near '.TableWeWillUse'`)
- Message: `MySql.Data.MySqlClient.MySqlException : You have an error in your SQL syntax; check the manual that corresponds to your MySQL server version for the right syntax to use near '.ChangeLog' at line 1`.
- Where it fails: the test helper `EnsureTableDoesNotExist(string tableName)` in `src/Test.Net.Sf.Dbdeploy/Database/MySqlDatabaseSchemaVersionManagerTest.cs` (line 141). It builds `DROP TABLE IF EXISTS {0}.{1}` from `new MySqlDbmsSyntax().GetTableInfo(tableName)`. The error text shows the statement starts with `.ChangeLog`, i.e. the schema part is empty.
- Suspected cause, not verified: `MySqlDbmsSyntax.GetTableInfo` returns an empty `Schema` for an unqualified table name (the SQL Server syntax presumably defaults to `dbo`). Whether the right fix is in the test helper (omit the schema when empty) or in `MySqlDbmsSyntax` (default schema = current database) has to be decided by reading how production code uses `TableInfo.Schema` for MySQL; the same empty schema may produce invalid SQL in production paths too.
- These tests fail in their first statement, so nothing after it has run against MySQL yet. The last commit on master before this effort is "Trying to get mysql compatibility"; expect more failures behind this one.
- This is test and possibly production code change; this item authorises both, limited to MySQL schema/table-name handling.

## Plan
- [ ] Read `MySqlDbmsSyntax`, `DbmsSyntax.GetTableInfo` and every use of `TableInfo.Schema`; confirm the cause.
- [ ] Decide where the fix belongs and apply it.
- [ ] Run `./psake start-mysql` and `./psake test`; record the six results.
- [ ] Create backlog items for failures that appear behind this one.

## Notes / log
- 2026-11-13: Created by 0007.

## Outcome
