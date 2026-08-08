#define MyAppName "Kappa Copy"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "Kappa"
#define MyAppExeName "KappaCopy.App.exe"

[Setup]
AppId={{7A7FA986-B2B7-4E29-A379-29B80B75103B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}

DefaultDirName={localappdata}\Programs\KappaCopy
DefaultGroupName=Kappa Copy

PrivilegesRequired=lowest

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

OutputDir=output
OutputBaseFilename=KappaCopy-Setup-v{#MyAppVersion}-x64

Compression=lzma2
SolidCompression=yes

WizardStyle=modern

UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}

CloseApplications=yes
RestartApplications=no

[Files]
Source: "..\publish\win-x64\*"; \
    DestDir: "{app}"; \
    Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Kappa Copy"; \
    Filename: "{app}\{#MyAppExeName}"

Name: "{userdesktop}\Kappa Copy"; \
    Filename: "{app}\{#MyAppExeName}"; \
    Tasks: desktopicon

[Tasks]
Name: "desktopicon"; \
    Description: "Crea un collegamento sul desktop"; \
    GroupDescription: "Collegamenti:"; \
    Flags: unchecked

; =========================================================
; KAPPA COPY
; Tasto destro su file e cartelle
; =========================================================

[Registry]

Root: HKCU; \
    Subkey: "Software\Classes\AllFileSystemObjects\shell\KappaCopy.Copy"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: "Kappa Copy"; \
    Flags: uninsdeletekey

Root: HKCU; \
    Subkey: "Software\Classes\AllFileSystemObjects\shell\KappaCopy.Copy"; \
    ValueType: string; \
    ValueName: "Icon"; \
    ValueData: "{app}\{#MyAppExeName},0"

Root: HKCU; \
    Subkey: "Software\Classes\AllFileSystemObjects\shell\KappaCopy.Copy"; \
    ValueType: string; \
    ValueName: "MultiSelectModel"; \
    ValueData: "Document"

Root: HKCU; \
    Subkey: "Software\Classes\AllFileSystemObjects\shell\KappaCopy.Copy\command"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: """{app}\{#MyAppExeName}"" --copy ""%1"""

; =========================================================
; KAPPA PASTE HERE
; Background cartella
; =========================================================

Root: HKCU; \
    Subkey: "Software\Classes\Directory\Background\shell\KappaCopy.PasteHere"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: "Kappa Paste Here"; \
    Flags: uninsdeletekey

Root: HKCU; \
    Subkey: "Software\Classes\Directory\Background\shell\KappaCopy.PasteHere"; \
    ValueType: string; \
    ValueName: "Icon"; \
    ValueData: "{app}\{#MyAppExeName},0"

Root: HKCU; \
    Subkey: "Software\Classes\Directory\Background\shell\KappaCopy.PasteHere\command"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: """{app}\{#MyAppExeName}"" --paste ""%V"""

; =========================================================
; KAPPA PASTE HERE
; Tasto destro direttamente sulla cartella
; =========================================================

Root: HKCU; \
    Subkey: "Software\Classes\Directory\shell\KappaCopy.PasteHere"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: "Kappa Paste Here"; \
    Flags: uninsdeletekey

Root: HKCU; \
    Subkey: "Software\Classes\Directory\shell\KappaCopy.PasteHere"; \
    ValueType: string; \
    ValueName: "Icon"; \
    ValueData: "{app}\{#MyAppExeName},0"

Root: HKCU; \
    Subkey: "Software\Classes\Directory\shell\KappaCopy.PasteHere\command"; \
    ValueType: string; \
    ValueName: ""; \
    ValueData: """{app}\{#MyAppExeName}"" --paste ""%1"""

[Run]
Filename: "{app}\{#MyAppExeName}"; \
    Description: "Avvia Kappa Copy"; \
    Flags: nowait postinstall skipifsilent

[UninstallDelete]

; Clipboard temporaneo
Type: files; \
    Name: "{localappdata}\KappaCopy\clipboard.json"

; Settings dell'app
Type: files; \
    Name: "{localappdata}\KappaCopy\settings.json"

; Rimuove la directory se vuota
Type: dirifempty; \
    Name: "{localappdata}\KappaCopy"