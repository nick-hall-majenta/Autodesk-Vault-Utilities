using System;
using System.Collections; //.Generic;
using System.Collections.Generic;
using System.IO;
using System.Text;

using CommandLine.Utility;

using System.Xml;
using System.Xml.XPath;

using System.Data;
using System.Data.OleDb;

using Autodesk.Connectivity.WebServices;

namespace VaultConditionalChangeState
{
    class Program
    {
        static void Main(string[] args)
        {
            const string version = "1.0";
            const string date = "26/10/2010";

            Arguments CommandLine = new Arguments(args);

            Program p = new Program();

            string server = "";
            string vault = "";
            string username = "";
            string password = "";
            string statename = "State";
            string statehistname = "State (Historical)";
            string state = "";
            string newstate = "";
            string revertstate = "";
            string checkstates = "";
            string lifecycledefinition = "";
            Boolean nobanner = false;
            
            if (CommandLine["server"] != null)
                server = CommandLine["server"];
            if (CommandLine["vault"] != null)
                vault = CommandLine["vault"];
            if (CommandLine["username"] != null)
                username = CommandLine["username"];
            if (CommandLine["password"] != null)
                password = CommandLine["password"];
            if (CommandLine["state"] != null)
                state = CommandLine["state"];
            if (CommandLine["newstate"] != null)
                newstate = CommandLine["newstate"];
            if (CommandLine["revertstate"] != null)
                revertstate = CommandLine["revertstate"];
            if (CommandLine["checkstates"] != null)
                checkstates = CommandLine["checkstates"];
            if (CommandLine["lifecycledefinition"] != null)
                lifecycledefinition = CommandLine["lifecycledefinition"];
            if (CommandLine["nobanner"] != null)
                nobanner = true;

            if (!nobanner)
            {
                Console.WriteLine("Autodesk Vault Conditional Change State Tool V" + version + " (" + date + ")");
                Console.WriteLine("(c) 2010 Alta Systems, a division of Majenta Solutions");
                Console.WriteLine("");
            }
            if (server == "" || vault == "" || username == "" || state == "" || newstate == "" || checkstates == "" || lifecycledefinition == "")
            {
                Console.WriteLine("Syntax: VaultConditionalChangeState -server servername -vault vaultname");
                Console.WriteLine("        -username user [-password pass] [-nobanner]");
                Console.WriteLine("        -state statename");
                Console.WriteLine("        -newstate newstatename");
                Console.WriteLine("        -revertstate revertstatename");
                Console.WriteLine("        -lifecycledefinition lifecycledefinition");
                Console.WriteLine("        -checkstates checkstates");
                Console.WriteLine("        pass default = \"\"");
                Console.WriteLine("        checkstates seperator = |");
                Console.WriteLine("");
            }
            else
            {
                Boolean oktorun = true;
                if (state == "" ^ statename == "")
                {
                    oktorun = false;
                    Console.WriteLine("Syntax: VaultConditionalChangeState -server servername -vault vaultname");
                    Console.WriteLine("        -username user [-password pass] [-nobanner]");
                    Console.WriteLine("        -state statename");
                    Console.WriteLine("        -newstate newstatename");
                    Console.WriteLine("        -revertstate revertstatename");
                    Console.WriteLine("        -lifecycledefinition lifecycledefinition");
                    Console.WriteLine("        -checkstates checkstates");
                    Console.WriteLine("        pass default = \"\"");
                    Console.WriteLine("        checkstates seperator = |");
                    Console.WriteLine("");
                }
                if (oktorun)
                {
                    if (!nobanner)
                    {
                        Console.WriteLine("Server                  : " + server);
                        Console.WriteLine("Vault                   : " + vault);
                        Console.WriteLine("Username                : " + username);
                        Console.WriteLine("Lifecycle definition    : " + lifecycledefinition);
                        Console.WriteLine("Checking files in state : " + state);
                        string[] states = checkstates.Split('|');
                        Console.WriteLine("States to compare       : " + states[0]);
                        for (int statecount = 1; statecount < states.Length; statecount++)
                        {
                            Console.WriteLine("                        : " + states[statecount]);
                        }
                        Console.WriteLine("State to change to      : " + newstate);
                        Console.WriteLine("State to revert to      : " + revertstate);
                        Console.WriteLine("");
                    }
                    p.RunCommand(server, vault, username, password, statename, statehistname, state, checkstates, newstate, revertstate, lifecycledefinition);
                }
            }
#if DEBUG
            Console.WriteLine("Press enter ...");
            Console.Read();
#endif
        }

