# Development Philosophy

This document explains how decisions should be made, not just what the architecture diagram says.

## Core Principles

- Clarity over abstraction.
- Atmosphere over feature count.
- Deterministic rendering over UI magic.
- Desktop-first mindset.
- Reading-first UX.
- Maintainability and explainability over cleverness.
- Symbolic seriousness over shallow engagement mechanics.
- Minimal architecture that still protects real boundaries.

## Practical Meaning

### Clarity Over Abstraction

Add abstractions when they protect an actual boundary or remove real duplication. Do not create interface layers or service wrappers just because the codebase is young.

### Atmosphere Over Feature Count

The product should feel deliberate, calm, and archival. A smaller set of well-shaped capabilities is preferable to a noisy surface full of shallow interactions.

### Deterministic Rendering Over UI Magic

Charts and technical visuals should come from explicit geometry and rendering contracts, not incidental UI layout behavior or control nesting tricks.

### Desktop-First Mindset

This is a serious desktop tool, not a mobile engagement product. Prioritize keyboard-friendly workflows, stable panels, inspectable views, and archival depth.

### Reading-First UX

Users should be able to study, compare, revisit, and annotate. The experience should support long-form inspection rather than impulsive tapping and novelty loops.

### Avoid Enterprise Complexity Without Real Need

Use clean boundaries, but do not build a ceremony-heavy architecture. Avoid speculative frameworks, generic managers, and indirection without concrete benefit.

### Avoid Shallow Engagement Mechanics

Do not design around streaks, cosmic scores, arbitrary urgency, or gamified attention traps. The product should reward contemplation and record-keeping.

### Preserve Symbolic Seriousness

Treat symbolic systems as curated interpretive systems with historical and cultural depth. Avoid irony, parody, meme language, and fake AI mysticism.

### Favor Maintainability and Explainability

Future developers and agents should be able to explain why a chart, interpretation, or visual output was produced. Hidden side effects and opaque rule chains are unacceptable.

### Avoid Hidden Architectural Coupling

If one layer quietly starts doing another layer's work, the project will become incoherent. Keep handoffs explicit and testable.
