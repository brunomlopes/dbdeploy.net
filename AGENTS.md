# AGENTS.md

Instructions for AI coding agents (and humans) working in this repository.
Read this file at the start of every session. Keep it short: it is loaded every time.

## Project

This project manages the deployment of numbered change scripts in versioned folders to a SQL, Oracle, or MySQL database, using a simple table in the database to track applied changes.

### Commands

- Build: `./psake build`

### Conventions


- Do not change public APIs, schemas, or dependencies without a work item that says so.
- Do not reverse a decision recorded in `AGENTS/docs/decisions/` without writing a new ADR that supersedes it.

## Agent workspace layout

All planning, tracking, and decision files live under `AGENTS/`:

```
AGENTS.md                  # this file
AGENTS/
  docs/
    decisions/             # Architecture Decision Records (ADRs)
      0001-short-title.md
  work/
    backlog/               # todo: not started
    doing/                 # in progress (ideally one or two items at a time)
    blocked/               # waiting on something external
    done/
      YYYY-MM/             # completed items, archived by month
    PROGRESS.md            # rolling session log and handoff notes
```

## Work items

Every piece of work (feature, bug, migration, chore) is one markdown file. Changes that are trivial (as defined in Lifecycle step 1) may be made without a work item; all other changes require one before code is written.

- File name: `NNNN-short-kebab-title.md`, where `NNNN` is the next unused number across all of `AGENTS/work/` (check every subfolder, including `done/`).
- The folder an item is in is its status. Keep the `status` field in the frontmatter in sync with the folder.
- Keep items small enough to finish in one session. If a plan grows beyond roughly 8 steps, split it into separate items and link them.

### Template

```markdown
---
id: NNNN
title: Short imperative title
status: backlog        # backlog | doing | blocked | done
type: feature          # feature | bug | migration | chore
created: YYYY-MM-DD
related: []            # other item ids or ADR numbers
---

## Goal
The outcome in one or two sentences, not the implementation.

## Acceptance criteria
- [ ] Verifiable condition (prefer ones a test or command can check)

## Context
Relevant files, ADRs, related items, links.

## Plan
- [ ] Step

## Notes / log
- YYYY-MM-DD: Findings, surprises, decisions made along the way.

## Outcome
Filled in when done: what changed, what was deferred, follow-up items created.
```

### Lifecycle

1. **Start:** Move the file from `backlog/` to `doing/` and set `status: doing`. If the Plan section is empty or vague, write the plan first and stop for human review before writing code, unless the task is trivial.
2. **Work:** Tick plan checkboxes as steps complete. Append dated notes when you learn something non-obvious. Commit work-item updates together with the code they describe.
3. **Blocked:** Move to `blocked/`, set `status: blocked`, and write in Notes exactly what is needed to unblock it.
4. **Discovered work:** Do not expand scope silently. Create a new backlog item and reference it in Notes.
5. **Finish:** Only when every acceptance criterion is met and tests pass. Fill in Outcome, set `status: done`, move the file to `done/YYYY-MM/`, and add an entry to `PROGRESS.md`.

## PROGRESS.md

A rolling log so the next session (or a different agent) can resume cheaply. At the end of every session, append an entry at the top:

```markdown
## YYYY-MM-DD
- Worked on: 0039 (migrate auth), 0043 (timezone bug)
- Done: 0043
- State: 0039 at plan step 3, backfill script written but not run on staging
- Next: run backfill on staging, then switch reads
- Watch out: <anything surprising>
```

When the file gets long, summarize entries older than a month into a short paragraph per month.

## Decision records (ADRs)

Write an ADR in `AGENTS/docs/decisions/` for any choice that a future contributor might otherwise "fix": architecture, libraries, data formats, migration strategy, deliberate trade-offs. Number them sequentially. Never delete or rewrite an accepted ADR; supersede it with a new one.

```markdown
# NNNN. Short title

- Status: proposed | accepted | superseded by NNNN
- Date: YYYY-MM-DD

## Context
What forces are at play.

## Decision
What we chose.

## Consequences
What becomes easier or harder as a result.
```

## Migrations

For migrations that touch many files or data:

- Create a parent work item of `type: migration` containing the strategy (or a link to its ADR) and a checklist inventory of every module, file, or table to migrate.
- Prefer incremental, reversible steps (dual-write, feature flags, shims) over a single big change.
- Work through the inventory in batches, one batch per session, ticking items off in the parent file.
- Make data migration scripts idempotent, and record in Notes which environments each script has run on.

## Session checklist

At the start of a session:
1. Read this file.
2. Read the latest entry in `AGENTS/work/PROGRESS.md`.
3. Look in `AGENTS/work/doing/` before picking anything new from `backlog/`.

At the end of a session:
1. Update the active work item (checkboxes, notes).
2. Append to `PROGRESS.md`.
3. Make sure tests pass, or record in the work item why they don't.