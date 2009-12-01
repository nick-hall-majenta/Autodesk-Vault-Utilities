/*=====================================================================
  
  This file is part of the Autodesk Vault API Code Samples.

  Copyright (C) Autodesk Inc.  All rights reserved.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
=====================================================================*/

using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.Xml.XPath;

using VaultFolderCreate.DocumentSvc;

namespace VaultFolderCreate
{
	/// <summary>
	/// Allows user to browse vault files and folders.  Also displays association information
	/// for a file. 
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
        private System.Windows.Forms.TreeView m_vaultFolderTree;
        private IContainer components;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem actionsToolStripMenuItem;
        private ToolStripMenuItem m_exitToolStripMenuItem;
        private ToolStripMenuItem m_addFileToolStripMenuItem;
        private ContextMenuStrip contextMenuStrip1;

        private DocumentService m_docSvc;

        private int DisplayLevels = 2;
        private System.Windows.Forms.Label lblFoldersToCreate;
        private ToolStripMenuItem m_openToolStripMenuItem;
        private TextBox txtFolders;
        private System.Windows.Forms.Label lblFolderToCreateIn;
        private ToolStripMenuItem m_addFolderstoolStripMenuItem;
        private string DisplayTree = "$/Libraries"; 
        
		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

            m_docSvc = ServiceManager.GetDocumentService();

