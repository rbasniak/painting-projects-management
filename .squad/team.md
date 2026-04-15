# Squad Team

> ppm

## Coordinator

| Name | Role | Notes |
|------|------|-------|
| Squad | Coordinator | Routes work, enforces handoffs and reviewer gates. |

## Members

| Name | Role | Charter | Status |
|------|------|---------|--------|
| Fury | Lead | .squad/agents/fury/charter.md | ✅ active |
| Stark | Architect | .squad/agents/stark/charter.md | ✅ active |
| Jarvis | Backend Developer | .squad/agents/jarvis/charter.md | ✅ active |
| Loki | Frontend Developer | .squad/agents/loki/charter.md | ✅ active |
| Hulk | Tester | .squad/agents/hulk/charter.md | ✅ active |
| Wong | Documentation Writer | .squad/agents/wong/charter.md | ✅ active |
| Uatu | Session Logger | .squad/agents/uatu/charter.md | ✅ active |
| Xavier | Work Monitor | .squad/agents/xavier/charter.md | ✅ active |

## Project Context

- **Project:** Painting Projects Management (ppm)
- **Owner:** Rodrigo Basniak
- **Created:** 2026-04-15
- **Stack:** .NET 10, ASP.NET Core, Blazor WASM, PostgreSQL, EF Core, RabbitMQ, .NET Aspire
- **Architecture:** Vertical slices, event-driven (inbox/outbox), module-per-feature
- **Module layout:** Each module = `*.Core` + `*.Core.Contracts` + `*.Web` + `*.Web.Contracts` + `*.UI` + `*.Integrations` + `*.Integrations.Contracts`
- **Solution root:** `src/` — see `src/README.md` for module conventions

## Issue Source

Not connected to GitHub Issues yet.
