# Lupus Noctis

| Metadata | Definition |
| --- | --- |
| Role | Canonical human-readable owner document for the Lupus Noctis artwork pack. |
| Read when | Researching, generating, reviewing, accepting, replacing, or productionizing any Lupus Noctis card. |
| Authoritative for | Working identity, world and art-direction rules, rejection history, meaning-first workflow, casting policy, generation provenance, and production-card records. |
| Not authoritative for | Tarot semantic identities, runtime manifests, programmatic frame/typography implementation, or owner artistic acceptance outside an explicit recorded decision. |

## Identity

- Working name: **Lupus Noctis**. The name is not final.
- It is the first deck in production order.
- In the future UI it is intended to appear second, after the classic deck.

## Current Status

- Accepted production cards: **6/78**.
- A0: **Rejected**.
- A1: **Rejected**.
- A2: **Accepted** as the first production wave.
- A2 generation method: built-in `imagegen`, text-only; no attachment, A0/A1 image, or newly generated card was used as a reference for another card. The only edit input was the first Star candidate itself for its targeted technical correction.
- A2 production cards:
  - `major.death` — owner acceptance: **Accepted**.
  - `minor.cups.six` — owner acceptance: **Accepted**.
  - `major.star` — owner acceptance: **Accepted**.
- A4: **Accepted** as the second production wave.
- A4 production cards:
  - `major.sun` — owner acceptance: **Accepted**.
  - `minor.swords.five` — owner acceptance: **Accepted**.
  - `major.moon` — owner acceptance: **Accepted**.

The accepted A2 and A4 images are shipped production assets listed by `artwork-pack.json`. No A4 study copies remain, the next generation batch remains deliberately unselected, and no contact sheets belong to this pack.

Runtime discovery, AppData seeding, user-pack behavior, normalization, tooling, and packaging are owned by [`docs/ASSET-PACK-RUNTIME.md`](../../../../../docs/ASSET-PACK-RUNTIME.md). This pack contains **6/78 accepted production cards** while that staged architecture is implemented independently.

## Core World

Lupus Noctis is a narrative fantasy artwork pack built around a world of lycanthropic mythology, not a deck of repetitive wolf portraits.

Its coherent fantasy world contains people, ordinary wolves, lycanthropes and werewolves, hunters, warriors, rulers, priests, families, clans, cities, villages, fortresses, temples, taverns, markets, forests, rituals, wars, celebrations, artifacts, and wolf heraldry, masks, shadows, spirits, and legends. A visible wolf is not required on every card.

## Wolf Motif Modes

Choose the mode that serves the individual card meaning:

1. **Literal wolf** — an ordinary wolf or pack.
2. **Lycanthrope** — full or partial transformation with serious anatomy; never furry.
3. **Human with wolf symbolism** — a human linked to the motif through heraldry, mask, pelt, shadow, spirit, tracks, or narrative.
4. **Wolf-world indirect** — no wolf is visible, but the scene unmistakably belongs to the Lupus Noctis world.

The card meaning selects the mode; the mode does not impose a wolf on every illustration.

## Casting Policy

Accidental same-face repetition is prohibited. By default each card receives independent casting across age, gender, facial structure, complexion, build, hairstyle, clothing, and social role.

Recurring characters are allowed only as a deliberate narrative decision: the character must be described in a tracked cast section and every participating card must refer to that record explicitly. The people currently shown in Death, Six of Cups, and the Star are not declared to be one person. Their accepted visual resemblance is not a casting or image reference for later generations.

## Style Unity

Deck unity comes from contemporary painted fantasy illustration, narrative staging quality, shared worldbuilding, a consistent rendering standard, natural brushwork, clear visual hierarchy, material-specific rendering, and the later programmatic frame and typography overlay.

Unity does **not** mean one palette, one darkness level, the same forest, the same lighting, the same wolf, one camera scale, one composition, or one texture spread over every surface.

The intended level is modern game loading-screen art, RPG story-card art, or chapter illustration: visually fresh, emotionally immediate, and readable at card size. Characters act, decide, or experience an event. Gestures and silhouettes are legible; the focal area carries controlled detail while the background is quieter. Stone, skin, fur, cloth, metal, water, and vegetation retain distinct material responses.

Forbidden series-wide treatments include old-master museum painting, Baroque stiffness, faux-antique art, parchment or old-book filters, engraving simulation, mosaic or crackle texture, tiled or scale-like microtexture, universal surface noise, and a gray-blue wash. Also forbidden: photorealism, 3D rendering, anime, comic cel shading, furry aesthetics, and generic wolf-calendar art.

## Meaning-First Workflow

Before creating every card:

1. Research stable traditional meanings.
2. Separate the base archetype, emotional tone, constructive aspects, tensions and risks, upright meaning, and materially relevant reversed meaning.
3. Do not copy modern interpretive prose; paraphrase source ideas.
4. Write a card brief covering narrative core, characters, event, wolf motif mode, framing, lighting, color script, symbolic anchors, and forbidden clichés.
5. Generate only after the research and brief are recorded.

The story must not be inferred only from the English card title.

## Emotional Range

The deck may contain dark, bright, neutral, solemn, tragic, joyful, intimate, conflict-driven, mystical, and quiet cards. The individual meaning determines the tone. There is no global dark filter.

## Typography and Overlay Boundary

- Illustrations contain no text, card title, number, frame, or border.
- Frame, number, and title are a future programmatic overlay.
- Future localization is expected to cover RU, EN, and roughly 20+ languages.
- Latin, Cyrillic, CJK, Arabic, and other writing systems may use different free font families.
- Fonts will be stored in the repository later, with license and provenance recorded in the README and `docs/THIRD-PARTY.md`.
- A2 adds no fonts.

## A0 and A1 Rejection Record

A0 and A1 were rejected for monochrome sameness, repetitive wolf landscapes, passive composition, weak meaning-based storytelling, global mosaic microtexture, insufficient fantasy-world diversity, and an outdated AI-art appearance. None of their images is accepted, retained as an anchor, used as a composition reference, edited for A2, or promoted to production.

## Research Sources

The summaries below synthesize and paraphrase historical sources; they do not copy modern commercial interpretations.

