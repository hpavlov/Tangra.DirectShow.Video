;
; Script generated by the ASCOM Driver Installer Script Generator 6.0.0.0
; Generated by Hristo Pavlov on 28/01/2014 (UTC)
;
[Setup]
AppID={{d4a590c6-6563-4826-b17e-6d6730e2e427}
AppName=Tangra Video Capture Driver
AppVerName=Tangra Video Capture Driver 1.0.8
AppVersion=1.0.8
AppPublisher=Hristo Pavlov <hristo_dpavlov@yahoo.com>
AppPublisherURL=mailto:hristo_dpavlov@yahoo.com
AppSupportURL=http://tech.groups.yahoo.com/group/ASCOM-Talk/
AppUpdatesURL=http://ascom-standards.org/
VersionInfoVersion=1.0.0
MinVersion=0,5.0.2195sp4
DefaultDirName="{cf}\ASCOM\Video"
DisableDirPage=yes
DisableProgramGroupPage=yes
OutputDir="."
OutputBaseFilename="Tangra Video Capture Setup"
Compression=lzma
SolidCompression=yes
; Put there by Platform if Driver Installer Support selected
WizardImageFile="C:\Program Files (x86)\ASCOM\Platform 6 Developer Components\Installer Generator\Resources\WizardImage.bmp"
LicenseFile="C:\Program Files (x86)\ASCOM\Platform 6 Developer Components\Installer Generator\Resources\CreativeCommons.txt"
; {cf}\ASCOM\Uninstall\Video folder created by Platform, always
UninstallFilesDir="{cf}\ASCOM\Uninstall\Video\Tangra Video Capture"

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: "{cf}\ASCOM\Uninstall\Video\Tangra Video Capture"
; TODO: Add subfolders below {app} as needed (e.g. Name: "{app}\MyFolder")


[Files]
Source: "D:\Hristo\ASCOM.DirectShow.Video\bin\Release\Tangra.DirectShow.Video.dll"; DestDir: "{app}"
; Require a read-me HTML to appear after installation, maybe driver's Help doc
Source: "D:\Hristo\ASCOM.DirectShow.Video\bin\Release\readme.html"; DestDir: "{app}"; Flags: isreadme
; Optional source files (COM and .NET aware)


; Only if driver is .NET
[Run]
; Only for .NET assembly/in-proc drivers
Filename: "{dotnet2032}\regasm.exe"; Parameters: "/codebase ""{app}\Tangra.DirectShow.Video.dll"""; Flags: runhidden 32bit
Filename: "{dotnet2064}\regasm.exe"; Parameters: "/codebase ""{app}\Tangra.DirectShow.Video.dll"""; Flags: runhidden 64bit; Check: IsWin64




; Only if driver is .NET
[UninstallRun]
; Only for .NET assembly/in-proc drivers
Filename: "{dotnet2032}\regasm.exe"; Parameters: "-u ""{app}\Tangra.DirectShow.Video.dll"""; Flags: runhidden 32bit
Filename: "{dotnet2064}\regasm.exe"; Parameters: "-u ""{app}\Tangra.DirectShow.Video.dll"""; Flags: runhidden 64bit; Check: IsWin64




[CODE]
//
// Before the installer UI appears, verify that the (prerequisite)
// ASCOM Platform 6.0 or greater is installed, including both Helper
// components. Utility is required for all types (COM and .NET)!
//
function InitializeSetup(): Boolean;
var
   U : Variant;
   H : Variant;
begin
   Result := FALSE;  // Assume failure
   // check that the DriverHelper and Utilities objects exist, report errors if they don't
   try
      H := CreateOLEObject('DriverHelper.Util');
   except
      MsgBox('The ASCOM DriverHelper object has failed to load, this indicates a serious problem with the ASCOM installation', mbInformation, MB_OK);
   end;
   try
      U := CreateOLEObject('ASCOM.Utilities.Util');
   except
      MsgBox('The ASCOM Utilities object has failed to load, this indicates that the ASCOM Platform has not been installed correctly', mbInformation, MB_OK);
   end;
   try
      if (U.IsMinimumRequiredVersion(6,0)) then	// this will work in all locales
         Result := TRUE;
   except
   end;
   if(not Result) then
      MsgBox('The ASCOM Platform 6.0 or greater is required for this driver.', mbInformation, MB_OK);
end;

// Code to enable the installer to uninstall previous versions of itself when a new version is installed
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  UninstallExe: String;
  UninstallRegistry: String;
begin
  if (CurStep = ssInstall) then // Install step has started
	begin
      // Create the correct registry location name, which is based on the AppId
      UninstallRegistry := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}' + '_is1');
      // Check whether an extry exists
      if RegQueryStringValue(HKLM, UninstallRegistry, 'UninstallString', UninstallExe) then
        begin // Entry exists and previous version is installed so run its uninstaller quietly after informing the user
          MsgBox('Setup will now remove the previous version.', mbInformation, MB_OK);
          Exec(RemoveQuotes(UninstallExe), ' /SILENT', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
          sleep(1000);    //Give enough time for the install screen to be repainted before continuing
        end
  end;
end;

