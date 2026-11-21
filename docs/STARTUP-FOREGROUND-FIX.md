# Startup foreground fix

The startup log identified a WPF resource type mismatch:
`C.Text` was declared as a `Color` but used as a `TextBlock.Foreground` / `Window.Foreground` value.
Those properties require a `Brush`.

The fix preserves `C.Text` as a `Color` and introduces `TextBrush` as a `SolidColorBrush`,
then updates the implicit Window and TextBlock styles to use `TextBrush`.

Rebuild on Windows:
```powershell
dotnet clean .\VexoJumper.sln
dotnet restore .\VexoJumper.sln
dotnet build .\VexoJumper.sln -c Release
.\build-and-publish.ps1 -SkipRun
```
