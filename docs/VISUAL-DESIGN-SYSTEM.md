# Visual Design System

| Metadata | Definition |
| --- | --- |
| Role | Stable visual design-system owner for Nox Aeterna. |
| Read when | Choosing, implementing, or reviewing colors, chart hierarchy, component states, contrast, or visual effects. |
| Authoritative for | Semantic color roles, paired dark/light palettes, chart visual hierarchy, component-state colors, contrast principles, allowed and forbidden effects, and palette evolution rules. |
| Not authoritative for | Layout geometry, astronomy, architecture dependencies, current project status, or implementation chronology. |

## Direction

The working palette direction is **Astral Archive**. This is an internal design direction, not a product name or user-facing brand.

The visual system describes a modern occult archive through deep cool neutrals, an amethyst primary accent, an astral-cyan secondary accent, and sparing solar gold. It must remain a calm desktop instrument: atmospheric without becoming game-like, modern without neon, and archival without sepia dominance.

Dark and light themes share semantic roles and hue families, but never assume identical RGB values. Each theme owns tones and luminance suited to its background.

## Application Semantic Palette

### Dark — Obsidian

| Token | Value |
| --- | --- |
| Canvas | `#090D18` |
| Surface | `#11182A` |
| SurfaceRaised | `#18213A` |
| Border | `#2B3552` |
| TextPrimary | `#F5F2FF` |
| TextSecondary | `#A9B0CB` |
| TextMuted | `#727B9A` |
| AccentPrimary | `#A78BFA` |
| AccentPrimaryStrong | `#8B5CF6` |
| AccentSecondary | `#45C7D9` |
| SolarAccent | `#F2C14E` |
| Success | `#3DDC97` |
| Warning | `#F2A640` |
| Error | `#FF667A` |

### Light — Porcelain

| Token | Value |
| --- | --- |
| Canvas | `#F7F5FC` |
| Surface | `#FFFFFF` |
| SurfaceRaised | `#EEEAF7` |
| Border | `#D6D1E3` |
| TextPrimary | `#1C1930` |
| TextSecondary | `#67617B` |
| TextMuted | `#8A849A` |
| AccentPrimary | `#7357D9` |
| AccentPrimaryStrong | `#5E3FC4` |
| AccentSecondary | `#147F91` |
| SolarAccent | `#8C5A00` |
| Success | `#137A55` |
| Warning | `#8C5A00` |
| Error | `#B8344D` |

The Avalonia dark/light dictionaries expose these values as paired `Design*Color` and `Design*Brush` resources. V2 applies those roles across the application shell and controls; Avalonia topology and style ownership belong to [`THEMES.md`](THEMES.md).

## Component States

### Dark — Obsidian

| Token | Value |
| --- | --- |
| SurfaceSunken | `#0D1424` |
| BorderStrong | `#3A476A` |
| AccentPrimarySoft | `#2E2759` |
| AccentSecondarySoft | `#123A46` |
| ControlFill | `#0D1424` |
| ControlFillHover | `#151F35` |
| ControlFillPressed | `#1D2944` |
| DisabledFill | `#141A29` |
| DisabledText | `#68718E` |
| SelectionForeground | `#FFFFFF` |
| FocusRing | `#45C7D9` |

### Light — Porcelain

| Token | Value |
| --- | --- |
| SurfaceSunken | `#F2EFF8` |
| BorderStrong | `#B7AEC9` |
| AccentPrimarySoft | `#E9E3FF` |
| AccentSecondarySoft | `#DDF3F6` |
| ControlFill | `#FFFFFF` |
| ControlFillHover | `#F1EDF9` |
| ControlFillPressed | `#E5DFF1` |
| DisabledFill | `#EEEAF3` |
| DisabledText | `#9A93A8` |
| SelectionForeground | `#FFFFFF` |
| FocusRing | `#147F91` |

- Default content uses `Surface`, `Border`, `TextPrimary`, and `TextSecondary`.
- Raised or hoverable regions may move to `SurfaceRaised`; hover must remain restrained and must not simulate illumination.
- Selected state uses `AccentPrimarySoft` with a restrained `AccentPrimaryStrong` indicator; primary actions use `AccentPrimaryStrong`.
- Keyboard focus uses `FocusRing`. Exceptional celestial markers may use `SolarAccent`; solar gold is not a general focus or decoration color.
- Informational secondary emphasis uses `AccentSecondary`.
- Validation and operation states use `Success`, `Warning`, and `Error`; do not substitute these roles for decorative accents.
- Disabled controls use the explicit `DisabledFill` and `DisabledText` pair and must not look like active data.

## Chart Palette V1

Chart colors are renderer-owned semantic roles. The chart palette follows the application hue families while using chart-specific tones against its own interior.

### Dark chart