            string defaultConfigFile = System.IO.Path.GetDirectoryName(Application.ExecutablePath)+ "\\VaultFolderCreate.xml";
            if (System.IO.File.Exists(defaultConfigFile))
            {
                LoadConfigurationFile(defaultConfigFile);
            }

			
		}

        private void LoadConfigurationFile(string configFile)
        {
            XPathNavigator nav;
            XPathDocument docNav;
            XPathNodeIterator nodeIter;
            docNav = new XPathDocument(@configFile);
            nav = docNav.CreateNavigator();
            nodeIter = nav.Select("/VAULTFOLDERCREATE/CONFIG/DISPLAYLEVELS");
            nodeIter.MoveNext();
            DisplayLevels = Convert.ToInt32(nodeIter.Current.Value);
            nodeIter = nav.Select("/VAULTFOLDERCREATE/CONFIG/DISPLAYTREE");
            nodeIter.MoveNext();
            DisplayTree = nodeIter.Current.Value;
            nodeIter = nav.Select("/VAULTFOLDERCREATE/FOLDERS/FOLDER");
            txtFolders.Clear();
            while (nodeIter.MoveNext())
            {
                string thisFolder = nodeIter.Current.Value;
                while (thisFolder.StartsWith("/"))
                {
                    thisFolder = thisFolder.Substring(1);
                }
                txtFolders.AppendText(thisFolder + Environment.NewLine);
            }
            // Populate the tree
            Folder rootFolder = m_docSvc.GetFolderRoot();
            TreeNode rootNode = new TreeNode(rootFolder.FullName);
            rootNode.Tag = rootFolder;
            m_vaultFolderTree.Nodes.Clear();
            m_vaultFolderTree.Nodes.Add(rootNode);
            AddChildFolders(rootNode);
        }

        // Make a server call and populate the folder tree 1 level deep
        // if the folders are already there, no call to the server is made.
        private void AddChildFolders(TreeNode parentNode)
        {
            Folder parentFolder = (Folder)parentNode.Tag;
            if (parentNode.Level >= DisplayLevels) 
            {
                return;
            }
            if (parentFolder.NumClds == parentNode.Nodes.Count)
                return;  // we already have the child nodes
            
            parentNode.Nodes.Clear();

            Folder [] childFolders = m_docSvc.GetFoldersByParentId(parentFolder.Id, false);
            foreach (Folder folder in childFolders)
            {
                if (folder.FullName.StartsWith(DisplayTree) )
                {
                TreeNode childNode = new TreeNode(folder.Name);
                childNode.Tag = folder;
                parentNode.Nodes.Add(childNode);
                }
            }
        }


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );

            OpenFileCommand.OnExit();
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.m_vaultFolderTree = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.m_addFolderstoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_addFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblFoldersToCreate = new System.Windows.Forms.Label();
            this.txtFolders = new System.Windows.Forms.TextBox();
            this.lblFolderToCreateIn = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_vaultFolderTree
            // 
            this.m_vaultFolderTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.m_vaultFolderTree.ContextMenuStrip = this.contextMenuStrip1;
            this.m_vaultFolderTree.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_vaultFolderTree.Location = new System.Drawing.Point(7, 43);
            this.m_vaultFolderTree.Name = "m_vaultFolderTree";
            this.m_vaultFolderTree.Size = new System.Drawing.Size(232, 271);
            this.m_vaultFolderTree.TabIndex = 0;
            this.m_vaultFolderTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.vaultFolderTree_BeforeExpand);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_addFolderstoolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(172, 48);
            // 
            // m_addFolderstoolStripMenuItem
            // 
            this.m_addFolderstoolStripMenuItem.Name = "m_addFolderstoolStripMenuItem";
            this.m_addFolderstoolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.m_addFolderstoolStripMenuItem.Text = "Create Folders ...";
            this.m_addFolderstoolStripMenuItem.Click += new System.EventHandler(this.m_addFileToolStripMenuItem_Click);
            // 
            // m_addFileToolStripMenuItem
            // 
            this.m_addFileToolStripMenuItem.Name = "m_addFileToolStripMenuItem";
            this.m_addFileToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.m_addFileToolStripMenuItem.Text = "Add File ...";
            this.m_addFileToolStripMenuItem.Paint += new System.Windows.Forms.PaintEventHandler(this.m_addFileToolStripMenuItem_Paint);
            this.m_addFileToolStripMenuItem.Click += new System.EventHandler(this.m_addFileToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.actionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(510, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_openToolStripMenuItem,
            this.m_exitToolStripMenuItem});
            this.fileToolStripMenuItem.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // m_openToolStripMenuItem
            // 
            this.m_openToolStripMenuItem.Name = "m_openToolStripMenuItem";
            this.m_openToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.m_openToolStripMenuItem.Text = "Open File";
            this.m_openToolStripMenuItem.Click += new System.EventHandler(this.m_openToolStripMenuItem_Click);
            // 
            // m_exitToolStripMenuItem
            // 
            this.m_exitToolStripMenuItem.Name = "m_exitToolStripMenuItem";
            this.m_exitToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.m_exitToolStripMenuItem.Text = "Exit";
            this.m_exitToolStripMenuItem.Click += new System.EventHandler(this.m_exitToolStripMenuItem_Click);
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_addFileToolStripMenuItem});
            this.actionsToolStripMenuItem.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.actionsToolStripMenuItem.Text = "Actions";
            // 
            // lblFoldersToCreate
            // 
            this.lblFoldersToCreate.AutoSize = true;
            this.lblFoldersToCreate.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFoldersToCreate.Location = new System.Drawing.Point(263, 27);
            this.lblFoldersToCreate.Name = "lblFoldersToCreate";
            this.lblFoldersToCreate.Size = new System.Drawing.Size(106, 13);
            this.lblFoldersToCreate.TabIndex = 7;
            this.lblFoldersToCreate.Text = "Folders to Create";
            // 
            // txtFolders
            // 
            this.txtFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFolders.Location = new System.Drawing.Point(266, 43);
            this.txtFolders.Multiline = true;
            this.txtFolders.Name = "txtFolders";
            this.txtFolders.ReadOnly = true;
            this.txtFolders.Size = new System.Drawing.Size(232, 271);
            this.txtFolders.TabIndex = 8;
            // 
            // lblFolderToCreateIn
            // 
            this.lblFolderToCreateIn.AutoSize = true;
            this.lblFolderToCreateIn.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolderToCreateIn.Location = new System.Drawing.Point(4, 27);
            this.lblFolderToCreateIn.Name = "lblFolderToCreateIn";
            this.lblFolderToCreateIn.Size = new System.Drawing.Size(119, 13);
            this.lblFolderToCreateIn.TabIndex = 9;
            this.lblFolderToCreateIn.Text = "Folder To Create In";
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(510, 326);
            this.Controls.Add(this.lblFolderToCreateIn);
            this.Controls.Add(this.txtFolders);
            this.Controls.Add(this.lblFoldersToCreate);
            this.Controls.Add(this.m_vaultFolderTree);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(266, 360);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Vault Folder Creator";
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
		#endregion

        private void vaultFolderTree_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
        {
            // get the next level in the tree
            m_vaultFolderTree.BeginUpdate();
            foreach (TreeNode node in e.Node.Nodes)
                AddChildFolders(node);
            m_vaultFolderTree.EndUpdate();
        }

        /// <summary>
        /// Add tree node for the association tree.
        /// </summary>
        /// <param name="parentNode">Node to add to</param>
        private void AddChildAssociation(TreeNode parentNode, 
            Dictionary<long, List<File>> associationsByFile)
        {
            // get the File object for the Node
            File parentFile = (File)parentNode.Tag;

            // if associations exist, create a Node for each one
            if (associationsByFile.ContainsKey(parentFile.Id))
            {
                List<File> list = associationsByFile[parentFile.Id];
                IEnumerator<File> listEnum = list.GetEnumerator();
                while (listEnum.MoveNext())
                {
                    File childFile = listEnum.Current;
                    TreeNode childNode = new TreeNode(childFile.Name);
                    childNode.Tag = childFile;
                    parentNode.Nodes.Add(childNode);

                    // add all of the Nodes for the children's children
                    AddChildAssociation(childNode, associationsByFile);
                    
                }
            }
        }

        private void m_exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void m_openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadConfigurationFile(openFileDialog.FileName);
            }
        }

        private void MakeFolders()
        {
            TreeNode node = m_vaultFolderTree.SelectedNode;
            string nodePath = "";
            while (node != m_vaultFolderTree.TopNode)
            {
                nodePath = "/" + nodePath;
                nodePath = node.Text + nodePath;
                node = node.Parent;
            }
            nodePath = "/" + nodePath;
            foreach (string newfolder in txtFolders.Lines)
            {
                CreateFolder(m_docSvc, "$" + nodePath + newfolder);
            }
        }

        private void m_openFileToolStripMenuItem_Paint(object sender, PaintEventArgs e)
        {
        }

        private void m_addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = m_vaultFolderTree.SelectedNode;
            if (node != null)
            {
                MakeFolders();
            }
        }

        private void m_addFileToolStripMenuItem_Paint(object sender, PaintEventArgs e)
        {
            // You can only make folders if there is a folder selected
            m_addFileToolStripMenuItem.Enabled = m_vaultFolderTree.SelectedNode != null;
        }

        private void m_vaultFolderTree_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            MakeFolders();
        }

        private int CreateFolder(DocumentService docSrv, string folderpath)
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
                        //Console.WriteLine("Creating folder " + thisPath);
                        filefolder = docSrv.AddFolder(foldername, thisfolder.Id, thisfolder.IsLib);
                    }
                    thisfolder = filefolder;
                }
            }
            return 0;
        }
    }

    
}
