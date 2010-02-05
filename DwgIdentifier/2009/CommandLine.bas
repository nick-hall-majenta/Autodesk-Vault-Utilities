Attribute VB_Name = "CommandLine"
Option Explicit

Private oApprenticeApp As ApprenticeServerComponent
Private oApprenticeServerDoc As ApprenticeServerDocument

Public Const sVersion As String = "1.0.0"
Public Const sDate As String = "08/07/2009"

Sub Main()
  Dim sData() As String
  CL_Get sData
  Dim bNobanner As Boolean
  bNobanner = CL_GetBParam(sData, "nobanner")
  Dim sFile As String
  sFile = sData(UBound(sData))
    
  If InIDE Then
    Dim iParam As Long
    For iParam = LBound(sData) To UBound(sData)
      WriteStdOut Format(iParam + 1, "00") & ": " & sData(iParam), True
    Next iParam
    WriteStdOut "sFile is " & sFile, True
    If bNobanner Then
      WriteStdOut "nobanner is true", True
    Else
      WriteStdOut "nobanner is false", True
    End If
  End If
   
  If Not bNobanner Then
    WriteStdOut "Autodesk DWG Identifier Tool V" + sVersion + " (" + sDate + ")", True
    WriteStdOut "(c) 2009 Alta Systems Ltd", True
    WriteStdOut "", True
  End If
  
  If sFile <> "" And Dir(sFile) = "" Then
    WriteStdOut "Error: Cannot find specified file", True
    WriteStdOut "", True
    Call Syntax
  Else
    If Len(sFile) >= 4 And UCase(Right(Command$, 4)) <> ".DWG" Then
      WriteStdOut "Error: Specified file is not a DWG file", True
      WriteStdOut "", True
      Call Syntax
    Else
      If sFile = "" Or Dir(sFile) = "" Then
        Call Syntax
      Else
        Dim bApprenticeOK As Boolean
        bApprenticeOK = True
        On Error GoTo appserver1
        Set oApprenticeApp = New ApprenticeServerComponent
        GoTo appserver2:
appserver1:
        bApprenticeOK = False
appserver2:
        If bApprenticeOK Then
          Dim bOpenedOK As Boolean
          bOpenedOK = True
          On Error GoTo openerr1
          Set oApprenticeServerDoc = oApprenticeApp.Open(sFile)
          GoTo openerr2
openerr1:
          bOpenedOK = False
openerr2:
          On Error GoTo 0
          If bOpenedOK = False Then
            WriteStdOut "AutoCAD DWG: " & sFile, True
            WriteStdOut "", True
          Else
            WriteStdOut "Inventor DWG: " & sFile, True
            WriteStdOut "", True
          End If
        Else
          WriteStdOut "Error: Cannot connect to Appentice Server component", True
          WriteStdOut "", True
        End If
      End If
    End If
  End If
End Sub

Sub Syntax()
  WriteStdOut "Syntax: DwgIdentifier [-nobanner] file", True
  WriteStdOut "", True
End Sub
