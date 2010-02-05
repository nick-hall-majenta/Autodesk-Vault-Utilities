using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using CommandLine.Utility;

using VaultExtractParentData.Document;
using VaultExtractParentData.Security;

namespace VaultExtractParentData
{
    class Program
    {
        static void Main(string[] args)
        {
            const string version = "0.1";
            const string date = "15/04/2009";

            Arguments CommandLine = new Arguments(args);

            Program p = new Program();
            
            string server = "";
            string vault = "";
            string username = "";
            string password = "";
            Boolean nobanner = false;
            Boolean xmloutput = false;

            if (CommandLine["server"] != null)
                server = CommandLine["server"];
            if (CommandLine["vault"] != null)
                vault = CommandLine["vault"];
            if (CommandLine["username"] != null)
                username = CommandLine["username"];
            if (CommandLine["password"] != null)
                password = CommandLine["password"];
            if (CommandLine["nobanner"] != null)
                nobanner = true;
            if (CommandLine["xmloutput"] != null)
            {
                nobanner = true;
                xmloutput = true;
            }
            
            if (!nobanner)
            {
                Console.WriteLine("Autodesk Vault File Size Tool V" + version + " (" + date + ")");
                Console.WriteLine("(c) 2009 Alta Systems Ltd");
                Console.WriteLine("");
            }
            if (server == "" || vault == "" || username == "")
            {
                Console.WriteLine("Syntax: VaultExtractParentData -server servername -vault vaultname -username user");
                Console.WriteLine("        [-password pass] [-size bytes] [-nobanner] [-xmloutput]");
                Console.WriteLine("        pass default = \"\"");
                Console.WriteLine("        -xmloutput implies -nobanner");
                Console.WriteLine("");
            }
            else
            {
                if (!nobanner)
                {
                    Console.WriteLine("Using server: " + server);
                    Console.WriteLine("Using vault: " + vault);
                    Console.WriteLine("Using username: " + username);
                    Console.WriteLine("Using password: " + password);
                    Console.WriteLine("");
                }
                if (xmloutput)
                {
                    Console.WriteLine("<VAULTFILES>");
                }
                p.RunCommand(server, vault, username, password, xmloutput);
                if (xmloutput)
                {
                    Console.WriteLine("</VAULTFILES>");
                }
            }
#if DEBUG
            Console.WriteLine("Press enter ...");
            Console.Read();
#endif
        }

        public void RunCommand(string server, string vault, string username, string password, Boolean xmloutput)
        {
            SecurityService secSrv = new SecurityService();
            secSrv.SecurityHeaderValue = new VaultExtractParentData.Security.SecurityHeader();
            secSrv.Url = "http://" + server + "/AutodeskDM/Services/SecurityService.asmx";

            try 
            {
                secSrv.SignIn(username, password, vault);
                DocumentService docSrv = new DocumentService();
                docSrv.SecurityHeaderValue = new VaultExtractParentData.Document.SecurityHeader();
                docSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId;
                docSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket;
                docSrv.Url = "http://" + server + "/AutodeskDM/Services/DocumentService.asmx";
                Folder root = docSrv.GetFolderRoot();
                PrintFilesInFolder(root, docSrv, xmloutput);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                return;
            }
        }