| Role | Value |
| --- | --- |
| InteriorBackground | `#0B1020` |
| PrimaryStructure | `#7F8AA8` |
| SecondaryStructure | `#3E4865` |
| ZodiacGlyph | `#FFF2C6` |
| PlanetGlyph | `#F8E8FF` |
| PlanetDegree | `#C9B8EA` |
| PlanetAnchor | `#8FA4C7` |
| HouseCusp | `#66718F` |
| HouseLabel | `#D8D3E8` |
| PrincipalAxis | `#F2C14E` |
| AspectCircle | `#48516D` |
| Conjunction | `#D8D3E8` |
| HardAspect | `#FF6275` |
| HarmoniousBlue | `#4EB7E8` |
| HarmoniousTeal | `#45D1A6` |

| Element | Sector fill |
| --- | --- |
| Fire | `#7A2F43` |
| Earth | `#285E4B` |
| Air | `#245A73` |
| Water | `#403777` |

### Light chart

| Role | Value |
| --- | --- |
| InteriorBackground | `#FAF9FE` |
| PrimaryStructure | `#5B5870` |
| SecondaryStructure | `#C8C3D5` |
| ZodiacGlyph | `#2C213C` |
| PlanetGlyph | `#2A1D3D` |
| PlanetDegree | `#615374` |
| PlanetAnchor | `#6A7088` |
| HouseCusp | `#8A8398` |
| HouseLabel | `#4A425D` |
| PrincipalAxis | `#8C5A00` |
| AspectCircle | `#C1BBCD` |
| Conjunction | `#514A60` |
| HardAspect | `#C43F5A` |
| HarmoniousBlue | `#1E8DB4` |
| HarmoniousTeal | `#20866A` |

| Element | Sector fill |
| --- | --- |
| Fire | `#E77886` |
| Earth | `#5BB187` |
| Air | `#58A9D8` |
| Water | `#8574D8` |

V1 sector fills are final theme-specific colors at full opacity. Zodiac glyphs use one high-contrast neutral/solar role rather than per-element glyph colors. Planet glyphs are the dominant annotation layer; degree and retrograde text are quieter but readable, while source ticks and connectors remain cool and subordinate.

Aspect semantics are theme-aware:

- square and opposition use `HardAspect`;
- trine uses `HarmoniousBlue`;
- sextile uses `HarmoniousTeal`;
- conjunction uses `Conjunction`.

Line weight, placement, and marker form accompany color so color is not the only carrier of aspect meaning.

## Chart Visual Hierarchy

From background to foreground:

1. Chart interior background.
2. Zodiac sector fills.
3. Outer and inner zodiac structure and separators.
4. House cusps with annotation intervals physically omitted.
5. Principal axes with annotation intervals physically omitted.
6. Aspect circle, aspect lines, and endpoint markers.
7. Planet source ticks and displaced connectors, occluded by protected annotation bounds.
8. Transparent planet vector glyphs.
9. Transparent degree and retrograde text.
10. Zodiac glyphs.
11. House numbers.
12. ASC, DSC, MC, and IC labels.

Planet annotation envelopes are invisible measurement contracts. They may control collision, line occlusion, connector termination, and viewport validation, but must never materialize as rectangles, rounded rectangles, circles, pills, cards, shadows, or background-colored patches.

## Contrast

For normal UI text:

- primary text targets at least `7:1` contrast where practical;
- secondary text targets at least `4.5:1`;
- disabled text may be lower, but must be visually distinct from active data.

For charts:

- planet glyphs and degree labels must remain readable at compact size;
- structural lines are not text and need not meet text contrast ratios;
- color is not the sole carrier of aspect semantics;
- line weight and radial location preserve hierarchy;
- dark and light screenshots are required acceptance evidence.

Contrast checks should stay small and deterministic. Do not introduce a general color framework solely to calculate these ratios.

## Effects

Allowed:

- flat semantic fills;
- deliberate line-weight hierarchy;
- restrained opacity for secondary structure;
- small corner radii on real controls and panels;
- crisp vector paths and deterministic line interruptions.

Forbidden:

- neon treatment;
- glow, bloom, or blur;
- decorative chart shadows;
- background shapes under planet annotations;
- dirty pastel or sepia-dominant hierarchy;
- brown-on-beige as the primary UI language;
- fantasy-game ornament;
- using color alone to convey state or aspect type.

## Palette Evolution

- Name colors by role, never by appearance (`Brown1`, `Purple2`, and similar names are invalid).
- Change paired dark/light roles together and assess each against its own background.
- Keep chart and application roles related but independent; chart contrast may require different tones.
- Preserve the semantic mapping of elements, aspects, state colors, and solar emphasis.
- Focused chart tuning may adjust hue or lightness by roughly 5–8% only when smoke evidence shows insufficient contrast, color collision, or layer dominance.
- Update this document, implementation tokens, focused tests, and dark/light acceptance screenshots in the same visual-system change.
- Keep application resource topology and Fluent integration details in `THEMES.md`; keep exact values here and in the paired dictionaries.