- Arthur Edward Waite, *The Pictorial Key to the Tarot*, [Part II: major-symbol discussion](https://en.wikisource.org/wiki/The_Pictorial_Key_to_the_Tarot/Part_2) and [Part III: divinatory meanings and Minor Arcana](https://en.wikisource.org/wiki/The_Pictorial_Key_to_the_Tarot/Part_3), first published in 1910. Part II connects Death with passage, changed consciousness, rebirth, and renewal, and connects the Star with hope, interior light, truth, and life-giving water; Part III presents the Six of Cups as childhood memory and happiness drawn from the past.
- S. L. MacGregor Mathers, *The Tarot: Its Occult Signification, Use in Fortune-Telling, and Method of Play* (1888), [public transcription](https://hermetics.net/media-library/tarot/samuel-liddell-macgregor-mathers-the-tarot/). Its concise earlier list associates Death with change and transformation, the Star with hope and bright expectation, and the Six of Cups with what has passed or vanished; its reversed entries add incomplete change, disappointed expectation, and movement toward the future.
- [WorldCat record for the 1888 Mathers edition](https://search.worldcat.org/title/The-Tarot-%3A-its-occult-signification-use-in-fortune-telling-and-method-of-play-etc/oclc/637249905) confirms the work's author, title, publisher, and publication year.

## A2 Card Records

### `major.death`

**Researched meaning summary.** The base archetype is an irreversible ending that changes the form of what continues. Its emotional tone is grief and necessity rather than literal murder. Constructively it releases an exhausted identity and permits renewal; its tension is loss, fear, or clinging to what can no longer remain. Upright emphasizes completion, transition, transformation, and the new path opened by surrender. Reversed is visually relevant as stalled, partial, or resisted transformation.

**Card brief.**

- Narrative core: a defeated warrior's old identity ends at ruined city gates while a dangerous transformation opens an uncertain life beyond the battle.
- Characters: the transforming human warrior is central; a second human survivor reacts in shock and concern; distant combatants recede rather than dominate.
- Event: the warrior moves through a painful partial lycanthropic change, lets a broken sword fall, and crosses from the finished battle toward an open dawn road while a black wolf spirit moves beside them.
- Wolf motif mode: **Lycanthrope + human narrative**.
- Framing: cinematic medium-wide portrait frame, diagonal forward movement, full event rather than static portrait; safe quiet zones at top and bottom.
- Lighting: dying crimson battle light behind, cold turquoise-green dawn ahead, bronze sparks crossing the diagonal.
- Color script: deep black, saturated crimson, cold turquoise-green dawn, bronze sparks, ivory highlights.
- Symbolic anchors: broken sword and discarded armor for the ended identity; ruined gate as threshold; black wolf spirit as the new nature; open road and dawn as continuation.
- Forbidden clichés: no skeletal grim reaper, gore spectacle, static monster portrait, triumphant superhero pose, furry humanoid, generic wolf-on-landscape staging, or purely literal physical death.

**Full generation prompt.**

```text
Use case: illustration-story
Asset type: borderless inner illustration for a 7:12 portrait Tarot card review candidate
Primary request: Create a contemporary narrative fantasy card illustration about an irreversible ending, painful transformation, release of an old identity, and passage into a new state; this is not a depiction of literal death.
Scene/backdrop: Ruined stone city gates immediately after a battle. Behind the gate the battle is visibly ending in smoke and torn crimson banners; through the broken arch ahead, a cold turquoise-green dawn reveals an open road into unfamiliar country.
Subject and event: In a cinematic medium-wide scene, a central human warrior moves diagonally through the gate while undergoing a difficult partial lycanthropic transformation. The body remains recognizably human but the silhouette, one arm, shoulders, jaw, and shadow are changing with serious plausible anatomy. Fragments of damaged armor fall away and a broken sword slips from a natural human hand, marking the end of the former identity. A black wolf spirit or living shadow runs beside the warrior toward the new road. A second human survivor nearby reacts with shock, grief, and concern, making the event relational rather than a solitary monster portrait. Distant figures and embers show the conflict receding behind them.
Style/medium: Contemporary painted fantasy illustration, modern RPG chapter art or high-quality game loading-screen narrative art, visually fresh, natural varied digital brushwork, strong silhouettes, clear focal hierarchy, controlled detail, background less detailed than the focal figures. Distinct rendering for skin, fur, cloth, bronze, broken steel, stone, smoke, and dawn air; no texture overlay shared across materials.
Composition/framing: Exact 7:12 portrait intent. Cinematic medium-wide framing with diagonal forward movement from lower battle-darkness toward upper dawn opening. Keep all essential faces, hands, limbs, sword, and wolf spirit inside generous side margins. Leave calm safe areas near the top and bottom for a future programmatic title and number overlay. Readable as a thumbnail; not a centered static portrait.
Lighting/mood: Dark, tragic, transitional. Saturated crimson backlight and bronze sparks from the finished battle contrast with cold turquoise-green dawn ahead; selective ivory highlights clarify the face, hand, broken steel, and changing anatomy.
Color palette: Deep black, saturated crimson, cold turquoise-green, bronze, ivory; avoid a gray-blue wash.
Constraints: No text, letters, numbers, Roman numerals, title, frame, border, watermark, signature, logo, heraldic writing, or pseudo-runes. Serious human, hand, face, wolf-spirit, and partial-lycanthrope anatomy. The transformation must not look like a furry costume, animal mascot, or wolf in armor. No graphic gore.
Avoid: Skeleton or grim reaper, literal corpse as focal point, static monster portrait, superhero pose, generic wolf calendar art, old-master or Baroque painting, faux antique art, parchment filter, old-book or engraving treatment, mosaic, crackle, tiled or scale-like microtexture, universal surface noise, photorealism, 3D render, anime, comic cel shading.
```

**Negative constraints.** No embedded typography or border; no literal reaper or gore; no furry anatomy; no static portrait; no antique, engraved, mosaic, crackle, repeated microtexture, or universal-noise treatment; no gray-blue wash; no photoreal, 3D, anime, or cel-shaded rendering.

- Production path: `cards/major/death.png`
- Actual dimensions: `952 × 1632 px` (exact `7:12`).
- SHA-256: `b5bb6ea0d42adc2d195494bb737b03d72a3c950ce90b2878bee974c9213dadc5`.
- Generation count: **1**.
- Technical review: The narrative threshold, diagonal movement, human reaction, partial lycanthropic change, black wolf spirit, warm/cool separation, and material hierarchy are present. No obvious extra limbs, malformed face or hand, embedded text, furry treatment, or repeating global texture was found. Minor staging divergence: the broken sword is already left at the threshold rather than visibly slipping from the changing hand; this is retained for owner review and is not treated as a technical-generation failure.
- Owner acceptance: **Accepted**.

## A4 Card Records

A4 is a text-only contrast batch. No attachment, external image, accepted production card, rejected A0/A1 image, or newly generated A4 card was an image reference for any generated card. The records below use the historical sources already identified in [Research Sources](#research-sources), together with project-authored paraphrases and narrative briefs. The owner explicitly accepted all three A4 cards for production; their independent casting does not establish recurring characters.

### `major.sun`

**Researched meaning summary.** The archetype is direct light, conscious clarity, vitality, openness, and a condition in which the result can be seen plainly. Its emotional center is physically believable shared joy arising from an actual achievement. Constructively it supports health, success, confidence, honest connection, recognition, and return to life. Its tensions are overconfidence, naivety, performative triumph, and truth that can no longer be avoided. Upright emphasizes happiness, clear results, warmth, satisfaction, and human closeness; reversed is materially relevant as light and joy that exist but are delayed, reduced, shadowed, or difficult to receive fully.

**Card brief.**

- Narrative core: after a severe winter, a mountain city celebrates a concrete shared achievement when a restored bronze sun mirror reignites its communal beacon.
- Characters: a dark-skinned girl apprentice, thirteen to fifteen, in practical cream and ochre workwear performs the decisive action; an older broad-built master mason with a damaged hand and dark-green work clothes watches with quiet pride; the background includes varied ages and social roles. This is independent casting and none is recurring cast.
- Event: the apprentice lowers the last stone-and-bronze lever; a direct reflected sunbeam crosses the frame, lights the public beacon, and draws the city from dark galleries back into work and community.
- Wolf motif mode: **Wolf-world indirect**, limited to restrained bronze mounting, pennant, or stone heraldry; no living wolf is required.
- Framing: vertical cinematic medium-wide scene; foreground mechanism and apprentice; master supports without obscuring the action; diagonal beam leads into opening gates and a living square; quiet safe areas at top and bottom.
- Lighting: clear direct morning sun, warm beam, cool clean post-winter air, pale-stone bounce, high but natural contrast without white clipping.
- Color script: sunflower yellow, warm ivory, clear turquoise sky, terracotta, restrained fresh green, bronze, and small cool shadows; no orange or sepia wash.
- Symbolic anchors: restored mirror, hand on the last lever, communal beacon, open gates, people leaving shadow, restrained wolf heraldry, and traces of the ended winter.
- Forbidden clichés: no white horse, classic Rider–Waite copy, giant sun face, lone sunrise watcher, generic sunset, saccharine children, gold filter, steampunk or science-fiction machinery, or pseudo-rune ceremony.

**Full generation prompt.**

```text
Use case: illustration-story
Asset type: borderless inner illustration for an exact 7:12 portrait Tarot review candidate

Primary request:
Create a contemporary narrative fantasy Tarot illustration about direct light, vitality, conscious clarity, shared achievement, openness, and physically believable human joy. The image must depict a specific event rather than a symbolic portrait or generic sunrise.

Scene and event:
At sunrise after a severe winter, the inhabitants of a high mountain city have finally restored an ancient polished-bronze sun mirror mounted on the city wall. A dark-skinned girl apprentice, approximately thirteen to fifteen years old, with short tight curls, a small energetic build, a practical cream shirt, ochre work vest, and leather forearm guards, pulls the final stone-and-bronze lever. A concentrated band of direct sunlight crosses the scene and ignites a long-dark communal beacon or hearth in the central square below.

Beside her stands an older broad-built master mason with sun-weathered light skin, a short gray beard, one visibly work-damaged hand, and restrained dark-green work clothing. He watches with quiet pride rather than theatrical celebration. In the square and open galleries beyond, people of several ages emerge into the light, reopen shutters and gates, embrace, laugh, carry tools, and resume ordinary life. The joy must arise from a visible collective accomplishment.

Wolf-world motif:
Use restrained indirect wolf symbolism only: a subtle wolf-shaped bronze mounting on the mirror, a small civic pennant, or weathered stone heraldry. A living wolf is not required and must not become the focal point.

Style and medium:
Contemporary painted narrative fantasy illustration, modern RPG chapter art or high-quality story-card artwork, visually fresh and emotionally immediate. Natural varied digital brushwork, strong silhouettes, expressive but believable faces and hands, clear focal hierarchy, controlled detail. Distinct material rendering for skin, worn cloth, leather, polished bronze, pale stone, timber, snow remnants, smoke, and clear mountain air. The background must be quieter than the girl, lever, master, sunlight beam, and newly ignited beacon.

Composition and framing:
Exact 7:12 portrait intent. Cinematic medium-wide vertical composition. Place the apprentice and lever in the foreground or middle foreground. The master supports the scene without obscuring her decisive action. Use the sunlight beam as a strong diagonal leading from the restored mirror into the living city square. Show opening gates and inhabited depth. Keep all essential faces, hands, limbs, lever elements, and the beacon inside generous side margins. Leave calm safe areas near the top and bottom for future programmatic title and number overlays. The scene must remain readable at Tarot-card size.

Lighting and mood:
Bright, warm, clear, vigorous, communal, and grounded. Direct golden morning sunlight, cool clean post-winter air, natural bounced light from pale stone, readable shadows, and selective ivory highlights. Avoid white clipping and avoid a universal orange filter.

Color script:
Sunflower yellow, warm ivory, clear turquoise sky, terracotta, restrained fresh green, bronze, and cool natural shadows.

Constraints:
No text, letters, numbers, Roman numerals, title, frame, border, watermark, signature, logo, readable heraldic writing, pseudo-runes, or tattoo-like symbols. Plausible human anatomy, natural hands, age-appropriate faces, and mechanically believable simple stone-and-bronze construction. No futuristic technology and no elaborate steampunk machinery.

Avoid:
Rider-Waite child on a white horse, giant sun with a face, generic sunrise wallpaper, static heroic portrait, saccharine greeting-card children, religious cult tableau, generic wolf-calendar art, old-master painting, Baroque stiffness, faux-antique art, parchment filter, engraving, mosaic, crackle, tiled microtexture, universal surface noise, photorealism, 3D rendering, anime, and comic cel shading.
```

**Targeted corrective generation prompt (generation 2).**

```text
Use case: illustration-story
Asset type: targeted second text-only generation of a borderless exact 7:12 portrait Tarot review candidate; do not use or imitate any prior image

Primary request:
Create a contemporary painted narrative fantasy illustration about direct light, vitality, conscious clarity, shared achievement, and physically believable communal joy. The image must depict a specific restoration event, not a symbolic portrait or generic sunrise. Correct one critical staging requirement: all wolf symbolism must be small, restrained, and clearly subordinate. Do not show a large wolf head, large wolf sculpture, large wolf banner, living wolf, or dominant heraldic motif.

Scene and event:
At sunrise after a severe winter, inhabitants of a high mountain city have restored an ancient polished-bronze sun mirror on the city wall. A dark-skinned girl apprentice, thirteen to fifteen, with short tight curls, a small energetic build, practical cream shirt, ochre work vest, and leather forearm guards, pulls the final simple stone-and-bronze lever. A concentrated beam of direct sunlight crosses the scene diagonally and ignites a long-dark communal beacon in the square below.

Beside her stands an older broad-built master mason with sun-weathered light skin, a short gray beard, one visibly work-damaged hand, and restrained dark-green work clothing. He watches with quiet pride. In the square and open galleries, people of several ages emerge into light, reopen shutters and gates, embrace, carry tools, and resume ordinary life. Joy arises from this visible collective accomplishment.

Wolf-world motif:
At most one small abstract wolf-like contour may appear in a functional bronze bracket or weathered stone carving, smaller than the apprentice's hand and not immediately noticeable at thumbnail size. No large heraldry, animal head, sculpture, banner emblem, living wolf, or wolf focal point.

Style and medium:
Contemporary painterly narrative fantasy illustration, modern RPG chapter art or high-quality story-card artwork. Clearly painted, not photorealistic and not a 3D render. Natural varied brushwork, strong silhouettes, believable faces and hands, clear focal hierarchy, controlled detail. Distinct skin, worn cloth, leather, polished bronze, pale stone, timber, snow remnants, smoke, and mountain air. Quieter background.

Composition and framing:
Exact 7:12 portrait intent, final normalized target 952 by 1632 pixels. Cinematic medium-wide vertical composition. Apprentice and lever in foreground or middle foreground; master does not obscure her action. The sunlight beam is the strong diagonal into the living city square. Opening gates and inhabited depth. Keep essential faces, hands, limbs, lever, mirror, and beacon inside generous margins. Calm safe areas near top and bottom. Readable at Tarot-card size.

Lighting and color:
Bright clear morning, warm controlled beam, cool post-winter air, pale-stone bounce, natural contrast without white clipping. Sunflower yellow, warm ivory, clear turquoise sky, terracotta, restrained fresh green, bronze, cool natural shadows. No global orange, sepia, or universal texture filter.

Constraints:
No text, letters, numbers, Roman numerals, title, frame, border, watermark, signature, logo, readable heraldic writing, pseudo-runes, tattoos, extra fingers, or extra limbs. Plausible human anatomy and simple mechanically believable construction. No futuristic or steampunk technology.

Avoid:
Large wolf head or wolf sculpture, prominent wolf banner, Rider-Waite child on a white horse, giant sun face, generic sunrise wallpaper, static heroic portrait, saccharine greeting-card children, religious cult tableau, generic wolf-calendar art, old-master painting, Baroque stiffness, faux-antique art, parchment, engraving, mosaic, crackle, tiled microtexture, universal surface noise, photorealism, 3D rendering, anime, comic cel shading.
```

**Negative constraints.** No embedded typography; no border; no giant sun face; no white horse; no generic sunrise composition; no steampunk; no pseudo-runes; no accidental extra fingers or limbs; no antique or universal-texture treatment; no photoreal, 3D, anime, or cel shading.

- Production path: `cards/major/sun.png`.
- Generation method: built-in `imagegen`, text-only; no image reference.
- Actual dimensions: `952 × 1632 px` (exact `7:12`).
- SHA-256: `d4b0e233f966b60c9541184a59e7e591ffba3f0117902557e0832ad52342b034`.
- Generation count: **2** (one initial text-only generation and one targeted text-only regeneration; neither used an image reference).
- Technical review: The first generation made a large bronze wolf head a competing focal point, violating the restrained indirect-motif contract. The one allowed targeted regeneration reduced the motif to a small stone relief while preserving the apprentice's decisive lever action, the master, diagonal beam, lit communal beacon, opening gate, multi-age city response, post-winter color contrast, and distinct material treatment. In the selected candidate, faces, hands, limbs, mechanism, and background figures show no obvious technical malformation; no embedded text, pseudo-runes, universal texture, or living wolf is visible. The generated source was losslessly decoded and high-quality normalized from `958 × 1642` to the required `952 × 1632` review size.
- Owner acceptance: **Accepted**.

### `minor.swords.five`

**Researched meaning summary.** The archetype is possession of the field after a conflict whose human cost has destroyed relationship, dignity, and much of the victory's meaning. Its emotional center is the cold aftermath: humiliation, shame, estrangement, bitterness, and recognition that formal success may be real loss. Constructively it asks the viewer to see the price, refuse escalation, recognize an unwinnable conflict, restore responsibility, and stop cycles of humiliation and revenge. Its tensions are contempt, domination, cruelty, public humiliation, broken trust, and victory at any cost. Upright emphasizes conflict, defeat, dishonor, loss, damaged relationships, and hollow victory; reversed does not promise easy reconciliation but foregrounds continuing grief, consequences, and the work left after conflict.

**Card brief.**

- Narrative core: a young officer owns a strategic bridge after breaking a negotiation truce, but the withdrawing parties and civilians reveal that his victory has isolated him.
- Characters: a slim olive-skinned officer, twenty-five to thirty, narrow-faced with low-tied copper-auburn hair and a soaked burgundy coat; an older stocky deep-brown-skinned woman commander in blue-green; a tall light-brown-skinned shaved-head medic in gray supporting the injured or carrying a medical satchel. This casting is independent and unrelated to other cards.
- Event: rain has nearly stopped; the victor remains by a broken negotiation table while the other side leaves and civilians tend damage, move supplies, and avoid him. Nobody celebrates.
- Wolf motif mode: **Human with restrained wolf symbolism**, limited to a small silver clasp, torn pennant, or damaged bridge heraldry; no living wolf.
- Exact sword contract: exactly five countable swords—one point-down in the victor's hand, two ceremonial swords on the table, one on wet stone, and one broken sword by the bridge edge; no other swords or sword-like props.
- Framing: human-eye-level medium-wide portrait; victor slightly off-center; withdrawing figures form a diagonal; five swords and broken table remain readable without diagrammatic staging; quiet safe areas top and bottom.
- Lighting: cold post-rain light with a narrow muted-yellow evening break; localized wet-stone reflections; natural readable faces.
- Color script: wet slate, oxidized teal, burgundy, dull steel, muted ochre, natural skin tones, and restrained dirty-gold reflections; no gray-blue wash.
- Symbolic anchors: broken negotiation table, exactly five swords, occupied bridge, departing representatives, absent celebration, torn pennant, civilians handling consequences, and rain-wet stone.
- Forbidden clichés: no heroic duel, action climax, conqueror pose, villain grin, gore, corpse pile, generic battlefield, Rider–Waite copy, empty shoreline sword collector, or theatrical defeat.

**Full generation prompt.**

```text
Use case: illustration-story
Asset type: borderless inner illustration for an exact 7:12 portrait Tarot review candidate

Primary request:
Create a contemporary narrative fantasy Tarot illustration about conflict, humiliation, damaged trust, and a victory whose moral and human cost has made it nearly worthless. The battle is already over. The emotional center is the cold aftermath, not action spectacle.

Scene and event:
A rain-lashed stone bridge connects two districts of a fortified fantasy city after a brief civil clash. Negotiations had been taking place at a simple wooden table on the bridge, but a young officer broke the truce and seized the crossing. He now remains beside the damaged negotiation table while the defeated representatives leave.

The apparent victor is a slim man approximately twenty-five to thirty years old, with olive skin, a narrow face, straight copper-auburn hair tied low at the nape, and an expensive but rain-soaked and mud-stained burgundy officer's coat. A small silver wolf clasp is his only restrained clan symbol. His posture still contains the remains of confidence, but his expression shows the first recognition that nobody respects the victory.

An older stocky woman commander with deep brown skin, close-cropped gray hair, and restrained blue-green clothing walks away without theatrical collapse. A tall younger medic or attendant with light-brown skin, a shaved head, and practical gray clothing supports an injured person or carries a clearly medical satchel. Nearby civilians move supplies, tend damage, and avoid the victor. Nobody cheers.

Exact sword requirement:
Show exactly five clearly distinguishable swords and no more.
1. The victor holds one sword point-down.
2. Two surrendered ceremonial swords lie on the broken negotiation table.
3. One sword lies on the wet bridge stones.
4. One visibly broken sword lies near the edge of the bridge.
Do not place swords on the belts, backs, or hands of the retreating figures. Do not include sword-shaped spears, weapon racks, decorative blades, or additional background swords.

Wolf-world motif:
Use only restrained human and architectural symbolism: the victor's small wolf clasp, a torn clan pennant, or weathered wolf heraldry carved into the bridge. No living wolf is required.

Style and medium:
Contemporary painted narrative fantasy illustration, modern RPG story-card or chapter artwork. Natural varied digital brushwork, believable anatomy and gestures, clear focal hierarchy, controlled detail, emotionally readable faces. Distinct material rendering for wet stone, soaked wool, leather, steel, splintered wood, skin, rain, and torn fabric. Background consequences should support the story without overwhelming the central figures and five swords.

Composition and framing:
Exact 7:12 portrait intent. Human-eye-level cinematic medium-wide composition. Place the victor slightly off-center beside the broken table. Use the retreating people as a strong diagonal leading away from him. Make the exact five swords visually countable without arranging them like a diagram. Show that the conflict has ended and that the victor is becoming isolated. Keep all important faces, hands, limbs, table edges, and swords inside generous side margins. Leave calm safe areas near the top and bottom for future programmatic overlays. Readable at Tarot-card size.

Lighting and mood:
Cold, bitter, quiet, and morally uncomfortable. Rain has nearly stopped. A thin muted yellow break in the clouds contrasts with cold wet stone and oxidized teal architecture. Use subtle reflections, natural skin tones, and readable faces. Do not apply a universal gray-blue wash.

Color script:
Wet slate, oxidized teal, burgundy, dull steel, muted ochre, natural skin tones, and restrained dirty-gold reflections.

Constraints:
Exactly five swords. No text, letters, numbers, Roman numerals, title, frame, border, watermark, signature, logo, readable heraldic writing, pseudo-runes, or tattoo-like symbols. Natural human anatomy and hands. No graphic gore. The victor must be morally ambiguous and recognizably human, not a cartoon villain.

Avoid:
Heroic duel, active battle climax, triumphant conqueror pose, evil grin, mountain of corpses, generic medieval battlefield, direct Rider-Waite composition copy, empty shoreline sword-collector scene, melodramatic defeated poses, generic wolf-calendar art, old-master painting, Baroque stiffness, faux-antique art, parchment filter, engraving, mosaic, crackle, tiled microtexture, universal surface noise, photorealism, 3D rendering, anime, and comic cel shading.
```

**Negative constraints.** Exactly five swords; no sixth sword or sword-like object; no action battle; no gore; no cartoon villain; no heroic victory pose; no embedded text; no border; no antique texture; no global gray-blue wash; no photoreal, 3D, anime, or cel shading.

- Production path: `cards/minor/swords/five.png`.
- Generation method: built-in `imagegen`, text-only; no image reference.
- Actual dimensions: `952 × 1632 px` (exact `7:12`).
- SHA-256: `0fffd6ec8c95f9ded1fe566e46e9a2340190261e4e678ea3d17684a46e58a124`.
- Generation count: **1** (text-only; no image reference).
- Technical review: The first generation passed the exact inventory gate: one point-down sword is held by the officer, two surrendered swords lie on the broken table, one sword lies on wet stone, and one visibly broken sword lies at the bridge edge—exactly five, with no belt, background, rack, spear-like, or other additional blade visible. The post-conflict withdrawal, isolated victor, commander, medic with a medical satchel, civilian consequences, wet materials, and restrained heraldic setting are readable. No obvious malformed hands, faces, or limbs, embedded text, pseudo-runes, gore, active-battle staging, or universal texture was found. The generated source was losslessly decoded and high-quality normalized from `958 × 1642` to the required `952 × 1632` review size.
- Owner acceptance: **Accepted**.

### `major.moon`

**Researched meaning summary.** The archetype is reflected light, imagination, and an unknown path where fear and perception create convincing images that may not be true. Its emotional center is anxious ambiguity: a capable person must move without complete knowledge and separate external evidence from projection. Constructively it supports respect for uncertainty, careful movement, contact with imagination and instinct, refusal of premature certainty, and recognition of illusion. Its tensions are deception, self-deception, hidden danger, panic, projection, error, and fear of the unknown. Upright emphasizes uncertainty, illusion, hidden influences, imagination, and a path seen by reflected light; reversed is materially relevant as deception or instability beginning to weaken without instantly becoming complete clarity.

**Card brief.**

- Narrative core: an experienced apothecary must choose between a broad route whose reflection lies and a narrow route indicated by a trained dog's grounded instinct.
- Characters: a sturdy medium-brown-skinned woman, forty-five to fifty-five, broad-faced with a heavy black-and-silver braid, layered indigo travel cloak, and leather medicine satchel; a small anatomically realistic working hound. This is independent casting and unrelated to other cards.
- Event: in a flooded old quarter, the real bridge is broken while its water reflection appears intact; an extinguished lantern reflects as lit; the hound pulls toward the true narrow passage; one distant amber window gives the medicine journey a human goal.
- Wolf motif mode: **Wolf-world indirect / reflected spirit symbolism**; a wolf-like shape exists only in the water, with no physical wolf.
- Framing: cinematic medium-wide portrait with reflection-bearing foreground water; woman paused in decision; leash tension points to the real route; clues reveal themselves gradually; no classic towers; quiet safe areas top and bottom.
- Lighting: reflected silver moonlight, dim ambient illumination, readable face and hand, one warm destination window; material-specific lighting rather than a blue wash.
- Color script: deep violet, petroleum teal, moon silver, muted plum, charcoal, natural skin and leather, and one restrained amber window.
- Symbolic anchors: physically broken bridge, falsely complete reflected bridge, extinguished lantern with false light, hound leading to the actual path, water-only wolf shadow, medicine satchel, distant warm window, and water as distortion.
- Forbidden clichés: no giant moon and howling wolf, wolf pack, classic two towers, dog-and-wolf lineup, moon face, transparent-dress woman in water, passive mystic portrait, gothic wallpaper, monster attack, generic blue forest, occult clutter, or pseudo-runes.

**Full generation prompt.**

```text
Use case: illustration-story
Asset type: borderless inner illustration for an exact 7:12 portrait Tarot review candidate

Primary request:
Create a contemporary narrative fantasy Tarot illustration about reflected light, imagination, uncertainty, hidden danger, distorted perception, instinct, and the necessity of choosing a path without complete knowledge. This must be a specific human event, not a generic moon-and-wolf landscape.

Scene and event:
At night, an experienced traveling apothecary reaches a flooded abandoned quarter of an old fantasy city while carrying urgently needed medicine toward one distant warm window. She is a sturdy woman approximately forty-five to fifty-five years old, with medium-brown skin, a broad expressive face, long black hair streaked with silver and gathered into a heavy braid, a layered indigo travel cloak, practical boots, and a weathered leather medicine satchel.

She stops where the submerged street divides. In physical reality, the broad stone bridge ahead is broken and cannot be crossed. In the dark floodwater, however, its moonlit reflection appears falsely complete and inviting. A narrower side passage looks threatening and almost lightless, but a small realistic working hound pulls insistently toward it. The dog is alert rather than monstrous.

An extinguished floating lantern lies on the water, while its reflection appears faintly lit. A wolf-shaped shadow or spirit form exists only inside the water reflection and has no corresponding physical animal in the scene. Far beyond the flooded quarter, one restrained amber window shows the destination and the human reason for continuing.

The narrative moment is the apothecary's decision under incomplete and contradictory information. There is no attack and no visible villain.

Wolf-world motif:
Use reflected or indirect wolf symbolism only. The wolf form must appear solely as an ambiguous shape in the water or reflected shadow. Do not include a physical wolf or wolf pack.

Style and medium:
Contemporary painted narrative fantasy illustration, modern RPG chapter art or high-quality story-card artwork. Natural varied digital brushwork, cinematic staging, believable anatomy and clothing, expressive but controlled face and gesture, strong focal hierarchy. Render skin, wet cloth, leather, stone, timber, water, moonlit mist, dog fur, and distant glass as distinct materials. Reflections must be visually meaningful but not become abstract visual noise.

Composition and framing:
Exact 7:12 portrait intent. Cinematic medium-wide composition with enough foreground water to reveal the false reflected bridge, false lantern light, and ambiguous wolf-shaped reflection. Place the apothecary at the point of decision, with the hound creating visible directional tension toward the real narrow passage. The broken bridge must be physically readable above the water while its reflection appears deceptively intact. Keep the face, hands, medicine satchel, dog, bridge edges, and important reflection clues inside generous side margins. Leave calm safe areas near the top and bottom for future programmatic overlays. The image must remain readable at Tarot-card size and reward a closer second look.

Lighting and mood:
Uncertain, quiet, tense, dreamlike, and purposeful rather than horror-driven. Use reflected silver moonlight, deep ambient shadow, restrained mist, readable facial modeling, and one distant amber window. Moonlight must interact differently with skin, water, stone, cloth, leather, and fur rather than covering every surface with the same blue filter.

Color script:
Deep violet, petroleum teal, moon silver, muted plum, charcoal, natural skin and leather tones, and one restrained amber destination light.

Constraints:
No text, letters, numbers, Roman numerals, title, frame, border, watermark, signature, logo, readable occult writing, pseudo-runes, or tattoo-like symbols. Plausible human and dog anatomy. The dog must look like a normal working hound, not a wolf, werewolf, furry character, or fantasy monster. The wolf-shaped form must exist only as an ambiguous reflection.

Avoid:
Giant full moon behind a howling wolf, wolf pack landscape, classic two-tower Tarot composition, dog-and-wolf lineup, moon with a face, passive woman standing in water, transparent mystical dress, gothic wallpaper, monster attack, generic blue forest, occult-symbol clutter, dreamcatcher motifs, generic wolf-calendar art, old-master painting, Baroque stiffness, faux-antique art, parchment filter, engraving, mosaic, crackle, tiled microtexture, universal surface noise, photorealism, 3D rendering, anime, and comic cel shading.
```

**Targeted corrective generation prompt (generation 2).**

```text
Use case: illustration-story
Asset type: targeted second text-only generation of a borderless exact 7:12 portrait Tarot review candidate; do not use or imitate any prior image

Primary request:
Create a contemporary painted narrative fantasy illustration about distorted perception, reflected light, instinct, and choosing under incomplete knowledge. Correct three critical narrative requirements: the physical floating lantern is visibly extinguished and dark while only its reflection appears faintly luminous; exactly one warm light exists in the entire scene, a distant destination window; and the working hound visibly pulls hard toward the real narrow side passage while the apothecary pauses between paths.

Scene:
A flooded abandoned quarter of an old fantasy city at night. An experienced traveling apothecary, a sturdy woman forty-five to fifty-five with medium-brown skin, a broad expressive face, long black hair streaked silver in a heavy braid, layered indigo travel cloak, practical boots, and a weathered leather medicine satchel, has stopped at a submerged fork while carrying urgent medicine toward the one distant amber window.

The broad stone bridge ahead is physically and unmistakably broken across its walking surface, with a visible impassable gap. In the dark water below, its moonlit reflection deceptively appears complete. A small realistic working hound strains sideways on a taut leash toward a narrow intact passage partly hidden between ruined buildings. The woman's stance and gaze show a real moment of decision.

A battered floating lantern lies dark and extinguished on the water: no flame, no glowing glass, no emitted light. Only its reflection below appears faintly lit, an impossible misleading image. An ambiguous wolf-shaped shadow exists only in the water reflection and has no physical animal above it. The one distant amber window is the only warm light in the whole image. No other lit windows, torches, wall lamps, lanterns, fires, or warm glows.

Style:
Contemporary painterly narrative fantasy, modern RPG chapter art or high-quality story-card illustration, clearly painted rather than photoreal or 3D. Natural varied brushwork, believable human and dog anatomy, expressive controlled face and gesture, strong focal hierarchy. Distinct skin, wet cloth, leather, stone, timber, water, mist, dog fur, and glass. No universal texture or blue wash.

Composition:
Exact 7:12 portrait intent, final normalized target 952 by 1632 pixels. Cinematic medium-wide portrait. Foreground water clearly shows the false complete bridge, false lantern light, and water-only wolf shape. The woman stands at the decision point. The hound and taut leash create a strong diagonal toward the actual narrow passage. The broken physical bridge and deceptive complete reflection are both readable. Keep face, hands, satchel, dog, bridge edges, lantern, and reflection clues inside generous margins. Quiet safe areas at top and bottom. Readable at card size.

Lighting and color:
Reflected silver moonlight, deep violet and petroleum teal ambient shadow, muted plum, charcoal, natural skin and leather. Exactly one restrained amber destination window. The woman’s face and one hand remain readable. Different materials react differently to moonlight.

Constraints:
No physical wolf, no wolf pack, no attack, no visible villain. The dog is an ordinary working hound, not a wolf or monster. The wolf form appears solely as ambiguous reflection. The real lantern is dark; only its reflection glows. Exactly one warm light in the scene. No text, letters, numbers, title, frame, border, watermark, signature, logo, pseudo-runes, occult writing, tattoos, extra fingers, or extra limbs.

Avoid:
Giant moon behind a howling wolf, classic two-tower composition, dog-and-wolf lineup, moon face, passive mystic portrait, woman standing in water, gothic wallpaper, generic blue forest, occult clutter, dreamcatcher motifs, wolf-calendar art, old-master painting, Baroque stiffness, faux-antique art, parchment, engraving, mosaic, crackle, tiled microtexture, universal noise, photorealism, 3D rendering, anime, comic cel shading.
```

**Negative constraints.** No physical wolf; no wolf pack; no howling wolf under a giant moon; no classic two-tower composition; no generic blue forest; no horror attack; no embedded text; no occult pseudo-writing; no border; no global blue wash; no antique texture; no photoreal, 3D, anime, or cel shading.

- Production path: `cards/major/moon.png`.
- Generation method: built-in `imagegen`, text-only; no image reference.
- Actual dimensions: `952 × 1632 px` (exact `7:12`).
- SHA-256: `5bf8f1d8436249b5a14791794cf062181e4050556fe2685f9567d60550a79b50`.
- Generation count: **2** (one initial text-only generation and one targeted text-only regeneration; neither used an image reference).
- Technical review: The first generation incorrectly lit the physical floating lantern, introduced multiple warm lights, and did not make the hound's directional pull sufficiently clear. The one allowed targeted regeneration leaves the physical lantern dark while its reflection glows, limits warm light to one distant destination window, shows the impassable bridge gap, places the water-only wolf form in reflection, and makes the hound pull on a taut leash toward the side passage. Human face, hands, dog limbs, medicine equipment, water, stone, cloth, leather, and reflection hierarchy show no obvious technical malformation; no embedded text, pseudo-runes, physical wolf, attack, or repeated global texture was found. The generated source was losslessly decoded and high-quality normalized from `958 × 1641` to the required `952 × 1632` review size.
- Owner acceptance: **Accepted**.

## A2 Card Records (continued)

### `minor.cups.six`

**Researched meaning summary.** The base archetype is the past returning through memory, relationship, and a recognizable gift. Its tone is warm and tender. Constructively it supports kindness, trust, reunion, continuity between childhood and adulthood, and uncomplicated generosity. Its tension is nostalgia that edits away difficulty or prevents engagement with the present. Upright emphasizes memory, childhood, return, and giving; reversed meaning matters here as the pull between the vanished past and a future that cannot simply recreate it.

**Card brief.**

- Narrative core: a traveler returns to a living courtyard and recognizes home through a child's offered cup and a small wolf-carved keepsake.
- Characters: returning adult traveler, old friend or sibling, child, and background townspeople; people and gestures remain central.
- Event: the child offers a familiar painted cup containing flowers while the returning adult kneels to receive it; the older friend reaches toward them with restrained emotion.
- Wolf motif mode: **Human social scene with restrained wolf symbolism**.
- Framing: multi-character narrative at human eye level, with a readable triangular gesture relationship and lived-in background; safe quiet zones at top and bottom.
- Lighting: warm late-afternoon or early-evening sunlight through leaves and timber arcades.
- Color script: amber, warm wood, living green, cream, terracotta, and a clear blue or floral accent.
- Symbolic anchors: offered cup, flowers, repaired carved wolf toy, worn threshold, familiar courtyard details, a single young domestic wolf in the background at most.
- Forbidden clichés: no animal-family portrait, no field full of wolves, no sentimental greeting-card staging, no saccharine children, and no passive lineup.

**Full generation prompt.**

```text
Use case: illustration-story
Asset type: borderless inner illustration for a 7:12 portrait Tarot card review candidate
Primary request: Create a warm contemporary narrative fantasy scene about memory, childhood, return, kindness, trust, a familiar gift, and the fragile bridge between past and present.
Scene/backdrop: A lively village inn courtyard or garden street in the Lupus Noctis fantasy world, filled with warm late-afternoon light, timber galleries, terracotta plaster, vines, herbs, laundry, tables, and a glimpse of neighbors continuing ordinary life. The place feels inhabited and specific, not a generic empty landscape.
Subject and event: A travel-worn adult has just returned after years away and kneels as a child offers a familiar hand-painted cup filled with small garden flowers. The adult recognizes it and reaches with natural, emotionally readable hands; an older friend or sibling stands close, one hand half-raised in restrained welcome and disbelief. The child's other hand holds a repaired carved wooden wolf toy, a quiet symbol linking generations. A single young domestic wolf may rest or look up in the middle background, secondary to the people. Show several human ages and small background interactions so the place feels alive, while the central exchange remains unmistakable.
Style/medium: Contemporary painted narrative fantasy illustration, modern RPG story-card or high-quality game chapter art, visually fresh, natural varied digital brushwork, expressive but believable faces and gestures, strong silhouette grouping, clear focal hierarchy, controlled detail. Render skin, woven cloth, leather, painted ceramic, flowers, timber, plaster, leaves, and fur as distinct materials; background softer and less detailed than the central hands, faces, cup, and toy.
Composition/framing: Exact 7:12 portrait intent. Human-eye-level multi-character composition with the child, kneeling traveler, and standing friend forming a readable triangular gesture pattern. Keep all key faces and hands unobscured and inside generous margins. Leave calm safe areas near the top and bottom for a future programmatic title and number overlay. Readable at card size, with depth and an active lived-in place; not a posed family portrait.
Lighting/mood: Warm, bright, nostalgic, kind, and grounded rather than saccharine. Amber sun filters through green leaves and catches the offered cup, faces, and hands; gentle cooler blue accents keep the palette fresh.
Color palette: Amber, warm wood, living green, cream, terracotta, and clear blue or floral accents; no global dark filter and no gray-blue wash.
Constraints: No text, letters, numbers, Roman numerals, title, frame, border, watermark, signature, logo, written signs, or pseudo-runes. Natural hands, faces, child proportions, and wolf anatomy. Humans and their relationship are central; restrained wolf symbolism only.
Avoid: Animal-family portrait, crowd of wolves, generic wolf landscape, sentimental greeting-card sweetness, theatrical posing, old-master or Baroque painting, faux antique art, parchment filter, old-book or engraving treatment, mosaic, crackle, tiled or scale-like microtexture, universal surface noise, photorealism, 3D render, anime, comic cel shading, furry aesthetic.
```

**Negative constraints.** No embedded typography or border; no wolf-filled composition; no saccharine greeting-card pose; no anatomy errors; no antique, engraved, mosaic, crackle, repeated microtexture, or universal-noise treatment; no dark or gray-blue series filter; no photoreal, 3D, anime, or cel-shaded rendering.

- Production path: `cards/minor/cups/six.png`
- Actual dimensions: `952 × 1632 px` (exact `7:12`).
- SHA-256: `ad71eb1e48abe8155aa6272164607c3005d7e4ad08569856274fd3f49557c0d5`.
- Generation count: **1**.
- Technical review: The human reunion, offered flower cup, multi-age lived-in courtyard, warm color script, readable gestures, carved wolf toy, and secondary young wolf are present. No obvious extra limbs, malformed hands or faces, embedded text, furry treatment, or repeating global texture was found. Minor staging divergence: both the carved toy and one real young wolf are visible, making the motif more noticeable than the minimum brief, but the people and exchange remain the clear focus.
- Owner acceptance: **Accepted**.

### `major.star`

**Researched meaning summary.** The base archetype is guidance and renewed life after disruption. Its tone is open, calm, and hopeful. Constructively it supports healing, honest vulnerability, replenishment, inner direction, and confidence that growth can resume. Its tension is hope detached from action, disappointment, or diminished trust in the guiding light. Upright emphasizes hope, renewal, clarity, and freely restored life; reversed is visually relevant as blocked replenishment or expectation without follow-through.

**Card brief.**

- Narrative core: after a storm, an unarmored traveler restores a damaged spring and lets clean water return to the land while following a newly visible guiding star.
- Characters: one human healer or survivor in a clear practical action; a quiet real wolf or reflected wolf spirit is secondary.
- Event: the figure removes damaged armor, binds a healed or healing forearm, clears storm debris from the source, and relights a small guide lantern from reflected starlight as water begins to flow.
- Wolf motif mode: **Human with wolf spirit or wolf-world indirect**.
- Framing: spacious and airy portrait composition with an open horizon, clear action, generous breathing room, and safe quiet zones at top and bottom.
- Lighting: clear blue early morning or last stars after a passed storm, luminous water, soft gold on the figure, clean silver highlights.
- Color script: clean sky blue, turquoise, silver, soft gold, white, and small green or pink accents.
- Symbolic anchors: restored spring, removed broken armor, bandaged but capable hand, guiding star reflected in water, new green shoots, secondary wolf reflection or distant quiet wolf.
- Forbidden clichés: no dungeon, central threat, devotional icon pose, passive pin-up, cosmic wallpaper, or dominant wolf portrait.

**Full generation prompt.**

```text
Use case: illustration-story
Asset type: borderless inner illustration for a 7:12 portrait Tarot card review candidate
Primary request: Create a luminous contemporary narrative fantasy illustration about hope after upheaval, healing, honesty, replenishment, inner guidance, and vulnerable strength expressed through a clear practical action.
Scene/backdrop: An open highland spring beside a broad lake just after a storm, at the boundary between the last clear stars and early morning. Broken clouds retreat across a clean sky; wet stone, fresh grass, and small new plants catch the returning light. The horizon is spacious and alive, never a dark prison or threatening ruin.
Subject and event: A human survivor or healer kneels at the damaged spring, having set aside cracked armor and an unfastened shoulder guard. With natural visible hands, the figure clears storm debris from the source and guides clean turquoise water back into its channel; a simple bandage shows a healing forearm without melodrama. A small practical guide lantern beside the spring catches a soft golden glint from one clear guiding star reflected in the water. A quiet real wolf stands far enough away to remain secondary, or its calm spirit appears only as a subtle reflection in the flowing water. The figure is actively restoring life, not posing devotionally.
Style/medium: Contemporary painted fantasy illustration, modern RPG chapter art or high-quality game loading-screen narrative art, visually fresh, emotionally immediate, natural varied digital brushwork, strong but graceful silhouette, clear focal hierarchy, controlled detail. Render skin, wet hair, linen bandage, leather, damaged metal, translucent water, stone, grass, clouds, and wolf fur as distinct materials; background softer and less detailed than the hands, face, spring, and reflected light.
Composition/framing: Exact 7:12 portrait intent. Spacious, airy composition with an open horizon and a clear diagonal flow from the guiding star through the figure's action into the renewed stream. Keep the full essential figure, both hands, removed armor, spring, and secondary wolf motif inside generous margins. Leave calm safe areas near the top and bottom for a future programmatic title and number overlay. Luminous and readable at card size; not an icon, portrait, or cosmic wallpaper.
Lighting/mood: Bright, clear, hopeful, quiet, and restorative after the storm. Clean sky-blue ambient light, turquoise water glow, silver wet highlights, soft gold on the figure and lantern, white cloud edges, tiny green and pale-pink life accents.
Color palette: Clean sky blue, turquoise, silver, soft gold, white, with small green or pale-pink accents; avoid gray-blue monotony by maintaining clean warm/cool separation.
Constraints: No text, letters, numbers, Roman numerals, title, frame, border, watermark, signature, logo, constellational lettering, or pseudo-runes. Natural hands, face, body, and wolf anatomy. Wolf motif remains secondary. The scene must show recovery through action and calm openness, with no central threat.
Avoid: Religious icon pose, pin-up pose, dark dungeon, tragic danger at the focal point, cosmic wallpaper, dominant wolf portrait, generic wolf calendar art, old-master or Baroque painting, faux antique art, parchment filter, old-book or engraving treatment, mosaic, crackle, tiled or scale-like microtexture, universal surface noise, photorealism, 3D render, anime, comic cel shading, furry aesthetic.
```

**Targeted correction prompt (generation 2).**

```text
Use case: precise-object-edit
Asset type: targeted technical correction to the previously generated borderless 7:12 Tarot card illustration
Primary request: Remove only the large circular tattoo, rune-like ornament, and all decorative markings from the exposed upper arm and shoulder of the kneeling human figure. Replace them with natural unmarked skin that matches the existing anatomy, lighting, moisture, and painterly rendering.
Invariants: Preserve the exact composition, crop, pose, identity, face, hair, both hands and fingers, bandage, clothing, cracked armor, helmet, spring, water flow, lantern, wolf, mountains, lake, sky, guiding star, colors, lighting, material separation, depth, and all other details. Do not redesign, restyle, add, move, or remove anything else.
Constraints: The corrected shoulder and upper arm must have no tattoo, letters, numbers, symbols, pseudo-runes, scar pattern, or decorative paint. Keep natural anatomy and skin texture. No text, title, frame, border, watermark, or signature anywhere.
```

**Negative constraints.** No embedded typography or border; no religious-icon or pin-up pose; no central threat or dominant wolf; no anatomy errors; no antique, engraved, mosaic, crackle, repeated microtexture, or universal-noise treatment; no gray-blue monotony; no photoreal, 3D, anime, or cel-shaded rendering.

- Production path: `cards/major/star.png`
- Actual dimensions: `952 × 1632 px` (exact `7:12`).
- SHA-256: `79cb0c40e926c2acafffd80fa622385b5a6d7534518d8050114027bb7be5ee95`.
- Generation count: **2** (one initial generation and one targeted technical correction).
- Technical review: The initial generation contained a large circular tattoo-like pseudo-symbol on the exposed shoulder, violating the no-pseudo-runes constraint. A single targeted edit removed only that marking. The final candidate retains the restorative action, open horizon, clear guiding star, removed armor, healing bandage, secondary wolf, bright tonal script, and material separation; no obvious extra limbs, malformed hands or face, embedded text, central threat, or repeating global texture was found after correction.
- Owner acceptance: **Accepted**.
