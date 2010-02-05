using System;
using System.Collections.Generic;
using System.Text;
using CommandLine.Utility;

using VaultFileRetrieve.Document;
using VaultFileRetrieve.Security;

namespace VaultFileRetrieve
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
            string file = "";
            Int32 fileversion = 1;
            string outputfile = "";
            Boolean nobanner = false;
            Boolean printerror = false;

            if (CommandLine["server"] != null)
                server = CommandLine["server"];
            if (CommandLine["vault"] != null)
                vault = CommandLine["vault"];
            if (CommandLine["username"] != null)
                username = CommandLine["username"];
            if (CommandLine["password"] != null)
                password = CommandLine["password"];
            if (CommandLine["file"] != null)
                file = CommandLine["file"];
            if (CommandLine["version"] != null)
                fileversion = Convert.ToInt32(CommandLine["version"]);
            if (CommandLine["outputfile"] != null)
                outputfile = CommandLine["outputfile"];
            if (CommandLine["nobanner"] != null)
                nobanner = true;
            if (CommandLine["printerror"] != null)
                printerror = true;
            
            if (!nobanner)
            {
                Console.WriteLine("Autodesk Vault File Retrieval Tool V" + version + " (" + date + ")");
                Console.WriteLine("(c) 2009 Alta Systems Ltd");
                Console.WriteLine("");
            }
            if (server == "" || vault == "" || username == "" || file == "" || outputfile == "")
            {
                Console.WriteLine("Syntax: VaultFileRetrieve -server servername -vault vaultname -username user");
                Console.WriteLine("        -file filepath -outputfile outputfilepath");
                Console.WriteLine("         [-password pass] [-version versionnumber] [-nobanner] [-printerror]");
                Console.WriteLine("        pass default = \"\"");
                Console.WriteLine("        versionnumber default = 1");
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
                    Console.WriteLine("Using file: " + file);
                    Console.WriteLine("Using version: " + fileversion.ToString());
                    Console.WriteLine("Using outputfile: " + outputfile);
                    Console.WriteLine("");
                }
                p.RunCommand(server, vault, username, password, file, fileversion, outputfile, printerror);
            }
#if DEBUG
            Console.WriteLine("Press a key ...");
            Console.Read();
#endif
        }

        public void RunCommand(string server, string vault, string username, string password, string file, Int32 fileversion, string outputfile, Boolean printerror)
        {
            SecurityService secSrv = new SecurityService();
            secSrv.SecurityHeaderValue = new VaultFileRetrieve.Security.SecurityHeader();
            secSrv.Url = "http://" + server + "/AutodeskDM/Services/SecurityService.asmx";

            try 
            {
                secSrv.SignIn(username, password, vault);
                DocumentService docSrv = new DocumentService();
                docSrv.SecurityHeaderValue = new VaultFileRetrieve.Document.SecurityHeader();
                docSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId;
                docSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket;
                docSrv.Url = "http://" + server + "/AutodeskDM/Services/DocumentService.asmx";
                Folder root = docSrv.GetFolderRoot();
                string filepath = System.IO.Path.GetDirectoryName(file);
                filepath = filepath.Replace("\\", "/");
                Folder filefolder = docSrv.GetFolderByPath(filepath);
                GetFilesInFolder(filefolder, docSrv, file, fileversion, outputfile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving file");
                if (printerror)
                {
                    Console.WriteLine(ex.ToString());
                }
                return;
            }
        }

        private void GetFilesInFolder(Folder parentFolder, DocumentService docSvc, string filepath, Int32 fileversion, string outputfile)
        {
            File[] files = docSvc.GetLatestFilesByFolderId(parentFolder.Id, false);
            if (files != null && files.Length > 0)
            {
                foreach (File file in files)
                {
                    if (parentFolder.FullName + "/" + file.Name == filepath)
                    {
                        for (int vernum = file.VerNum; vernum >= 1; vernum--)
                        {
                            File verFile = docSvc.GetFileByVersion(file.MasterId, vernum);
                            if (vernum == fileversion)
                            {
                                Console.WriteLine(String.Format("{0,12:0,0}", verFile.FileSize) + " " + parentFolder.FullName + "/" + verFile.Name + " (Version " + vernum.ToString() + ")");
                                Console.WriteLine("Writing to " + outputfile);
                                byte[] bytes;
                                for (int counter = 0; counter < outputfile.Length; counter++)
                                {
                                    if (outputfile.Substring(counter,1) == "\\")
                                    {
                                        try 
                                        {
                                            System.IO.Directory.CreateDirectory(outputfile.Substring(0,counter));
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.ToString());
                                            Console.WriteLine(outputfile.Substring(0, counter));
                                        }
                                    }
                                }
                                
                                string fileName = docSvc.DownloadFile(verFile.Id, true, out bytes);
                                System.IO.File.WriteAllBytes(outputfile, bytes);
                            }
                        }
                    }
                }
            }
        }
    }
}
