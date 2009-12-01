/*=====================================================================
  
  This file is part of the Autodesk Vault API Code Samples.

  Copyright (C) Autodesk Inc.  All rights reserved.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
=====================================================================*/


using System;
using System.Collections.Generic;
using System.Text;

namespace VaultFolderCreate
{
    [Serializable]
    public class LoginInfo
    {
        private static string HTTPS_PREFIX = "https://";

        public string Username;
        public string Password;
        public string Server;
        public bool SSL = false;

        /// <summary>
        /// A value of 0 means the default port should be used
        /// </summary>
        public int Port = 0;

        public string Vault;

        /// <summary>
        /// The server location formatted the same way as Vault Explorer.
        /// Format:  [https://]servername[:port]
        /// </summary>
        public string ServerString
        {
            get
            {
                string scheme = SSL ? "https://" : "";
                string port = Port != 0 ? ":" + Port.ToString() : "";
                return scheme + Server + port;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serverStr">Formatted information about the server.
        /// Format:  [https://]servername[:port]</param>
        public LoginInfo(string username, string password, string serverStr, string vault)
        {
            this.Username = username;
            this.Password = password;
            this.Vault = vault;

            // parse the server string

            // check to see if an SSL connection is needed
            if (serverStr.StartsWith(HTTPS_PREFIX, StringComparison.CurrentCultureIgnoreCase))
            {
                this.SSL = true;
                serverStr = serverStr.Remove(0, HTTPS_PREFIX.Length);
            }
            else
                this.SSL = false;


            // check to see if a non-default port is needed
            int index = serverStr.LastIndexOf(':');
            if (index >= 0 && serverStr.Length > index + 2)
            {
                string portStr = serverStr.Substring(index + 1);
                serverStr = serverStr.Remove(index+1);
                Int32.TryParse(portStr, out this.Port);
            }
            else
                this.Port = 0;

            this.Server = serverStr;
        }
    }
}
