' Copyright 2004-2006 by Autodesk, Inc.

'Permission to use, copy, modify, and distribute this software in
'object code form for any purpose and without fee is hereby granted, 
'provided that the above copyright notice appears in all copies and 
'that both that copyright notice and the limited warranty and
'restricted rights notice below appear in all supporting 
'documentation.

'AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
'AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
'MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
'DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
'UNINTERRUPTED OR ERROR FREE.

'Use, duplication, or disclosure by the U.S. Government is subject to 
'restrictions set forth in FAR 52.227-19 (Commercial Computer
'Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
'(Rights in Technical Data and Computer Software), as applicable.

' Hello World VB.NET sample
' by Cyrille Fauvel - Autodesk Developer Technical Services
' Copyright Autodesk (c) 2003

Option Explicit On 

Imports System
'Imports System.Type
'Imports System.CLSCompliantAttribute
'Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

Imports Autodesk.AutoCAD.Runtime
Imports Autodesk.AutoCAD.Interop
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput

<Assembly: ExtensionApplication(GetType(AcadVaultVersion.AcadVaultVersionApp))> 
<Assembly: CommandClass(GetType(AcadVaultVersion.AcadVaultVersionCommands))> 

Namespace AcadVaultVersion

  Public Class AcadVaultVersionApp
    Implements Autodesk.AutoCAD.Runtime.IExtensionApplication

    
    Public Sub Initialize() Implements Autodesk.AutoCAD.Runtime.IExtensionApplication.Initialize
      ' Create an AutoCAD toolbar with 4 buttons linked to the 4 commands defined below

      Dim AcadVaultVersionModule As System.Reflection.Module = System.Reflection.Assembly.GetExecutingAssembly().GetModules()(0)
      Dim AcadVaultVersionModulePath As String = AcadVaultVersionModule.FullyQualifiedName
      Try
        AcadVaultVersionModulePath = AcadVaultVersionModulePath.Substring(0, AcadVaultVersionModulePath.LastIndexOf("\"))
      Catch
        MsgBox("Error with Module Path")
        Exit Sub
      End Try

      Dim acadApp As Autodesk.AutoCAD.Interop.AcadApplication = Autodesk.AutoCAD.ApplicationServices.Application.AcadApplication
      Dim hwTb As Autodesk.AutoCAD.Interop.AcadToolbar = acadApp.MenuGroups.Item(0).Toolbars.Add("VaultVersion")
      Dim tbBut0 As Autodesk.AutoCAD.Interop.AcadToolbarItem = hwTb.AddToolbarButton(0, "AcadVaultVersion", "AcadVaultVersion Tool - AcadVaultVersion command", "_ACADVAULTVERSION ")
      tbBut0.SetBitmaps(AcadVaultVersionModulePath + "\AcadVaultVersion.bmp", AcadVaultVersionModulePath + "\AcadVaultVersion.bmp")
    End Sub

    Public Sub Terminate() Implements Autodesk.AutoCAD.Runtime.IExtensionApplication.Terminate
    End Sub

  End Class

  Public Class AcadVaultVersionCommands

    Public Const appversion As String = "1.0.1"
    Public Const acadversion As String = "2010"

    ' Defines a command which prompt a message on the AutoCAD command line
    <Autodesk.AutoCAD.Runtime.CommandMethod("ACADVAULTVERSION")> _
    Public Sub AcadVaultVersionCommand()
      Dim db As Database = HostApplicationServices.WorkingDatabase
      Dim ed As Editor = Application.DocumentManager.MdiActiveDocument.Editor
      ed.WriteMessage(vbCrLf)
      ed.WriteMessage("Autodesk Vault Version Retrieval & Drawing Update" + vbCrLf)
      ed.WriteMessage("  for AutoCAD/Autodesk Vault " + acadversion + vbCrLf)
      ed.WriteMessage("Alta Systems, 2009" + vbCrLf)
      ed.WriteMessage("Version " + appversion + vbCrLf)
      Using trans As Transaction = db.TransactionManager.StartTransaction()
        Dim vv As VaultVersion.DocVersionNo = New VaultVersion.DocVersionNo
        Dim sServer As String = ""
        Dim sUserName As String = ""
        Dim sPassword As String = ""
        Dim sVault As String = ""
        Dim sWorkingFolder As String = ""
        Dim sTitleBlock As String = ""
        Dim sVersionAttribute As String = ""
        Dim sRevisionAttribute As String = ""
        Dim bAllKeysPresent As Boolean = True
        Try
          Dim regHkcu As RegistryKey
          regHkcu = Registry.CurrentUser
          Dim regSoftware As RegistryKey
          Try
            regSoftware = regHkcu.OpenSubKey("Software")
            Dim regAltaSystems As RegistryKey
            regAltaSystems = regSoftware.OpenSubKey("Alta Systems")
            Dim regVaultTools As RegistryKey
            regVaultTools = regAltaSystems.OpenSubKey("Vault Tools")
            sServer = regVaultTools.GetValue("Server")
            If sServer = "" Then bAllKeysPresent = False
            sUserName = regVaultTools.GetValue("UserName")
            If sUserName = "" Then bAllKeysPresent = False
            sPassword = regVaultTools.GetValue("Password")
            sVault = regVaultTools.GetValue("Vault")
            If sVault = "" Then bAllKeysPresent = False
            sWorkingFolder = regVaultTools.GetValue("Working Folder")
            If sWorkingFolder = "" Then bAllKeysPresent = False
            Dim regAutoCADVersion As RegistryKey
            regAutoCADVersion = regVaultTools.OpenSubKey("AutoCADVersion")
            sTitleBlock = regAutoCADVersion.GetValue("TitleBlock")
            If sTitleBlock = "" Then bAllKeysPresent = False
            sVersionAttribute = regAutoCADVersion.GetValue("VersionAttribute")
            sRevisionAttribute = regAutoCADVersion.GetValue("RevisionAttribute")
            If sVersionAttribute = "" And sRevisionAttribute = "" Then bAllKeysPresent = False
          Catch
            ed.WriteMessage("Error: Cannot get registry key" & vbCrLf)
            bAllKeysPresent = False
          End Try
          If Not bAllKeysPresent Then
            ed.WriteMessage("Error: One or more registry keys is null, please check" & vbCrLf)
            writeKeysMessage(ed)
          Else
            Dim sDrawingPath As String = ed.Document.Name
            'sDrawingPath = sDrawingPath.Replace(sWorkingFolder, "")
            sDrawingPath = Strings.Replace(sDrawingPath, sWorkingFolder, "", , , CompareMethod.Text)
            sDrawingPath = sDrawingPath.Replace("\", "/")
            If sDrawingPath.Substring(0, 1) <> "/" Then
              sDrawingPath = "/" & sDrawingPath
            End If
            sDrawingPath = "$" & sDrawingPath
            Dim iVersion As Integer = vv.GetVersion(sServer, sVault, sUserName, sPassword, sDrawingPath)
            Dim sRevision As String = vv.GetRevision(sServer, sVault, sUserName, sPassword, sDrawingPath)
            If iVersion < 0 Then
              ed.WriteMessage("Error: This drawing does not appear to be in the Vault." & vbCrLf)
              ed.WriteMessage("       Server   - " & sServer & vbCrLf)
              ed.WriteMessage("       Username - " & sUserName & vbCrLf)
              ed.WriteMessage("       Password - ******" & vbCrLf)
              ed.WriteMessage("       Vault    - " & sVault & vbCrLf)
              ed.WriteMessage("       Folder   - " & sWorkingFolder & vbCrLf)
              ed.WriteMessage("       Drawing  - " & sDrawingPath & vbCrLf)
            Else
              ed.WriteMessage("Drawing version retrieved from vault - version " & iVersion.ToString() & vbCrLf)
              ed.WriteMessage("Drawing revision retrieved from vault - revision " & sRevision & vbCrLf)
              ed.WriteMessage("Searching for Title Block [" + sTitleBlock.ToUpper() + "]..." & vbCrLf)
              Try
                Dim id As ObjectId
                Dim btrSrc As BlockTableRecord
                Dim bt As BlockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead)
                If bt.Has(sTitleBlock) Then
                  btrSrc = trans.GetObject(bt.Item(sTitleBlock), OpenMode.ForRead)
                  id = btrSrc.Id
                  Dim filterlist(1) As Autodesk.AutoCAD.DatabaseServices.TypedValue
                  filterlist(0) = New Autodesk.AutoCAD.DatabaseServices.TypedValue(0, "INSERT")
                  filterlist(1) = New Autodesk.AutoCAD.DatabaseServices.TypedValue(2, sTitleBlock)
                  Dim filter As Autodesk.AutoCAD.EditorInput.SelectionFilter
                  filter = New Autodesk.AutoCAD.EditorInput.SelectionFilter(filterlist)
                  Dim selRes As PromptSelectionResult
                  selRes = ed.SelectAll(filter)
                  If (selRes.Status <> Autodesk.AutoCAD.EditorInput.PromptStatus.OK) Then
                    ed.WriteMessage("No block references in model space" & vbCrLf)
                  Else
                    Dim objIdArray() As ObjectId
                    objIdArray = selRes.Value.GetObjectIds()
                    Dim objId As ObjectId
                    Dim dbObj As DBObject
                    For Each objId In objIdArray
                      dbObj = trans.GetObject(objId, OpenMode.ForRead)
                      ed.WriteMessage("You selected: " + dbObj.GetType().FullName)
                      Dim blkObj As BlockReference
                      blkObj = dbObj
                      ed.WriteMessage("Block Name: " & blkObj.BlockName & vbCrLf)
                      'If blkObj.BlockName.ToUpper() = sTitleBlock.ToUpper() Then
                      Dim attId As ObjectId
                      For Each attId In blkObj.AttributeCollection
                        Dim attrib As AttributeReference
                        attrib = trans.GetObject(attId, OpenMode.ForWrite)
                        ed.WriteMessage("Checking Title Block Attribute: " & attrib.Tag.ToUpper() & vbCrLf)
                        If sVersionAttribute <> "" Then
                          If attrib.Tag.ToUpper() = sVersionAttribute.ToUpper() Then
                            attrib.TextString = iVersion.ToString
                            ed.WriteMessage("Updating Title Block: " & sTitleBlock.ToUpper() & vbCrLf)
                            ed.WriteMessage("         Attribute:   " & sVersionAttribute.ToUpper() & vbCrLf)
                            ed.WriteMessage("         New Value:   " & iVersion.ToString & vbCrLf)
                          End If
                        End If
                        If sRevisionAttribute <> "" Then
                          If attrib.Tag.ToUpper() = sRevisionAttribute.ToUpper() Then
                            attrib.TextString = sRevision
                            ed.WriteMessage("Updating Title Block: " & sTitleBlock.ToUpper() & vbCrLf)
                            ed.WriteMessage("         Attribute:   " & sRevisionAttribute.ToUpper() & vbCrLf)
                            ed.WriteMessage("         New Value:   " & sRevision & vbCrLf)
                          End If
                        End If
                      Next
                      'End If
                    Next
                  End If
                Else
                  ed.WriteMessage("Block " & sTitleBlock & " does not exist in the drawing." & vbCrLf)
                End If
              Catch ex As System.Exception
                ed.WriteMessage(ex.ToString())
                ed.WriteMessage("Block " & sTitleBlock & " does not exist in the drawing.")
              End Try
            End If
          End If
        Catch ex As System.Exception
          ed.WriteMessage("Error:" & vbCrLf)
          ed.WriteMessage(ex.ToString())
          Return
        End Try
        trans.Commit()
      End Using
    End Sub
    Sub writeKeysMessage(ByVal ed As Editor)
      ed.WriteMessage("       HKCU\Software\Alta Systems\Vault Tools" & vbCrLf)
      ed.WriteMessage("       Required Keys - Server, UserName, Vault, Working Folder" & vbCrLf)
      ed.WriteMessage("       Optional Keys - Password" & vbCrLf)
      ed.WriteMessage("       HKCU\Software\Alta Systems\Vault Tools\AutoCADVersion" & vbCrLf)
      ed.WriteMessage("       Required Keys - TitleBlock" & vbCrLf)
      ed.WriteMessage("       Optional Keys - VersionAttribute, RevisionAttribute (at least one required)" & vbCrLf)
    End Sub
  End Class

End Namespace