/*=====================================================================
  
  This file is part of the Autodesk Vault API Code Samples.

  Copyright (C) Autodesk Inc.  All rights reserved.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
=====================================================================*/

using System;
using VaultFolderCreate.SecuritySvc;
using VaultFolderCreate.DocumentSvc;


namespace VaultFolderCreate
{
	/// <summary>
	/// A singleton class for managing web service objects and connection information.
	/// </summary>
	public class ServiceManager
	{
        private static ServiceManager mgr = null;

        private SecurityService secSvc = null;
        private DocumentService docSvc = null;
        private LoginInfo loginInfo;

        
		private ServiceManager()
		{}

        /// <summary>
        /// Gets the ServiceManager instance
        /// </summary>
        public static ServiceManager GetServiceManager()
        {
            if (mgr == null)
                mgr = new ServiceManager();
            return mgr;
        }

        /// <summary>
        /// Gets the security service object, or creates one if needed.
        /// </summary>
        public static SecurityService GetSecurityService(LoginInfo loginInfo)
        {
            ServiceManager mgr = GetServiceManager();

            if (mgr.secSvc == null)
            {
                mgr.loginInfo = loginInfo;

                mgr.secSvc = new SecurityService();
                mgr.secSvc.Url = mgr.SetSvcUrl(mgr.secSvc);
                mgr.secSvc.SecurityHeaderValue = new SecuritySvc.SecurityHeader();
                mgr.secSvc.SignIn(loginInfo.Username, loginInfo.Password, loginInfo.Vault);
            }

            return mgr.secSvc;
        }

        
        /// <summary>
        /// Gets the Document service object, or creates one if needed.
        /// </summary>
        public static DocumentService GetDocumentService()
        {
            ServiceManager mgr = GetServiceManager();
            
            if (mgr.docSvc == null)
            {
                mgr.docSvc = new DocumentService();
                mgr.docSvc.Url = mgr.SetSvcUrl(mgr.docSvc);
                mgr.docSvc.SecurityHeaderValue = new DocumentSvc.SecurityHeader();
                mgr.docSvc.SecurityHeaderValue.Ticket = mgr.secSvc.SecurityHeaderValue.Ticket;
                mgr.docSvc.SecurityHeaderValue.UserId = mgr.secSvc.SecurityHeaderValue.UserId;
            }
            
            return mgr.docSvc;
        }

        /// <summary>
        /// Set the URL of the web service.  This is how you point the service to a specific server. 
        /// </summary>
        private string SetSvcUrl(System.Web.Services.Protocols.SoapHttpClientProtocol svc)
        {
            UriBuilder url = new UriBuilder(svc.Url);
            url.Host = this.loginInfo.Server;

            if (this.loginInfo.Port != 0)
                url.Port = this.loginInfo.Port;

            if (this.loginInfo.SSL)
                url.Scheme = "https";

            svc.Url = url.Uri.ToString(); 
            return svc.Url;
        }

	}


    
}
