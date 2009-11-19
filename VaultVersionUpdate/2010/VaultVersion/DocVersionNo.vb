

Imports VaultVersion.Document
Imports VaultVersion.Security

Public Class DocVersionNo

  Public Function GetVersion(ByVal sServerName As String, ByVal sVaultName As String, ByVal sUserName As String, ByVal sPassword As String, ByVal sFileName As String) As Integer
    ' Returns version number
    ' or
    ' -1 - Cannot login to Vault
    ' -2 - Error traversing tree
    ' -3 - Found path but not file
    ' -4 - 
    GetVersion = -3
    Dim secSrv As SecurityService
    Dim docSrv As DocumentService
    Try
      secSrv = New SecurityService()
      secSrv.SecurityHeaderValue = New Security.SecurityHeader()
      secSrv.Url = "http://" + sServerName + "/AutodeskDM/Services/SecurityService.asmx"
      secSrv.SignIn(sUserName, sPassword, sVaultName)

      docSrv = New DocumentService()
      docSrv.SecurityHeaderValue = New Document.SecurityHeader()
      docSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId
      docSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket
      docSrv.Url = "http://" + sServerName + "/AutodeskDM/Services/DocumentService.asmx"
    Catch ex As Exception
      GetVersion = -1
      Exit Function
    End Try
    Try
      Dim folderCurrent As Folder = docSrv.GetFolderRoot()
      Debug.Print(sFileName)
      If sFileName.Substring(0, 1) = "$" Then
        sFileName = sFileName.Substring(1)
      End If
      If sFileName.Substring(0, 1) = "/" Then
        sFileName = sFileName.Substring(1)
      End If
      Debug.Print(sFileName)
      Dim iCnt As Integer
      Dim iLastSlash As Integer = 0
      For iCnt = 0 To sFileName.Length - 1
        If sFileName.Substring(iCnt, 1) = "/" Then
          Dim sFolderName As String
          sFolderName = sFileName.Substring(iLastSlash, iCnt - iLastSlash)
          Dim foldersSub As Folder() = docSrv.GetFoldersByParentId(folderCurrent.Id, False)
          If (Not foldersSub Is Nothing AndAlso foldersSub.Length > 0) Then
            For Each folderThis As Folder In foldersSub
              If folderThis.Name.ToUpper() = sFolderName.ToUpper() Then
                Debug.Print(sFolderName)
                folderCurrent = folderThis
                Debug.Print(folderCurrent.FullName)
              End If
            Next folderThis
          End If
          iLastSlash = iCnt + 1
        End If
      Next
      Debug.Print(sFileName.Substring(iLastSlash))
      Dim fileCollection As File() = docSrv.GetLatestFilesByFolderId(folderCurrent.Id, False)
      If (Not fileCollection Is Nothing AndAlso fileCollection.Length > 0) Then
        For Each fileThis As File In fileCollection
          If fileThis.Name.ToUpper() = sFileName.Substring(iLastSlash).ToUpper() Then
            GetVersion = fileThis.VerNum
          End If
        Next fileThis
      End If
    Catch ex As Exception
      GetVersion = -2
    End Try
  End Function

  Public Function GetRevision(ByVal sServerName As String, ByVal sVaultName As String, ByVal sUserName As String, ByVal sPassword As String, ByVal sFileName As String) As String
    ' Returns version number
    ' or
    ' -1 - Cannot login to Vault
    ' -2 - Error traversing tree
    ' -3 - Found path but not file
    ' -4 - 
    GetRevision = "-3"
    Dim secSrv As SecurityService
    Dim docSrv As DocumentService
    Try
      secSrv = New SecurityService()
      secSrv.SecurityHeaderValue = New Security.SecurityHeader()
      secSrv.Url = "http://" + sServerName + "/AutodeskDM/Services/SecurityService.asmx"
      secSrv.SignIn(sUserName, sPassword, sVaultName)

      docSrv = New DocumentService()
      docSrv.SecurityHeaderValue = New Document.SecurityHeader()
      docSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId
      docSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket
      docSrv.Url = "http://" + sServerName + "/AutodeskDM/Services/DocumentService.asmx"
    Catch ex As Exception
      GetRevision = "-1"
      Exit Function
    End Try
    Try
      Dim folderCurrent As Folder = docSrv.GetFolderRoot()
      Debug.Print(sFileName)
      If sFileName.Substring(0, 1) = "$" Then
        sFileName = sFileName.Substring(1)
      End If
      If sFileName.Substring(0, 1) = "/" Then
        sFileName = sFileName.Substring(1)
      End If
      Debug.Print(sFileName)
      Dim iCnt As Integer
      Dim iLastSlash As Integer = 0
      For iCnt = 0 To sFileName.Length - 1
        If sFileName.Substring(iCnt, 1) = "/" Then
          Dim sFolderName As String
          sFolderName = sFileName.Substring(iLastSlash, iCnt - iLastSlash)
          Dim foldersSub As Folder() = docSrv.GetFoldersByParentId(folderCurrent.Id, False)
          If (Not foldersSub Is Nothing AndAlso foldersSub.Length > 0) Then
            For Each folderThis As Folder In foldersSub
              If folderThis.Name.ToUpper() = sFolderName.ToUpper() Then
                Debug.Print(sFolderName)
                folderCurrent = folderThis
                Debug.Print(folderCurrent.FullName)
              End If
            Next folderThis
          End If
          iLastSlash = iCnt + 1
        End If
      Next
      Debug.Print(sFileName.Substring(iLastSlash))
      Dim fileCollection As File() = docSrv.GetLatestFilesByFolderId(folderCurrent.Id, False)
      If (Not fileCollection Is Nothing AndAlso fileCollection.Length > 0) Then
        For Each fileThis As File In fileCollection
          If fileThis.Name.ToUpper() = sFileName.Substring(iLastSlash).ToUpper() Then
            GetRevision = fileThis.FileRev.Label
          End If
        Next fileThis
      End If
    Catch ex As Exception
      GetRevision = "-2"
    End Try
  End Function
End Class
