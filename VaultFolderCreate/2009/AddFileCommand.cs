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
using System.IO;
using System.Text;

using VaultFolderCreate.DocumentSvc;

namespace VaultFolderCreate
{
    class AddFileCommand
    {
        // by default, 50 MB is the max request length
        // 49 MB is used as the max file size so that there is enough room for the SOAP headers
        private static int MAX_FILE_SIZE_BYTES = 49 * 1024 * 1024;

        /// <summary>
        /// Upload a file to Vault
        /// </summary>
        /// <param name="filePath">The full path to a local file.</param>
        /// <param name="folderId">The ID of the Vault folder where the file will be uploaded.</param>
        public static void Execute(string filePath, long folderId)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            if (fileInfo.Length > MAX_FILE_SIZE_BYTES)
                MultiUpload(fileInfo, folderId);
            else
                SimpleUpload(filePath, folderId);
        }

        private static void SimpleUpload(string filePath, long folderId)
        {
            DocumentService docSvc = ServiceManager.GetDocumentService();

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] fileData = new byte[stream.Length];
                stream.Read(fileData, 0, fileData.Length);

                docSvc.AddFile(folderId, Path.GetFileName(filePath), "Added by VaultFolderCreate",
                    DateTime.Now, null, null, null, null, null,
                    FileClassification.None, false, fileData);
            }
        }

        private static void MultiUpload(FileInfo fileInfo, long folderId)
        {
            DocumentService docSvc = ServiceManager.GetDocumentService();
            Guid guid = Guid.Empty;

            using (FileStream stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                byte[] fileData = new byte[MAX_FILE_SIZE_BYTES];

                long unreadBytes;
                while ( (unreadBytes = stream.Length - stream.Position) > 0)
                {
                    if (unreadBytes < MAX_FILE_SIZE_BYTES)
                        fileData = new byte[unreadBytes];   // resize the buffer

                    stream.Read(fileData, 0, fileData.Length);

                    guid = docSvc.UploadFilePart(guid, fileData);
                }

                docSvc.AddUploadedFile(folderId, fileInfo.Name, "Added by VaultFolderCreate",
                    DateTime.Now, null, null, null, null, null,
                    FileClassification.None, false, guid);
            }
        }
    }
}
