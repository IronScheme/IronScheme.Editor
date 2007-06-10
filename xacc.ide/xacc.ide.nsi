; Script generated by the HM NIS Edit Script Wizard.

; HM NIS Edit Wizard helper defines
!define PRODUCT_NAME "xacc.ide"
!define PRODUCT_VERSION "0.2.0.75"
!define PRODUCT_PUBLISHER "leppie"
!define PRODUCT_WEB_SITE "http://blogs.wdevs.com/leppie/"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\xacc.ide.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

SetCompressor /SOLID lzma
XPStyle on

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "xacc.ide-${PRODUCT_VERSION}-setup.exe"
InstallDir "$PROGRAMFILES\xacc.ide"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show


; MUI 1.67 compatible ------
!include "MUI.nsh"

; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "..\..\..\xacc\Resources\atb.ico"
!define MUI_UNICON "..\..\..\xacc\Resources\atb.ico"
!define MUI_COMPONENTSPAGE_NODESC
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "..\..\..\xacc\Resources\header.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP "..\..\..\xacc\Resources\installer.bmp"

; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
!define MUI_LICENSEPAGE_BUTTON
!insertmacro MUI_PAGE_LICENSE "..\..\..\license.txt"
;!insertmacro MUI_PAGE_COMPONENTS
; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
!define MUI_FINISHPAGE_RUN "$INSTDIR\xacc.ide.exe"
!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language files
!insertmacro MUI_LANGUAGE "English"

; Reserve files
!insertmacro MUI_RESERVEFILE_INSTALLOPTIONS


; MUI end ------

!define BASE_URL http://download.microsoft.com/download
!define URL_DOTNET "http://download.microsoft.com/download/5/6/7/567758a3-759e-473e-bf8f-52154438565a/dotnetfx.exe"

LangString DESC_SHORTDOTNET ${LANG_ENGLISH} ".Net Framework 2.0"
LangString DESC_LONGDOTNET ${LANG_ENGLISH} "Microsoft .Net Framework 2.0"
LangString DESC_DOTNET_DECISION ${LANG_ENGLISH} "$(DESC_SHORTDOTNET) is required.$\nIt is strongly \
  advised that you install$\n$(DESC_SHORTDOTNET) before continuing.$\nIf you choose to continue, \
  you will need to connect$\nto the internet before proceeding.$\nWould you like to continue with \
  the installation?"
LangString SEC_DOTNET ${LANG_ENGLISH} "$(DESC_SHORTDOTNET) "
LangString DESC_INSTALLING ${LANG_ENGLISH} "Installing"
LangString DESC_DOWNLOADING1 ${LANG_ENGLISH} "Downloading"
LangString DESC_DOWNLOADFAILED ${LANG_ENGLISH} "Download Failed:"
LangString ERROR_DOTNET_DUPLICATE_INSTANCE ${LANG_ENGLISH} "The $(DESC_SHORTDOTNET) Installer is \
  already running."
LangString ERROR_NOT_ADMINISTRATOR ${LANG_ENGLISH} "$(DESC_000022)"
LangString ERROR_INVALID_PLATFORM ${LANG_ENGLISH} "$(DESC_000023)"
LangString DESC_DOTNET_TIMEOUT ${LANG_ENGLISH} "The installation of the $(DESC_SHORTDOTNET) \
  has timed out."
LangString ERROR_DOTNET_INVALID_PATH ${LANG_ENGLISH} "The $(DESC_SHORTDOTNET) Installation$\n\
  was not found in the following location:$\n"
LangString ERROR_DOTNET_FATAL ${LANG_ENGLISH} "A fatal error occurred during the installation$\n\
  of the $(DESC_SHORTDOTNET)."
LangString FAILED_DOTNET_INSTALL ${LANG_ENGLISH} "The installation of $(PRODUCT_NAME) will$\n\
  continue. However, it may not function properly$\nuntil $(DESC_SHORTDOTNET)$\nis installed."

Var NETPATH

; IsDotNETInstalled
;
; Usage:
;   Call IsDotNETInstalled
;   Pop $0
;   StrCmp $0 1 found.NETFramework no.NETFramework

