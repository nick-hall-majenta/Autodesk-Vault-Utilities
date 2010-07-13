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

using Autodesk.Connectivity.WebServices;
using Autodesk.Connectivity.WebServices.DocumentExSvc;

//using VaultUpdateLifeCycle.Document;
//using VaultUpdateLifeCycle.Security;

namespace VaultUpdateLifeCycle
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
            string lifecycledef = "";
            Boolean nobanner = false;
            Boolean force = false;
            string state = "";
            string comment = "Lifecycle changed";

            if (CommandLine["server"] != null)
                server = CommandLine["server"];
            if (CommandLine["vault"] != null)
                vault = CommandLine["vault"];
            if (CommandLine["username"] != null)
                username = CommandLine["username"];
            if (CommandLine["password"] != null)
                password = CommandLine["password"];
            if (CommandLine["lifecycledef"] != null)
                lifecycledef = CommandLine["lifecycledef"];
            if (CommandLine["state"] != null)
                state = CommandLine["state"];
            if (CommandLine["comment"] != null)
                comment = CommandLine["comment"];
            if (CommandLine["nobanner"] != null)
                nobanner = true;
            if (CommandLine["force"] != null)
                force = true;

            if (!nobanner)
            {
                Console.WriteLine("Autodesk Vault VaultUpdateLifeCycle Tool V" + version + " (" + date + ")");
                Console.WriteLine("(c) 2010 Alta Systems Ltd");
                Console.WriteLine("");
            }
            if (server == "" || vault == "" || username == "" || lifecycledef == "")
            {
                Console.WriteLine("Syntax: VaultUpdateLifeCycle -server servername -vault vaultname -username user");
                Console.WriteLine("        -lifecycledef lifecycledef [-state state]");
                Console.WriteLine("        [-password pass] [-nobanner] [-force]");
                Console.WriteLine("        [-comment comment]");
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
                    Console.WriteLine("Using lifecycledef: " + lifecycledef);
                    Console.WriteLine("Using state: " + state);
                    Console.WriteLine("");
                }
                Boolean oktorun = true;
                if (oktorun)
                {
                    p.RunCommand(server, vault, username, password, lifecycledef, state, force, comment);
                }
            }
#if DEBUG
            Console.WriteLine("Press enter ...");
            Console.Read();
#endif
        }

        public void RunCommand(string server, string vault, string username, string password, string lifecycledef, string state, Boolean force, string comment)
        {
            SecurityService secSrv = new SecurityService();
            secSrv.SecurityHeaderValue = new Autodesk.Connectivity.WebServices.SecuritySvc.SecurityHeader();
            secSrv.Url = "http://" + server + "/AutodeskDM/Services/SecurityService.asmx";

            try 
            {
                secSrv.SignIn(username, password, vault);

                Autodesk.Connectivity.WebServices.DocumentService docSrv = new Autodesk.Connectivity.WebServices.DocumentService();
                docSrv.SecurityHeaderValue = new Autodesk.Connectivity.WebServices.DocumentSvc.SecurityHeader();
                docSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId;
                docSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket;
                docSrv.Url = "http://" + server + "/AutodeskDM/Services/DocumentService.asmx";

                DocumentServiceExtensions docExSrv = new DocumentServiceExtensions();
                docExSrv.Url = "http://" + server + "/AutodeskDM/Services/DocumentServiceExtensions.asmx";
                docExSrv.SecurityHeaderValue = new Autodesk.Connectivity.WebServices.DocumentExSvc.SecurityHeader();
                docExSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket;
                docExSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId;

                LfCycDef[] lcDefs = docExSrv.GetAllLifeCycleDefinitions();
                long lcfound = -1;
                long lcstate = -1;
                if (lcDefs != null)
                {
                    Console.WriteLine("Defined LifeCycles");
                    Console.WriteLine("------------------");
                    foreach (LfCycDef lcDef in lcDefs)
                    {
                        Console.WriteLine("  LifeCycle: " + lcDef.DispName);
                        if (lcDef.DispName == lifecycledef)
                        {
                            lcfound = lcDef.Id;
                            foreach (LfCycState lcState in lcDef.StateArray)
                            {
                                Console.WriteLine("   LifeCycle State: " + lcState.DispName);
                                if (lcState.DispName == state)
                                {
                                    Console.WriteLine("   Overriding LifeCycle State: " + lcState.DispName);
                                    lcstate = lcState.Id;
                                }
                                if ((lcState.IsDflt) && (lcstate == -1))
                                {
                                    Console.WriteLine("   Using Default LifeCycle State: " + lcState.DispName);
                                    lcstate = lcState.Id;
                                }
                            }
                        }
                    }
                }
                if (lcfound != -1)
                {
                    Folder root = docSrv.GetFolderRoot();
                    //root = docSrv.GetFolderByPath("$/Designs/Designs/C690 T3");
                    //root = docSrv.GetFolderByPath("$/Code Numbers");
                    ProcessFilesInFolder(root, docSrv, docExSrv, lifecycledef, state, lcfound, lcstate, force, comment);
                }
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("ERROR: Requested LifeCycle not defined [" + lifecycledef + "]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                return;
            }
        }

        private void ProcessFilesInFolder(Folder parentFolder, DocumentService docSvc, DocumentServiceExtensions docExSvc, string lifecycledef, string state, long lcid, long lcstate, Boolean force, string comment)
        {
            Autodesk.Connectivity.WebServices.File[] files = docSvc.GetLatestFilesByFolderId(parentFolder.Id, false);
            if (files != null && files.Length > 0)
            {
                foreach (Autodesk.Connectivity.WebServices.File file in files)
                {
                    Console.WriteLine("");
                    Console.WriteLine(" " + parentFolder.FullName + "/" + file.Name);
                    Console.WriteLine("  Master ID   : " + String.Format("{0:0}", file.MasterId));
                    Console.WriteLine("  LifeCycle ID: " + file.FileLfCyc.LfCycDefId.ToString());
                    Console.WriteLine("      State   : " + file.FileLfCyc.LfCycStateName);
                    if (file.FileLfCyc.LfCycDefId != -1)
                    {
                        if (force) 
                            Console.WriteLine("  LifeCycle is already set: Forcing change");
                        else
                            Console.WriteLine("  LifeCycle is already set: Use -force to change");
                        
                    }
                    if ((file.FileLfCyc.LfCycDefId == -1) || (force))
                    {
                        try
                        {
                            docExSvc.UpdateFileLifeCycleDefinitions(new long[] { file.MasterId }, new long[] { lcid }, new long[] { lcstate }, comment);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("ERROR: Changing LifeCycle " + parentFolder.FullName + "/" + file.Name + " (New LifeCycle - " + lifecycledef + ")");
                            Console.WriteLine(ex.Message.ToString());
                        }
                        finally 
                        {
                        }
                    }
#if DEBUG
                    Console.WriteLine("Press enter ...");
                    Console.ReadLine();
#endif
                }
            }

            Folder[] folders = docSvc.GetFoldersByParentId(parentFolder.Id, false);
            if (folders != null && folders.Length > 0)
            {
                foreach (Folder folder in folders)
                {
                    ProcessFilesInFolder(folder, docSvc, docExSvc, lifecycledef, state, lcid, lcstate, force, comment);
                }
            }
        }
    }
}
