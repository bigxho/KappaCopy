# Kappa Copy

**Kappa Copy** is a free and open-source desktop file-copy utility built with **C#**, **.NET 8**, and **Avalonia UI**.

On Windows, Kappa Copy uses the native **Robocopy** engine to provide fast and reliable file and directory transfers through a clean graphical interface and Windows Explorer integration.

The project is also intended as a practical codebase for developers who want to learn about .NET desktop development, Avalonia UI, process execution, filesystem operations, Windows Shell integration, deployment, and cross-platform software architecture.

> **Project status:** Active development / early release.  
> The current primary platform is Windows. macOS and Linux support are planned through additional copy-engine implementations.

---

## Why Kappa Copy?

Kappa Copy explores a simple idea: combine proven operating-system copy engines with a modern, approachable desktop interface.

The project aims to:

- use efficient native copy engines where appropriate;
- provide a simple graphical workflow;
- expose useful transfer information and errors;
- integrate with the operating system's file manager;
- keep destructive behavior disabled by default;
- separate the UI from platform-specific copy engines;
- provide a real-world open-source project that developers can study, modify, and improve.

Kappa Copy is not intended to replace Robocopy. On Windows, it provides a graphical and workflow-oriented layer around it.

---

## Features

Current functionality includes:

- Individual file copying
- Multiple file copying
- Directory copying
- Multiple directory copying
- Mixed file and directory selections
- Windows Robocopy engine
- Multithreaded copy profiles
- Transfer progress
- Current-operation status
- Elapsed time
- Robocopy output and logging
- Copy cancellation
- Completion notification sound
- Enable/disable completion sound
- Persistent user preferences
- Internal Kappa Copy clipboard
- Windows Explorer context-menu integration
- `Kappa Copy`
- `Kappa Paste Here`
- Self-contained Windows publishing
- Inno Setup installer support

---

## Windows Explorer Integration

Kappa Copy integrates with Windows Explorer.

Select one or more supported filesystem items:

```text
Right Click
    ↓
Kappa Copy
```

Navigate to the destination:

```text
Right Click
    ↓
Kappa Paste Here
```

Kappa Copy opens with the selected sources and destination prepared for transfer.

The application uses its own temporary clipboard, so normal Windows `Ctrl+C` / `Ctrl+V` operations remain independent.

> On Windows 11, the current static context-menu integration may appear under **Show more options**. Native Windows 11 context-menu integration is a future goal.

---

## Copy Profiles

Kappa Copy currently provides three profiles.

### Automatic

Balanced default configuration.

Typical Robocopy options:

```text
/COPY:DAT
/DCOPY:DAT
/R:2
/W:1
/MT:8
```

### Fast

Designed for high-throughput storage such as SSDs, NVMe drives, fast external storage, and suitable network/storage configurations.

Typical additional options:

```text
/MT:16
/J
```

### Safe

Designed for situations where restartable copying is preferred.

Typical additional options:

```text
/MT:8
/Z
```

These profiles may evolve as the project gains adaptive storage detection and benchmarking.

---

## Safety by Default

Kappa Copy intentionally avoids destructive Robocopy modes in its standard copy workflow.

For example, it does **not** enable:

```text
/MIR
```

by default.

A normal copy operation should not unexpectedly delete unrelated files from the destination.

Users should still maintain backups of important data and review source and destination paths before large transfers.

---

## Robocopy Exit Codes

Robocopy uses exit codes differently from many command-line applications.

In general:

```text
0-7    Success or non-fatal result
8+     At least one copy failure occurred
```

Kappa Copy accounts for these semantics rather than treating every non-zero Robocopy exit code as a fatal failure.

---

## Technology Stack

- **C#**
- **.NET 8**
- **Avalonia UI 11.3**
- **Robocopy** on Windows
- **Windows Shell / Registry integration**
- **Inno Setup**
- **Git / GitHub**

Current target framework:

```text
net8.0
```

