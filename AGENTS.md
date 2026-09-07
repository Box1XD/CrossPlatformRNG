# AGENTS.md — RandomNumberAvalonia

## Build & Test

```powershell
# Build the solution
dotnet build RandomNumberAvalonia.sln

# Run all tests (two separate projects)
dotnet test RandomNumberAvalonia.Core.UnitTests
dotnet test RandomNumberAvalonia.UnitTests
```

Publishing (Desktop, self-contained win-x64):
```powershell
dotnet publish RandomNumberAvalonia.Desktop -c Release -r win-x64 --self-contained true
```

## Project Map

| Project | What |
|---|---|
| `RandomNumberAvalonia.Core` | Library — `IRandomNumberGenerator`, `StrongRandomNumberGenerator`, `FakeRandomNumberGenerator` |
| `RandomNumberAvalonia` | Avalonia UI — Views, ViewModels, Services, Converters, App.axaml, ViewLocator |
| `RandomNumberAvalonia.Desktop` | Windows Desktop exe entry point |
| `RandomNumberAvalonia.Core.UnitTests` | xUnit tests for Core (RNG speed benchmarks) |
| `RandomNumberAvalonia.UnitTests` | xUnit tests for ViewModels |

## UI Structure (mobile-style)

- `MainView` = shell: content area + bottom navigation (首页 / 历史 / 设置), switching via `MainViewModel.CurrentPage` (`AppPage` enum) + `EnumToBoolConverter`.
- Pages: `HomePageView` (random number + min/max only), `SettingsPageView` (all other settings), `HistoryPageView` (sessions grouped per app launch). All bind to the same `MainViewModel`.
- Responsive: content max-width ~520–560 centered, big number in a `Viewbox`; works in 16:9 and 9:16.

## Persistence (`RandomNumberAvalonia/Services`)

- `AppSettings` — config.json in `%APPDATA%\RandomNumberAvalonia`, loaded at startup, auto-saved on any setting change (`SaveSettings()` in `MainViewModel`).
- `HistoryStore` — history.json, one session per app launch, one entry per generated number; appended + saved on every generation.
- `MainViewModel()` parameterless ctor = no file I/O (used by designer & unit tests); `App.axaml.cs` passes real services.

## Key Conventions

- **Central Package Management**: NuGet versions live in `Directory.Packages.props`. `.csproj` files reference packages *without* a version attribute.
- **MVVM via CommunityToolkit.Mvvm**: Use `[ObservableProperty]` and `[RelayCommand]` source generators. Base class is `ViewModelBase : ObservableValidator`.
- **ViewLocator convention**: Replaces `"ViewModel"` with `"View"` in the type name to resolve views. The pattern is `*ViewModel` → `*View`.
- **Compiled bindings**: `AvaloniaUseCompiledBindingsByDefault` is `true` in `RandomNumberAvalonia.csproj`.

## RNG Gotchas

- `Next()` is **synchronous** (`int Next()`) — RNG is a pure CPU op, no async needed.
- `StrongRandomNumberGenerator` uses `RandomNumberGenerator.GetInt32` → range is **exclusive** of min: `(min, max)`. Lock-free — the static method is thread-safe.
- `FakeRandomNumberGenerator` uses `Random.Shared.Next` → range is **inclusive** of min: `[min, max)`. Lock-free — `Random.Shared` is thread-safe.
- Both are `Lazy<T>` singletons.

## Avalonia App Startup

- Desktop-only: `Program.cs` → `App.axaml.cs` creates `MainWindow` with `MainViewModel` as `DataContext`. Removes duplicate data validation binding plugin.
