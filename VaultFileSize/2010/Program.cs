using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using CommandLine.Utility;

using VaultFileSize.Document;
using VaultFileSize.Security;

namespace VaultFileSize
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
            double size = 10 * 1024 * 1024;
            Boolean nobanner = false;

            if (CommandLine["server"] != null)
                server = CommandLine["server"];
            if (CommandLine["vault"] != null)
                vault = CommandLine["vault"];
            if (CommandLine["username"] != null)
                username = CommandLine["username"];
            if (CommandLine["password"] != null)
                password = CommandLine["password"];
            if (CommandLine["size"] != null)
                size = Convert.ToDouble(CommandLine["size"]);
            if (CommandLine["nobanner"] != null)
                nobanner = true;
            
            if (!nobanner)
            {
                Console.WriteLine("Autodesk Vault File Size Tool V" + version + " (" + date + ")");
                Console.WriteLine("(c) 2009 Alta Systems Ltd");
                Console.WriteLine("");
            }
            if (server == "" || vault == "" || username == "")
            {
                Console.WriteLine("Syntax: VaultFileSize -server servername -vault vaultname -username user");
                Console.WriteLine("        [-password pass] [-size bytes] [-nobanner]");
                Console.WriteLine("        pass default = \"\"");
                Console.WriteLine("        bytes default = 10Mb");
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
                    Console.WriteLine("Using size: " + size.ToString());
                    Console.WriteLine("");
                }
                p.RunCommand(server, vault, username, password, size);
            }
#if DEBUG
            Console.WriteLine("Press a key ...");
            Console.Read();
#endif
        }

        public void RunCommand(string server, string vault, string username, string password, double size)
        {
            SecurityService secSrv = new SecurityService();
            secSrv.SecurityHeaderValue = new VaultFileSize.Security.SecurityHeader();
            secSrv.Url = "http://" + server + "/AutodeskDM/Services/SecurityService.asmx";

            try 
            {
                secSrv.SignIn(username, password, vault);
                DocumentService docSrv = new DocumentService();
                docSrv.SecurityHeaderValue = new VaultFileSize.Document.SecurityHeader();
                docSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId;
                docSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket;
                docSrv.Url = "http://" + server + "/AutodeskDM/Services/DocumentService.asmx";
                Folder root = docSrv.GetFolderRoot();
                PrintFilesInFolder(root, docSrv, size);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                return;
            }
        }

        private void PrintFilesInFolder(Folder parentFolder, DocumentService docSvc, double size)
        {
            File[] files = docSvc.GetLatestFilesByFolderId(parentFolder.Id, false);
            if (files != null && files.Length > 0)
            {
                foreach (File file in files)
                {
                    if (file.FileSize >= size)
                    {
                        for (int vernum = file.VerNum; vernum >= 1; vernum--)
                        {
                            File verFile = docSvc.GetFileByVersion(file.MasterId, vernum);
                            Console.WriteLine(String.Format("{0,12:0,0}", verFile.FileSize) + " " + parentFolder.FullName + "/" + verFile.Name + " (Version " + vernum.ToString() + ")");
                        }
                    }
                }
            }

            Folder[] folders = docSvc.GetFoldersByParentId(parentFolder.Id, false);
            if (folders != null && folders.Length > 0)
            {
                foreach (Folder folder in folders)
                {
                    PrintFilesInFolder(folder, docSvc, size);
                }
            }
        }
    }
}