Current Windows deployment target:

```text
win-x64
```

---

## Architecture

The project separates shared application logic from platform-specific copy engines.

```text
KappaCopy/
│
├── src/
│   ├── KappaCopy.App/
│   │   └── Avalonia desktop UI and application startup
│   │
│   ├── KappaCopy.Core/
│   │   └── Shared models and copy-domain logic
│   │
│   ├── KappaCopy.Engine/
│   │   └── Copy-engine abstractions
│   │
│   └── KappaCopy.Engine.Windows/
│       └── Windows Robocopy implementation
│
├── installer/
│   └── Inno Setup configuration
│
├── KappaCopy.sln
├── global.json
└── README.md
```

Conceptually:

```text
Avalonia UI
    │
    ▼
ICopyEngine
    │
    ├── RobocopyEngine       Windows
    │
    ├── Future macOS Engine
    │
    └── Future Linux Engine
```

Platform-specific filesystem and process behavior should remain outside the shared UI and domain layers whenever practical.

---

## Requirements

### Building from source

Currently recommended:

- Windows 10 or Windows 11
- .NET 8 SDK
- Git
- Visual Studio, Visual Studio Code, JetBrains Rider, or another compatible .NET development environment

Robocopy is included with supported modern Windows versions.

---

## Build from Source

Clone the repository:

```bash
git clone https://github.com/YOUR-USERNAME/KappaCopy.git
```

Enter the project directory:

```bash
cd KappaCopy
```

Check the SDK:

```bash
dotnet --version
```

Restore dependencies:

```bash
dotnet restore
```

Build:

```bash
dotnet build KappaCopy.sln
```

Run:

```bash
dotnet run --project ./src/KappaCopy.App/KappaCopy.App.csproj
```

A successful build should complete with no errors.

---

## Publish for Windows

Create a self-contained Windows x64 build:

```powershell
dotnet publish .\src\KappaCopy.App\KappaCopy.App.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o .\publish\win-x64
```

The published application will be placed in:

```text
publish/win-x64/
```

A self-contained deployment includes the required .NET runtime, so the target computer does not need a separate .NET runtime installation.

---

## Command-Line Integration

Windows Explorer integration currently uses internal startup commands.

Copy an item:

```text
KappaCopy.App.exe --copy "C:\Path\To\File"
```

Prepare a destination:

```text
KappaCopy.App.exe --paste "D:\Destination"
```

These commands are primarily intended for Kappa Copy's Windows Shell integration.

---

## Internal Clipboard

Kappa Copy maintains a temporary clipboard independently of the operating-system clipboard.

This allows the application to collect source paths from Windows Explorer without interfering with normal copy/paste operations.

The temporary clipboard is stored under the user's local application data and is removed when the main Kappa Copy window closes.

---

## Completion Sound

Kappa Copy can play a notification sound when a transfer completes successfully.

The user can enable or disable this behavior from the application. The preference is stored locally and restored on future launches.

---

## Windows Installer

The Windows installer is built with **Inno Setup**.

Installer definition:

```text
installer/KappaCopy.iss
```

The installer can:

- install Kappa Copy for the current user;
- install the self-contained .NET application;
- create Start Menu shortcuts;
- optionally create a Desktop shortcut;
- register `Kappa Copy`;
- register `Kappa Paste Here`;
- remove Shell integration during uninstall;
- remove temporary Kappa Copy application data during uninstall.

---

## Platform Support

| Platform | Status | Copy Engine |
|---|---|---|
| Windows 10/11 | Active development | Robocopy |
| macOS | Planned | TBD |
| Linux | Planned / future target | TBD |

Avalonia provides the cross-platform UI foundation. Copy engines remain platform-specific.

---

## Roadmap

Planned and potential improvements include:

