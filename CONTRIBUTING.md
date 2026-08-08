# Contributing to Kappa Copy

Thank you for your interest in contributing to **Kappa Copy**.

Kappa Copy is a free and open-source desktop project built with C#, .NET, Avalonia UI, and platform-specific copy engines. One of the project's goals is to remain useful not only as an application, but also as a practical codebase for people learning modern desktop development.

Contributions from both experienced developers and beginners are welcome.

## Code of Conduct

Be respectful, constructive, and professional.

Harassment, personal attacks, discrimination, intentionally disruptive behavior, or hostility toward beginners are not acceptable.

Review code and ideas, not people.

## Ways to Contribute

You can help with:

- C# and .NET development
- Avalonia UI
- Windows Shell integration
- Robocopy integration
- macOS copy-engine research and implementation
- Linux copy-engine research and implementation
- performance measurement
- UI/UX
- accessibility
- localization and translations
- automated tests
- installer and deployment improvements
- documentation
- bug reports
- feature proposals

Small, focused improvements are welcome.

## Development Environment

The current Windows development setup uses:

- .NET 8
- Avalonia UI 11.3.x
- Windows 10 or Windows 11
- Robocopy
- Git
- an editor or IDE such as Visual Studio, Visual Studio Code, or Rider

Clone your fork:

```bash
git clone https://github.com/YOUR-USERNAME/KappaCopy.git
cd KappaCopy
```

Restore packages:

```bash
dotnet restore
```

Build:

```bash
dotnet build KappaCopy.sln
```

Run the desktop application:

```bash
dotnet run --project ./src/KappaCopy.App/KappaCopy.App.csproj
```

Before opening a pull request, the solution should build without errors.

## Branches

Create a branch for your change.

Examples:

```bash
git checkout -b fix/context-menu-selection
git checkout -b feature/copy-queue
git checkout -b docs/localization-guide
```

Prefer one logical change per branch.

## Commit Messages

Use short, descriptive commit messages.

Good examples:

```text
Fix clipboard reset for new shell selections
Add English localization resources
Improve Robocopy error reporting
Document Windows installer workflow
```

Avoid vague messages such as:

```text
update
changes
fix stuff
```

## Pull Requests

A good pull request should:

1. explain the problem or goal;
2. describe the solution;
3. mention important design decisions;
4. include testing steps;
5. remain focused on one logical change;
6. avoid unrelated formatting or refactoring.

If a change affects the UI, screenshots are helpful.

If a change affects copy behavior, explain the test source, destination type, copy profile, and expected result without exposing private paths or confidential filenames.

## Architecture Guidelines

Kappa Copy separates platform-independent application logic from platform-specific copy engines.

Keep that separation whenever possible:

```text
Avalonia UI
    |
    v
KappaCopy.Engine / ICopyEngine
    |
    +-- KappaCopy.Engine.Windows / Robocopy
    +-- future macOS engine
    +-- future Linux engine
```

Avoid putting Windows-only code into shared projects unless there is a strong reason.

Prefer:

```text
KappaCopy.Core
KappaCopy.Engine
KappaCopy.Engine.Windows
```

over operating-system checks scattered throughout the UI.

## Copy Safety

Filesystem code requires extra care.

Do not introduce destructive behavior as a default.

Features such as mirroring, deletion, replacement, or source cleanup must be explicit and clearly communicated to the user.

For example, Robocopy `/MIR` should never silently replace the behavior of a normal copy operation.

New copy features should be tested with disposable test data before being used with important files.

## Robocopy

Remember that Robocopy exit codes do not follow the usual `0 = success, non-zero = failure` convention.

In general:

```text
0-7  success or non-fatal state
8+   at least one failure
```

Do not change this interpretation without verifying the documented Robocopy semantics.

## Localization

User-visible text should be localizable.

Do not add new hard-coded UI strings when a resource key is appropriate.

Current language resources live under:

```text
src/KappaCopy.App/Localization/
```

When adding a new UI string:

1. add the same key to every supported language;
2. use `{DynamicResource KeyName}` for runtime-switchable XAML text;
3. keep resource keys in English and stable;
4. avoid embedding paths, numbers, or state directly into translated strings when formatting can be used;
5. test that switching language does not require restarting the application.

English is the fallback/default international language. Italian is also maintained by the project.

Additional translations are welcome.

## Formatting and Style

Follow the existing C# style in the repository.

Prefer:

- clear names;
- small focused methods;
- nullable reference types;
- `async`/`await` for asynchronous work;
- cancellation support for long-running work;
- comments that explain why, not obvious syntax;
- minimal platform coupling.

Avoid adding dependencies unless they provide clear value.

## Testing

At minimum, run:

```bash
dotnet restore
dotnet build KappaCopy.sln
```

For copy-engine changes, manually test representative cases where relevant:

- one file;
- multiple files;
- one directory;
- multiple directories;
- mixed files and directories;
- paths containing spaces;
- destination already containing files;
- cancellation;
- inaccessible source/destination;
- large files;
- shell `Kappa Copy` / `Kappa Paste Here`.

Never test destructive changes on important data.

## Bug Reports

A useful bug report includes:

- Kappa Copy version or commit;
- operating system;
- reproduction steps;
- expected behavior;
- actual behavior;
- relevant application/Robocopy log;
- source and destination storage types where relevant.

Remove private information before posting logs publicly.

## Feature Requests

Feature proposals are welcome.

Please explain:

- the user problem;
- the proposed behavior;
- why it belongs in Kappa Copy;
- possible safety implications;
- platform-specific implications.

Implementation details are welcome but not required.

## Security Issues

Do not publish sensitive security vulnerabilities with detailed exploitation steps in a public issue.

Until the repository publishes a dedicated private security-reporting process, contact the maintainers privately through the repository owner's available contact channel.

## Licensing Contributions

Kappa Copy is licensed under the MIT License.

By submitting a contribution, you agree that your contribution may be distributed under the repository's MIT License.

Do not submit code that you do not have the right to contribute.

## Questions

If you are learning and are unsure how to contribute, opening a focused discussion or issue is fine.

A small, well-tested improvement is more valuable than a large unfinished rewrite.

Thank you for helping make Kappa Copy better.
