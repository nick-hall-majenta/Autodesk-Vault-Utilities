@echo off 
if "%1"=="" goto NOPARAM 
set VAULTNAME=[%1] 
 
@echo Setting database recovery model to simple...  
osql -E -S ".\AutodeskVault" -Q "ALTER DATABASE %VAULTNAME% SET RECOVERY SIMPLE" 
@echo Setting database Autogrowth value...  
osql -E -S ".\AutodeskVault" -Q "ALTER DATABASE %VAULTNAME%  MODIFY FILE(NAME=%VAULTNAME%, FILEGROWTH=100MB)" 
@echo Shrinking %1 database... 
osql -E -S ".\AutodeskVault" -Q "USE %VAULTNAME% DBCC SHRINKDATABASE(%VAULTNAME%, 10)" 
 
@echo Reindexing %1 database... 
 osql -E -S ".\AutodeskVault" -Q "USE %VAULTNAME% DECLARE tableCursor CURSOR FOR SELECT NAME FROM sysobjects WHERE xtype in('U') DECLARE @tableName nvarchar(128) OPEN tableCursor FETCH NEXT FROM tableCursor INTO @tableName WHILE @@FETCH_STATUS = 0 BEGIN DBCC DBREINDEX(@tableName, '') FETCH NEXT FROM tableCursor INTO @tableName END CLOSE tableCursor DEALLOCATE tableCursor" 
 
@echo Updating statistics on %1 database... 
osql -E -S ".\AutodeskVault" -Q "EXEC sp_updatestats" 
 
goto EXIT 
 
:NOPARAM 
echo [FAIL] Please indicate Vault database 
 
pause 
:EXIT