- More accurate transfer-speed monitoring
- Remaining-time estimation
- Advanced transfer statistics
- Copy queue management
- Better large-selection handling
- File conflict policies
- Overwrite / skip / rename decisions
- Verification options
- Transfer history
- Improved error reporting
- Native Windows 11 context-menu integration
- `Kappa Cut`
- Adaptive thread selection
- SSD / NVMe / HDD detection
- Network-copy optimization
- macOS copy engine
- Linux copy engine
- Automated tests
- GitHub Actions CI
- Signed Windows releases
- Automatic update support
- Localization / multiple languages
- Accessibility improvements

The roadmap is intentionally open to community discussion and contributions.

---

## Learning and Contributing

One of Kappa Copy's goals is to be useful to people learning software development.

Contributions are welcome from both experienced developers and beginners.

Useful contribution areas include:

- C# and .NET
- Avalonia UI
- Windows integration
- macOS integration
- Linux integration
- UI/UX
- Accessibility
- Testing
- Documentation
- Performance measurement
- Installer and deployment
- Localization

A good first contribution workflow is:

1. Fork the repository.
2. Create a focused branch.
3. Make one clear change.
4. Build and test it.
5. Open a pull request explaining what changed and why.

Before submitting code, verify:

```bash
dotnet restore
dotnet build KappaCopy.sln
```

Platform-specific code should remain isolated from shared components whenever possible.

---

## Development Principles

### Performance

Use efficient operating-system facilities and native copy engines where they provide a clear advantage.

### Reliability

Copy errors should be visible and understandable.

### Safety

Destructive behavior should never be an unexpected default.

### Simplicity

Common operations should remain easy to understand and use.

### Modularity

The user interface should not be tightly coupled to one operating system or copy engine.

### Education

Code and documentation should be approachable enough that other developers can learn from the project.

### Community

Constructive issues, discussions, pull requests, testing, and documentation improvements are welcome.

---

## Reporting Bugs

When reporting a bug, please include relevant information where possible:

- Kappa Copy version
- Operating-system version
- Source storage type
- Destination storage type
- Whether files, directories, or both were selected
- Copy profile
- Relevant Robocopy output
- Steps to reproduce the problem

Do **not** publish sensitive filenames, private paths, credentials, or confidential data in public issues.

---

## Security

If you discover a security-sensitive issue, avoid publishing exploit details in a public issue until a responsible reporting process is available.

Kappa Copy should never require passwords, authentication tokens, or other secrets for ordinary local file-copy operations.

---

## License

Kappa Copy is intended to be **free and open-source software**.

The recommended license for this project is the **MIT License**, a permissive open-source license that allows people to use, study, modify, distribute, and build upon the software while retaining the required copyright and license notice.

See the repository's `LICENSE` file for the authoritative license terms.

---

## Open-Source Philosophy

Kappa Copy is developed openly so people can:

- use the application;
- inspect how it works;
- learn from the source code;
- experiment with desktop development;
- improve existing features;
- build new copy engines;
- fix bugs;
- improve documentation;
- contribute ideas and code back to the community.

The goal is not only to build a useful copy utility, but also to maintain a practical project where developers can learn by working with real application code.

---

## Disclaimer

Kappa Copy performs filesystem operations and invokes operating-system copy tools.

Always maintain appropriate backups of important data.

The project maintainers and contributors cannot guarantee against data loss, interrupted transfers, filesystem errors, hardware failures, configuration mistakes, or other unexpected conditions.

Use the software responsibly and verify important transfers.

---

## Acknowledgements

Kappa Copy is built with open-source technologies including .NET and Avalonia UI and uses the native Robocopy utility on Windows.

Thanks to everyone who tests the project, reports problems, improves the documentation, submits code, or helps other developers learn from it.

---

## Project Status

**Kappa Copy is under active development.**

Expect APIs, UI elements, installer behavior, and internal architecture to evolve as the project matures.

If Kappa Copy is useful to you, consider starring the repository, testing releases, opening constructive issues, or contributing improvements.

---

**Kappa Copy — fast file copying, open development, and a practical project for learning modern .NET desktop programming.**
