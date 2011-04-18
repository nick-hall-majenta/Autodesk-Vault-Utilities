/*=====================================================================
  
  This file is part of the Autodesk Vault API Code Samples.

  Copyright (C) Autodesk Inc.  All rights reserved.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
=====================================================================*/

using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.Connectivity.Explorer.Extensibility;
using Autodesk.Connectivity.WebServices;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;



[assembly: AssemblyCompany("Autodesk")]
[assembly: AssemblyProduct("VaultCreateFromTemplateCommandExtension")]
[assembly: ApiVersion("3.0")]


namespace VaultCreateFromTemplate
{

    public class VaultCreateFromTemplateCommandExtension : IExtension
    {
        private DocumentService mDocSvc;

        private int CreateFolder(DocumentService docSrv, string folderpath)
        {
            String[] foldernames;
            char[] delimiterChars = { '/' };
            foldernames = folderpath.Split('/');
            String thisPath = "";
            Folder filefolder;
            Folder thisfolder;
            thisfolder = docSrv.GetFolderRoot();
            foreach (String foldername in foldernames)
            {
                if (foldername == "$")
                {
                    thisfolder = docSrv.GetFolderRoot();
                    thisPath = "$";
                }
                else
                {
                    thisPath += "/";
                    thisPath += foldername;
                    try
                    {
                        filefolder = docSrv.GetFolderByPath(thisPath);
                    }
                    catch
                    {
                        //Console.WriteLine("Creating folder " + thisPath);
                        filefolder = docSrv.AddFolder(foldername, thisfolder.Id, thisfolder.IsLib);
                    }
                    thisfolder = filefolder;
                }
            }
            return 0;
        }

        private List<string> GetFolderStructure(DocumentService docSrv, string folderpath)
        {
            List<string> foldernames;
            Folder filefolder;
            Folder thisfolder;
            thisfolder = docSrv.GetFolderRoot();
            filefolder = docSrv.GetFolderByPath(folderpath);

            foldernames = GetFoldersInFolder(filefolder, docSrv);
            return foldernames;
        }

        private List<string> GetFoldersInFolder(Folder parentFolder, DocumentService docSvc)
		{
            List<string> foldernames = new List<string>();
            List<string> newfoldernames;
            Folder[] folders = docSvc.GetFoldersByParentId(parentFolder.Id, false);
			if (folders != null && folders.Length > 0)
			{
				foreach (Folder folder in folders)
				{
                    foldernames.Add(folder.FullName);
                    newfoldernames = GetFoldersInFolder(folder, docSvc);
                    foreach (string newfolder in newfoldernames )
                    {
                        foldernames.Add(newfolder);
                    }
				}
			}
            return foldernames;
		}
	
