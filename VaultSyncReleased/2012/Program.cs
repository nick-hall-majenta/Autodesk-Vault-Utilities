using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using CommandLine.Utility;

using System.Xml;
using System.Xml.XPath;

using System.Data;
using System.Data.OleDb;

using Autodesk.Connectivity.WebServices;
using Autodesk.Connectivity.WebServicesTools;

namespace VaultSyncReleased
{
    class Program
    {
        static void Main(string[] args)
        {
            const string version = "0.2";
            const string date = "08/01/2011";

            Arguments CommandLine = new Arguments(args);

            Program p = new Program();

            string server = "";
            string vault = "";
            string username = "";
            string password = "";
            string rootfolder = "";
            string property = "State";
            string propertyvalue = "Released";
            Boolean nobanner = false;
            Boolean exportdwfs = false;
            Boolean allfiles = false;

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
            if (CommandLine["property"] != null)
                property = CommandLine["property"];
            if (CommandLine["propertyvalue"] != null)
                propertyvalue = CommandLine["propertyvalue"];
            if (CommandLine["exportdwfs"] != null)
                exportdwfs = true;
            if (CommandLine["allfiles"] != null)
            {
                allfiles = true;
                property = "Version";
            }
            if (CommandLine["nobanner"] != null)
                nobanner = true;

            if (!nobanner)
            {
                Console.WriteLine("Autodesk Vault VaultSyncReleased Tool V" + version + " (" + date + ")");
                Console.WriteLine("(c) 2010 Alta Systems Ltd");
                Console.WriteLine("(c) 2011 Majenta Solutions");
                Console.WriteLine("");
            }
            if (server == "" || vault == "" || username == "" || rootfolder == "")
            {
                Console.WriteLine("Syntax: VaultSyncReleased -server servername -vault vaultname -username user");
                Console.WriteLine("        -rootfolder rootfolder [-nobanner] [-exportdwfs] [-allfiles]");
                Console.WriteLine("        [-password pass]");
                Console.WriteLine("        [-extension ext]");
                Console.WriteLine("        [-property prop -propertyvalue propval]");
                Console.WriteLine("        pass default = \"\"");
                Console.WriteLine("        prop + propval must be used together");
                Console.WriteLine("        prop default = \"State\", propval default = \"Released\"");
                Console.WriteLine("");
            }
            else
            {
                Boolean oktorun = true;
                if (propertyvalue == "" ^ property == "")
                {
                    oktorun = false;
                    Console.WriteLine("Syntax: VaultSyncReleased -server servername -vault vaultname -username user");
                    Console.WriteLine("        -rootfolder rootfolder [-nobanner] [-exportdwfs] [-allfiles]");
                    Console.WriteLine("        [-password pass]");
                    Console.WriteLine("        [-extension ext]");
                    Console.WriteLine("        [-property prop -propertyvalue propval]");
                    Console.WriteLine("        pass default = \"\"");
                    Console.WriteLine("        prop + propval must be used together");
                    Console.WriteLine("        prop default = \"State\", propval default = \"Released\"");
                    Console.WriteLine("");
                }
                if (oktorun)
                {
                    if (!nobanner)
                    {
                        Console.WriteLine("Using server: " + server);
                        Console.WriteLine("Using vault: " + vault);
                        Console.WriteLine("Using username: " + username);
                        Console.WriteLine("Using password: " + password);
                        Console.WriteLine("Using rootfolder: " + rootfolder);
                        Console.WriteLine("Using prop: " + property);
                        Console.WriteLine("Using propval: " + propertyvalue);
                        Console.WriteLine("");
                    }
                    p.RunCommand(server, vault, username, password, rootfolder, exportdwfs, allfiles, property, propertyvalue);
                }
            }
#if DEBUG
            Console.WriteLine("Press enter ...");
            Console.Read();
#endif
        }

