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
| Avalonia | Avalonia UI contributors | MIT | Cross-platform desktop UI base package for the desktop app shell | https://github.com/AvaloniaUI/Avalonia | Added in `NoxAeterna.App`, version `12.0.2` |
| Avalonia.Desktop | Avalonia UI contributors | MIT | Desktop lifetime and platform integration for the app shell | https://www.nuget.org/packages/Avalonia.Desktop | Added in `NoxAeterna.App`, version `12.0.2` |
| Avalonia.Themes.Fluent | Avalonia UI contributors | MIT | Minimal application theme for the Avalonia shell | https://docs.avaloniaui.net/docs/basics/user-interface/styling/themes/fluent | Added in `NoxAeterna.App`, version `12.0.2` |
| CommunityToolkit.Mvvm | Microsoft | To be verified at adoption time | MVVM support | https://github.com/CommunityToolkit/dotnet | Planned |
| NodaTime | Noda Time contributors | To be verified at adoption time | Time modeling and timezone handling | https://nodatime.org/ | Planned |
| Dapper | Dapper contributors | To be verified at adoption time | Lightweight data access | https://github.com/DapperLib/Dapper | Planned |
| Serilog | Serilog contributors | To be verified at adoption time | Logging | https://github.com/serilog/serilog | Planned |
| Microsoft.NET.Test.Sdk | Microsoft | MIT | Test host integration for `dotnet test` | https://github.com/microsoft/vstest | Added in `NoxAeterna.Tests`, version `17.14.1` |
| xunit | xUnit contributors | Apache-2.0 | Unit test framework | https://github.com/xunit/xunit | Added in `NoxAeterna.Tests`, version `2.9.3` |
| xunit.runner.visualstudio | xUnit contributors | Apache-2.0 | VSTest adapter for `dotnet test` and IDE test runners | https://github.com/xunit/visualstudio.xunit | Added in `NoxAeterna.Tests`, version `3.1.4` |
| SwissEphNet or equivalent | To be verified | To be verified | Ephemeris access | To be verified | Package choice still open |

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
