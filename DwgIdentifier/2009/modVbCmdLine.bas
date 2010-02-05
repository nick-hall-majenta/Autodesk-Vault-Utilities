Attribute VB_Name = "modVbCmdLine"
Option Explicit

Private Const sQuote As String = """"
Private Const sSpace As String = " "

Private Sub CL_AddItem(ByRef sCLParam() As String, ByVal sNewString As String)
  'Code to add a new item into the params array
  
  'First off all check if array needs resizing or if we can use element zero
  If sCLParam(0) <> vbNullString Then
      ReDim Preserve sCLParam(UBound(sCLParam) + 1) As String
  End If
  
  'Store the data
  sCLParam(UBound(sCLParam)) = sNewString
  
  'Strip quotes off either side if present
  If Left(sCLParam(UBound(sCLParam)), 1) = sQuote And Right(sCLParam(UBound(sCLParam)), 1) = sQuote Then
      sCLParam(UBound(sCLParam)) = Mid(sCLParam(UBound(sCLParam)), 2, Len(sCLParam(UBound(sCLParam))) - 2)
  End If
  
  
End Sub

Private Function CL_CountChar(ByVal sInput As String, ByVal sChar As String) As Long
  'Code to count the number of occurances of character X in string Y
  Dim iPos As Long
  
  CL_CountChar = 0
  iPos = 1
  Do Until InStr(iPos, sInput, sChar) = 0
      CL_CountChar = CL_CountChar + 1
      iPos = InStr(iPos, sInput, sChar) + Len(sChar)
  Loop
  
End Function

Public Sub CL_Get(ByRef sCLParam() As String)
  Dim sTemp As String
  Dim iCounter As Long, iPos(1) As Long
  
  ReDim sCLParam(0) As String
  sTemp = Command
  
  'ensure that number of " mod 2 = 0
  If CL_CountChar(sTemp, sQuote) Mod 2 = 1 Then
      Exit Sub
  Else
      iPos(0) = 1
      iPos(1) = 1
      Do Until InStr(iPos(0), sTemp, sSpace) = 0
          
          'first of all find the next space
          iPos(1) = InStr(iPos(0), sTemp, sSpace)
          'check if quotes are in use inside this segment
          If CL_CountChar(Mid(sTemp, iPos(0), iPos(1) - iPos(0)), sQuote) Mod 2 = 1 Then
              'if so proceed until the next quote
              iPos(1) = InStr(iPos(1), sTemp, sQuote) + Len(sQuote)
          End If
          
          'check if it is okay to store this param
          If iPos(1) > 0 Then
              CL_AddItem sCLParam, Mid(sTemp, iPos(0), iPos(1) - iPos(0))
              iPos(0) = iPos(1) + 1
          Else
              'just in case the code gets lost somehow...
              Stop
          End If
      
      Loop
      
      'handle any left-over data
      If iPos(0) <= Len(sTemp) Then
          iPos(1) = Len(sTemp) + 1
          CL_AddItem sCLParam, Mid(sTemp, iPos(0), iPos(1) - iPos(0))
      End If
      
  End If
  
End Sub

Function CL_GetBParam(ByRef sCLParam() As String, sParamName As String) As Boolean
  Dim iParam As Long
  CL_GetBParam = False
  For iParam = LBound(sCLParam) To UBound(sCLParam)
    If "-" & sParamName = sCLParam(iParam) Or "/" & sParamName = sCLParam(iParam) Then
      CL_GetBParam = True
    End If
  Next iParam
End Function

Function CL_GetSParam(ByRef sCLParam() As String, sParamName As String) As String
  Dim iParam As Long
  CL_GetSParam = ""
  For iParam = LBound(sCLParam) To UBound(sCLParam)
    If "-" & sParamName = sCLParam(iParam) Or "/" & sParamName = sCLParam(iParam) Then
      If iParam < UBound(sCLParam) Then
        CL_GetSParam = sCLParam(iParam + 1)
      End If
    End If
  Next iParam
End Function

