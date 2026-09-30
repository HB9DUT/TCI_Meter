; Copyright © 2026, HB9DUT
; SPDX-License-Identifier: GPL-3.0-or-later
;
; Inno Setup script for TCI-Meter. Built by build-installer.ps1 (passes /DAppVersion=...).

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#define AppName "TCI-Meter"
#define AppExe  "TciMeter.exe"

[Setup]
AppId={{8E3B6C1A-4F27-4D9B-A5C1-7B2E9D4F1A63}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=HB9DUT
AppCopyright=Copyright © 2026, HB9DUT
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
; Per-user install by default (no admin rights needed); the user may choose "for all users" in a dialog.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
LicenseFile=..\LICENSE
SetupIconFile=..\Assets\app.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
OutputDir=Output
OutputBaseFilename=TciMeter-Setup-{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
VersionInfoVersion={#AppVersion}
VersionInfoCompany=HB9DUT
VersionInfoProductName={#AppName}
VersionInfoCopyright=Copyright © 2026, HB9DUT

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
german.DeleteSettings=Sollen auch die gespeicherten Einstellungen (Rufzeichen, TCI-Host usw.) gelöscht werden?
english.DeleteSettings=Also delete the saved settings (callsign, TCI host, etc.)?

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\installer\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Code]
// Settings live in %AppData%\TciMeter (per Windows user); ask before removing them.
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  SettingsDir: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    SettingsDir := ExpandConstant('{userappdata}\TciMeter');
    if DirExists(SettingsDir) then
      if not UninstallSilent then
        if MsgBox(CustomMessage('DeleteSettings'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
          DelTree(SettingsDir, True, True, True);
  end;
end;
