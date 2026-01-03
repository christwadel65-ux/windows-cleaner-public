[Setup]
AppId={{8B5E5F6D-9C3A-4E2B-A1D7-3F8C9E4A6B5D}
AppName=Windows Cleaner
AppVersion=2.0.5
AppPublisher=Windows Cleaner
AppPublisherURL=https://github.com/christwadel65-ux/windows-cleaner
AppSupportURL=https://github.com/christwadel65-ux/windows-cleaner
AppUpdatesURL=https://github.com/christwadel65-ux/windows-cleaner
DefaultDirName={autopf}\WindowsCleaner
DefaultGroupName=Windows Cleaner
OutputDir=..\Output
OutputBaseFilename=WindowsCleaner-Setup-2.0.5
SetupIconFile=..\src\WindowsCleaner\UI\app.ico
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\windows-cleaner.exe
LicenseFile=..\LICENSE

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
french.AppDescription=Outil professionnel de nettoyage et d'optimisation pour Windows
english.AppDescription=Professional cleaning and optimization tool for Windows

french.ComponentsDescription=Sélectionnez les composants à installer
english.ComponentsDescription=Select the components to install

french.MainFiles=Fichiers principaux de l'application
english.MainFiles=Main application files

french.WelcomeLabel2=Cet assistant va installer [name/ver] sur votre ordinateur.%n%nIl est recommandé de fermer toutes les autres applications avant de continuer.%n%n[name] nécessite les droits administrateur pour fonctionner correctement.
english.WelcomeLabel2=This will install [name/ver] on your computer.%n%nIt is recommended that you close all other applications before continuing.%n%n[name] requires administrator rights to function properly.

french.FinishedLabel=L'installation de [name] est terminée. L'application peut être lancée en sélectionnant l'icône installée.
english.FinishedLabel=[name] installation is complete. The application can be launched by selecting the installed icon.

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; 
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\bin\Release\net10.0-windows\windows-cleaner.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net10.0-windows\windows-cleaner.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net10.0-windows\windows-cleaner.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net10.0-windows\windows-cleaner.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net10.0-windows\windows-cleaner.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\WindowsCleaner\UI\app.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Windows Cleaner"; Filename: "{app}\windows-cleaner.exe"; WorkingDir: "{app}"
Name: "{group}\{cm:UninstallProgram,Windows Cleaner}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\Windows Cleaner"; Filename: "{app}\windows-cleaner.exe"; WorkingDir: "{app}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\Windows Cleaner"; Filename: "{app}\windows-cleaner.exe"; WorkingDir: "{app}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\windows-cleaner.exe"; Description: "{cm:LaunchProgram,Windows Cleaner}"; WorkingDir: "{app}"; Flags: postinstall skipifsilent nowait shellexec; Verb: runas

; Supprimer dossiers et clés registre à la désinstallation
[UninstallDelete]
Type: filesandordirs; Name: "{app}\\logs";               
Type: filesandordirs; Name: "{userappdata}\\WindowsCleaner"; 
Type: filesandordirs; Name: "{commonappdata}\\WindowsCleaner";

[Registry]
Root: HKCU; Subkey: "Software\\WindowsCleaner"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\\WindowsCleaner"; Flags: uninsdeletekey

[Code]
function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  // Chercher avec l'AppId actuel
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

function GetOldUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
  sDisplayName: String;
begin
  // Chercher par DisplayName pour Windows Cleaner (toutes versions)
  sUnInstPath := 'Software\Microsoft\Windows\CurrentVersion\Uninstall';
  sUnInstallString := '';
  
  // Essayer d'abord avec le suffixe _is1
  if RegQueryStringValue(HKLM, sUnInstPath + '\Windows Cleaner_is1', 'UninstallString', sUnInstallString) then
    Result := sUnInstallString
  else if RegQueryStringValue(HKCU, sUnInstPath + '\Windows Cleaner_is1', 'UninstallString', sUnInstallString) then
    Result := sUnInstallString
  // Chercher par DisplayName en parcourant les sous-clés
  else if RegQueryStringValue(HKLM, sUnInstPath + '\{8B5E5F6D-9C3A-4E2B-A1D7-3F8C9E4A6B5D}_is1', 'UninstallString', sUnInstallString) then
    Result := sUnInstallString
  else if RegQueryStringValue(HKCU, sUnInstPath + '\{8B5E5F6D-9C3A-4E2B-A1D7-3F8C9E4A6B5D}_is1', 'UninstallString', sUnInstallString) then
    Result := sUnInstallString
  else
    Result := '';
end;

function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '') or (GetOldUninstallString() <> '');
end;

function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
  iRetries: Integer;
begin
  Result := 0;
  iRetries := 0;
  
  // Essayer d'abord avec l'AppId actuel
  sUnInstallString := GetUninstallString();
  
  // Si pas trouvé, chercher les anciennes versions
  if sUnInstallString = '' then
    sUnInstallString := GetOldUninstallString();
  
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    
    // Tenter la désinstallation silencieuse avec retries
    while (iRetries < 3) and (Result = 0) do
    begin
      if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      begin
        Result := 1; // Succès
      end
      else
      begin
        iRetries := iRetries + 1;
        Sleep(500); // Attendre 500ms avant retry
      end;
    end;
    
    // Si échec du désinstalleur, essayer la suppression du dossier
    if Result = 0 then
    begin
      if DirExists(ExpandConstant('{autopf}\WindowsCleaner')) then
      begin
        if DelTree(ExpandConstant('{autopf}\WindowsCleaner'), True, True, True) then
          Result := 1;
      end;
    end;
  end else
    Result := 0;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  // On ne fait rien ici, la désinstallation est gérée dans InitializeSetup
end;

function InitializeSetup(): Boolean;
var
  sUnInstallString: String;
  iResultCode: Integer;
  iRetries: Integer;
  iAttempts: Integer;
begin
  Result := True;
  iRetries := 0;
  
  // Chercher l'ancienne version
  sUnInstallString := GetUninstallString();
  if sUnInstallString = '' then
    sUnInstallString := GetOldUninstallString();
  
  // Si trouvée, désinstaller complètement
  if sUnInstallString <> '' then
  begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    
    // Tentatives multiples de désinstallation
    iAttempts := 0;
    while iAttempts < 3 do
    begin
      if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
        Break;
      
      iAttempts := iAttempts + 1;
      Sleep(1000); // Attendre 1s entre les tentatives
    end;
    
    // Attendre un peu pour libérer les fichiers
    Sleep(2000);
  end;
  
  // Forcer la suppression du dossier d'installation (fallback agressif)
  if DirExists(ExpandConstant('{autopf}\WindowsCleaner')) then
  begin
    // Attendre que l'app se ferme complètement
    Sleep(1000);
    
    // Supprimer le dossier avec force
    if not DelTree(ExpandConstant('{autopf}\WindowsCleaner'), True, True, True) then
    begin
      // Si échec, c'est ok, on continue quand même
      // Les fichiers existants seront remplacés par ignoreversion
    end;
  end;
end;
