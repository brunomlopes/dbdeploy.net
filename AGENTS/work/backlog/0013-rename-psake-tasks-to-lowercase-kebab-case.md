---
id: 0013
title: Rename all psake tasks to lowercase kebab-case
status: backlog
type: chore
created: 2026-11-13
related: [0004, 0005, 0007, 0008]
---

## Goal
Every psake task has a lowercase name with words separated by hyphens (for example `build-package`, `publish-package`, `start-databases`), and that convention is written down so new tasks follow it.

## Acceptance criteria
- [ ] Every `Task` in `psakefile.ps1` is declared with a lowercase kebab-case name; `./psake help` lists only such names (apart from psake's required `default`).
- [ ] `-depends` references inside `psakefile.ps1` use the new names.
- [ ] Each renamed task runs under its new name: `./psake build`, `./psake clean`, `./psake help`, `./psake test` behave as before the rename.
- [ ] The old multi-word names (`BuildPackage`, `PublishPackage`) no longer work. No aliases are added for the old names.
- [ ] AGENTS.md "Commands" uses the new names, and "Conventions" states the naming rule for psake tasks.
- [ ] Open work items that name psake tasks (0003, 0007 to 0012) use the new names; ADRs 0001 and 0002 are not rewritten (see Context).
- [ ] No task body is changed.

## Context
- Decided by the maintainer on 2026-11-13, with the examples `start-databases`, `publish-package`, `build-package`. Also decided: no aliases for the old names; do not use psake's `-alias` parameter to keep them alive.
- Tasks in `psakefile.ps1` today (after 0005) and their new names:

  | Current | New |
  |---|---|
  | `Build` | `build` |
  | `Test` (depends on `Build`) | `test` |
  | `Clean` | `clean` |
  | `BuildPackage` (depends on `Clean`) | `build-package` |
  | `PublishPackage` | `publish-package` |
  | `Help` | `help` |
  | `default` (depends on `Help`) | unchanged; psake requires this name |

- Tasks added by other items must follow the same rule. If 0007 and 0008 are done first, rename their tasks here; if this item is done first, update their suggested names. Suggested: `start-sqlserver`, `start-mysql`, `start-postgres`, `start-databases` (starts all of them, the maintainer's example) and `stop-databases`. 0007 currently suggests `StartSqlServer`, `StartMySql`, `StopDatabases`.
- psake 5.0.4 looks tasks up case-insensitively (`Tools/psake/public/Task.ps1` stores them under `Name.ToLower()`), so `./psake build` and `./psake Build` already reach the same task. For the single-word tasks the rename only changes how the name is declared and displayed; `./psake Test` in existing docs keeps working. Only the two-word tasks get names that are typed differently.
- Hyphenated names have not been tried with this psake version or with the wrapper. Check that `Task build-package ...` parses as a bare word (quote it if not), that `-depends` accepts it, and that `psake.cmd build-package` passes through `psake.ps1`: the wrapper splits its first argument on commas and spaces to build the task list, not on hyphens.
- `psakefile.ps1` has CR-only line endings; edit it with a tool that preserves them (see `PROGRESS.md`).
- `build-package` and `publish-package` cannot be run to completion today: they depend on ILMerge, `Write-Zip` and Bintray (see 0003). Verify them by `./psake help` and by checking that psake resolves the name and starts the task, not by a successful run.
- Task names are part of how people and scripts call the build, so this is a (small) interface change; this item is the authorisation. No CI configuration exists in the repo, and `go.bat` / `dbdeploy.build` are the old NAnt entry points and do not call psake.
- ADRs are not rewritten after acceptance (AGENTS.md). ADR 0001 and 0002 mention `./psake build`, `./psake Clean`, `./psake Help` and `./psake Test`; those still work because of the case-insensitive lookup, so leave them.
- Finished work items in `done/` keep the names that were true when they were written.

## Plan
- [ ] Record the output of `./psake help` before the change.
- [ ] Rename the task declarations and `-depends` references in `psakefile.ps1`.
- [ ] Run `./psake help`, `./psake build`, `./psake clean` and `./psake test`; confirm `build-package` and `publish-package` are resolved by name.
- [ ] Update AGENTS.md (Commands and the naming rule under Conventions).
- [ ] Update task names in the open work items, and rename tasks from 0007 / 0008 if they already exist.

## Notes / log
- 2026-11-13: Item created. Nothing was changed.
- 2026-11-13: Maintainer decision: no aliases for the old names during a transition; the rename is a clean cut.

## Outcome
