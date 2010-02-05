Attribute VB_Name = "ModStdIO"
Option Explicit

Private Declare Function GetStdHandle Lib "kernel32" (ByVal nStdHandle As Long) As Long
Private Declare Function ReadFile Lib "kernel32" (ByVal hFile As Long, _
    lpBuffer As Any, ByVal nNumberOfBytesToRead As Long, _
    lpNumberOfBytesRead As Long, lpOverlapped As Any) As Long
Private Declare Function WriteFile Lib "kernel32" (ByVal hFile As Long, _
    lpBuffer As Any, ByVal nNumberOfBytesToWrite As Long, _
    lpNumberOfBytesWritten As Long, lpOverlapped As Any) As Long

Private Const STD_OUTPUT_HANDLE = -11&
Private Const STD_INPUT_HANDLE = -10&

Function ReadStdIn(Optional ByVal NumBytes As Long = -1) As String
    Dim StdIn As Long
    Dim Result As Long
    Dim Buffer As String
    Dim BytesRead As Long
    StdIn = GetStdHandle(STD_INPUT_HANDLE)
    Buffer = Space$(1024)
    Do
        Result = ReadFile(StdIn, ByVal Buffer, Len(Buffer), BytesRead, ByVal 0&)
        If Result = 0 Then
            Err.Raise 1001, , "Unable to read from standard input"
        End If
        ReadStdIn = ReadStdIn & Left$(Buffer, BytesRead)
    Loop Until BytesRead < Len(Buffer)
End Function

Function WriteStdOut(ByVal Text As String, bAddCrLf As Boolean)
  If InIDE Then
    Debug.Print Text
  Else
    Dim StdOut As Long
    Dim Result As Long
    Dim BytesWritten As Long
    StdOut = GetStdHandle(STD_OUTPUT_HANDLE)
    Result = WriteFile(StdOut, ByVal Text, Len(Text), BytesWritten, ByVal 0&)
    If Result = 0 Then
        Err.Raise 1001, , "Unable to write to standard output"
    ElseIf BytesWritten < Len(Text) Then
        Err.Raise 1002, , "Incomplete write operation"
    End If
    If bAddCrLf Then
      WriteStdOut vbCrLf, False
    End If
  End If
End Function


Public Function InIDE() As Boolean
  On Error Resume Next
  Debug.Assert 1 / 0
  InIDE = (Err <> 0)
  On Error GoTo 0
End Function



