# Foreground resource type correction

The supplied startup log still reports `#FFF2F7FF` as an invalid Foreground value.
This patch changes `C.Text` itself from `Color` to `SolidColorBrush`, so any remaining
`{StaticResource C.Text}` reference is valid for Foreground. It also clears the project's
`bin` and `obj` directories before rebuilding to prevent stale generated WPF resources.

Extract this ZIP into a NEW directory. Do not merge it over the old folder.
Then run:
```powershell
Set-ExecutionPolicy -Scope Process Bypass -Force
.\build-and-publish.ps1 -SkipRun
```
