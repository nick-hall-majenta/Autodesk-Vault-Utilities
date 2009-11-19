@echo off 
if "%1"=="" goto NOPARAM 
set VAULTNAME=[%1] 

osql -E -S ".\AutodeskVault" -Q "Use KnowledgeVaultMaster; Exec sp_MSForEachTable 'Update Statistics ? WITH FULLSCAN';" -o "resultsKnowledgeVaultMaster.txt"
osql -E -S ".\AutodeskVault" -Q "Use %VAULTNAME%; Exec sp_MSForEachTable 'Update Statistics ? WITH FULLSCAN';" -o "results%VAULTNAME%.txt"

NET STOP MSSQL$AUTODESKVAULT
NET START MSSQL$AUTODESKVAULT

goto EXIT 
 
:NOPARAM 
echo [FAIL] Please indicate Vault database 
 
pause 
:EXIT