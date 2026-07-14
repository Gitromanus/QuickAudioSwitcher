; Inno Setup Script for QuickAudioSwitcher
; Requires Inno Setup 6+

#define MyAppName "QuickAudioSwitcher"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Gitromanus"
#define MyAppURL "https://github.com/Gitromanus/QuickAudioSwitcher"
#define MyAppExeName "QuickAudioSwitcher.exe"

[Setup]
AppId={{B8F4A3D2-1C5E-4A7F-9B6D-3E2F1C8A5D7B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=.\installer_output
OutputBaseFilename=QuickAudioSwitcher-{#MyAppVersion}-Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[Files]
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\*.dll"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "publish\*.pdb"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist deleteafterinstall
Source: "publish\*.config"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "publish\*.xml"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Kill the process if running
Filename: "{cmd}"; Parameters: "/C taskkill /f /im {#MyAppExeName} 2>nul"; Flags: runhidden

[Code]
function IsAppRunning: Boolean;
var
  ErrorCode: Integer;
begin
  Result := ShellExec('open', 'taskkill', '/f /im ' + '{#MyAppExeName}',
    '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    // Kill running instance before installing
    IsAppRunning();
  end;
end;