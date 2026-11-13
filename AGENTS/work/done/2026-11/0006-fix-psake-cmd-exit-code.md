---
id: 0006
title: Make psake.cmd return a non-zero exit code when the build fails
status: done
type: bug
created: 2026-11-13
related: [0004, 0005]
---

## Goal
A process that runs the build through `psake.cmd` (a CI job, a script, an agent) can tell from the exit code whether psake succeeded: 0 on success, non-zero on failure.

## Acceptance criteria
- [x] From PowerShell, `cmd /c .\psake.cmd NoSuchTask; $LASTEXITCODE` gives a non-zero value.
- [x] From PowerShell, `.\psake.cmd NoSuchTask; $LASTEXITCODE` gives a non-zero value.
- [x] From cmd, `psake.cmd NoSuchTask & call echo %^errorlevel%` prints a non-zero value.
- [x] The same three invocations with `Help` give 0.
- [x] A task whose `Exec` command fails (e.g. a build with a compile error) gives non-zero through `psake.cmd`.
- [x] `psake.cmd /?`, `-h` and `-help` still print help and return 0.
- [x] `./psake build` still succeeds and returns 0.
- [x] No file under `Tools/psake/` is modified (ADR 0001).

## Context
- Found during 0004, on 2026-11-13, on Windows 11 with Windows PowerShell 5.1 as `powershell.exe`:
  - `cmd /c ".\psake.cmd NoSuchTask"` run from PowerShell left `$LASTEXITCODE` at 0 although psake printed "Task 'NoSuchTask' does not exist".
  - Inside a single cmd session, `.\psake.cmd NoSuchTask >nul 2>&1 & call echo errorlevel=%^errorlevel%` printed `errorlevel=1`. So the PowerShell child process does exit with 1 and `ERRORLEVEL` is set; it is the exit code of the batch file itself, as seen by its caller, that is lost.
  - Running the same command line as `psake.cmd` by hand (`powershell -NoProfile -ExecutionPolicy Bypass -Command "& '.\psake.ps1' NoSuchTask; if ($psake.build_success -eq $false) { exit 1 } else { exit 0 }"`) returns 1.
  - The behaviour is identical with the original psake 4.2.0 files, so it predates 0004. `psake.cmd` has not been changed in this effort.
- `psake.cmd` today (CRLF line endings): three `if '%1'==... goto help` lines, then the `powershell ... -Command "& '%~dp0\psake.ps1' %*; if ($psake.build_success -eq $false) { exit 1 } else { exit 0 }"` line followed by `goto :eof`, then a `:help` label.
- Suspected cause, not verified: the script ends through `goto :eof` without an explicit `exit /b %errorlevel%`, and the caller does not receive the last `ERRORLEVEL`. Confirm by experiment before settling on the fix.
- `$psake.build_success` is still set by psake 5.0.4, and `Invoke-psake` also returns a `PsakeBuildResult` with a `Success` property, which the wrapper `psake.ps1` currently filters out. Either can drive the exit code. Making `psake.ps1` itself exit non-zero on failure is in scope if it is the cleaner fix, as long as `psake.ps1` can still be called from an interactive PowerShell session without closing it.
- Affected by this bug: 0005 (`./psake Test` must fail visibly when tests fail) and any future CI.

## Plan
- [x] Reproduce the three invocations in the acceptance criteria and record the current exit codes in Notes.
- [x] Find the cause by experiment (explicit `exit /b %errorlevel%`, removing `goto :eof`, argument quoting with `%*`).
- [x] Apply the smallest fix in `psake.cmd` (and `psake.ps1` only if needed); keep CRLF line endings.
- [x] Re-run every acceptance check, including the help switches and a real failing `Exec`.
- [x] Remove the "returns 0 to the caller" warning from the latest `PROGRESS.md` entry and from the Context of 0005; update AGENTS.md if it mentions the limitation.

## Notes / log
- 2026-11-13: Item created from the finding in 0004. Nothing was changed.
- 2026-11-13: Reproduced before the fix. From PowerShell, `cmd /c .\psake.cmd NoSuchTask` and `.\psake.cmd NoSuchTask` both gave `$LASTEXITCODE` 0; inside cmd, `%errorlevel%` after the call was 1. `Help` gave 0 everywhere.
- 2026-11-13: Cause confirmed by experiment: the main path of `psake.cmd` ended with `goto :eof`. Replacing that one line with `exit /b %errorlevel%` makes the caller receive the PowerShell exit code. Nothing else was needed; `psake.ps1` and the `:help` branch are unchanged.
- 2026-11-13: After the fix, from PowerShell unless noted:
  - `cmd /c .\psake.cmd NoSuchTask` → 1; `.\psake.cmd NoSuchTask` → 1; from cmd, `%errorlevel%` → 1.
  - `Help` through the same three invocations → 0. Bare `.\psake.cmd` → 0.
  - A task whose `Exec` command fails → 1 (checked with a temporary build file outside the repo containing `Exec { cmd /c exit 3 }`, not with a real compile error); a passing task in the same file → 0.
  - `.\psake.cmd /?`, `-h`, `-help` → 0 and print the `Invoke-Psake` help.
  - `.\psake.cmd build` → 0, 0 errors.
- 2026-11-13: The exit code is always 0 or 1; the failing command's own code (3 in the check above) is not passed through.

## Outcome
`psake.cmd` now ends its main path with `exit /b %errorlevel%` instead of `goto :eof`, so a failed psake run returns 1 to the calling process and a successful one returns 0. One line changed; no file under `Tools/psake/` was touched.

The "returns 0 to the caller" warnings were removed from the latest `PROGRESS.md` entry and from the Context of 0005. AGENTS.md did not mention the limitation.