        public void RunCommand(string server, string vault, string username, string password, string statename, string statehistname, string state, string checkstates, string newstate, string revertstate, string lifecycledefinition)
        {
            long stateid = 0;
            long statehistid = 0;
            SecurityService secSrv = new SecurityService();
            secSrv.SecurityHeaderValue = new Autodesk.Connectivity.WebServices.SecuritySvc.SecurityHeader();
            secSrv.Url = "http://" + server + "/AutodeskDM/Services/SecurityService.asmx";

            try 
            {
                secSrv.SignIn(username, password, vault);
                DocumentService docSrv = new DocumentService();
                docSrv.SecurityHeaderValue = new Autodesk.Connectivity.WebServices.DocumentSvc.SecurityHeader();
                docSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId;
                docSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket;
                docSrv.Url = "http://" + server + "/AutodeskDM/Services/DocumentService.asmx";
                DocumentServiceExtensions docextSrv = new DocumentServiceExtensions();
                docextSrv.SecurityHeaderValue = new Autodesk.Connectivity.WebServices.DocumentExSvc.SecurityHeader();
                docextSrv.SecurityHeaderValue.UserId = secSrv.SecurityHeaderValue.UserId;
                docextSrv.SecurityHeaderValue.Ticket = secSrv.SecurityHeaderValue.Ticket;
                docextSrv.Url  = "http://" + server + "/AutodeskDM/Services/DocumentServiceExtensions.asmx";
                Folder root = docSrv.GetFolderRoot();
                Autodesk.Connectivity.WebServices.PropDef[] defs = docSrv.GetAllPropertyDefinitions();
                foreach (Autodesk.Connectivity.WebServices.PropDef pd in defs)
                {
                    if (pd.DispName.ToString().ToLower() == statename.ToLower())
                    {
                        stateid = pd.Id;
                    }
                    if (pd.DispName.ToString().ToLower() == statehistname.ToLower())
                    {
                        statehistid = pd.Id;
                    }
                }
                long revertstateid = -1;
                long newstateid = -1;
                long newlcdid = -1;
                LfCycDef[] lifecycledefs = docextSrv.GetAllLifeCycleDefinitions();
                foreach (LfCycDef lcd in lifecycledefs)
                {
                    if (lcd.DispName.ToString().ToLower() == lifecycledefinition.ToLower())
                    {
                        newlcdid = lcd.Id;
                    }
                }
                long[] transitionids = docextSrv.GetAllowedFileLifeCycleStateTransitionIds();
                LfCycTrans[] transitions = docextSrv.GetLifeCycleStateTransitionsByIds(transitionids);
                List<long> tostateidlist = new List<long>();
                foreach (LfCycTrans thistransition in transitions)
                {
                    if (!tostateidlist.Contains(thistransition.ToId))
                    {
                        tostateidlist.Add(thistransition.ToId);
                    }
                }
                long[] tostateids = tostateidlist.ToArray();
                LfCycState[] lifecyclestates = docextSrv.GetLifeCycleStatesByIds(tostateids);
                foreach (LfCycState lcs in lifecyclestates)
                {
                    if ((lcs.DispName.ToString().ToLower() == newstate.ToLower()) && (newlcdid == lcs.LfCycDefId))
                    {
                        newstateid = lcs.Id;
                    }
                    if ((lcs.DispName.ToString().ToLower() == revertstate.ToLower()) && (newlcdid == lcs.LfCycDefId))
                    {
                        revertstateid = lcs.Id;
                    }
                }
                if ((statehistid == 0) && (stateid == 0) && (newstateid == -1))
                {
                    if (statehistid == 0)
                        Console.WriteLine("Error: \"State (Historical)\" property undefined");
                    if (stateid == 0)
                        Console.WriteLine("Error: \"State\" property undefined");
                    if (newstateid == -1)
                        Console.WriteLine("Error: State \"" + newstate + "\" not defined for lifecycle \"" + lifecycledefinition + "\"");
                }
                else
                {
                    ProcessFilesInFolder(root, docSrv, docextSrv, statename, state, stateid, statehistid, checkstates, newstateid, newlcdid, newstate, revertstateid, revertstate, lifecycledefinition);
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

        private void DownloadFileInParts(Autodesk.Connectivity.WebServices.File file, DocumentService docSvc, string outputfile)
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

        private void ProcessFilesInFolder(Folder parentFolder, DocumentService docSvc, DocumentServiceExtensions docextSrv, string statename, string state, long propid, long prophistid, string checkstates, long newstateid, long newlcdid, string newstate, long revertstateid, string revertstate, string lifecycledefinition)
        {
            string[] states = checkstates.Split('|');
            Autodesk.Connectivity.WebServices.File[] files = docSvc.GetLatestFilesByFolderId(parentFolder.Id, false);
            if (files != null && files.Length > 0)
            {
                foreach (Autodesk.Connectivity.WebServices.File file in files)
                {
                    Boolean matchesprop = false;
                    Console.WriteLine("");
                    Console.WriteLine("Checking: " + parentFolder.FullName + "/" + file.Name);
                    Autodesk.Connectivity.WebServices.PropInst[] fileProperties = docSvc.GetProperties(new long[] { file.Id }, new long[] { propid });
                    if (fileProperties[0].Val != null)
                    {
                        //Console.WriteLine(" Property: " + statename + " - " + fileProperties[0].Val.ToString());
                        if (fileProperties[0].Val.ToString() == state)
                        {
                            matchesprop = true;
                        }
                    }
                    //string outputfile = parentFolder.FullName.Substring(1) + "/" + file.Name;
                    //outputfile = outputfile.Replace("/", "\\");
                    if (matchesprop && (file.VerNum > states.Length))
                    {
                        Hashtable users = new Hashtable();
                        for (int vernum = file.VerNum; vernum > file.VerNum - states.Length; vernum--)
                        {
                            //Console.WriteLine(" Property Check: " + vernum.ToString());
                            //Console.WriteLine(" State Check: " + (file.VerNum - vernum).ToString());
                            Console.Write(" Checking State: " + states[file.VerNum - vernum]);
                            Autodesk.Connectivity.WebServices.File verFile = docSvc.GetFileByVersion(file.MasterId, vernum);
                            Autodesk.Connectivity.WebServices.PropInst[] verFileProperties = docSvc.GetProperties(new long[] { verFile.Id }, new long[] { propid });
                            if (verFileProperties[0].Val != null)
                            {
                                //Console.WriteLine(" Property: " + statename + " - " + verFileProperties[0].Val.ToString());
                                //Console.WriteLine(" Property: " + statename + " - " + verFile.CreateUserName);
                                if (fileProperties[0].Val.ToString() == state)
                                {
                                    users.Add(verFileProperties[0].Val.ToString(), verFile.CreateUserName);
                                    Console.Write(" : " + verFile.CreateUserName);
                                }
                            }
                            else
                            {
                                verFileProperties = docSvc.GetProperties(new long[] { verFile.Id }, new long[] { prophistid });
                                if (verFileProperties[0].Val != null)
                                {
                                    //Console.WriteLine(" Property: " + statename + " - " + verFileProperties[0].Val.ToString());
                                    //Console.WriteLine(" Property: " + statename + " - " + verFile.CreateUserName);
                                    if (fileProperties[0].Val.ToString() == state)
                                    {
                                        users.Add(verFileProperties[0].Val.ToString(), verFile.CreateUserName);
                                        Console.Write(" : " + verFile.CreateUserName);
                                    }
                                }
                            }
                            Console.WriteLine("");
                        }
                        Boolean statesmatch = true;
                        List<string> userlist = new List<string>();
                        foreach (string thisstate in states)
                        {
                            //Console.WriteLine("|" + thisstate + "|");
                            //Console.WriteLine("|" + users[thisstate].ToString() + "|");
                            if (!userlist.Contains(users[thisstate].ToString()))
                            {
                                userlist.Add(users[thisstate].ToString());
                            }
                            if (users[thisstate].ToString() == "")
                            {
                                statesmatch = false;
                            }
                        }
                        //Console.WriteLine(users.Count.ToString());
                        //Console.WriteLine(userlist.Count.ToString());
                        if (users.Count != userlist.Count)
                        {
                            statesmatch = false;
                        }
                        //Console.WriteLine(statesmatch.ToString());
                        if (statesmatch)
                        {
                            Console.WriteLine("");
                            Console.WriteLine(" " + parentFolder.FullName + "/" + file.Name);
                            //Console.WriteLine("  Master ID : " + String.Format("{0:0}", file.MasterId));
                            Console.WriteLine("  LifeCycle : " + lifecycledefinition);
                            Console.WriteLine("  State     : " + file.FileLfCyc.LfCycStateName);
                            Console.WriteLine("  New State : " + newstate);
                            try
                            {
                                docextSrv.UpdateFileLifeCycleStates(new long[] { file.MasterId }, new long[] { newstateid }, "Automatic State Change");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("ERROR: Changing state (New State - " + newstate + ")");
                                Console.WriteLine(ex.Message.ToString());
                            }
                            finally
                            {
                            }
                        }
                        else
                        {
                            if (revertstateid == -1)
                            {
                                Console.WriteLine("ERROR: Cannot change state, previous changes not performed by unique users");
                                Console.WriteLine("     : No revert state specified");
                            }
                            else
                            {
                                Console.WriteLine("");
                                Console.WriteLine(" " + parentFolder.FullName + "/" + file.Name);
                                //Console.WriteLine("  Master ID : " + String.Format("{0:0}", file.MasterId));
                                Console.WriteLine("  LifeCycle    : " + lifecycledefinition);
                                Console.WriteLine("  State        : " + file.FileLfCyc.LfCycStateName);
                                Console.WriteLine("  Reverting to : " + revertstate);
                                try
                                {
                                    docextSrv.UpdateFileLifeCycleStates(new long[] { file.MasterId }, new long[] { revertstateid }, "Automatic State Revert");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("ERROR: Changing state (New State - " + revertstate + ")");
                                    Console.WriteLine(ex.Message.ToString());
                                }
                                finally
                                {
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("");
                    }
                }
            }

            Folder[] folders = docSvc.GetFoldersByParentId(parentFolder.Id, false);
            if (folders != null && folders.Length > 0)
            {
                foreach (Folder folder in folders)
                {
                    ProcessFilesInFolder(folder, docSvc, docextSrv, statename, state, propid, prophistid, checkstates, newstateid, newlcdid, newstate, revertstateid, revertstate, lifecycledefinition);
                }
            }
        }
    }
}
