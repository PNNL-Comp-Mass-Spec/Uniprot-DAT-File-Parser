; This is an Inno Setup configuration file
; http://www.jrsoftware.org/isinfo.php

#define ApplicationVersion GetFileVersion('..\bin\UniprotDATFileParser.exe')

[CustomMessages]
AppName=Uniprot DAT File Parser

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.
; Example with multiple lines:
; WelcomeLabel2=Welcome message%n%nAdditional sentence
[Files]
Source: ..\bin\UniprotDATFileParser.exe   ; DestDir: {app}
Source: ..\bin\UniprotDATFileParser.pdb   ; DestDir: {app}
Source: ..\Readme.txt                     ; DestDir: {app}
Source: ..\RevisionHistory.txt            ; DestDir: {app}
Source: Images\delete_16x.ico             ; DestDir: {app}

Source: ..\bin\SampleData.gbk             ; DestDir: {app}
Source: ..\bin\SampleData.fasta           ; DestDir: {app}
Source: ..\bin\SampleData_Columns.txt     ; DestDir: {app}
Source: ..\bin\SampleData_output.txt      ; DestDir: {app}

Source: ..\bin\uniprot_sprot_human_excerpt.dat                   ; DestDir: {app}
Source: ..\bin\uniprot_sprot_human_excerpt_output.fasta          ; DestDir: {app}
Source: ..\bin\uniprot_sprot_human_Columns.txt                   ; DestDir: {app}
Source: ..\bin\uniprot_sprot_human_excerpt_Columns.txt           ; DestDir: {app}
Source: ..\bin\uniprot_sprot_human_excerpt_OrganismMap.txt       ; DestDir: {app}
Source: ..\bin\uniprot_sprot_human_excerpt_OrganismSummary.txt   ; DestDir: {app}
Source: ..\bin\uniprot_sprot_human_excerpt_output.txt            ; DestDir: {app}

Source: ..\bin\ipi.HUMAN.v3.20_Excerpt.dat                 ; DestDir: {app}
Source: ..\bin\ipi.HUMAN.v3.20_Excerpt_output.txt          ; DestDir: {app}
Source: ..\bin\ipi.HUMAN.v3.20_Excerpt_output.fasta        ; DestDir: {app}


[Dirs]
Name: {commonappdata}\UniprotDATFileParser; Flags: uninsalwaysuninstall

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked
; Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

[Icons]
; Name: {commondesktop}\Uniprot DAT File Parser; Filename: {app}\UniprotDATFileParser.exe; Tasks: desktopicon; Comment: UniprotDATFileParser
Name: {group}\Uniprot DAT File Parser; Filename: {app}\UniprotDATFileParser.exe; Comment: Uniprot DAT File Parser

[Setup]
AppName=Uniprot DAT File Parser
AppVersion={#ApplicationVersion}
;AppVerName=UniprotDATFileParser
AppID=UniprotDATFileParserId
AppPublisher=Pacific Northwest National Laboratory
AppPublisherURL=http://omics.pnl.gov/software
AppSupportURL=http://omics.pnl.gov/software
AppUpdatesURL=http://omics.pnl.gov/software
DefaultDirName={pf}\UniprotDATFileParser
DefaultGroupName=PAST Toolkit
AppCopyright=© PNNL
;LicenseFile=.\License.rtf
PrivilegesRequired=poweruser
OutputBaseFilename=UniprotDATFileParser_Installer
VersionInfoVersion={#ApplicationVersion}
VersionInfoCompany=PNNL
VersionInfoDescription=Uniprot DAT File Parser
VersionInfoCopyright=PNNL
DisableFinishedPage=true
ShowLanguageDialog=no
ChangesAssociations=false
EnableDirDoesntExistWarning=false
AlwaysShowDirOnReadyPage=true
UninstallDisplayIcon={app}\delete_16x.ico
ShowTasksTreeLines=true
OutputDir=.\Output

[Registry]
;Root: HKCR; Subkey: MyAppFile; ValueType: string; ValueName: ; ValueDataMyApp File; Flags: uninsdeletekey
;Root: HKCR; Subkey: MyAppSetting\DefaultIcon; ValueType: string; ValueData: {app}\wand.ico,0; Flags: uninsdeletevalue

[UninstallDelete]
Name: {app}; Type: filesandordirs
