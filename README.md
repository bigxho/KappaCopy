# Kappa Copy v0.2.0

Desktop app in C# / .NET 8 / Avalonia UI 12.1.0 that uses Windows `robocopy.exe` as the copy engine.
<img width="981" height="790" alt="KappaCopy" src="https://github.com/user-attachments/assets/ce4f51ca-38a2-48ba-bbbf-388e233fc54f" />

## Current features

- Multiple file selection
- Multiple folder selection
- Mixed files + folders in one copy job
- Destination folder picker
- Automatic / Fast / Safe profiles
- Robocopy process hidden from the user
- Live Robocopy log
- Overall progress approximation and item counters
- Total source size calculation
- Elapsed time
- Cancellation (terminates the Robocopy process tree)
- Robocopy exit-code handling (`0..7` non-fatal, `>=8` error)
- Prevents choosing a destination inside a selected source folder

## Requirements

- Windows 10 or Windows 11
- .NET 8 SDK for development
- `robocopy.exe` (included with Windows)

## Build

Open a terminal in the project root:

```powershell
dotnet restore
dotnet build KappaCopy.sln -c Debug
```

Run:

```powershell
dotnet run --project .\src\KappaCopy.App\KappaCopy.App.csproj
```

## Publish a Windows x64 executable

Framework-dependent:

```powershell
dotnet publish .\src\KappaCopy.App\KappaCopy.App.csproj -c Release -r win-x64 --self-contained false -o .\publish\win-x64
```

Self-contained:

```powershell
dotnet publish .\src\KappaCopy.App\KappaCopy.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish\win-x64-selfcontained
```

## Copy profiles

### Automatic
`/MT:8 /COPY:DAT /DCOPY:DAT /R:2 /W:1 /TEE /BYTES /ETA`

### Fast
Automatic options plus `/MT:16 /J`.

### Safe
Uses `/MT:8 /Z` for restartable copy.

`/MIR` is intentionally not used in v0.1 because it can delete destination content.

## Architecture

- `KappaCopy.App`: Avalonia desktop UI
- `KappaCopy.Core`: copy job/progress/result models
- `KappaCopy.Engine`: platform-independent engine contract
- `KappaCopy.Engine.Windows`: Robocopy implementation

A future macOS engine can implement `ICopyEngine` without changing the core UI model.
