# Third-Party Tracking

Track every external dependency, asset source, borrowed component, dataset, font, rendering system, tool, and generated asset source that materially enters the project.

This file is intentionally future-ready. If a category is unused yet, leave it empty rather than guessing.

## Tracking Rules

For each entry, record:

- Name
- Author or organization
- License
- Purpose in project
- Official repository or site
- Notes, including attribution or packaging concerns

## Planned Dependencies

| Name | Author or organization | License | Purpose in project | Official repository or site | Notes |
| --- | --- | --- | --- | --- | --- |
| .NET 10 SDK | Microsoft | To be verified at adoption time | Core SDK and runtime | https://dotnet.microsoft.com/ | Planned, not yet added as a tracked runtime dependency in code |
| Avalonia | Avalonia UI contributors | MIT | Cross-platform desktop UI base package for the desktop app shell and controlled rendering layer | https://github.com/AvaloniaUI/Avalonia | Added in `NoxAeterna.App` and `NoxAeterna.Rendering`, version `12.0.2` |
| Avalonia.Desktop | Avalonia UI contributors | MIT | Desktop lifetime and platform integration for the app shell | https://www.nuget.org/packages/Avalonia.Desktop | Added in `NoxAeterna.App`, version `12.0.2` |
| Avalonia.Themes.Fluent | Avalonia UI contributors | MIT | Minimal application theme for the Avalonia shell | https://docs.avaloniaui.net/docs/basics/user-interface/styling/themes/fluent | Added in `NoxAeterna.App`, version `12.0.2` |
| CommunityToolkit.Mvvm | Microsoft | To be verified at adoption time | MVVM support | https://github.com/CommunityToolkit/dotnet | Planned |
| NodaTime | The Noda Time Authors | Apache-2.0 | Time modeling and timezone handling | https://nodatime.org/ | Added in `NoxAeterna.Domain` and `NoxAeterna.Astronomy`, version `3.3.2` |
| Dapper | Dapper contributors | To be verified at adoption time | Lightweight data access | https://github.com/DapperLib/Dapper | Planned |
| Serilog | Serilog contributors | To be verified at adoption time | Logging | https://github.com/serilog/serilog | Planned |
| Microsoft.NET.Test.Sdk | Microsoft | MIT | Test host integration for `dotnet test` | https://github.com/microsoft/vstest | Added in `NoxAeterna.Tests`, version `17.14.1` |
| xunit | xUnit contributors | Apache-2.0 | Unit test framework | https://github.com/xunit/xunit | Added in `NoxAeterna.Tests`, version `2.9.3` |
| xunit.runner.visualstudio | xUnit contributors | Apache-2.0 | VSTest adapter for `dotnet test` and IDE test runners | https://github.com/xunit/visualstudio.xunit | Added in `NoxAeterna.Tests`, version `3.1.4` |
| SwissEphNet | Yan Grenier | Package embeds the upstream Swiss Ephemeris dual-license notice (GPL-2.0-or-later or professional license) | First real .NET ephemeris adapter package used behind `IEphemerisCalculator` | https://github.com/ygrenier/SwissEphNet | Added in `NoxAeterna.Infrastructure`, version `2.8.0.2`. Last NuGet update is from 2019. The package is a managed C# port and currently runs without native DLL setup. |
| Swiss Ephemeris | Astrodienst AG; authors Dieter Koch and Alois Treindl | Official documentation currently describes dual licensing via AGPL or Swiss Ephemeris Professional License | Upstream astronomical calculation engine underlying the wrapper | https://www.astro.com/swisseph-download | Current app spike uses SwissEphNet with built-in Moshier fallback because external `.se1` files are not configured yet. Exact project-license alignment and redistribution terms for any bundled data files remain open and must be resolved before release packaging. |

## Assets, Fonts, and Generated Material

Add future entries here for:

- Tarot illustrations
- Textures
- Decorative engravings
- Fonts
- Icon sets
- Generated imagery
- Adapted historical material

No assets are locked yet.
Project-owned application assets should live in the repository once selected and curated. User-specific runtime data such as saved charts, preferences, caches, and local history belong in AppData instead.

Do not commit:

- raw AI-generation dumps;
- temporary export batches;
- unverified internet images;
- unlicensed fonts or icon packs.

No custom assets or fonts are locked yet.
