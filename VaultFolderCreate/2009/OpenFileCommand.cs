using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

using VaultFolderCreate.DocumentSvc;

namespace VaultFolderCreate
{
    public class OpenFileCommand
    {
        // by default, 50 MB is the max request length
        // 49 MB is used as the max file size so that there is enough room for the SOAP headers
        private static int MAX_FILE_SIZE_BYTES = 49 * 1024 * 1024;

        private static List<string> m_downloadedFiles = new List<string>();

        /// <summary>
        /// Downloads a file from Vault and opens it.  The program used to load the file is 
        /// based on the user's OS settings.
        /// </summary>
        /// <param name="fileId"></param>
        public static void Execute(DocumentSvc.File file)
        {
            string filePath = Application.LocalUserAppDataPath + "\\" + file.Name;

            //determine if the file already exists
            if (System.IO.File.Exists(filePath))
            {
                //we'll try to delete the file so we can get the latest copy
                try
                {
                    System.IO.File.Delete(filePath);

                    //remove the file from the collection of downloaded files that need to be removed when the application exits
                    if (m_downloadedFiles.Contains(filePath))
                        m_downloadedFiles.Remove(filePath);
                }
                catch (IOException)
                {
                    throw new Exception("The file you are attempting to open already exists and can not be overwritten. This file may currently be open, try closing any application you are using to view this file and try opening the file again.");
                }
            }

            if (file.FileSize > MAX_FILE_SIZE_BYTES)
                MultiDownload(file, filePath);
            else
                SimpleDownload(file, filePath);

            //Create a new ProcessStartInfo structure.
            ProcessStartInfo pInfo = new ProcessStartInfo();
            //Set the file name member. 
            pInfo.FileName = filePath;
            //UseShellExecute is true by default. It is set here for illustration.
            pInfo.UseShellExecute = true;
            Process p = Process.Start(pInfo);
        }


        /// <summary>
        /// The entire file can be downloaded in a single call
        /// </summary>
        private static void SimpleDownload(DocumentSvc.File file, string filePath)
        {
            DocumentService docSvc = ServiceManager.GetDocumentService();

            // stream the data to the client
            byte[] fileData;
            string fileName = docSvc.DownloadFile(file.Id, true, out fileData);

            using (FileStream stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                //add the newly created file to the collection of files that need to be removed when the application exits
                m_downloadedFiles.Add(filePath);

                //write the downloaded file to a physical file on the users machine
                stream.Write(fileData, 0, fileData.Length);
            }
        }

        /// <summary>
        /// The file is too big to download in a single call, so the file is downloaded in parts.
        /// </summary>
        private static void MultiDownload(DocumentSvc.File file, string filePath)
        {
            DocumentService docSvc = ServiceManager.GetDocumentService();

            // stream the data to the client
            byte[] fileData;
            string fileName = docSvc.DownloadFile(file.Id, true, out fileData);

            using (FileStream stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                //add the newly created file to the collection of files that need to be removed when the application exits
                m_downloadedFiles.Add(filePath);

                long startByte = 0;
                long endByte = MAX_FILE_SIZE_BYTES;
                byte[] buffer;

                while (startByte < file.FileSize)
                {
                    endByte = startByte + MAX_FILE_SIZE_BYTES;
                    if (endByte > file.FileSize)
                        endByte = file.FileSize;

                    buffer = docSvc.DownloadFilePart(file.Id, startByte, endByte, true);
                    stream.Write(buffer, 0, buffer.Length);
                    startByte += buffer.Length;
                }
            }
        }

        /// <summary>
        /// This should be called when the application exits
        /// </summary>
        public static void OnExit()
        {
            // try and clean up any files which were downloaded
            foreach (string file in m_downloadedFiles)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch (Exception) { }
            }
        }
    }
}
