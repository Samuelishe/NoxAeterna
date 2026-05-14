# Roadmap

This roadmap is staged to keep the project coherent and reviewable. Stages may overlap when useful, but each stage should leave the repository in a buildable and understandable state.

## Stage 0: Documentation Foundation

Create agent-oriented documentation, project vision, architecture notes, logs, known risks, and immediate next steps.

Status: in progress, documentation initialized.

## Stage 1: Solution Scaffold and Project Structure

Create .NET 10 solution projects with empty or minimal class libraries, project references, test project, package baseline, formatting settings, and build/test commands.

No product behavior is expected yet.

## Stage 2: Domain Model Skeleton

Create core entities and value objects for personal profiles, birth data, zodiac longitude, planetary positions, aspects, Tarot readings, symbolic factors, and interpretation blocks.

Add focused unit tests for normalization and basic invariants.

## Stage 3: Astronomy Abstraction and Time Model

Define `IEphemerisCalculator`, time conversion pipeline, zodiac longitude conventions, aspect calculation, retrograde direction, transit snapshot contracts, and lunar phase contracts.

Implementation may start with a deterministic test adapter before binding Swiss Ephemeris.

## Stage 4: Geometry Model for Charts

Define render-independent chart geometry models, angular math, radial positioning, glyph slots, aspect line models, and collision strategy interfaces.

Test core geometry math.

## Stage 5: Interpretation Engine Prototype

Implement structured symbolic factors, meaning fragments, modifiers, tension/reinforcement detection, and interpretation blocks without LLM dependency.

## Stage 6: Rendering Prototype

Create Avalonia-compatible chart rendering via `CustomControl`, `DrawingContext`, prepared geometry models, vector-style drawing, DPI-aware scaling, hover/selection state, and testable render models.

## Stage 7: Avalonia Shell and First UI Flow

Create the application shell, navigation, profile entry flow, and a first read-only astrology chart or profile view.

## Stage 8: Persistence and Profile Archive

Introduce SQLite storage, repositories, profile archive, saved readings, saved interpretations, and basic migration direction.

## Stage 9: Tarot MVP

Implement a deliberate Tarot reading flow, likely single card and three-card spread first, with upright/reversed support, saved sessions, and structured interpretation.

## Stage 10: Polish, Assets, Export, Packaging

Refine visual identity, asset pipeline, chart export, packaging, logging, diagnostics, and release workflow.
