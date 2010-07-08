using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using CommandLine.Utility;

using System.Xml;
using System.Xml.XPath;

using System.Data;
using System.Data.OleDb;
//using System.Linq;

using VaultDump.Document;
using VaultDump.Security;

namespace VaultDump
{
    class Program
    {
        static void Main(string[] args)
        {
            const string version = "0.1";
            const string date = "02/06/2010";

            Arguments CommandLine = new Arguments(args);

            Program p = new Program();

            string server = "";
            string vault = "";
            string username = "";
            string password = "";
            string rootfolder = "";
            Boolean nobanner = false;
            Boolean hidden = false;

            if (CommandLine["server"] != null)
                server = CommandLine["server"];
            if (CommandLine["vault"] != null)
                vault = CommandLine["vault"];
            if (CommandLine["username"] != null)
                username = CommandLine["username"];
            if (CommandLine["password"] != null)
                password = CommandLine["password"];
            if (CommandLine["rootfolder"] != null)
                rootfolder = CommandLine["rootfolder"];
            if (CommandLine["hidden"] != null)
                hidden = true;
            if (CommandLine["nobanner"] != null)
                nobanner = true;

            if (!nobanner)
            {
                Console.WriteLine("Autodesk Vault VaultDump Tool V" + version + " (" + date + ")");
                Console.WriteLine("(c) 2010 Alta Systems Ltd");
                Console.WriteLine("");
            }
            if (server == "" || vault == "" || username == "" || rootfolder == "")
            {
                Console.WriteLine("Syntax: VaultDump -server servername -vault vaultname -username user");
                Console.WriteLine("        -rootfolder rootfolder [-nobanner] [-hidden]");
                Console.WriteLine("        [-password pass]");
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
                    Console.WriteLine("Using rootfolder: " + rootfolder);
                    Console.WriteLine("");
                }
                Boolean oktorun = true;
                if (oktorun)
                {
                    p.RunCommand(server, vault, username, password, rootfolder, hidden);
                }
            }
#if DEBUG
            Console.WriteLine("Press enter ...");
            Console.Read();
#endif
        }

        public void RunCommand(string server, string vault, string username, string password, string rootfolder, Boolean hidden)
        {
            SecurityService secSrv = new SecurityService();
            secSrv.SecurityHeaderValue = new VaultDump.Security.SecurityHeader();
            secSrv.Url = "http://" + server + "/AutodeskDM/Services/SecurityService.asmx";

            try 
            {
                secSrv.SignIn(username, password, vault);
                DocumentService docSrv = new DocumentService();
                docSrv.SecurityHeaderValue = new VaultDump.Document.SecurityHeader();
                docSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId;
                docSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket;
                docSrv.Url = "http://" + server + "/AutodeskDM/Services/DocumentService.asmx";
                Folder root = docSrv.GetFolderRoot();
                //root = docSrv.GetFolderByPath("$/Designs/Designs/C690 T3");
                //root = docSrv.GetFolderByPath("$/Code Numbers");
                ProcessFilesInFolder(root, docSrv, rootfolder, hidden);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                return;
            }
        }

        private string Reverse(string str)
        {
            // convert the string to char array
            char[] charArray = str.ToCharArray();
            int len = str.Length - 1;
            /*
            now this for is a bit unconventional at first glance because there
            are 2 variables that we're changing values of: i++ and len--.
            the order of them is irrelevant. so basicaly we're going with i from 
            start to end of the array. with len we're shortening the array by one
            each time. this is probably understandable.
            */
            for (int i = 0; i < len; i++, len--)
            {
                /*
                now this is the tricky part people that should know about it don't.
                look at the table below to see what's going on exactly here.
                */
                charArray[i] ^= charArray[len];
                charArray[len] ^= charArray[i];
                charArray[i] ^= charArray[len];
            }
            return new string(charArray);
        }

        private void DownloadFileInParts(Document.File file, DocumentService docSvc, string outputfile)
        {
            int MAX_BUFFER_SIZE = 1024 * 1024 * 4;    // 49 MB buffer size
            System.IO.FileStream outputStream = null;

            try
            {
                long startByte = 0;
                long endByte;

                // create the output file
                outputStream = System.IO.File.OpenWrite(outputfile);

                // for each loop, the MAX_BUFFER_SIZE number of bytes gets downloaded from the server and written
                // to disk
                while (startByte < file.FileSize)
                {
                    byte[] buffer; 
                    endByte = startByte + MAX_BUFFER_SIZE;
                    if (endByte > file.FileSize)
                        endByte = file.FileSize;

                    // grab the file part from the server
                    buffer = docSvc.DownloadFilePart(file.Id, startByte, endByte, true);

                    // write the data to the file
                    outputStream.Write(buffer, 0, buffer.Length);

                    startByte += buffer.Length;
                }
            }
            finally
            {
                if (outputStream != null)
                    outputStream.Close();
            }
        }
 
        private void ProcessFilesInFolder(Folder parentFolder, DocumentService docSvc, string rootfolder, Boolean hidden)
        {
            VaultDump.Document.File[] files = docSvc.GetLatestFilesByFolderId(parentFolder.Id, hidden);
            if (files != null && files.Length > 0)
            {
                foreach (VaultDump.Document.File file in files)
                {
                    for (int vernum = file.VerNum; vernum >= file.VerNum; vernum--)
                    {
                        VaultDump.Document.File verFile = docSvc.GetFileByVersion(file.MasterId, vernum);
                        Console.WriteLine("");
                        Console.WriteLine(" " + parentFolder.FullName + "/" + verFile.Name + " (Version " + vernum.ToString() + ")");
                        Console.WriteLine(" Created By: " + verFile.CreateUserName);
                        //Console.WriteLine(" Comment: " + verFile.Comm);
                        //Console.WriteLine("             Master ID: " + String.Format("{0,6:0,0}", verFile.MasterId) + " ID: " + String.Format("{0,6:0,0}", verFile.Id));
                        //byte[] bytes;
                        string outputfile = rootfolder + parentFolder.FullName.Substring(1) + "/" + verFile.Name;
                        outputfile = outputfile.Replace("/", "\\");
                        for (int counter = 0; counter < outputfile.Length; counter++)
                        {
                            if (outputfile.Substring(counter, 1) == "\\")
                            {
                                try
                                {
                                    System.IO.Directory.CreateDirectory(outputfile.Substring(0, counter));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                    Console.WriteLine(outputfile.Substring(0, counter));
                                }
                            }
                        }

                        try
                        {
                            //string fileName = docSvc.DownloadFile(verFile.Id, true, out bytes);
                            //System.IO.File.WriteAllBytes(outputfile, bytes);
                            DownloadFileInParts(verFile, docSvc, outputfile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("ERROR: Saving " + parentFolder.FullName + "/" + verFile.Name + " (Version " + vernum.ToString() + ")");
                            Console.WriteLine(ex.Message.ToString());
                        }

                        //Console.ReadLine();
                    }
                }
            }

            Folder[] folders = docSvc.GetFoldersByParentId(parentFolder.Id, false);
            if (folders != null && folders.Length > 0)
            {
                foreach (Folder folder in folders)
                {
                    ProcessFilesInFolder(folder, docSvc, rootfolder, hidden);
                }
            }
        }
    }
}
