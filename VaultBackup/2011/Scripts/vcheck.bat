@echo off
setlocal enableextensions

REM Checks that vault is running
REM and tries to restart it

REM Emails result from %emailfrom%
REM Emails result to %emailto%
REM Using SMTP server %emailserver%
REM Run Time & Date are in the email subject
REM If email fails, copies logs to %failedlogsfolder%

REM blat.exe must be in %installfolder%
REM http://sourceforge.net/projects/blat

REM wget.exe must be in %installfolder%
REM http://www.christopherlewis.com/WGet/WGetFiles.htm
REM wget support files
REM      libeay32.dll
REM      msvcr71.dll
REM      msvcr80.dll
REM      openssl.exe
REM      ssleay32.dll
REM      wget.hlp

set installfolder=C:\VaultBackup
set failedlogsfolder=%installfolder%\Logs

set emailserver=192.168.1.5
set emailfrom=vault@vault.com
set emailto=vaultadmin@vault.com

set url=http://localhost/AutodeskDM/Services/SecurityService.asmx

if exist "%failedlogsfolder%" goto logsfolderexists
echo Creating logs folder %failedlogsfolder%
mkdir %failedlogsfolder%
:logsfolderexists

for /f "Tokens=1" %%i in ('time /t') do set tm=%%i
for /f "Tokens=1-4 Delims=/ " %%i in ('date /t') do  set datetime=%%i/%%j/%%k %tm%
set emailheader=Vault Check - %datetime%
for /f "Tokens=1-2 Delims=:" %%i in ('time /t') do set tm=%%i%%j
for /f "Tokens=1-4 Delims=/ " %%i in ('date /t') do  set logtime=%%k%%j%%i%tm%

set emailfile=%installfolder%\Vault Check - %logtime%.log
set wgetfile=%installfolder%\Vault Check - %logtime%.txt

if exist "%emailfile%" del /F /Q "%emailfile%"
if exist "%wgetfile%" del /F /Q "%wgetfile%"

echo Checking for working Vault
%installfolder%\wget -o "%emailfile%" -O "%wgetfile%" %url%
goto EL%errorlevel%

:EL0
echo Vault is running
set emailsubject=%emailheader% - Running
goto END

:EL1
echo Vault is stopped
echo Attempting to start vault
IISRESET /STOP > NUL
NET STOP MSSQL$AUTODESKVAULT > NUL
NET START MSSQL$AUTODESKVAULT > NUL
IISRESET /START > NUL
%installfolder%\wget -o "%emailfile%" -O "%wgetfile%" %url%
goto ELR%errorlevel%

:ELR0
echo Vault has been restarted
set emailsubject=%emailheader% - Restarted
goto END

:ELR1
echo Vault cannot be restarted
set emailsubject=%emailheader% - Stopped
goto END

:END

echo Sending %emailsubject%
echo To %emailto%
echo.

%installfolder%\blat "%emailfile%" -server %emailserver% -f %emailfrom% -to %emailto% -subject "%emailsubject%" > NUL

goto ELB%errorlevel%

:ELB1
echo Cannot email log file
echo. >> "%emailfile%"
echo %emailsubject% >> "%emailfile%"
if exist "%emailfile%" copy "%emailfile%" "%failedlogsfolder%" > NUL
goto END

:ELB0
echo Log file emailed
goto END


:END
if exist "%emailfile%" del /F /Q "%emailfile%"
if exist "%wgetfile%" del /F /Q "%wgetfile%"
setlocal disableextensions

