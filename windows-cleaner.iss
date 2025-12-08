[Setup]
AppName=Windows Cleaner
AppVersion=1.0.6
AppPublisher=Windows Cleaner
AppPublisherURL=https://github.com/christwadel65-ux/windows-cleaner
AppSupportURL=https://github.com/christwadel65-ux/windows-cleaner
AppUpdatesURL=https://github.com/christwadel65-ux/windows-cleaner
DefaultDirName={pf}\WindowsCleaner
DefaultGroupName=Windows Cleaner
OutputDir=Output
OutputBaseFilename=WindowsCleaner-Setup-1.0.6
SetupIconFile=app.ico
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\windows-cleaner.exe
LicenseFile=LICENSE

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\Release\net10.0-windows\windows-cleaner.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net10.0-windows\windows-cleaner.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net10.0-windows\*.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net10.0-windows\*.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net10.0-windows\*.pdb"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "app.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Windows Cleaner"; Filename: "{app}\windows-cleaner.exe"; WorkingDir: "{app}"
Name: "{group}\{cm:UninstallProgram,Windows Cleaner}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\Windows Cleaner"; Filename: "{app}\windows-cleaner.exe"; WorkingDir: "{app}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\Windows Cleaner"; Filename: "{app}\windows-cleaner.exe"; WorkingDir: "{app}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\windows-cleaner.exe"; Description: "{cm:LaunchProgram,Windows Cleaner}"; Flags: nowait postinstall skipifsilent
