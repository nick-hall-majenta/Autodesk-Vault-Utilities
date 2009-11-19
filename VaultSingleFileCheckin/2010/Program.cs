using System;
using System.Collections.Generic;
using System.Text;
using CommandLine.Utility;

using VaultSingleFileCheckin.Document;
using VaultSingleFileCheckin.Security;

namespace VaultSingleFileCheckin
{
    class Program
    {
        static void Main(string[] args)
        {
            const string version = "0.1";
            const string date = "04/06/2009";

            Arguments CommandLine = new Arguments(args);

            Program p = new Program();
            
            string server = "";
            string vault = "";
            string username = "";
            string password = "";
            string file = "";
            string workingfolder = "";
            string checkinfile = "";
            string comment = "";
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
            if (CommandLine["workingfolder"] != null)
                workingfolder = CommandLine["workingfolder"];
            if (CommandLine["checkinfile"] != null)
                checkinfile = CommandLine["checkinfile"];
            if (CommandLine["comment"] != null)
                comment = CommandLine["comment"];
            if (CommandLine["nobanner"] != null)
                nobanner = true;
            if (CommandLine["printerror"] != null)
                printerror = true;
            
            if (!nobanner)
            {
                Console.WriteLine("Autodesk Vault Single File Checkin Tool V" + version + " (" + date + ")");
                Console.WriteLine("(c) 2009 Alta Systems Ltd");
                Console.WriteLine("");
            }
            if (server == "" || vault == "" || username == "" || file == "" || checkinfile == "")
            {
                Console.WriteLine("Syntax: VaultSingleFileCheckin -server servername -vault vaultname -username user");
                Console.WriteLine("        -file filepath -checkinfile checkinfilepath -workingfolder workingfolder");
                Console.WriteLine("         [-password pass] [-nobanner] [-printerror]");
                Console.WriteLine("         [-comment comment]");
                Console.WriteLine("        pass default = \"\"");
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
                    Console.WriteLine("Using comment: " + comment);
                    Console.WriteLine("Using workingfolder: " + workingfolder);
                    Console.WriteLine("Using checkinfile: " + checkinfile);
                    Console.WriteLine("");
                }
                p.RunCommand(server, vault, username, password, file, checkinfile, printerror, comment);
            }
#if DEBUG
            Console.WriteLine("Press a key ...");
            Console.Read();
#endif
        }

