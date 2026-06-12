---
agent: 'agent'
description: 'Read minimal hot-tier project context, then produce a self-contained implementation prompt for Agent mode.'
---

# Plan a Task

## Step 1 — Read the hot tier (always, in this order)
1. `.github/agent.md` — agent identity, scope rules, memory protocol
2. `.github/memories/current_state.md` — current project state and "Next Up"
3. `.github/memories/pending_decisions.md` — open decisions only

Do NOT read `architectural_decisions.md`'s linked `adr/*.md` files,
`pending_decisions_archive.md`, `user_preference.md`, or any `work_log_*.md` yet — those are
loaded selectively in Step 2, only if relevant.

## Step 2 — Pull in only what this task needs

Task(s):
${input:tasks:Describe the task(s) to plan}

Based on the task description and `current_state.md`:
- Read `.github/memories/architectural_decisions.md` (it's a short index table) and open only
  the specific `adr/ADR-NNN-*.md` files whose "Areas touched" overlap this task.
- Read only the section(s) of `.github/user_preference.md` that apply to this task (e.g. CQRS
  Use-Case Files, logging conventions, DI registration, testing strategy) — skip the rest.
- Open `work_log_*.md` or `pending_decisions_archive.md` only if you need session-level detail
  or historical rationale not captured in an ADR.

## Step 3 — Produce the Agent-mode prompt

Write a single self-contained prompt for the Agent-mode execution session. It must:
- State the task(s) verbatim.
- List the exact file paths (specific ADRs, `user_preference.md` sections, relevant excerpts
  from `current_state.md`) the execution agent needs, so it does not re-read everything from
  scratch.
- Include the conventions/constraints that apply to this task (naming, file layout, logging,
  validation, testing requirements).
- Include the memory-update instructions from Step 4 verbatim.

Output ONLY this prompt as your final response — do not start implementing the task yourself.

## Step 4 — Memory update instructions (include verbatim in the generated prompt)

After completing the work, the execution agent must:
- **Overwrite** `.github/memories/current_state.md` with the new current state (do not append
  to the old content). Keep it under ~50 lines: branch, last session's completed work, a short
  project-structure snippet, "Next Up", "Ready to Proceed".
- For any new architectural decision: add `adr/ADR-NNN-short-slug.md` (next sequential
  number) and add one row to the index table in `architectural_decisions.md`.
- For any new open question: add an entry to `pending_decisions.md`. For any resolved item:
  move its entry from `pending_decisions.md` to `pending_decisions_archive.md`.
- Create or update `work_log_[DATE]_[branch].md` with full session detail (this is the durable
  history; it is not read by default in future sessions).
