# Squad Decisions

## Active Decisions

### Documentation Decision: Architecture Section in src/README.md

**Date:** 2026-04-15  
**Status:** ✅ Decided & Implemented  
**Owner:** Wong (Documentation Writer)

**Context**

The `src/README.md` only documented module/project structure (what goes where in each `*.Core`, `*.Web`, `*.UI` project). It was missing:
- How to run the application (Docker, Aspire command, service URLs)
- Solution-level folder structure (Aspire, Application, Features, Infrastructure, Persistance)
- Runtime architecture (how layers fit together, how events flow)
- Event-driven communication patterns (inbox/outbox, domain vs integration events)
- Cross-module communication strategy (sync via IDispatcher vs async via events)
- Shared infrastructure (rbkApiModules, two EF contexts, telemetry)
- Testing guidance (TUnit, Testcontainers, Playwright)
- Seeded dev accounts

**Decision**

Extended the README with 8 new sections:
1. **Solution structure** — top-level folder layout
2. **How to run** — Docker setup, Aspire command, service URLs table, HTTPS warning
3. **Architecture overview** — high-level layers, flow diagram (text)
4. **Event-driven communication** — step-by-step inbox/outbox flow, when to use events
5. **Cross-module communication** — IDispatcher vs events, guideline
6. **Shared infrastructure** — rbkApiModules, two EF contexts, telemetry, file storage, multi-tenancy
7. **Running tests** — TUnit filter syntax, Testcontainers, Playwright browser install
8. **Seeded accounts** — dev credentials table

Matched existing README style: concise, developer-focused, tables and code blocks for clarity, no marketing language.

**Consequences**

**Positive:**
- Developers can now onboard without external documentation or tribal knowledge
- HTTPS endpoint requirement is clearly documented (prevents silent API call failures)
- Event flow is demystified (two-tier outbox pattern is non-obvious)
- TUnit filter syntax is documented (prevents "why doesn't `--filter` work?" confusion)

**Risks:**
- README is now 300+ lines. May need to split into separate docs if it grows further (e.g., `ARCHITECTURE.md`, `RUNNING.md`)
- Must keep in sync with actual implementation (especially service URLs, seeded accounts)

**Alternatives Considered**

1. **Separate ARCHITECTURE.md** — Rejected. The README is the first place developers look. Splitting would fragment critical info.
2. **Link to external wiki** — Rejected. Code should document itself. External docs go stale.
3. **Minimal README + inline code comments** — Rejected. High-level architecture (Aspire orchestration, event flow) doesn't fit in code comments.

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
