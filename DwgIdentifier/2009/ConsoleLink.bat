@echo off
echo Compiling DwgIdentifier.vbp
"C:\Program Files\Microsoft Visual Studio\VB98\VB6.EXE" /m DwgIdentifier.vbp
echo Linking DwgIdentifier.exe for console
"C:\Program Files\Microsoft Visual Studio\vb98\LINK.EXE" /EDIT /SUBSYSTEM:CONSOLE DwgIdentifier.exe
pause