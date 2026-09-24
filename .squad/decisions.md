# Squad Decisions

## Active Decisions

### Architectural Enforcement: Module Boundary Tests with ArchUnitNET

**Date:** 2026-04-15  
**Status:** ✅ Decided & Implemented  
**Owner:** Hulk (Tester) — corrected by Rodrigo Basniak  
**Amended:** 2026-04-15 — initial implementation used NetArchTest.Rules in error; replaced with ArchUnitNET per user directive.

**Context**

Architecture review found module boundary violations (Projects.Web → Subscriptions.Core, Inventory.Web → Inventory.UI) and recommended automated tests to prevent future violations. Manual code review is insufficient for catching layer violations.

**Decision**

Created `src/Features/Architecture.Tests/` project using **TngTech.ArchUnitNET v0.13.3** enforcing:
1. UI projects reference only Core.Contracts and Web.Contracts
2. UI projects don't reference Web.UseCases
3. Web projects don't reference other modules' Core
4. Core projects don't reference Web or UI
5. All modules follow naming convention
6. UI only references Contracts projects

Tests run as part of `dotnet test` in CI, failing the build on violations.

**Implementation pattern:**
- Assemblies loaded once at class level via `new ArchLoader().LoadAssemblies(...).Build()`
- Rules expressed with ArchUnitNET fluent API: `Classes().That().ResideInNamespace("A").Should().NotDependOnAnyTypesThat().ResideInNamespace("B")`
- Rules stored as `IArchRule` and checked via `rule.HasNoViolations(Arch)` + Shouldly (no xUnit/NUnit adapter needed for TUnit)

**Consequences**

**Positive:**
- Violations caught at build time, not runtime
- Tests serve as executable documentation of boundary rules
- No manual code review needed for layer violations
- ArchUnitNET is more expressive and type-safe than NetArchTest for complex cross-project rules

**Risks:**
- ArchUnitNET uses reflection — requires loading all module assemblies in one test project
- If modules become too large, test project becomes a coupling point

**Alternatives Considered**

1. **Manual code review checklist** — Rejected. Human review is slower and error-prone for structural rules.
2. **Separate test project per module** — Rejected. Boundary rules span multiple modules; single test project with complete assembly set is cleaner.
3. **NetArchTest.Rules** — Rejected by user directive. ArchUnitNET is the mandated library.

---

### Layer Violation Fixes: Projects.Web, Inventory.Web, Projects.UI

**Date:** 2026-04-15  
**Status:** ✅ Fixed & Verified  
**Owner:** Jarvis (Backend Developer)

**Context**

Architecture review identified 3 layer violations:
1. Projects.Web directly references Subscriptions.Core (should use integration layer)
2. Inventory.Web references Inventory.UI (dead reference, inverted layer direction)
3. Projects.UI.csproj contains AI/editor artifact entries

**Decision**

1. **Projects.Web → Subscriptions.Core:** Removed Core reference, changed `UploadProjectReferencePicture.Validator` to fail open (return `true`) when subscription entitlement query fails. Respects module boundaries; availability over strict enforcement during degradation.

2. **Inventory.Web → Inventory.UI:** Removed dead reference from .csproj.

3. **Projects.UI artifacts:** Removed `.codex1`, `.codex2`, `.cursor`, `.AntiGravity` from .csproj and deleted files from filesystem.

**Consequences**

**Positive:**
- Module boundaries properly enforced
- No dead references or artifact clutter
- Fail-open pattern improves availability (Subscriptions outage doesn't break Projects)

**Risks:**
- `UploadProjectReferencePicture` now allows operations when subscription service is degraded
- Acceptable trade-off: availability > strict tier enforcement during degradation

**Alternatives Considered**

1. **Circuit breaker pattern** — Rejected (over-engineered for this use case).
2. **Event-based fallback** — Rejected (subscription tier changes are rare; synchronous query is sufficient).
3. **Fail closed (reject operation)** — Rejected (poor user experience when Subscriptions is temporarily unavailable).

---

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
