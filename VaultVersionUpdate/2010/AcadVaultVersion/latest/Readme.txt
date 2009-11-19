
AcadVaultVersion AutoCAD .NET API

(c) Alta Systems, 2009

Installation
------------

This tool contains 5 files 

AcadVaultVersion.bmp
AcadVaultVersion.dll
VaultVersion.dll
AcadVaultVersion.reg
Readme.txt (this file)

Copy the following files to a folder

AcadVaultVersion.bmp
AcadVaultVersion.dll
VaultVersion.dll

Set up the following Registry entries

In HKEY_CURRENT_USER\SOFTWARE\Alta Systems\Vault Tools

Server = The name of your Vault server
UserName = Your username on your Vault server
Password = Your password on your Vault server (optional)
Vault = The vault you are using on your Vault server
Working Folder = Your vault working folder

In HKEY_CURRENT_USER\SOFTWARE\Alta Systems\Vault Tools\AutoCADVersion

TitleBlock = The name of your title block
VersionAttribute = The tag of the attribute in your title block that shows 
                   the drawing version (optional if RevisionAttribute present)
RevisionAttribute = The tag of the attribute in your title block that shows 
                    the drawing revision (optional if VersionAttribute present)

AcadVaultVersion.reg will set up all of the above keys, most will be set to a 
null string, so will need editing first.

Running the tool
----------------

Use NETLOAD witrhin AutoCAD to load AcadVaultVersion.dll

One command is implemented - ACADVAULTVERSION

You should see a toolbar, with one button which calls ACADVAULTVERSION

ACADVAULTVERSION reads the version of the drawing you are working on from 
Vault, and sets the specified attribute in the specified block to the value
retrieved.


Alta Systems
16/06/2009