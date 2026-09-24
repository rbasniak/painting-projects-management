# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|---------|
| Project leadership, scope, priorities | Fury | What to build next, trade-offs, architectural gates |
| Architecture design, ADRs, module design | Stark | New module structure, cross-cutting concerns, tech decisions |
| Backend, API endpoints, domain models, EF Core | Jarvis | New use cases, handlers, commands, queries, migrations |
| Frontend, Blazor UI, components, service wrappers | Loki | New UI slices, pages, `*.UI` service wrappers, navigation |
| Testing, QA, edge cases, coverage | Hulk | Unit tests, integration tests, test scenarios from spec |
| Documentation, changelogs, ADR write-ups | Wong | README updates, module docs, onboarding guides |
| Session logging, decisions merging, history | Uatu | Automatic — never needs routing |
| Work queue monitoring, issue backlog | Xavier | Automatic when activated |
| Infrastructure, Aspire, Docker, CI/CD, migrations | Rocket | AppHost config, container issues, build pipelines |
| Code review | Fury | Review PRs, check quality, enforce vertical slice rules |
| Architecture review | Stark | Review cross-module contracts, module boundary violations |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Fury |
| `squad:fury` | Pick up issue and complete the work | Fury |
| `squad:stark` | Pick up issue and complete the work | Stark |
| `squad:jarvis` | Pick up issue and complete the work | Jarvis |
| `squad:loki` | Pick up issue and complete the work | Loki |
| `squad:hulk` | Pick up issue and complete the work | Hulk |
| `squad:wong` | Pick up issue and complete the work | Wong |
| `squad:rocket` | Pick up issue and complete the work | Rocket |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, **Fury** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Fury's review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Uatu always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for simple questions.
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn Hulk to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. Fury handles all `squad` (base label) triage.