        public void RunCommand(string server, string vault, string username, string password, string file, string checkinfile, Boolean printerror, string comment)
        {
            SecurityService secSrv = new SecurityService();
            secSrv.SecurityHeaderValue = new VaultSingleFileCheckin.Security.SecurityHeader();
            secSrv.Url = "http://" + server + "/AutodeskDM/Services/SecurityService.asmx";
            try 
            {
                secSrv.SignIn(username, password, vault);
                DocumentService docSrv = new DocumentService();
                docSrv.SecurityHeaderValue = new VaultSingleFileCheckin.Document.SecurityHeader();
                docSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId;
                docSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket;
                docSrv.Url = "http://" + server + "/AutodeskDM/Services/DocumentService.asmx";
                Folder root = docSrv.GetFolderRoot();
                string filepath = System.IO.Path.GetDirectoryName(file);
                filepath = filepath.Replace("\\", "/");
                CreateFolder(docSrv, filepath);
                Folder filefolder = docSrv.GetFolderByPath(filepath);
                int fileflag = IsFileInFolder(filefolder, docSrv, file);
                //Console.WriteLine("File " + fileflag.ToString());
                byte[] bytes;
                switch (fileflag)
                {
                    case 0:
                        Console.WriteLine("File is not in vault");
                        bytes = System.IO.File.ReadAllBytes(checkinfile);
                        File addedfile = docSrv.AddFile(filefolder.Id, System.IO.Path.GetFileName(checkinfile), comment,
                            System.IO.File.GetLastWriteTime(checkinfile), null, null, null, null, null,
                            FileClassification.None, false, bytes);
                        if (addedfile == null)
                        {
                            Console.WriteLine("ERROR: File not checked in to vault");
                        }
                        else
                        {
                            Console.WriteLine("File checked in to vault");
                        }
                        break;
                    case 1:
                        Console.WriteLine("File is in vault (not checked out)");
                        File[] files = docSrv.GetLatestFilesByFolderId(filefolder.Id, true);
                        foreach (File afile in files)
                        {           
                            if (filefolder.FullName + "/" + afile.Name == file)
                            {
                                //docSrv.CheckoutFile(filefolder.Id, afile.MasterId,
                                //    Environment.MachineName, Environment.GetEnvironmentVariable("TEMP"), comment,
                                //    true, true, out bytes);
                                docSrv.CheckoutFile(filefolder.Id, afile.Id, CheckoutFileOptions.Master,
                                                Environment.MachineName, "c:\\", "Temporary Checkout",
                                                false, true, out bytes);
                                bytes = System.IO.File.ReadAllBytes(checkinfile);
                                File updatedfile = docSrv.CheckinFile(afile.MasterId, comment,
                                    false, System.IO.File.GetLastWriteTime(checkinfile), null, null, null, null, null, false,
                                    System.IO.Path.GetFileName(checkinfile), afile.FileClass, afile.Hidden, bytes);
                                if (updatedfile.Id  == afile.Id)
                                {
                                    Console.WriteLine("ERROR: File not checked in to vault");
                                }
                                else
                                {
                                    Console.WriteLine("File checked in to vault");
                                }
                            }
                        }
                        break;
                    case 2:
                        Console.WriteLine("File is in vault (checked out to you)");
                        break;
                    default:
                        Console.WriteLine("File is in vault (checked out to someone else)");
                        Console.WriteLine("Cannot check in file");
                        break;
                }
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

        private void GetFilesInFolder(Folder parentFolder, DocumentService docSvc, string filepath, Int32 fileversion, string checkinfile)
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
                                Console.WriteLine("Writing to " + checkinfile);
                                byte[] bytes;
                                for (int counter = 0; counter < checkinfile.Length; counter++)
                                {
                                    if (checkinfile.Substring(counter,1) == "\\")
                                    {
                                        try 
                                        {
                                            System.IO.Directory.CreateDirectory(checkinfile.Substring(0,counter));
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.ToString());
                                            Console.WriteLine(checkinfile.Substring(0, counter));
                                        }
                                    }
                                }
                                
                                string fileName = docSvc.DownloadFile(verFile.Id, true, out bytes);
                                System.IO.File.WriteAllBytes(checkinfile, bytes);
                            }
                        }
                    }
                }
            }
        }

        private int IsFileInFolder(Folder parentFolder, DocumentService docSvc, string filepath)
        {
            // Returns 0 if not found
            // Returns 1 if found & not checked out
            // Returns 2 if found & checked out to you
            // Returns 3 if found & checked out to someone else

            int flag = 0; 
            File[] files = docSvc.GetLatestFilesByFolderId(parentFolder.Id, false);
            if (files != null && files.Length > 0)
            {
                foreach (File file in files)
                {
                    if (parentFolder.FullName + "/" + file.Name == filepath)
                    {
                        Console.WriteLine(filepath + " is in the vault.");
                        if (file.CheckedOut)
                        {
                            if (file.CkOutUserId == docSvc.SecurityHeaderValue.UserId)
                            {
                                flag = 2;
                            }
                            else
                            {
                                flag = 3;
                            }
                        }
                        else
                        {
                            flag = 1;
                        }
                    }
                }
            }
            return flag;
        }

        private int CreateFolder ( DocumentService docSrv, string folderpath )
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
                        Console.WriteLine("Creating folder " + thisPath);
                        filefolder = docSrv.AddFolder(foldername, thisfolder.Id, false);
                    }
                    thisfolder = filefolder;
                }
            }
            return 0;
        }
    }
}
