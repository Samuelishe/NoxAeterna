# Interpretation Engine

The interpretation engine produces structured user-facing meaning from symbolic factors. It must be rule-based, compositional, and grounded in curated symbolic knowledge.

## Critical Rule

Do not generate meaningless LLM-style text.

The interpretation system should not use a language model as the source of symbolic logic. Future LLM use may exist only as an optional narrative polishing layer over already structured meanings.

## Layered Interpretation

Interpretation should combine symbolic layers such as:

- Base archetype.
- Zodiac modifier.
- House modifier.
- Aspect modifier.
- Retrograde modifier.
- Transit context.
- Lunar context.
- Personal profile context.

Example:

```text
Mars
+ Scorpio
+ 8th house
+ square Saturn
```

This should be interpreted by combining structured symbolic fragments, not by hardcoding every possible combination.

## Pipeline Direction

Future pipeline:

1. Collect symbolic factors.
2. Normalize meaning fragments.
3. Apply contextual modifiers.
4. Detect tensions and reinforcements.
5. Produce structured interpretation blocks.
6. Optionally produce atmospheric prose from curated lexicon and narrative templates.

## Avoiding Combinatorial Explosion

Do not create a static rule for every possible combination of planet, sign, house, aspect, transit, lunar phase, and profile context.

Instead:

- Store atomic symbolic fragments.
- Add typed modifiers.
- Define composition rules.
- Track tensions and reinforcements.
- Produce structured blocks with clear contributing factors.

## Output Shape

Interpretation output should be structured before it becomes prose.

Expected future output:

- Title.
- Contributing factors.
- Primary theme.
- Supporting fragments.
- Tension markers.
- Reinforcement markers.
- Practical or reflective prompts, if appropriate.
- Optional narrative prose.

## Optional Narrative Layer

If a future LLM or template system is added, it may only polish or reshape curated structured output. It must not invent the symbolic basis.

Generated narrative should remain calm, restrained, serious, and non-ironic.
