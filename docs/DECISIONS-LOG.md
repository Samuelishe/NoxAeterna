# Decisions Log

Lightweight Architecture Decision Record log. New entries should include date, decision, reason, and consequences.

## 2026-05-14: Use .NET 10

Decision: Target .NET 10 for the application and libraries.

Reason: The project is starting fresh and should use the intended modern .NET stack.

Consequences: Dependencies must be verified for .NET 10 compatibility before adoption.

## 2026-05-14: Use Avalonia UI

Decision: Use Avalonia for the cross-platform desktop UI.

Reason: Avalonia supports cross-platform desktop development in C# and fits the target technology direction.

Consequences: UI and rendering code must still respect architectural boundaries and avoid leaking Avalonia into domain, astronomy, interpretation, or persistence.

## 2026-05-14: Use NodaTime

Decision: Use NodaTime as the main time model.

Reason: Birth data, historical timezone behavior, DST, ambiguous local times, and reproducibility require a robust time library.

Consequences: Date/time APIs should flow through NodaTime types. Direct `DateTime` use should be limited to interop boundaries.

## 2026-05-14: Hide Swiss Ephemeris Behind an Interface

Decision: Use Swiss Ephemeris or an equivalent wrapper behind an interface such as `IEphemerisCalculator`.

Reason: The rest of the system should not depend on a specific package or native interop detail.

Consequences: Package choice can change without rewriting UI, domain, geometry, or interpretation logic.

## 2026-05-14: Separate Astronomy, Geometry, Rendering, and Interpretation

Decision: Treat astronomy calculation, chart geometry, drawing, and symbolic interpretation as separate subsystems.

Reason: Each subsystem has different responsibilities, dependencies, and test strategies.

Consequences: More project boundaries are required, but the application remains testable and avoids god objects.

## 2026-05-14: Create a Dedicated Symbolics Layer

Decision: Add a Symbolics layer distinct from Interpretation.

Reason: Structured symbolic knowledge and user-facing interpretation composition are different responsibilities.

Consequences: Symbolic correspondences can be curated, tested, and reused without being buried in prose generation.

## 2026-05-14: Start With Documentation Before Code

Decision: Initialize agent-oriented documentation before writing application code.

Reason: Future autonomous sessions need continuity, boundaries, and product direction before implementation begins.

Consequences: The repository currently documents intent only. Implementation must not be assumed until later stages create code.