Function IsDotNETInstalled
   Push $0
   Push $1
   Push $2
   Push $3
   Push $4

   ReadRegStr $4 HKEY_LOCAL_MACHINE \
     "Software\Microsoft\.NETFramework" "InstallRoot"
   # remove trailing back slash
   Push $4
   Exch $EXEDIR
   Exch $EXEDIR
   Pop $4
   # if the root directory doesn't exist .NET is not installed
   IfFileExists $4 0 noDotNET

   StrCpy $0 0

   EnumStart:

     EnumRegKey $2 HKEY_LOCAL_MACHINE \
       "Software\Microsoft\.NETFramework\Policy"  $0
     IntOp $0 $0 + 1
     StrCmp $2 "" noDotNET

     StrCpy $1 0

     EnumPolicy:

       EnumRegValue $3 HKEY_LOCAL_MACHINE \
         "Software\Microsoft\.NETFramework\Policy\$2" $1
       IntOp $1 $1 + 1
        StrCmp $3 "" EnumStart
         IfFileExists "$4\v2.0.$3" foundDotNET EnumPolicy

   noDotNET:
     DetailPrint ".NET 2.0 not detected."
     StrCpy $0 0
     Goto done

   foundDotNET:
     DetailPrint ".NET 2.0 detected @ $4\v2.0.$3."
     StrCpy $0 "$4\v2.0.$3"

   done:
     Pop $4
     Pop $3
     Pop $2
     Pop $1
     Exch $0
FunctionEnd

InstType "Full"
InstType "Minimal"

Section -$(SEC_DOTNET) SECDOTNET
SectionIn 1 2 RO

Goto Start

AbortInstall:
Abort

Start:
Call IsDotNETInstalled
Pop $NETPATH
StrCmp $NETPATH 0 PromptDownload Install

PromptDownload:

MessageBox MB_ICONEXCLAMATION|MB_YESNO|MB_DEFBUTTON2 "$(DESC_DOTNET_DECISION)" /SD IDNO IDYES DownloadNET IDNO AbortInstall

DownloadNET:

nsisdl::download /TIMEOUT=60000 "${URL_DOTNET}" "$TEMP\dotnetfx.exe"
Pop $0
StrCmp "$0" "success" InstallNET AbortInstall

InstallNET:
Exec '"$TEMP\dotnetfx.exe" /q:a /c:"install.exe /qb"'

Install:

SectionEnd

Section "xacc.ide ${PRODUCT_VERSION}" SEC01
SectionIn 1 2 RO
  SetOutPath "$INSTDIR"
  SetOverwrite ifnewer
  DetailPrint "Removing previous native images (if any)..."
  nsExec::ExecToStack '"$NETPATH\ngen.exe" uninstall "$INSTDIR\xacc.lexers.managed.dll"'
  CreateDirectory "$SMPROGRAMS\xacc.ide"
  CreateShortCut "$SMPROGRAMS\xacc.ide\xacc.ide.lnk" "$INSTDIR\xacc.ide.exe"
	;CreateShortCut "$SMPROGRAMS\xacc.ide\xacc.languagedesigner.lnk" "$INSTDIR\xacc.languagedesigner.exe"
	CreateShortCut "$SMPROGRAMS\xacc.ide\xacc.ide (Debug mode).lnk" "$INSTDIR\xacc.ide.exe" "-debug"
  CreateShortCut "$DESKTOP\xacc.ide.lnk" "$INSTDIR\xacc.ide.exe"
	CreateShortCut "$SMPROGRAMS\xacc.ide\xacc.license.lnk" "$INSTDIR\license.txt"
	CreateShortCut "$SMPROGRAMS\xacc.ide\xacc.config.lnk" "$INSTDIR\xacc.ide.exe" "-listermode xacc.config.xml"
	File "xacc.ide.exe"
	;File "xacc.languagedesigner.exe"
  File "xacc.dll"
	File "..\..\..\license.txt"
  File "WeifenLuo.WinFormsUI.Docking.dll"
	File "xacc.runtime.dll"
	File "LSharp.dll"
	;File "antlr.runtime.dll"
	;File "Core.dll"
	;File "Gui.Diagram.dll"
	;File "Translations.dll"
	;File "vs.dst"
	;File "xacc.nclass.dll"
	File "Aga.Controls.dll"
	;File "mdbg.dll"
	;File "lsc.exe"
	File "xacc.lexers.managed.dll"
  File "xacc.config.xml"
  File "xacc.imports"
  File "xacc.config.xsl"
	File "Readme.txt"
	File "Changelog.txt"
	File "command.ls"
	File "profile.ls"
	File "xacc.ide.exe.config"