        private void PrintFilesInFolder(Folder parentFolder, DocumentService docSvc, Boolean xmloutput)
        {
            File[] files = docSvc.GetLatestFilesByFolderId(parentFolder.Id, false);
            if (files != null && files.Length > 0)
            {
                foreach (File file in files)
                {
                    for (int vernum = file.VerNum; vernum >= 1; vernum--)
                    {
                        File verFile = docSvc.GetFileByVersion(file.MasterId, vernum);
                        if (xmloutput)
                        {
                            //TODO: Change & to &amp;
                            Console.WriteLine(" <VAULTFILE>");
                            Console.WriteLine("  <FILE>" + parentFolder.FullName + "/" + verFile.Name + "</FILE>");
                            Console.WriteLine("  <VERSION>" + vernum.ToString() + "</VERSION>");
                            Console.WriteLine("  <CREATEDBY>" + verFile.CreateUserName + "</CREATEDBY>");
                            Console.WriteLine("  <COMMENT>" + verFile.Comm + "</COMMENT>");
                        }
                        else
                        {
                            Console.WriteLine("");
                            Console.WriteLine(" " + parentFolder.FullName + "/" + verFile.Name + " (Version " + vernum.ToString() + ")");
                            Console.WriteLine(" Created By: " + verFile.CreateUserName);
                            Console.WriteLine(" Comment: " + verFile.Comm);
                            //Console.WriteLine("             Master ID: " + String.Format("{0,6:0,0}", verFile.MasterId) + " ID: " + String.Format("{0,6:0,0}", verFile.Id));
                        }
                        FileAssocArray[] fileAssocs = docSvc.GetFileAssociationsByIds(
                            new long[] { verFile.Id }, 
                            FileAssociationTypeEnum.All,
                            false,
                            FileAssociationTypeEnum.None,
                            false,
                            false,
                            false);
                        if (fileAssocs != null)
                        {
                            FileAssoc[] associations = fileAssocs[0].FileAssocs;
                            if (associations != null)
                            {
                                if (xmloutput)
                                {
                                    Console.WriteLine("   <WHEREUSED>");
                                }
                                else
                                {
                                    Console.WriteLine("      Parents: " + String.Format("{0,6:0,0}", associations.Length));
                                }
                                foreach (FileAssoc fileassoc in associations)
                                {
                                    File parent = fileassoc.ParFile;
                                    File parFile = docSvc.GetFileById(parent.Id);
                                    Folder[] parFolders = docSvc.GetFoldersByFileMasterId(parent.MasterId);
                                    foreach (Folder parfolder in parFolders)
                                    {
                                        if (xmloutput)
                                        {
                                            //TODO: Change & to &amp;
                                            Console.WriteLine("    <USEDFILE>");
                                            Console.WriteLine("     <FILE>" + parfolder.FullName + "/" + parFile.Name + "</FILE>");
                                            Console.WriteLine("     <VERSION>" + parFile.VerNum.ToString() + "</VERSION>");
                                            Console.WriteLine("    </USEDFILE>");
                                        }
                                        else
                                        {
                                            //Console.WriteLine("             Parent ID: " + String.Format("{0,6:0,0}", parent.Id));
                                            Console.WriteLine("             " + parfolder.FullName + "/" + parFile.Name + " (Version " + parFile.VerNum.ToString() + ")");
                                            //Console.WriteLine("             Parent FolderID: " + String.Format("{0,6:0,0}", parfolder.Id));
                                            //Console.WriteLine("             Parent Folder: " + parfolder.FullName);
                                        }
                                    }
                                }
                                if (xmloutput)
                                {
                                    Console.WriteLine("   </WHEREUSED>");
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                                if (xmloutput)
                                {
                                }
                                else
                                {
                                    Console.WriteLine("      No Parents: ");
                                }
                            }
                        }
                        fileAssocs = docSvc.GetFileAssociationsByIds(
                            new long[] { verFile.Id },
                            FileAssociationTypeEnum.None,
                            false,
                            FileAssociationTypeEnum.All,
                            false,
                            false,
                            false);
                        if (fileAssocs != null)
                        {
                            FileAssoc[] associations = fileAssocs[0].FileAssocs;
                            if (associations != null)
                            {
                                if (xmloutput)
                                {
                                    Console.WriteLine("   <USES>");
                                }
                                else
                                {
                                    Console.WriteLine("      Children: " + String.Format("{0,6:0,0}", associations.Length));
                                }
                                foreach (FileAssoc fileassoc in associations)
                                {
                                    File child = fileassoc.CldFile;
                                    File chiFile = docSvc.GetFileById(child.Id);
                                    Folder[] chiFolders = docSvc.GetFoldersByFileMasterId(child.MasterId);
                                    foreach (Folder chifolder in chiFolders)
                                    {
                                        if (xmloutput)
                                        {
                                            //TODO: Change & to &amp;
                                            Console.WriteLine("    <USEFILE>");
                                            Console.WriteLine("     <FILE>" + chifolder.FullName + "/" + chiFile.Name + "</FILE>");
                                            Console.WriteLine("     <VERSION>" + chiFile.VerNum.ToString() + "</VERSION>");
                                            Console.WriteLine("    </USEFILE>");
                                        }
                                        else
                                        {
                                            //Console.WriteLine("             Parent ID: " + String.Format("{0,6:0,0}", parent.Id));
                                            Console.WriteLine("             " + chifolder.FullName + "/" + chiFile.Name + " (Version " + chiFile.VerNum.ToString() + ")");
                                            //Console.WriteLine("             Child ID: " + String.Format("{0,6:0,0}", child.Id));
                                            //Console.WriteLine("             Child Folder: " + chifolder.FullName);
                                        }
                                    }
                                }
                                if (xmloutput)
                                {
                                    Console.WriteLine("   </USES>");
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                                if (xmloutput)
                                {
                                }
                                else
                                {
                                    Console.WriteLine("      No Children: ");
                                }
                            }
#if DEBUG
                            if (verFile.Name.EndsWith(".iam", true, null))
                            {
                                Console.WriteLine("Assembly, press enter ...");
                                Console.Read();
                            }
#endif
                        }
                        if (xmloutput)
                        {
                            Console.WriteLine(" </VAULTFILE>");
                        }
                        else
                        {
                            Console.WriteLine("");
                        }
                    }
                }
            }

            Folder[] folders = docSvc.GetFoldersByParentId(parentFolder.Id, false);
            if (folders != null && folders.Length > 0)
            {
                foreach (Folder folder in folders)
                {
                    PrintFilesInFolder(folder, docSvc, xmloutput);
                }
            }
        }
    }
}