        public void RunCommand(string server, string vault, string username, string password, string rootfolder, Boolean exportdwfs, Boolean allfiles, string property, string propertyvalue)
        {
            long propertyid = 0;
            Autodesk.Connectivity.WebServicesTools.WebServiceManager m_serviceManager;

            m_serviceManager = new Autodesk.Connectivity.WebServicesTools.WebServiceManager(new UserPasswordCredentials(server, vault, username, password, true));

            //Autodesk.Connectivity.WebServices.SecurityService secSrv = new Autodesk.Connectivity.WebServices.SecurityService();
            //secSrv.SecurityHeaderValue = new Autodesk.Connectivity.WebServices.SecuritySvc.SecurityHeader();
            //secSrv.Url = "http://" + server + "/AutodeskDM/Services/SecurityService.asmx";

            try 
            {
                //secSrv.SignIn(username, password, vault);
                //Autodesk.Connectivity.WebServices.DocumentService docSrv = new Autodesk.Connectivity.WebServices.DocumentService();
                //docSrv.SecurityHeaderValue = m_serviceManager.DocumentSvc.SecurityHeader();
                //docSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId;
                //docSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket;
                //docSrv.Url = "http://" + server + "/AutodeskDM/Services/DocumentService.asmx";

                Autodesk.Connectivity.WebServices.Folder root = m_serviceManager.DocumentService.GetFolderRoot();
                //root = docSrv.GetFolderByPath("$/Designs/Designs/C690 T3");
                //root = docSrv.GetFolderByPath("$/Amendment");
                Autodesk.Connectivity.WebServices.PropDef[] defs = m_serviceManager.DocumentService.GetAllPropertyDefinitions();
                foreach (Autodesk.Connectivity.WebServices.PropDef pd in defs)
                {
                    if (pd.DispName.ToString().ToLower() == property.ToLower())
                    {
                        propertyid = pd.Id;
                    }
                }
                if ((property != "") && (propertyid == 0))
                {
                    Console.WriteLine("Error: Property notdefined in Vault [" + property + "]");
                    Console.WriteLine("Properties that ARE defined in Vault [" + vault + "]:");
                    foreach (Autodesk.Connectivity.WebServices.PropDef pd in defs)
                    {
                        Console.WriteLine("  " + pd.DispName.ToString());
                    }
                }
                else
                {
                    ProcessFilesInFolder(root, m_serviceManager.DocumentService, rootfolder, exportdwfs, allfiles, property, propertyvalue, propertyid);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                return;
            }
        }

        private string Right(string param, int iLength)
        {
            string result;
            result = param.Substring(param.Length - iLength, iLength); 
            return result;
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

        private void DownloadFileInParts(Autodesk.Connectivity.WebServices.File file, Autodesk.Connectivity.WebServices.DocumentService docSvc, string outputfile)
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

        private void ProcessFilesInFolder(Autodesk.Connectivity.WebServices.Folder parentFolder, Autodesk.Connectivity.WebServices.DocumentService docSvc, string rootfolder, Boolean exportdwfs, Boolean allfiles, string property, string propertyvalue, long propid)
        {
            Autodesk.Connectivity.WebServices.File[] files = docSvc.GetLatestFilesByFolderId(parentFolder.Id, false);
            if (files != null && files.Length > 0)
            {
                foreach (Autodesk.Connectivity.WebServices.File file in files)
                {
                    Boolean matchesprop = false;
                    Console.WriteLine("Checking: " + parentFolder.FullName + "/" + file.Name);
                    Autodesk.Connectivity.WebServices.PropInst[] fileProperties = docSvc.GetProperties(new long[] { file.Id }, new long[] { propid });
                    if (allfiles)
                    {
                        matchesprop = true;
                    }
                    else
                    {
                        if (fileProperties[0].Val != null)
                        {
                            //Console.WriteLine(" Property: " + property + " - " + fileProperties[0].Val.ToString());
                            if (fileProperties[0].Val.ToString() == propertyvalue)
                            {
                                matchesprop = true;
                            }
                        }
                    }
                    string outputfile = rootfolder + parentFolder.FullName.Substring(1) + "/" + file.Name;
                    outputfile = outputfile.Replace("/", "\\");
                    if (matchesprop)
                    {
                        int vernum = file.VerNum;
                        Autodesk.Connectivity.WebServices.File verFile;
                        verFile = null;
                        /*
                        if (exportdwfs)
                        {
                            FileAssocArray[] visfiles = docSvc.GetDesignVisualizationAttachmentsByFileMasterIds(new long[] { file.MasterId });
                            if (visfiles[0].FileAssocs != null)
                            {
                                Console.WriteLine(visfiles[0].FileAssocs.GetUpperBound(0).ToString());
                                foreach (FileAssoc fileassoc in visfiles[0].FileAssocs)
                                {
                                    if (fileassoc.ParFile.Id == file.Id)
                                    {
                                        verFile = docSvc.GetFileById(fileassoc.Id);
                                    }
                                }
                            }
                        }
                        else
                        {
                            verFile = docSvc.GetFileByVersion(file.MasterId, vernum);
                        }
                        */
                        verFile = docSvc.GetFileByVersion(file.MasterId, vernum);
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
                            if (System.IO.File.Exists(outputfile))
                            {
                                FileInfo fi = new FileInfo(outputfile);
                                if ((verFile.ModDate == fi.LastWriteTime) && (verFile.FileSize == fi.Length))
                                {
                                    Console.WriteLine("File is uptodate: " + outputfile);
                                }
                                else
                                {
                                    Console.WriteLine("Saving: " + outputfile);
                                    System.IO.File.Delete(outputfile); 
                                    DownloadFileInParts(verFile, docSvc, outputfile);
                                    FileInfo finew = new FileInfo(outputfile);
                                    finew.LastWriteTime = verFile.ModDate;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Saving: " + outputfile);
                                System.IO.File.Delete(outputfile); 
                                DownloadFileInParts(verFile, docSvc, outputfile);
                                FileInfo finew = new FileInfo(outputfile);
                                finew.LastWriteTime = verFile.ModDate;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("ERROR: Saving " + outputfile);
                            Console.WriteLine(ex.Message.ToString());
                        }
                        Console.WriteLine("");
                        //Console.ReadLine();
                    }
                    else
                    {
                        if (System.IO.File.Exists(outputfile))
                        {
                            try
                            {
                                Console.WriteLine("Deleting: " + outputfile);
                                System.IO.File.Delete(outputfile);
                            }
                            catch
                            {
                                Console.WriteLine("ERROR: Deleting " + outputfile);
                            }
                        }
                        Console.WriteLine("");
                    }
                }
            }

            Autodesk.Connectivity.WebServices.Folder[] folders = docSvc.GetFoldersByParentId(parentFolder.Id, false);
            if (folders != null && folders.Length > 0)
            {
                foreach (Autodesk.Connectivity.WebServices.Folder folder in folders)
                {
                    ProcessFilesInFolder(folder, docSvc, rootfolder, exportdwfs, allfiles, property, propertyvalue, propid);
                }
            }
        }
    }
}
