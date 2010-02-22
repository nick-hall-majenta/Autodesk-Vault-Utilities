@echo off
echo.

if #%1# == ## goto syntax

if not exist %~dp0gawk.exe goto nogawk
echo gawk.exe found

if not exist %~dp0DwgUpdater.awk goto noawkprogram
echo DwgUpdater.awk found

if not exist %~dp0DwgUpdater.txt goto noheader
echo DwgUpdater.txt found

if not exist %~dp0DwgUpdater.scr goto noscript
echo DwgUpdater.scr found

type %~dp0DwgUpdater.txt > %~dp0DwgUpdateRun.bat
echo set SCRIPT="%~dp0DwgUpdater.scr" >> %~dp0DwgUpdateRun.bat
echo. >> %~dp0DwgUpdateRun.bat

%~dp0gawk.exe -F, -f%~dp0DwgUpdater.awk %1 >> %~dp0DwgUpdateRun.bat

%~dp0DwgUpdateRun.bat

goto end

:syntax
echo Syntax: DwgUpdater.bat AutoloaderCSV.csv
goto end

:nogawk
echo Cannot find gawk.exe in %~dp0
goto end

:noawkprogram
echo Cannot find DwgUpdater.awk in %~dp0
goto end

:noheader
echo Cannot find DwgUpdater.txt in %~dp0
goto end

:noscript
echo Cannot find DwgUpdater.scr in %~dp0
goto end

:end
echo.

