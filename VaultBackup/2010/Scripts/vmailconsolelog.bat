@echo off

REM Emails todays ADMSConsoleLog 
REM Emails result from %emailfrom%
REM Emails result to %emailto%
REM Using SMTP server %emailserver%

REM blat.exe must be in %installfolder%
REM http://sourceforge.net/projects/blat/

set installfolder=C:\VaultBackup

set blankfile=%installfolder%\blank.txt

set emailserver=192.168.1.5
set emailfrom=vault@vault.com
set emailto=vaultadmin@vault.com
set logfolder=C:\Documents and Settings\All Users\Application Data\Autodesk\VaultServer\FileStore

for /f "Tokens=1-4 Delims=/ " %%i in ('date /t') do  set today=%%k%%j%%i

set emailfile=%logfolder%\ADMSConsoleLog-%today%.txt
set emailsubject=Vault ADMSConsole Log

if exist "%emailfile%" goto consolelog
set emailfile=%blankfile%
set emailsubject=Vault ADMSConsole Log not found

:consolelog
echo Sending %emailsubject%
echo.
echo To %emailto%
echo.
echo Attached %emailfile%

%installfolder%\blat "%emailfile%" -server %emailserver% -f %emailfrom% -to %emailto% -subject "%emailsubject%" > NUL
goto ELA%errorlevel%

:ELA0
echo Log file emailed
goto VLOG

:ELA1
echo Cannot email log file
goto VLOG

:VLOG
set emailfile=%logfolder%\vlog-%today%.txt
set emailsubject=Vault Log

if exist "%emailfile%" goto vaultlog
set emailfile=%blankfile%
set emailsubject=Vault Log not found
goto vaultlogsend

:vaultlog
copy "%emailfile%" %installfolder%\vlog-%today%.txt > NUL
set emailfile=%installfolder%\vlog-%today%.txt

:vaultlogsend
echo Sending %emailsubject%
echo.
echo To %emailto%
echo.
echo Attached %emailfile%

%installfolder%\blat "%emailfile%" -server %emailserver% -f %emailfrom% -to %emailto% -subject "%emailsubject%" > NUL
goto ELB%errorlevel%

:ELB0
echo Log file emailed
if exist %emailfile% del /F /Q %emailfile%
goto END

:ELB1
echo Cannot email log file
goto END

:END
