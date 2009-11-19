@echo off
setlocal enableextensions
REM Kills ADMS Console if running
REM Checks size of SQL Server databases
REM Backs up Vault 
REM Emails logfile from %emailfrom%
REM Emails logfile to %emailto%
REM Using SMTP server %emailserver%
REM If email fails, copies logs to %failedlogsfolder%

REM blat.exe must be in %installfolder%
REM http://sourceforge.net/projects/blat/

REM find.exe must be in %installfolder%
REM http://gnuwin32.sourceforge.net/packages/findutils.htm
REM find support files
REM      libiconv2.dll
REM      libintl3.dll
REM      pcre3.dll
REM      regex2.dll

REM process.exe must be in %installfolder%
REM http://www.beyondlogic.org/consulting/processutil/processutil.htm

REM sed.exe must be in %installfolder%
REM awk.exe must be in %installfolder%
REM egrep.exe must be in %installfolder%
REM http://gnuwin32.sourceforge.net/

set installfolder=C:\VaultBackup
set backupfolder=C:\Backups\Vault
set osqlfolder=C:\Program Files\Microsoft SQL Server\90\Tools\Binn
set admsinstallfolder=C:\Program Files\Autodesk\ADMS Manufacturing 2010\ADMS Console
set databasefolder=C:\Program Files\Microsoft SQL Server\MSSQL.1\MSSQL\Data
set failedlogsfolder=%installfolder%\Logs
set blankfile=%installfolder%\blank.txt
set filetosend=%installfolder%\email.txt
set dbthreshold=650M

set emailserver=192.168.1.5
set emailfrom=vault@vault.com
set emailto=vaultadmin@vault.com
set emailsubject=Vault Backup Log
set backupuser=administrator
set backuppassword=

for /f "Tokens=1" %%i in ('time /t') do set tm=%%i
for /f "Tokens=1-4 Delims=/ " %%i in ('date /t') do  set logtime=%%i/%%j/%%k %tm%
set emailsubject=%emailsubject% - %logtime%
for /f "Tokens=1-2 Delims=:" %%i in ('time /t') do set tm=%%i%%j
for /f "Tokens=1-4 Delims=/ " %%i in ('date /t') do  set logtime=%%k%%j%%i%tm%
set logfile=Vault Backup - %logtime%.txt
set emailfile=%installfolder%\%logfile%
set dbreportfile=%installfolder%\databases%logtime%.txt

if exist "%failedlogsfolder%" goto logsfolderexists
echo Creating logs folder %failedlogsfolder%
mkdir %failedlogsfolder%
:logsfolderexists

%installfolder%\process | %installfolder%\grep "Connectivity.ADMSConsole.exe" > NUL
goto EL%errorlevel%

:EL0
echo ADMS Console is running. Attempting to close
%installfolder%\process -k Connectivity.ADMSConsole.exe > nul
%installfolder%\process | %installfolder%\grep "Connectivity.ADMSConsole.exe" > NUL
goto ELL%errorlevel%

:ELL0
echo ADMS Console cannot be closed
echo. > "%emailfile%"
echo Vault Backup Failed >> "%emailfile%"
echo. >> "%emailfile%"
echo ADMS Console cannot be closed >> "%emailfile%"
echo. >> "%emailfile%"
set emailsubject=%emailsubject% - Failed [ADMS Console cannot be closed]
goto EMAIL

:ELL1
:EL1

echo Checking database sizes (threshold %dbthreshold%)
echo Databases larger than threshold (%dbthreshold%) > %dbreportfile%
echo. >> %dbreportfile%
%installfolder%\find "%databasefolder%" -name "*.mdf" -size +%dbthreshold% >> %dbreportfile%
echo Checking database fragmentation
echo. >> %dbreportfile%
echo USE VAULT > %installfolder%\vfragmentation.sql
echo GO >> %installfolder%\vfragmentation.sql
"%osqlfolder%\osql" -E -S ".\AutodeskVault" -i %installfolder%\vaulttables.sql | %installfolder%\egrep "^ [^-].*" | %installfolder%\sed "s/^ /DBCC SHOWCONTIG \('/" | %installfolder%\sed "s/ *$/'\)\nGO/" >> %installfolder%\vfragmentation.sql
"%osqlfolder%\osql" -E -S ".\AutodeskVault" -i %installfolder%\vfragmentation.sql | %installfolder%\egrep "Fragment|Table" | %installfolder%\sed "s/' (.*/'/" | %installfolder%\sed "s/^Table/\nTable/"  | %installfolder%\awk -F : -f %installfolder%\vfragmentation.awk >> %dbreportfile%

if not exist "%installfolder%\Vault Backup*.txt" goto nologfiles
echo Deleting old log files
del /F /Q "%installfolder%\Vault Backup*.txt"
:nologfiles

if exist "%backupfolder%" goto backupexists
echo Creating backup folder %backupfolder%
mkdir %backupfolder%
:backupexists

REM Delete B and cascade A backup subdirectories
if not exist "%backupfolder%\B" goto nobackupb
echo Removing 'B' backup files
rmdir /Q /S "%backupfolder%\B"
:nobackupb

if not exist "%backupfolder%\A" goto nobackupa
echo Moving 'A' backup files to 'A' backup
ren "%backupfolder%\A" B
:nobackupa

REM Create a new directory for the backup
if exist "%backupfolder%\A" goto backupaexists
echo Making 'A' backup folder
mkdir "%backupfolder%\A\"
:backupaexists

echo Creating 'A' backup
"%admsinstallfolder%\Connectivity.ADMSConsole.exe" -Obackup -B"%backupfolder%\A" -VU%backupuser% -VP%backuppassword% -S -L"%emailfile%" -DBSC

:EMAIL
echo Emailing report
echo Autodesk Vault Backup Report > %filetosend%
echo. >> %filetosend%
type "%dbreportfile%" >> %filetosend%
echo. >> %filetosend%
type "%emailfile%" >> %filetosend%

%installfolder%\blat "%filetosend%" -server %emailserver% -f %emailfrom% -to %emailto% -subject "%emailsubject%" > NUL
goto ELB%errorlevel%

:ELB1
echo Cannot email log file
if exist "%dbreportfile%" copy "%dbreportfile%" "%failedlogsfolder%\databases%logtime%.txt" > NUL
if exist "%emailfile%" copy "%emailfile%" "%failedlogsfolder%\%logfile%" > NUL
goto END

:ELB0
echo Log file emailed
goto END


:END
if exist "%dbreportfile%" del /F /Q "%dbreportfile%"
if exist "%filetosend%" del /F /Q "%filetosend%"
if exist "%emailfile%" del /F /Q "%emailfile%"
setlocal disableextensions
