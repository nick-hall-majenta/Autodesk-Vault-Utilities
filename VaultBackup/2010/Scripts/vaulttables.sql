USE VAULT 
GO
select name from sysobjects where type = 'U' order by name
GO