SectionEnd

;Section "Tools" SEC02
;SectionIn 1
;  SetOutPath "$INSTDIR\Tools"	
;  File "..\..\..\Tools\cs_lex.exe"
;SectionEnd

;Section "Source" SEC03
;SectionIn 1
;  SetOutPath "$INSTDIR\source"
;	File /r source\*.*
;SectionEnd

;Section "Documentation" SEC04
;SectionIn 1
;  SetOutPath "$INSTDIR"
;	CreateShortCut "$SMPROGRAMS\xacc.ide\xacc.sdk.lnk" "$INSTDIR\xacc.sdk.chm"
;  File "xacc.xml"
;  File "xacc.sdk.chm"
;SectionEnd

;Section "Samples" SEC05
;SectionIn 1
;  SetOutPath "$INSTDIR\Samples"
;  File /r Sample\*.*
;SectionEnd

;Section "Debug support" SEC06
;SectionIn 1
;SetOutPath "$INSTDIR"
;File "xacc.pdb"
;File "xacc.ide.pdb"
;SectionEnd

;Section "Optimize" SEC07
;SectionIn 1
;	SetOutPath "$INSTDIR"
;ExecShell "print" "$INSTDIR\readme.txt"
;SectionEnd

Section -AdditionalIcons
  CreateShortCut "$SMPROGRAMS\xacc.ide\xacc.uninstall.lnk" "$INSTDIR\xacc.uninstall.exe"
SectionEnd

Section -Post
  DetailPrint "Generating native images..."
  nsExec::ExecToStack '"$NETPATH\ngen.exe" install xacc.lexers.managed.dll'
  WriteUninstaller "$INSTDIR\xacc.uninstall.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\xacc.ide.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\xacc.uninstall.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\xacc.ide.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd

Function un.IsDotNETInstalled
   Push $0
   Push $1
   Push $2
   Push $3
   Push $4

   ReadRegStr $4 HKEY_LOCAL_MACHINE \
     "Software\Microsoft\.NETFramework" "InstallRoot"
   # remove trailing back slash
   Push $4
   Exch $EXEDIR
   Exch $EXEDIR
   Pop $4
   # if the root directory doesn't exist .NET is not installed
   IfFileExists $4 0 noDotNET

   StrCpy $0 0

   EnumStart:

     EnumRegKey $2 HKEY_LOCAL_MACHINE \
       "Software\Microsoft\.NETFramework\Policy"  $0
     IntOp $0 $0 + 1
     StrCmp $2 "" noDotNET

     StrCpy $1 0

     EnumPolicy:

       EnumRegValue $3 HKEY_LOCAL_MACHINE \
         "Software\Microsoft\.NETFramework\Policy\$2" $1
       IntOp $1 $1 + 1
        StrCmp $3 "" EnumStart
         IfFileExists "$4\v2.0.$3" foundDotNET EnumPolicy

   noDotNET:
     StrCpy $0 0
     Goto done

   foundDotNET:
     StrCpy $0 "$4\v2.0.$3"

   done:
     Pop $4
     Pop $3
     Pop $2
     Pop $1
     Exch $0
FunctionEnd

Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer."
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
  Abort
FunctionEnd

Section Uninstall
  Call un.IsDotNETInstalled
  Pop $NETPATH
  DetailPrint "Removing native images..."
  nsExec::ExecToStack '"$NETPATH\ngen.exe" uninstall "$INSTDIR\xacc.lexers.managed.dll"'
  Delete "$DESKTOP\xacc.ide.lnk"
  Delete "$SMPROGRAMS\xacc.ide\xacc.uninstall.lnk"
  Delete "$SMPROGRAMS\xacc.ide\xacc.ide.lnk"
	Delete "$SMPROGRAMS\xacc.ide\xacc.languagedesigner.lnk"
  Delete "$SMPROGRAMS\xacc.ide\xacc.license.lnk"
	Delete "$SMPROGRAMS\xacc.ide\xacc.config.lnk"
	Delete "$SMPROGRAMS\xacc.ide\xacc.sdk.lnk" 
	Delete "$SMPROGRAMS\xacc.ide\xacc.ide (Debug mode).lnk"
	
  RMDir "$SMPROGRAMS\xacc.ide"
  RMDir /r "$INSTDIR"

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  SetAutoClose true
SectionEnd
