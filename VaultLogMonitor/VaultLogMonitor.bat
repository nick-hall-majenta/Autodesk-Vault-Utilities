:
: Vault Log Monitor
: Version 1.0
: (c) Majenta Solutions 2011
:
: This software is provided "as is" and any expressed or implied warranties,
: including, but not limited to, the implied warranties of merchantability and
: fitness for a particular purpose are disclaimed. in no event shall the 
: regents or contributors be liable for any direct, indirect, incidental, 
: special, exemplary, or consequential damages (including, but not limited to,
: procurement of substitute goods or services; loss of use, data, or profits;
: or business interruption) however caused and on any theory of liability,
: whether in contract, strict liability, or tort (including negligence or
: otherwise) arising in any way out of the use of this software, even if
: advised of the possibility of such damage.

@echo off
:
: Change these as required
:
set windowssystemfolder=C:\Windows\System32
set installfolder=C:\VaultLogMonitor
set logfolder=C:\ProgramData\Autodesk\VaultServer\FileStore
set emailserver=192.168.1.5
set emailfrom=vault@vault.com
set emailto=vaultadmin@vault.com
set emailsubject=Vault Log Monitor

set optionaltestfile=%installfolder%\optionaltest.bat
set optionalresultfile=%installfolder%\optionalresult.txt
set optionalactionfile=%installfolder%\optionalaction.bat

set savedlogsfolder=%installfolder%\Logs
if not exist "%failedlogsfolder%" mkdir "%failedlogsfolder%"

: Console Log File is ADMSConsoleLog-YYYYMMDD.txt
: VLog File is vlog-YYYYMMDD.txt

for /f "Tokens=1-2 Delims=:" %%i in ('time /t') do set logtime=%%i%%j
for /f "Tokens=1-4 Delims=/ " %%i in ('date /t') do  set logdate=%%k%%j%%i

set lastconsolelogfile=%installfolder%\lastconsolelog.txt
set lastvlogfile=%installfolder%\lastvlog.txt

set timestampfile=%installfolder%\timestamp-%logdate%
if exist "%timestampfile%" GOTO TIMESTAMPOK
echo Timestamp file not found - changing date
if exist "%installfolder%\timestamp-*" del "%installfolder%\timestamp-*" > NUL
"%installfolder%\touch.exe" "%timestampfile%"
if exist "%lastconsolelogfile%" del "%lastconsolelogfile%" > NUL
if exist "%lastvlogfile%" del "%lastvlogfile%"> NUL
:TIMESTAMPOK

set consolelogdiff=%installfolder%\consolelogdiff.txt
set vlogdiff=%installfolder%\vlogdiff.txt

set consolelogfound=%installfolder%\consolelogfound.txt
set vlogfound=%installfolder%\vlogfound.txt

set consolelogawk=%installfolder%\consolelog.awk
set vlogawk=%installfolder%\vlog.awk

if not exist "%lastconsolelogfile%" "%installfolder%\touch.exe" "%lastconsolelogfile%"
if not exist "%lastvlogfile%" "%installfolder%\touch.exe" "%lastvlogfile%"

set consolelogfile=%logfolder%\ADMSConsoleLog-%logdate%.txt
set vlogfile=%logfolder%\vlog-%logdate%.txt
if not exist "%consolelogfile%" "%installfolder%\touch.exe" "%consolelogfile%"
if not exist "%vlogfile%" "%installfolder%\touch.exe" "%vlogfile%"
"%installfolder%\diff.exe" "%lastconsolelogfile%" "%consolelogfile%" | "%installfolder%\grep.exe" "> .*" | "%installfolder%\sed.exe" "s/> //" > "%consolelogdiff%"
"%installfolder%\diff.exe" "%lastvlogfile%" "%vlogfile%" | "%installfolder%\grep.exe" "> .*" | "%installfolder%\sed.exe" "s/> //" > "%vlogdiff%"

copy "%consolelogfile%" "%lastconsolelogfile%" > NUL
copy "%vlogfile%" "%lastvlogfile%" > NUL

"%installfolder%\gawk.exe" -f "%consolelogawk%" "%consolelogdiff%" > "%consolelogfound%"
"%installfolder%\gawk.exe" -f "%vlogawk%" "%vlogdiff%" > "%vlogfound%"

FOR /F "usebackq" %%A IN ('%consolelogfound%') DO set consolelogfsize=%%~zA
FOR /F "usebackq" %%A IN ('%vlogfound%') DO set vlogfsize=%%~zA

if %consolelogfsize% GTR 0 (
"%installfolder%\blat" "%consolelogfound%": -server %emailserver% -f %emailfrom% -to %emailto% -subject "%emailsubject% - Console Log" > NUL
copy "%consolelogfound%" "%savedlogsfolder%\ConsoleLog-%logdate%-%logtime%.txt" > NUL
)

if %vlogfsize% GTR 0 (
"%installfolder%\blat" "%vlogfound%": -server %emailserver% -f %emailfrom% -to %emailto% -subject "%emailsubject% - VLog" > NUL
copy "%vlogfound%" "%savedlogsfolder%\VLog-%logdate%-%logtime%.txt" > NUL
)

: If the optional test batch file exists, call it 
: It must output the optional result file
: If the optional result file exists and is larger than 0 bytes, the optional action batch file will be run
if not exist "%optionaltestfile%" GOTO END
set optionaltestfile=%installfolder%\optionaltest.bat
if exist "%optionalresultfile%" del "%optionalresultfile%"
call "%optionaltestfile%" "%consolelogdiff%" "%vlogdiff%"
if not exist "%optionalresultfile%" GOTO END
FOR /F "usebackq" %%A IN ('%optionalresultfile%') DO set orfsize=%%~zA
if %orfsize% GTR 0 (
call "%optionalactionfile%"
)

:END
if exist "%optionalresultfile%" del "%optionalresultfile%"

if exist %consolelogdiff% del %consolelogdiff%
if exist %vlogdiff% del %vlogdiff%

if exist %consolelogfound% del %consolelogfound%
if exist %vlogfound% del %vlogfound%
