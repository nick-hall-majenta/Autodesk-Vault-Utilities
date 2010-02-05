using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using CommandLine.Utility;

using VaultPathCheck.Document;
using VaultPathCheck.Security;

namespace VaultPathCheck
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
            string workingfolder = "";
            Boolean usemovecopy = false;
            Boolean nobanner = false;
            Int32 size = 0;
            Int32 maxpath = 260;

            
            if (CommandLine["server"] != null)
                server = CommandLine["server"];
            if (CommandLine["vault"] != null)
                vault = CommandLine["vault"];
            if (CommandLine["username"] != null)
                username = CommandLine["username"];
            if (CommandLine["password"] != null)
                password = CommandLine["password"];
            if (CommandLine["workingfolder"] != null)
                workingfolder = CommandLine["workingfolder"];
            if (CommandLine["maxpath"] != null)
                maxpath = Convert.ToInt32(CommandLine["maxpath"]);
            if (CommandLine["movecopy"] != null)
                usemovecopy = true;
            if (CommandLine["nobanner"] != null)
                nobanner = true;
            
            if (!nobanner)
            {
                Console.WriteLine("Autodesk Vault Path Check Tool V" + version + " (" + date + ")");
                Console.WriteLine("(c) 2009 Alta Systems Ltd");
                Console.WriteLine("");
            }

            Boolean cannotcontinue = false;
            if (workingfolder != "" && usemovecopy)
                cannotcontinue = true;
            if (workingfolder == "" && !usemovecopy)
                cannotcontinue = true;

            if (server == "" || vault == "" || username == "" || cannotcontinue)
            {
                Console.WriteLine("Syntax: VaultPathCheck -server servername -vault vaultname -username user");
                Console.WriteLine("        -workingfolder folder|-movecopy");
                Console.WriteLine("        [-maxpath length] [-password pass] [-nobanner]");
                Console.WriteLine("        pass default = \"\"");
                Console.WriteLine("        maxpath default = 260");
                Console.WriteLine("");
            }
            else
            {
                if (!nobanner)
                {
                    /*
                    if (workingfolder == "")
                    {
                        workingfolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        workingfolder += "\\autodesk\\vaultcommon\\servers";
                        string [] subdirEntries = Directory.GetDirectories(workingfolder);
                        foreach(string subdir in subdirEntries)
                        {
                            if ((System.IO.File.GetAttributes(subdir) &
                                    FileAttributes.ReparsePoint) !=
                                    FileAttributes.ReparsePoint)
                            {
                                string workingfolderfile = subdir + "\\";
                                workingfolderfile += server + "\\vaults\\";
                                workingfolderfile += vault + "\\Objects\\WorkingFolders.xml";
                                Console.WriteLine("Checking working folder file: " + workingfolderfile);
                            }
                        }
                    }
                    */
                    Console.WriteLine("Using server: " + server);
                    Console.WriteLine("Using vault: " + vault);
                    Console.WriteLine("Using username: " + username);
                    Console.WriteLine("Using password: " + password);
                    Console.WriteLine("Using maxpath: " + maxpath.ToString());
                    size = workingfolder.Length;
                    if (usemovecopy)
                    {
                        Console.WriteLine("Calculating for Vault Internal move/copy/rename");
                        Console.WriteLine("Using folder %TEMP%\\GUID");
                        string tempfolder = Environment.GetEnvironmentVariable("TEMP");
                        Console.WriteLine("Vault temporary folder:\n " + tempfolder + "\\CF4DD528-2868-477D-86A8-5350EDE5FD08");
                        size = tempfolder.Length + 37;
                    }
                    else
                    {
                        Console.WriteLine("Calculating for Vault Local Workspace");
                        Console.WriteLine("Using working folder: " + workingfolder);
                    }
                    Console.WriteLine("");
                }
                p.RunCommand(server, vault, username, password, size, maxpath );
            }
#if DEBUG
            Console.WriteLine("Press a key ...");
            Console.Read();
#endif
        }

        public void RunCommand(string server, string vault, string username, string password, Int32 size, Int32 maxpath)
        {
            SecurityService secSrv = new SecurityService();
            secSrv.SecurityHeaderValue = new VaultPathCheck.Security.SecurityHeader();
            secSrv.Url = "http://" + server + "/AutodeskDM/Services/SecurityService.asmx";

            try 
            {
                secSrv.SignIn(username, password, vault);
                DocumentService docSrv = new DocumentService();
                docSrv.SecurityHeaderValue = new VaultPathCheck.Document.SecurityHeader();
                docSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId;
                docSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket;
                docSrv.Url = "http://" + server + "/AutodeskDM/Services/DocumentService.asmx";
                Folder root = docSrv.GetFolderRoot();
                PrintFilesInFolder(root, docSrv, size, maxpath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                return;
            }
        }

        private void PrintFilesInFolder(Folder parentFolder, DocumentService docSvc, Int32 size, Int32 maxpath)
        {
            Document.File[] files = docSvc.GetLatestFilesByFolderId(parentFolder.Id, false);
            if (files != null && files.Length > 0)
            {
                foreach (Document.File file in files)
                {
                    for (int vernum = file.VerNum; vernum >= 1; vernum--)
                    {
                        Document.File verFile = docSvc.GetFileByVersion(file.MasterId, vernum);
                        Int32 filepathlength = parentFolder.FullName.Length + verFile.Name.Length + size;
                        if (filepathlength >= maxpath)
                        {
                            Console.WriteLine(String.Format("{0,4:0,0}", filepathlength) + " chars: " + parentFolder.FullName + "/" + verFile.Name + " (Version " + vernum.ToString() + ")");
                        }
                    } 
                }
            }

            Folder[] folders = docSvc.GetFoldersByParentId(parentFolder.Id, false);
            if (folders != null && folders.Length > 0)
            {
                foreach (Folder folder in folders)
                {
                    PrintFilesInFolder(folder, docSvc, size, maxpath);
                }
            }
        }
    }
}
