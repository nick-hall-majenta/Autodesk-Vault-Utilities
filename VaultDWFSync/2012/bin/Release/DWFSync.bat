@echo off
set server=localhost
set vault=DemoVault
set username=Administrator
set password=""
set rootfolder="C:\temp\vault"

echo VaultDWFSync > VaultDWFSync.log
echo. >> VaultDWFSync.log

if not exist %rootfolder% goto NOFOLDER
rd /S /Q %rootfolder% >> VaultDWFSync.log
:NOFOLDER
md %rootfolder% >> VaultDWFSync.log
VaultDWFSync.exe -server %server% -vault %vault% -username %username% -password %password% -rootfolder %rootfolder% >> VaultDWFSync.log