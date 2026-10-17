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
german.RuntimeDownload=.NET 8 Desktop Runtime wird heruntergeladen ...
english.RuntimeDownload=Downloading .NET 8 Desktop Runtime ...
german.RuntimeInstalling=.NET 8 Desktop Runtime wird installiert ...
english.RuntimeInstalling=Installing .NET 8 Desktop Runtime ...
german.RuntimeFailed=Die .NET 8 Desktop Runtime konnte nicht installiert werden.%nBitte manuell von https://dotnet.microsoft.com/download/dotnet/8.0 installieren und das Setup erneut starten.
english.RuntimeFailed=The .NET 8 Desktop Runtime could not be installed.%nPlease install it manually from https://dotnet.microsoft.com/download/dotnet/8.0 and run this setup again.

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
const
  RuntimeUrl = 'https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe';

var
  DownloadPage: TDownloadWizardPage;

// True if a .NET 8.x Desktop Runtime (x64) is installed.
function DesktopRuntimeInstalled: Boolean;
var
  Names: TArrayOfString;
  I: Integer;
  Found: TFindRec;
begin
  Result := False;
  // the runtime folder is the reliable check (SDK-only machines have no registry entry)
  if FindFirst(ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App\8.*'), Found) then
  begin
    FindClose(Found);
    Result := True;
    Exit;
  end;
  if RegGetValueNames(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', Names) then
    for I := 0 to GetArrayLength(Names) - 1 do
      if Copy(Names[I], 1, 2) = '8.' then
      begin
        Result := True;
        Exit;
      end;
end;

procedure InitializeWizard;
begin
  DownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), CustomMessage('RuntimeDownload'), nil);
end;

// Download and install the runtime (UAC prompt) before the files are copied.
function NextButtonClick(CurPageID: Integer): Boolean;
var
  Installer: String;
  ResultCode: Integer;
begin
  Result := True;
  if (CurPageID = wpReady) and not DesktopRuntimeInstalled then
  begin
    DownloadPage.Clear;
    DownloadPage.Add(RuntimeUrl, 'windowsdesktop-runtime-win-x64.exe', '');
    DownloadPage.Show;
    try
      try
        DownloadPage.Download;
      except
        MsgBox(CustomMessage('RuntimeFailed') + #13#10#13#10 + GetExceptionMessage, mbError, MB_OK);
        Result := False;
        Exit;
      end;
    finally
      DownloadPage.Hide;
    end;
    Installer := ExpandConstant('{tmp}\windowsdesktop-runtime-win-x64.exe');
    WizardForm.StatusLabel.Caption := CustomMessage('RuntimeInstalling');
    if not ShellExec('runas', Installer, '/install /quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode)
       or not ((ResultCode = 0) or (ResultCode = 3010)) then
    begin
      MsgBox(CustomMessage('RuntimeFailed'), mbError, MB_OK);
      Result := False;
    end;
  end;
end;

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