        private void LoadConfigurationFile(string configFile, string rootfolderpath)
        {
            XPathNavigator nav;
            XPathDocument docNav;
            XPathNodeIterator nodeIter;
            docNav = new XPathDocument(@configFile);
            nav = docNav.CreateNavigator();
            nodeIter = nav.Select("/VAULTFOLDERCREATE/SOURCE");
            nodeIter.MoveNext();
            string vaultsource = "";
            try
            {
                vaultsource = nodeIter.Current.TypedValue.ToString();
            }
            catch
            {
            }
            nodeIter = nav.Select("/VAULTFOLDERCREATE/TEMPLATE");
            nodeIter.MoveNext();
            string vaulttemplate = "";
            try
            {
                vaulttemplate = nodeIter.Current.TypedValue.ToString();
            }
            catch
            {
            }
            MessageBox.Show(vaultsource);
            MessageBox.Show(vaulttemplate);
            if (vaulttemplate != "")
            {
                Regex hashesPattern = new Regex("[^#]*#+[^#]*");
                if (hashesPattern.Matches(vaulttemplate).Count == 1)
                {
                    string regexstring = vaulttemplate.Replace("#","[0-9]");
                    Regex hashPattern = new Regex("#+");
                    Regex formatPattern = new Regex(hashPattern.Match(vaulttemplate).ToString());
                    string formatstring = new string('0', hashPattern.Match(vaulttemplate).Length);
                    formatstring = "{0:" + formatstring + "}";
                    formatstring = formatPattern.Replace(vaulttemplate, formatstring);
                    MessageBox.Show(String.Format(formatstring, 123));
                    //string formatstring = vaulttemplate.Substring( Replace("#", "[0-9]");
                    Folder thisfolder;
                    thisfolder = mDocSvc.GetFolderByPath(rootfolderpath);
                    Folder[] folders = mDocSvc.GetFoldersByParentId(thisfolder.Id, false);
                    long nextincrement = 0;
                    Regex foldermatch = new Regex(regexstring);
                    if (folders != null && folders.Length > 0)
                    {
                        foreach (Folder folder in folders)
                        {
                            MessageBox.Show(foldermatch.Match(folder.Name).ToString());
                            //if ( folder.FullName)
                            {
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Pattern [" + vaulttemplate + "]", "Create Folder Tool", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
                
                
            
            }
            if (vaultsource.StartsWith("$/"))
            {
                List<string> foldernames = GetFolderStructure(mDocSvc,vaultsource);
                MessageBox.Show(foldernames.Count.ToString());
                foreach (string foldername in foldernames)
                {
                    //MessageBox.Show(foldername);
                    string thisFolder = foldername.Replace(vaultsource, rootfolderpath);
                    //MessageBox.Show(thisFolder);
                    //CreateFolder(mDocSvc, thisFolder);
                }
            }
            else
            {
                nodeIter = nav.Select("/VAULTFOLDERCREATE/FOLDERS/FOLDER");
                while (nodeIter.MoveNext())
                {
                    string thisFolder = nodeIter.Current.Value;
                    if (thisFolder.StartsWith("/"))
                    {
                        thisFolder = rootfolderpath + thisFolder;
                    }
                    else
                    {
                        thisFolder = rootfolderpath + "/" + thisFolder;
                    }
                    CreateFolder(mDocSvc, thisFolder);
                }
            }
        }

        void VaultCreateFromTemplateCommandHandler(object s, CommandItemEventArgs e)
        {
            ISelection selectedFile = null;
            CommandItem ci = (CommandItem) s;
            foreach (ISelection f in e.Context.CurrentSelectionSet)
            {
                selectedFile = f;
            }

            if (ci.Id == "\\More...\\")
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                string extensionfolder = Application.ExecutablePath;
                extensionfolder = System.IO.Path.GetDirectoryName(extensionfolder);
                extensionfolder += "\\Extensions\\VaultCreateFromTemplate";
                openFileDialog.InitialDirectory = extensionfolder;
                openFileDialog.Filter = "XML Files(*.xml)|*.xml";
                openFileDialog.Multiselect = false;
                openFileDialog.ShowDialog();
                if (openFileDialog.FileName != "")
                {
                    LoadConfigurationFile(openFileDialog.FileName, mDocSvc.GetFolderById(selectedFile.Id).FullName);
                }
            }
            else
            {
                LoadConfigurationFile(ci.Id, mDocSvc.GetFolderById(selectedFile.Id).FullName);
            }
        }

        #region IExtension Members

        // Returns information on commands added in this extension
        //
        public IEnumerable<CommandSite> CommandSites()
        {
            // Create command item for Hello World command
            //
            List<CommandItem> menuitems = new List<CommandItem>();

            string extensionfolder = Application.ExecutablePath;
            extensionfolder = System.IO.Path.GetDirectoryName(extensionfolder);
            extensionfolder += "\\Extensions\\VaultCreateFromTemplate";
            string[] filePaths = Directory.GetFiles(extensionfolder,"*.xml");
            int numXmlfiles = 0;
            foreach (string filepath in filePaths)
            {
                //if (filepath.EndsWith(".xml")) 
                //{
                    CommandItem CmdItem = new CommandItem(System.IO.Path.GetFileName(filepath), System.IO.Path.GetFileNameWithoutExtension(filepath)) { NavigationTypes = new SelectionTypeId[] { SelectionTypeId.Folder }, MultiSelectEnabled = false };
                    CmdItem.Execute += VaultCreateFromTemplateCommandHandler;
                    menuitems.Add(CmdItem);
                    numXmlfiles++;
                //}
            }

            CommandItem VaultCreateFromTemplateCmdItem;
            CommandSite folderContextCmdSite;
            List<CommandSite> sites = new List<CommandSite>();
            if (numXmlfiles == 1)
            {
                folderContextCmdSite = new CommandSite("VaultCreateFromTemplateCommand.FolderContextMenu", "Create Folders...") { Location = CommandSiteLocation.FolderContextMenu, DeployAsPulldownMenu = false };
                //VaultCreateFromTemplateCmdItem = new CommandItem("", "Create From Template") { NavigationTypes = new SelectionTypeId[] { SelectionTypeId.Folder }, MultiSelectEnabled = false };
                //VaultCreateFromTemplateCmdItem.Execute += VaultCreateFromTemplateCommandHandler;
                //folderContextCmdSite.AddCommand(VaultCreateFromTemplateCmdItem);
                foreach (CommandItem CmdItem in menuitems)
                {
                    CommandItem NewCmdItem = new CommandItem(CmdItem.Id, "Create Folders...") { NavigationTypes = new SelectionTypeId[] { SelectionTypeId.Folder }, MultiSelectEnabled = false };
                    NewCmdItem.Execute += VaultCreateFromTemplateCommandHandler;
                    folderContextCmdSite.AddCommand(NewCmdItem);
                }
            }
            else
            {
                folderContextCmdSite = new CommandSite("VaultCreateFromTemplateCommand.FolderContextMenu", "Create Folders...") { Location = CommandSiteLocation.FolderContextMenu, DeployAsPulldownMenu = true };
                int count = 0;
                foreach (CommandItem CmdItem in menuitems)
                {
                    if (count < 10)
                    {
                        folderContextCmdSite.AddCommand(CmdItem);
                    }
                    count++;
                }
                if (menuitems.Count >= 10)
                {
                    VaultCreateFromTemplateCmdItem = new CommandItem("\\More...\\", "More ...") { NavigationTypes = new SelectionTypeId[] { SelectionTypeId.Folder }, MultiSelectEnabled = false };
                    VaultCreateFromTemplateCmdItem.Execute += VaultCreateFromTemplateCommandHandler;
                    folderContextCmdSite.AddCommand(VaultCreateFromTemplateCmdItem);
                }
            }
            sites.Add(folderContextCmdSite);
            return sites;
        }

        public IEnumerable<DetailPaneTab> DetailTabs()
        {
//            // Create a DetailPaneTab list to return from method
//            //
//            List<DetailPaneTab> fileTabs = new List<DetailPaneTab>();
//
//            // Create Selection Info tab for Files
//            //
//            DetailPaneTab filePropertyTab = new DetailPaneTab("File.Tab.PropertyGrid",
//                                                        "Selection Info",
//                                                        TabContainer.File,
//                                                        typeof(MyCustomTabControl));
//            filePropertyTab.SelectionChanged += propertyTab_SelectionChanged;
//            fileTabs.Add(filePropertyTab);
//
//            // Create Selection Info tab for Items
//            //
//            DetailPaneTab itemPropertyTab = new DetailPaneTab("Item.Tab.PropertyGrid",
//                                                        "Selection Info",
//                                                        TabContainer.Item,
//                                                        typeof(MyCustomTabControl));
//            itemPropertyTab.SelectionChanged += propertyTab_SelectionChanged;
//            fileTabs.Add(itemPropertyTab);
//
//            // Create Selection Info tab for Change Orders
//            //
//            DetailPaneTab coPropertyTab = new DetailPaneTab("Co.Tab.PropertyGrid",
//                                                        "Selection Info",
//                                                        TabContainer.ChangeOrder,
//                                                        typeof(MyCustomTabControl));    
//            coPropertyTab.SelectionChanged += propertyTab_SelectionChanged;
//            fileTabs.Add(coPropertyTab);
//
//            // Return tabs
//            //
//            return fileTabs;
            return null;
        }

        // Selection changed handler for all of our tabs
        //
//        void propertyTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            MyCustomTabControl tabControl = e.Context.UserControl as MyCustomTabControl;
//            tabControl.SetSelectedObject(e.Context.SelectedObject);
//        }

        // Seed the document service with security ticket and user id.
        // This allows us to call document service methods as the user
        // currently logged into Vault
        //
        public void OnLogOn(IApplication application)
        {
            mDocSvc = new DocumentService();
            mDocSvc.Url = application.VaultContext.RemoteBaseUrl + "DocumentService.asmx";
            mDocSvc.SecurityHeaderValue = new Autodesk.Connectivity.WebServices.DocumentSvc.SecurityHeader();
            mDocSvc.SecurityHeaderValue.Ticket = application.VaultContext.Ticket;
            mDocSvc.SecurityHeaderValue.UserId = application.VaultContext.UserId;
        }

        public void OnLogOff(IApplication application)
        {
            mDocSvc = null;
        }

        public void OnShutdown(IApplication application)
        { return; }

        public void OnStartup(IApplication application)
        { return; }

        public string ResourceCollectionName()
        { return String.Empty; }

        public IEnumerable<string> HiddenCommands()
        { return null; }

#endregion
    }
}
